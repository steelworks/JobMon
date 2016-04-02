using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using JobDatabase;

namespace JobMon
{
    class JsJobEmail : JobEmail
    {
        // Enum to iterate through the lines of a job ad
        private enum State
        {
            JobTitle,
            JobLocation,
            JobSalary,
            JobType,
            JobWho,
            JobSummary,
            JobLink,
            JobEnd
        }

        public JsJobEmail(string aPath, JobDatabase.JobDatabase aJobDb) : base(aPath, aJobDb)
        {
            iType = "JobServe";
        }

        #region Protected methods

        /// <summary>
        /// Process the initial part of the email prior to the jobs
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
                if (line.StartsWith("Daily jobs", StringComparison.CurrentCultureIgnoreCase))
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

                // The sections of the email are divided by lines of hyphens, so
                // a line of hyphens ends the preamble. Unfortunately it is difficult
                // to tell which sections are jobs and which are ads.
                if (line.StartsWith("----"))
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
                else if (line.StartsWith("Copyright", StringComparison.CurrentCultureIgnoreCase) && 
                         line.Contains("JobServe"))
                {
                    // Reached the email trailer - this is not a job
                    return null;
                }

                // Process line of job
                switch (state)
                {
                    case State.JobTitle:
                        job.Title = line;
                        state++;
                        break;

                    case State.JobLocation:
                        job.Location = line;
                        state++;
                        break;

                    case State.JobSalary:
                        job.Salary = line;
                        state++;
                        break;

                    case State.JobType:
                        job.Type = line;
                        state++;
                        break;

                    case State.JobWho:
                        // Sometimes employer, usually agency - add to description
                        job.AddDescription(line);
                        state++;
                        break;

                    case State.JobSummary:
                        // Several lines of summary until we see a link
                        if (line.StartsWith("http:"))
                        {
                            // Link should be the final item in the ad
                            job.Link = line;
                            state = State.JobEnd;
                        }
                        else
                        {
                            // No advancement of the state until we find the link
                            job.AddDescription(line);
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
