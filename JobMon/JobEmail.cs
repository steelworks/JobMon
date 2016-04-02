using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using JobDatabase;
using SpeechLib;

namespace JobMon
{
    // Base class for different types of job email
    public class JobEmail
    {
        string iPath = string.Empty;
        JobDatabase.JobDatabase iJobDb = null;
        int iNewJobCount = 0;                       // Tally of good looking jobs in email
        int iDuplicateJobCount = 0;                 // Tally of duplicate jobs in email
                                                    // ie, those already in the database
        int iBadJobCount = 0;                       // Tally of bad job titles in email
        int iBadLocationCount = 0;                  // Tally of bad job locations in email
        static int iGoodJobCount = 0;               // Tally of all good jobs in all emails
        protected DateTime iEmailDate = DateTime.Now;
        protected string iType = "Abstract";

        /// <summary>
        /// Constructor: called by the Create method
        /// </summary>
        /// <param name="aPath">Path to the job email</param>
        public JobEmail(string aPath, JobDatabase.JobDatabase aJobDb)
        {
            iPath = aPath;
            iJobDb = aJobDb;
        }

        /// <summary>
        /// Instantiate a specific job email from its path
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        public static JobEmail Create(string aPath, JobDatabase.JobDatabase aJobDb)
        {
            string pattern = @"(\w+)-.*";
            Regex regexp = new Regex(pattern, RegexOptions.IgnoreCase);
            Match regionMatch = regexp.Match(aPath.ToLower());
            if (regionMatch.Success)
            {
                Group components = regionMatch.Groups[1];
                if (components.Value == "cw")
                {
                    return new CwJobEmail(aPath, aJobDb);
                }
                else if (components.Value == "it")
                {
                    return new ItJobEmail(aPath, aJobDb);
                }
                else if (components.Value == "js")
                {
                    return new JsJobEmail(aPath, aJobDb);
                }
                else if (components.Value == "tj")
                {
                    return new TjJobEmail(aPath, aJobDb);
                }
                else if (components.Value == "jobsite")
                {
                    return new JobSiteEmail(aPath, aJobDb);
                }
                else
                {
                    return new JobEmail(aPath, aJobDb);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Process the email text at the preset path and add the jobs to the database
        /// </summary>
        /// <param name="aTextBoxTrace">Text box for diagnostic trace</param>
        /// <param name="aProblem">Holds error details if applicable</param>
        /// <returns>True: processed successfully, False: error</returns>
        public bool GetJobs(System.Windows.Forms.RichTextBox aTextBoxTrace, out string aProblem)
        {
            aProblem = string.Empty;

            try
            {
                using (StreamReader emailReader = new StreamReader(iPath, Encoding.GetEncoding(1252)))
                {
                    iEmailDate = ProcessPreamble(emailReader);

                    List<Job> jobs = GetJobs(emailReader);
                    foreach (Job job in jobs)
                    {
                        // Test whether the job is to be filtered out (of no interest)
                        bool badJob = false;
                        bool badLocation = false;
                        if (iJobDb.IsFiltered(job, out badJob, out badLocation))
                        {
                            if (badJob)
                            {
                                if (aTextBoxTrace != null)
                                {
                                    TraceJob(aTextBoxTrace, "Bad job: " + job.Title,
                                             System.Drawing.Color.Red);
                                }

                                iBadJobCount++;
                            }

                            if (badLocation)
                            {
                                if (aTextBoxTrace != null)
                                {
                                    TraceJob(aTextBoxTrace, "Bad location: " + job.Location,
                                             System.Drawing.Color.Brown);
                                }

                                iBadLocationCount++;
                            }

                            // Filtered out: skip further processing of this job
                            continue;
                        }

                        // Test whether this job is already in the database
                        int existingId;
                        DateTime lastSeen;
                        bool databaseError;
                        if (iJobDb.AlreadySeen(job, out existingId, out lastSeen, out databaseError))
                        {
                            // Merge new job with existing job
                            if (!iJobDb.Merge(job, existingId, lastSeen))
                            {
                                aProblem = iJobDb.Diagnostic;
                                return false;
                            }

                            if (aTextBoxTrace != null)
                            {
                                TraceJob(aTextBoxTrace, "Duplicate job: " + job.Title,
                                         System.Drawing.Color.Blue);
                            }

                            iDuplicateJobCount++;
                        }
                        else
                        {
                            // Add new job into the database (but first check that AlreadySeen
                            // did not return an error)
                            if (databaseError || !iJobDb.Add(job))
                            {
                                aProblem = iJobDb.Diagnostic;
                                return false;
                            }

                            TraceJob(aTextBoxTrace, "Good job: " + job.Title,
                                     System.Drawing.Color.Green);
                            iNewJobCount++;

                            SpVoice commentator = new SpVoiceClass();
                            string commentary = string.Format("Job number {0}: {1}, {2}",
                                                              ++iGoodJobCount, job.Source, job.Title);
                            commentator.Speak(commentary, SpeechVoiceSpeakFlags.SVSFDefault);
                        }
                    }

                    return true;
                }
            }
            catch ( Exception ex )
            {
                // Seen this when Dylys fetched the emails - Steelworks did not have access to
                // the temporary files created by Eudora
                aProblem = "Failed when processing " + iPath + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Display the supplied text in the supplied rich text box, in the specified colour
        /// </summary>
        /// <param name="aTextBoxTrace"></param>
        /// <param name="aText"></param>
        /// <param name="aColour"></param>
        private static void TraceJob(System.Windows.Forms.RichTextBox aTextBoxTrace, string aText, 
                                     System.Drawing.Color aColour)
        {
            aTextBoxTrace.SelectionColor = aColour;
            aTextBoxTrace.AppendText(aText + "\r\n");
            aTextBoxTrace.ScrollToCaret();
            System.Windows.Forms.Application.DoEvents();
        }

        public override string ToString()
        {
            return string.Format("{0} job email for {1}\r\n" +
                                 "{2} new jobs, {3} dup jobs, {4} bad jobs, {5} bad locations", 
                                 iType, iEmailDate,
                                 iNewJobCount, iDuplicateJobCount, iBadJobCount, iBadLocationCount);
        }

        #region Protected methods

        /// <summary>
        /// Process the initial part of the email prior to the jobs
        /// </summary>
        /// <param name="aReader">Access to the email</param>
        /// <returns>Email date extracted from the preamble</returns>
        protected virtual DateTime ProcessPreamble(StreamReader aReader)
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Base method should never be called. But don't think the base class
        /// can be abstract, in order to handle unrecognised email types.
        /// </summary>
        /// <param name="aReader"></param>
        /// <returns>A list of supposedly valid jobs</returns>
        protected virtual List<Job> GetJobs(StreamReader aReader)
        {
            return new List<Job>();
        }

        // Strip HTML, leaving plain text
        // But if the HTML contains a link (<a> tag), return the link
        protected void StripHtml(ref string aLine, out string aLink)
        {
            aLink = string.Empty;

            // Remove tags
            string tag = @"(<[^<>]*>)";
            Regex regexp = new Regex(tag, RegexOptions.IgnoreCase);
            Match regionMatch = regexp.Match(aLine);
            while (regionMatch.Success)
            {
                if( regionMatch.Value.Contains("href="))
                {
                    string linkTag = @"href=""([^""]*)""";
                    Regex linkRegexp = new Regex(linkTag, RegexOptions.IgnoreCase);
                    Match linkMatch = linkRegexp.Match( regionMatch.Value );
                    if (linkMatch.Success)
                    {
                        aLink = linkMatch.Groups[1].Value;
                    }
                }
                Group components = regionMatch.Groups[1];
                aLine = aLine.Replace(components.Value, "");
                regionMatch = regexp.Match(aLine);
            }

            // Replace common escapes
            aLine = aLine.Replace("&amp;", "&");
            aLine = aLine.Replace("&nbsp;", " ");
            aLine = aLine.Replace("&lt;", "<");
            aLine = aLine.Replace("&gt;", ">");
            aLine = aLine.Replace("&quot;", "\"");

            // Trim white space
            aLine = aLine.Trim();
        }

        // Strip HTML, leaving plain text
        protected void StripHtml( ref string aLine )
        {
            string unwantedLink;
            StripHtml( ref aLine, out unwantedLink );
        }

        #endregion Protected methods
    }
}
