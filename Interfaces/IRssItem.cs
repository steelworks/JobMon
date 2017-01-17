using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IRssItem
    {
        string Title { get; set; }
        string Link { get; set; }
        string Description { get; set; }
        DateTime Date { get; set; }
    }
}
