using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using JobDatabase;

namespace JobMon
{
    class JobSiteEmail : JobEmail
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

        public JobSiteEmail(string aPath, JobDatabase.JobDatabase aJobDb) : base(aPath, aJobDb)
        {
            iType = "JobSite";
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
                // The preamble ends with a line of "=" characters. Sometimes there is then
                // an ad and another line of "=" characters. We do not attempt to recognise 
                // the ads.
                if ( line.StartsWith( "====" ) || line.Contains( "Hello Graham" ) )
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

            string line;
            while ((line = aReader.ReadLine()) != null)
            {
                if( line.Contains( "JOB LISTING"))
                {
                    // We have found the start of a new job - return any previous job
                    return job;
                }

                string link;
                StripHtml(ref line, out link);
                if ( link.Length > 0 )
                {
                    job.Link = link;
                    if ( line.Length > 0 )
                    {
                        // Found a job definition in which a line is a link to the full job
                        if ( !GetToken( job, line ) )
                        {
                            // No obvious token in the line: assume it is the job title
                            job.Title = line;
                        }
                        continue;
                    }
                }

                if (line.Length == 0)
                {
                    // Skip blank lines
                    continue;
                }
                else if (line.StartsWith("Feedback", StringComparison.CurrentCultureIgnoreCase) || 
                         line.StartsWith("____"))
                {
                    // Reached the email trailer - this is not a job
                    return null;
                }
                else if (line.StartsWith("To view the full job", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                else if (line.StartsWith("http:"))
                {
                    // Link should be the final item in the job
                    job.Link = line;
                    return job;
                }
                else if (line.Contains(":"))
                {
                    if ( !GetToken( job, line ) )
                    {
                        // No token: just treat the line as part of description
                        job.AddDescription( line );
                    }
                }
                else
                {
                    // Not expecting this for JobSite, but treat it as description
                    job.AddDescription(line);
                }
            }

            // Hit the end of file
            return null;
        }

        /// <summary>
        /// Test whether the supplied line contains a known token
        /// </summary>
        /// <param name="aJob">Job to be populated with token value</param>
        /// <param name="aLine">Line to be tested</param>
        /// <returns>True if token found</returns>
        static bool GetToken( Job aJob, string aLine )
        {
            char[] delimiter = { ':' };

            string[] tokens = aLine.Split( delimiter, 2 );
            if( tokens.Length < 2 )
            {
                // Can't be a token
                return false;
            }

            string tag = tokens[0].Trim().ToUpper();
            string value = tokens[1].Trim();
            if ( tag.EndsWith( "TITLE" ) )              // "JOB TITLE" or "TITLE"
            {
                aJob.Title = value;
                return true;
            }
            else if ( tag == "LOCATION" )
            {
                aJob.Location = value;
                return true;
            }
            else if ( ( tag == "TYPE" ) || ( tag == "JOB TYPE" ) )
            {
                aJob.Type = value;
                return true;
            }
            else if ( tag == "SALARY" )
            {
                aJob.Salary = value;
                return true;
            }

            return false;
        }

        #endregion
    }
}
