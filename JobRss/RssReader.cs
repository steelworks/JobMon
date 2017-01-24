using Interfaces;
using SpeechLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace JobRss
{
    public class RssReader
    {
        IJobProcessor iJobDb = null;
        string iFeed;
        string iSource;
        int iNewJobCount = 0;                       // Tally of good looking jobs in RSS items
        int iDuplicateJobCount = 0;                 // Tally of duplicate jobs in RSS items
                                                    // ie, those processed previously
        int iBadJobCount = 0;                       // Tally of bad job titles in RSS items
        int iBadLocationCount = 0;                  // Tally of bad job locations in RSS items
        static int iGoodJobCount = 0;               // Tally of all good jobs in all RSS items

        public RssReader(string aSource, string aFeed, IJobProcessor aJobProcessor)
        {
            iSource = aSource;
            iFeed = aFeed;
            iJobDb = aJobProcessor;
        }

        /// <summary>
        /// Process the RSS feed and add the jobs to the database
        /// </summary>
        /// <param name="aTextBoxTrace">Text box for diagnostic trace</param>
        /// <param name="aProblem">Holds error details if applicable</param>
        /// <returns>True: processed successfully, False: error</returns>
        public bool GetJobs(System.Windows.Forms.RichTextBox aTextBoxTrace, out string aProblem)
        {
            aProblem = string.Empty;

            try
            {
                foreach ( var rssJob in ParseRssFile(iFeed))
                {
                    // Test whether the job is to be filtered out (of no interest)
                    bool badJob = false;
                    bool badLocation = false;
                    var job = iJobDb.Create(iSource, rssJob);
                    if (iJobDb.IsFiltered(job, out badJob, out badLocation))
                    {
                        if (badJob)
                        {
                            if (aTextBoxTrace != null)
                            {
                                TraceJob(aTextBoxTrace, "Bad job: " + job.Title, System.Drawing.Color.Red);
                            }

                            iBadJobCount++;
                        }

                        if (badLocation)
                        {
                            if (aTextBoxTrace != null)
                            {
                                TraceJob(aTextBoxTrace, "Bad location: " + job.Location, System.Drawing.Color.Brown);
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
                        // Add new job into the database (but first check that AlreadySeen did not return an error)
                        if (databaseError || !iJobDb.Add(job))
                        {
                            aProblem = iJobDb.Diagnostic;
                            return false;
                        }

                        TraceJob(aTextBoxTrace, "Good job: " + job.Title, System.Drawing.Color.Green);
                        iNewJobCount++;

                        SpVoice commentator = new SpVoiceClass();
                        string commentary = string.Format("RSS Job number {0}: {1}, {2}", ++iGoodJobCount, iSource, job.Title);
                        commentator.Speak(commentary, SpeechVoiceSpeakFlags.SVSFDefault);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Could be lack of Internet access
                aProblem = "Failed when processing " + iFeed + ": " + ex.Message;
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

        protected virtual IEnumerable<RssItem> ParseRssFile(string aFeed)
        {
            XmlDocument rssXmlDoc = new XmlDocument();

            // Load the RSS file from the RSS URL
            try
            {
                rssXmlDoc.Load(aFeed);
            }
            catch
            {
                throw new Exception("Cannot access RSS feed " + aFeed);
            }

            // Parse the Items in the RSS file
            XmlNodeList rssNodes = rssXmlDoc.SelectNodes("rss/channel/item");

            // Iterate through the items in the RSS file
            foreach (XmlNode rssNode in rssNodes)
            {
                XmlNode rssSubNode = rssNode.SelectSingleNode("title");
                string title = rssSubNode != null ? rssSubNode.InnerText : "";

                rssSubNode = rssNode.SelectSingleNode("link");
                string link = rssSubNode != null ? rssSubNode.InnerText : "";

                rssSubNode = rssNode.SelectSingleNode("description");
                string description = rssSubNode != null ? rssSubNode.InnerText : "";

                rssSubNode = rssNode.SelectSingleNode("pubDate");
                string pubDate = rssSubNode != null ? rssSubNode.InnerText : "";

                DateTime parsedDate;
                if (!DateTime.TryParse(pubDate, out parsedDate))
                {
                    parsedDate = DateTime.Now;
                }

                yield return new RssItem()
                {
                    Title = title,
                    Link = link,
                    Description = StripHtml(description),
                    Date = parsedDate,
                };
            }
        }


        // Strip HTML, leaving plain text
        protected string StripHtml(string aLine)
        {
            // Remove tags
            string tag = @"(<[^<>]*>)";
            Regex regexp = new Regex(tag, RegexOptions.IgnoreCase);
            Match regionMatch = regexp.Match(aLine);
            while (regionMatch.Success)
            {
                Group components = regionMatch.Groups[1];
                aLine = aLine.Replace(components.Value, "");
                regionMatch = regexp.Match(aLine);
            }

            // Replace common escapes, trim white space, and return
            return aLine.Replace("&amp;", "&")
                        .Replace("&nbsp;", " ")
                        .Replace("&lt;", "<")
                        .Replace("&gt;", ">")
                        .Replace("&quot;", "\"")
                        .Trim();
        }

    }
}
