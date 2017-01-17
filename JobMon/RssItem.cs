using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobMon
{
    /// <summary>
    /// Item from an RSS feed
    /// </summary>
    public class RssItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }

        /// <summary>
        /// Format the RSS item as HTML
        /// </summary>
        public override string ToString()
        {
            return "<a href='" + Link + "'>" + Title + "</a><br>" + Description + "<hr>" + Date + "<hr>";
        }
    }
}
