using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using JobDatabase;

namespace JobMon
{
    class CwJobEmail : JobEmail
    {
        // Constructor
        public CwJobEmail(string aPath, JobDatabase.JobDatabase aJobDb)
            : base(aPath, aJobDb)
        {
            iType = "CW Jobs";
        }

        #region Protected methods

        /// <summary>
        /// Process the initial part of the email prior to the jobs.
        /// CW Jobs does not state the date explicitly, so look for it in the email
        /// header.
        /// </summary>
        /// <param name="aReader">Access to the email</param>
        /// <returns>Email date extracted from the preamble</returns>
        protected override DateTime ProcessPreamble(StreamReader aReader)
        {
            // Default date for the email, but we hope to find an explicit one
            DateTime emailDate = DateTime.Now;

            string line;
            while ((line = aReader.ReadLine()) != null)
            {
                StripHtml(ref line);
                if (line.StartsWith("Date:", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Anticipating a date of the form dd mmm yyyy
                    string pattern = @"(\d+\s+\w+\s+\d+)";
                    Regex regexp = new Regex(pattern, RegexOptions.IgnoreCase);
                    Match dateMatch = regexp.Match(line);
                    if (dateMatch.Success)
                    {
                        Group components = dateMatch.Groups[1];
                        if (!DateTime.TryParse(components.Value, out emailDate))
                        {
                            emailDate = DateTime.Now;
                        }
                    }
                }

                // Hoping for this identifier to appear towards the end of the preamble
                if (line.StartsWith("Jobs by Email search"))
                {
                    break;
                }
            }

            return emailDate;
        }

        /// <summary>
        /// Return list of all the jobs in the email
        /// </summary>
        /// <param name="aReader"></param>
        /// <returns></returns>
        protected override List<Job> GetJobs(StreamReader aReader)
        {
            List<Job> jobs = new List<Job>();
            Job thisJob = null;
            while ((thisJob = GetJob(aReader)) != null)
            {
                if (thisJob.IsComplete)
                {
                    jobs.Add(thisJob);
                }

                // Otherwise it is not a job, maybe an ad
            }

            return jobs;
        }

        #endregion Protected methods

        #region Private methods

        /// <summary>
        /// Return one job from the supplied input stream.
        /// The caller must check the Job.IsComplete property to determine whether the
        /// job is valid.
        /// </summary>
        /// <param name="aReader"></param>
        /// <returns>The job, or null if end of file</returns>
        Job GetJob(StreamReader aReader)
        {
            Job job = new Job(iType, iEmailDate);   // Skeletal job

            char[] delimiter = { ':' };

            string line;
            while ((line = aReader.ReadLine()) != null)
            {
                StripHtml(ref line);

                if (line.Length == 0)
                {
                    // Jobs are divided by blank lines: recognise end of job.
                    // It may not be valid, caller should test the Job.IsComplete property
                    return job;
                }

                // There is no line break between the preamble and the first real job.
                // Recognise this and remove the preamble.
                int pos;
                if ( (pos = line.IndexOf(".Job Title")) >= 0 )
                {
                    line = line.Substring(pos + 1);
                }

                // Process line of job
                // Parsing lines of the form "Name: value"
                string[] tokens = line.Split(delimiter, 2);
                if (tokens.Length == 2)
                {
                    string type = tokens[0].ToLower();
                    string value = tokens[1];

                    if (type == "job title")
                    {
                        job.Title = value;
                    }
                    else if (type == "salary")
                    {
                        job.Salary = value;
                    }
                    else if (type == "job position")
                    {
                        job.Type = value;
                    }
                    else if (type == "location")
                    {
                        job.Location = value;
                    }
                    else if (type == "company")
                    {
                        // Sometimes employer, usually agency - add to description
                        job.AddDescription(value);
                    }
                    else if (type == "published date")
                    {
                        DateTime jobDate = DateTime.Now;
                        if (DateTime.TryParse(value, out jobDate))
                        {
                            // Specific date for the job
                            job.Date = jobDate;
                        }
                        else
                        {
                            // Failed to parse: use generic email date
                            job.Date = iEmailDate;
                        }
                    }
                    else if (type == "link")
                    {
                        job.Link = value;
                    }
                }
            }

            // Hit the end of file: there is no post-amble in CW Jobs emails
            return null;
        }

        #endregion
    }
}
