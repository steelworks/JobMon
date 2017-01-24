using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobRss
{
    public static class RssReaderFactory
    {
        public static RssReader Construct(string aSource, string aFeed, IJobProcessor aJobProcessor)
        {
            return (aSource == "jobserve" ? new JobServeRssReader(aFeed, aJobProcessor) : new RssReader(aSource, aFeed, aJobProcessor));
        }
    }
}
