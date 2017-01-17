using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IJob
    {
        int Id { get; set; }
        string Source { get; set; }
        DateTime Date { get; set; }
        DateTime FirstSeen { get; set; }
        DateTime LastSeen { get; set; }
        string Title { get; set; }
        string Location { get; set; }
        string Salary { get; set; }
        string Type { get; set; }
        string Link { get; set; }
        int Count { get; set; }
        bool Interesting { get; set; }
        List<string> Description { get; set; }
        List<string> RationalisedDescription { get; }
        List<string> Detail { get; set; }
        bool IsComplete { get; }
    }
}
