using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobRss
{
    /// <summary>
    /// Item from an RSS feed
    /// </summary>
    public class RssItem : IRssItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public RssExtendedInformation Extended { get; set; }

        /// <summary>
        /// Format the RSS item as HTML
        /// </summary>
        public override string ToString()
        {
            return "<a href='" + Link + "'>" + Title + "</a><br>" + Description + "<hr>" + Date + "<hr>";
        }
    }
}
