using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IJobProcessor
    {
        /// <summary>
        /// Textual details of any error in processing the job source
        /// </summary>
        string Diagnostic { get; }

        /// <summary>
        /// Return an empty Job to be populated by the caller
        /// </summary>
        /// <param name="aSource">CW Jobs, JobServe, etc</param>
        /// <param name="aDate">Date of email in which job occurs</param>
        IJob Create(string aSource, DateTime aDate);

        /// <summary>
        /// Return a Job populated from the RSS item
        /// </summary>
        /// <param name="aSource">CW Jobs, JobServe, etc</param>
        /// <param name="aItem">RSS item for one job</param>
        IJob Create(string aSource, IRssItem aItem);

        /// <summary>
        /// Record a new processed job
        /// </summary>
        /// <param name="aJob"></param>
        /// <returns></returns>
        bool Add(IJob aJob);

        /// <summary>
        /// Match the supplied job against an existing job in the database
        /// </summary>
        /// <param name="aJob">Job to be matched</param>
        /// <param name="aExistingId">Database Id of matching job</param>
        /// <param name="aLastSeen">Date last seen of matching job</param>
        /// <param name="aError">Flag set if "no match" is due to error</param>
        /// <returns>True if match found, false if no match found</returns>
        bool AlreadySeen(IJob aJob, out int aExistingId, out DateTime aLastSeen, out bool aError);

        /// <summary>
        /// Check if the job title contains an unwanted word, and if the location
        /// contains an unwanted word
        /// </summary>
        /// <param name="aJob">Job to be checked</param>
        /// <param name="aBadJob"></param>
        /// <param name="aBadLocation"></param>
        /// <returns>True if job is to be filtered out (ie, unwanted)</returns>
        bool IsFiltered(IJob aJob, out bool aBadJob, out bool aBadLocation);

        /// <summary>
        /// Merge the supplied job with the specified existing job
        /// </summary>
        /// <param name="aId">Id of existing job</param>
        /// <param name="aJob">New job details to be merged in</param>
        /// <returns></returns>
        bool Merge(IJob aJob, int aId, DateTime aLastSeen);
    }
}
