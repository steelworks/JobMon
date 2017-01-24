using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JobRss
{
    public class JobServeRssReader : RssReader
    {

        public JobServeRssReader(string aFeed, IJobProcessor aJobProcessor) : base("jobserve", aFeed, aJobProcessor)
        { }

        /// <summary>
        /// Parse a JobServe-specific RSS file
        /// </summary>
        /// <param name="aFeed">The JobServe RSS feed</param>
        /// <returns></returns>
        protected override IEnumerable<RssItem> ParseRssFile(string aFeed)
        {
            string[] lineBreak = new string[] { "<br/>" };
            string[] nonBreakingSpace = new string[] { "&nbsp;" };

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
                string baseDescription = rssSubNode != null ? rssSubNode.InnerText : "";
                var descriptionLines = baseDescription.Split(lineBreak, StringSplitOptions.RemoveEmptyEntries);
                string description = descriptionLines.Last();
                string salary = string.Empty;
                string location = string.Empty;
                foreach (var line in descriptionLines.Take(descriptionLines.Length - 1))
                {
                    foreach (var subLine in line.Split(nonBreakingSpace, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (subLine.Contains("Rate:"))
                        {
                            // Salary record
                            salary = StripHtml(subLine).Replace("Rate:", string.Empty);
                        }
                        else if (subLine.Contains("Location:"))
                        {
                            // Location record
                            location = StripHtml(subLine).Replace("Location:", string.Empty);
                        }
                        else
                        {
                            // Unrecognised record: retain the original as the description
                            description = baseDescription;
                        }
                    }
                }

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
                    Extended = new RssExtendedInformation()
                    {
                        Salary = salary,
                        Location = location,
                    },
                };
            }
        }
    }
}
