using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using JobDatabase;

namespace JobMon
{
    class ItJobEmail : JobEmail
    {
        // Enum to iterate through the lines of a job ad
        private enum State
        {
            JobTitle,
            JobDetails,
            JobEnd
        }

        public ItJobEmail(string aPath, JobDatabase.JobDatabase aJobDb)
            : base(aPath, aJobDb)
        {
            iType = "IT Job Board";
        }

        #region Protected methods

        /// <summary>
        /// Process the initial part of the email prior to the jobs
        /// </summary>
        /// <param name="aReader">Access to the email</param>
        /// <returns>Email date extracted from the preamble</returns>
        protected override DateTime ProcessPreamble(StreamReader aReader)
        {
            // Default date for the email
            DateTime emailDate = DateTime.Now;

            string line;
            while ((line = aReader.ReadLine()) != null)
            {
                // The sections of the email are divided by lines of hyphens, so
                // a line of hyphens ends the preamble.
                if (line.Trim().StartsWith("----"))
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

            State state = State.JobTitle;           // State machine initial state

            string line;
            while ((line = aReader.ReadLine()) != null)
            {
                StripHtml(ref line);

                if (line.Length == 0)
                {
                    // Skip blank lines
                    continue;
                }
                else if (line.StartsWith("----"))
                {
                    // Recognise end of job - it may not be valid, caller should
                    // test the Job.IsComplete property
                    return job;
                }
                else if (line.Contains("If you are having problems"))
                {
                    // Reached the email trailer - this is not a job
                    return null;
                }

                // Process line of job
                switch (state)
                {
                    case State.JobTitle:
                        // Job is summarised with numbered summary - ignore this
                        string pattern = @"^\d+.";
                        Regex regexp = new Regex(pattern, RegexOptions.IgnoreCase);
                        Match summaryMatch = regexp.Match(line);
                        if (summaryMatch.Success)
                        {
                            // Ignore the summary
                            break;
                        }
                        else if (line.Length > 0)
                        {
                            // Get the job title
                            job.Title = line;
                            state++;
                        }
                        break;

                    case State.JobDetails:
                        if (line.Length > 0)
                        {
                            if (line.StartsWith("http:"))
                            {
                                // Link should be the final item in the ad
                                job.Link = line;
                                state = State.JobEnd;
                            }
                            else
                            {
                                // Parse the bar-separated components
                                string[] components = line.Split('|');
                                foreach (string component in components)
                                {
                                    string trimmedComponent = component.Trim();
                                    if (trimmedComponent.StartsWith("SALARY:"))
                                    {
                                        if (trimmedComponent.Length > 8)
                                        {
                                            job.Salary = trimmedComponent.Substring(8);
                                        }
                                    }
                                    else if (trimmedComponent.StartsWith("LOCATION:"))
                                    {
                                        if (trimmedComponent.Length > 10)
                                        {
                                            job.Location = trimmedComponent.Substring(10);
                                        }
                                    }
                                    else if (trimmedComponent.StartsWith("TYPE:"))
                                    {
                                        if (trimmedComponent.Length > 6)
                                        {
                                            job.Type = trimmedComponent.Substring(6);
                                        }
                                    }
                                    else
                                    {
                                        // Not sure about this component: just add it to the description
                                        job.AddDescription(trimmedComponent);
                                    }
                                }

                            }
                        }
                        break;

                    default:
                        // Don't know what this is
                        break;
                }
            }

            // Hit the end of file
            return null;
        }

        #endregion
    }
}
