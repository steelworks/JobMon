using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public class RssExtendedInformation
    {
        public string Salary { get; set; }
        public string Location { get; set; }
    }

    /// <summary>
    /// The first four fields are the standard fields in an RSS record.
    /// The Extended information is non-standard and specific to JobMon.
    /// </summary>
    public interface IRssItem
    {
        string Title { get; set; }
        string Link { get; set; }
        string Description { get; set; }
        DateTime Date { get; set; }
        RssExtendedInformation Extended { get; set;  }
    }
}
