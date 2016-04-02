using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using JobDatabase;

namespace JobMon
{
    class TjJobEmail : JobEmail
    {
        // Enum to iterate through the lines of a job ad
        private enum State
        {
            JobSummary,
            JobMoreInformation,
            JobLink,
            JobEnd
        }

        public TjJobEmail(string aPath, JobDatabase.JobDatabase aJobDb)
            : base(aPath, aJobDb)
        {
            iType = "TotalJobs";
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

                // The sections of the email are divided by lines of hyphens, so
                // a line of hyphens ends the preamble.
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

            State state = State.JobSummary;           // State machine initial state

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
                else if (line.StartsWith("This has been a message", StringComparison.CurrentCultureIgnoreCase) &&
                         line.Contains("TotalJobs"))
                {
                    // Reached the email trailer - this is not a job
                    return null;
                }

                // Process line of job
                switch (state)
                {
                    case State.JobSummary:
                        // Anticipating a summary of the form
                        //  <Title> - Salary: <salary> - Location: <Location>
                        string tokenisedLine = line.Replace(" · Salary: ", "~").Replace(" · Location: ", "~");
                        string[] tokens = tokenisedLine.Split("~".ToCharArray());
                        if (tokens.Length == 3)
                        {
                            job.Title = tokens[0];
                            job.Salary = tokens[1];
                            job.Location = tokens[2];
                            state++;
                        }
                        break;

                    case State.JobMoreInformation:
                        if ( line.StartsWith("For more information", StringComparison.CurrentCultureIgnoreCase) )
                        {
                            state++;
                        }
                        break;

                    case State.JobLink:
                        // Several lines of summary until we see a link
                        if (line.StartsWith("http:"))
                        {
                            // Link should be the final item in the ad
                            job.Link = line;
                            state = State.JobEnd;
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
