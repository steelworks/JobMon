using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SQLite;

namespace JobDatabase
{
    /// <summary>
    /// Class to represent each "word" to be filtered in the GetFilterColours method
    /// </summary>
    public class FilterResult
    {
        public int Index;
        public int Length;

        public FilterResult(int aIndex, int aLength)
        {
            Index = aIndex;
            Length = aLength;
        }
    }

    public class JobDatabase
    {
        // Private data members
        IDbConnection iCon = null;
        IDataReader iQueryResults = null;
        string iDiagnostic = string.Empty;
        bool iHealthy = false;

        Dictionary<string, int> iJobFilters = null;
        Dictionary<string, int> iGoodLocFilters = null;
        Dictionary<string, int> iBadLocFilters = null;
        List<string> iWebFilters = null;
        List<string> iWebJoins = null;

        // Public properties
        public string Diagnostic
        {
            get { return iDiagnostic; }
        }

        // Constructor
        public JobDatabase()
        {
            // Load filtering stuff into memory for performance
            iHealthy = false;

            if (!LoadFilter(out iJobFilters, "job"))
            {
                return;
            }

            if (!LoadFilter(out iGoodLocFilters, "goodloc"))
            {
                return;
            }

            if (!LoadFilter(out iBadLocFilters, "badloc"))
            {
                return;
            }

            if (!LoadFilter(out iWebFilters, "webfilter"))
            {
                return;
            }

            if (!LoadFilter(out iWebJoins, "webjoin"))
            {
                return;
            }

            // Successfully loaded all filters
            iHealthy = true;
        }

        #region Public methods

        /// <summary>
        /// Sanity check to be called after the constructor
        /// </summary>
        /// <param name="aInformation">Statistical information, or diagnostic if not valid</param>
        /// <returns></returns>
        public bool IsValid(out string aInformation)
        {
            if (iHealthy)
            {
                aInformation = string.Format("Working with {0} job filters\r\n" +
                                             "Working with {1} goodloc filters\r\n" +
                                             "Working with {2} badloc filters\r\n" +
                                             "Working with {3} web filters\r\n" +
                                             "Working with {4} web joins\r\n",
                                             iJobFilters.Count, iGoodLocFilters.Count,
                                             iBadLocFilters.Count, iWebFilters.Count,
                                             iWebJoins.Count);
                return true;
            }
            else
            {
                aInformation = iDiagnostic;
                return false;
            }
        }

        /// <summary>
        /// Close the database, updating the filters to reflect processing during the session.
        /// </summary>
        /// <returns>True if filters updated successfully, false if not</returns>
        public bool Close()
        {
            // Only update the filters if we have a valid database connection
            if (iHealthy)
            {
                return UpdateFilter(iJobFilters, "job") &&
                       UpdateFilter(iGoodLocFilters, "goodloc") &&
                       UpdateFilter(iBadLocFilters, "badloc");
            }
            else
            {
                // Not really updated successfully, but there is no new failure
                return true;
            }
        }

        /// <summary>
        /// Create new record in MySQL tables
        /// </summary>
        /// <param name="aJob"></param>
        /// <returns></returns>
        public bool Add(Job aJob)
        {
            // Update the Jobs table
            string sql = string.Format("INSERT INTO jobs " +
                                       "(source, title, type, salary, location, " +
                                       " first_posting, last_posting, date_found, link, " +
                                       " comment, count, interesting) " +
                                       "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', " +
                                       "        'None', 1, 0)",
                                       aJob.Source, SqlSanitise(aJob.Title), aJob.Type,
                                       SqlSanitise(aJob.Salary), SqlSanitise(aJob.Location),
                                       SqliteDate(aJob.Date), SqliteDate(aJob.Date),
                                       SqliteDate(DateTime.Now), aJob.Link);
            int rowsAffected = RunNonQuery(sql);
            if (rowsAffected != 1)
            {
                return false;
            }

            // Retrieve the job id for the record just created
            int jobId = RunScalar("SELECT MAX(id) FROM Jobs");
            if (jobId < 0)
            {
                iDiagnostic = "Add: failed to get max id";
                return false;
            }

            // Update the summary table with rationalised lines (broken into short lines)
            foreach (string line in aJob.RationalisedDescription)
            {
                sql = string.Format("INSERT INTO summary " +
                                    "(jobid, line) " +
                                    "VALUES ('{0}', '{1}')",
                                    jobId, SqlSanitise(line));
                int summaryRowsAffected = RunNonQuery(sql);
                if (summaryRowsAffected != 1)
                {
                    return false;
                }
            }

            // Update the detail table.
            // The old Perl Jobs Monitor used to store a text version of the details
            // from the web page here. This was useful in the days of dial-up.
            // Now the link is better. This is also stored in the Jobs table, so
            // ultimately the Detail table can be dropped.
            // Note: long links may be divided over multiple lines of detail.
            foreach (string shortLine in GetShortLines(aJob.Link))
            {
                sql = string.Format("INSERT INTO detail " +
                                    "(jobid, line) " +
                                    "VALUES ('{0}', '{1}')",
                                    jobId, shortLine);
                int detailRowsAffected = RunNonQuery(sql);
                if (detailRowsAffected != 1)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Merge the supplied job with the specified existing job in the database
        /// </summary>
        /// <param name="aId">Id of existing job in database</param>
        /// <param name="aJob">New job details to be merged in</param>
        /// <returns></returns>
        public bool Merge(Job aJob, int aId, DateTime aLastSeen)
        {
            // We increment the count in the existing job in the database only if it was
            // last seen on a previous day - ie, count the number of days on which the job
            // has been seen rather than the number of times it has been seen.
            int deltaCount = (aJob.Date > aLastSeen ? 1 : 0);

            // Update the Jobs table
            string sql = string.Format("UPDATE jobs SET count=count+{0}, " +
                                       "                first_posting=min(first_posting, \"{1}\"), " +
                                       "                last_posting=max(last_posting, \"{2}\") " +
                                       "WHERE id = {3}",
                                       deltaCount, SqliteDate(aJob.Date), SqliteDate(aJob.Date), aId);
            int rowsAffected = RunNonQuery(sql);

            // No change to Summary or Details table
            return (rowsAffected == 1);
        }

        /// <summary>
        /// Match the supplied job against an existing job in the database
        /// </summary>
        /// <param name="aJob">Job to be matched</param>
        /// <param name="aExistingId">Database Id of matching job</param>
        /// <param name="aLastSeen">Date last seen of matching job</param>
        /// <param name="aError">Flag set if "no match" is due to error</param>
        /// <returns>True if match found, false if no match found</returns>
        public bool AlreadySeen(Job aJob, out int aExistingId, out DateTime aLastSeen,
                                out bool aError)
        {
            aExistingId = -1;
            aLastSeen = DateTime.Now;
            aError = false;

            // Should try matching the link URL - if this matches, it is definitely the
            // same job!

            // Basic match on Jobs table
            string basicQuery = string.Format("SELECT id, last_posting, link FROM jobs " +
                                              "WHERE title = '{0}' AND " +
                                              "      salary = '{1}' AND " +
                                              "      location = '{2}'",
                                              SqlSanitise(aJob.Title), SqlSanitise(aJob.Salary),
                                              SqlSanitise(aJob.Location));

            // Accumulate a list of existing job ids that give a basic match with aJob.
            // List is a dictionary mapping onto the date of the existing job.
            Dictionary<int, DateTime> basicMatches = new Dictionary<int, DateTime>();

            // AlreadySeen method needs to distinguish between seen, not seen, and error!
            bool success = QueryBegin(basicQuery);
            if (success)
            {
                IDataReader existingJob;
                while (QueryGetNextRecord(out existingJob))
                {
                    // If the link matches, it is definitely the same job, and we can avoid
                    // examining the summary table
                    aExistingId = (int)(long)existingJob["id"];
                    if (!existingJob.IsDBNull(2))
                    {
                        string existingLink = existingJob["link"].ToString();
                        if (aJob.Link == existingLink)
                        {
                            QueryEnd();
                            return true;
                        }
                    }

                    DateTime lastSeen = (DateTime)existingJob["last_posting"];
                    basicMatches.Add(aExistingId, lastSeen);
                }

                QueryEnd();
            }
            else
            {
                // MySQL query failure: details are in the Diagnostic
                aError = true;
                return false;
            }

            // Perform a detailed match on the basic matching job ids
            foreach (int jobId in basicMatches.Keys)
            {
                int matchedLines = 0;
                int mismatchedLines = 0;
                foreach (string line in aJob.RationalisedDescription)
                {
                    string summaryLine = SqlSanitise(line);
                    string detailedQuery = string.Format("SELECT id FROM summary " +
                                                         "WHERE jobid = {0} AND line = \"{1}\"",
                                                         jobId, summaryLine);
                    success = QueryBegin(detailedQuery);
                    if (success)
                    {
                        IDataReader existingSummary;
                        if (QueryGetNextRecord(out existingSummary))
                        {
                            // There is a summary line in the database job matching the supplied job
                            matchedLines++;
                        }
                        else
                        {
                            mismatchedLines++;
                        }

                        QueryEnd();
                    }
                    else
                    {
                        // MySQL query failure: details are in the Diagnostic
                        aError = true;
                        return false;
                    }
                }

                // Here is how we determine if the detailed match is satisfactory
                if ((matchedLines > mismatchedLines) && (mismatchedLines <= 2))
                {
                    // Match this database job because it is "good enough".
                    // Ideally we would iterate through the remaining jobs and select
                    // the best match.
                    aExistingId = jobId;
                    aLastSeen = basicMatches[jobId];
                    return true;
                }
            }

            // No satisfactory matches in database
            return false;
        }

        /// <summary>
        /// Check if the job title contains an unwanted word, and if the location
        /// contains an unwanted word
        /// </summary>
        /// <param name="aJob">Job to be checked</param>
        /// <param name="aBadJob"></param>
        /// <param name="aBadLocation"></param>
        /// <returns>True if job is to be filtered out (ie, unwanted)</returns>
        public bool IsFiltered(Job aJob, out bool aBadJob, out bool aBadLocation)
        {
            aBadJob = false;
            aBadLocation = false;

            // Check if any job filter word is contained in the supplied Job title
            foreach (string filter in iJobFilters.Keys)
            {
                // Case insensitive match
                if (aJob.Title.ToLower().Contains(filter.ToLower()))
                {
                    // Match on the filter, increment the count for this filter
                    iJobFilters[filter]++;
                    aBadJob = true;
                    return true;
                }
            }

            // Check if any bad location filter matches the Job location exactly
            foreach (string filter in iBadLocFilters.Keys)
            {
                // Case insensitive match
                if (aJob.Location.ToLower().Equals(filter.ToLower()))
                {
                    // Match on the filter, increment the count for this filter
                    iBadLocFilters[filter]++;
                    aBadLocation = true;
                    return true;
                }
            }

            // Check if any good location filter matches the Job location at all.
            // Note: if Job location contains a bad loc and a good loc, it counts as good.
            foreach (string filter in iGoodLocFilters.Keys)
            {
                // Case insensitive match
                if (aJob.Location.ToLower().Contains(filter.ToLower()))
                {
                    // Match on the filter, increment the count for this filter.
                    // Return false: good location, do not want to filter the job out
                    iGoodLocFilters[filter]++;
                    return false;
                }
            }

            // Check if any bad location filter matches the Job location at all
            foreach (string filter in iBadLocFilters.Keys)
            {
                // Case insensitive match
                if (aJob.Location.ToLower().Contains(filter.ToLower()))
                {
                    // Match on the filter, increment the count for this filter
                    iBadLocFilters[filter]++;
                    aBadLocation = true;
                    return true;
                }
            }

            // No filter match: indifference means good
            return false;
        }

        /// <summary>
        /// Return list of jobs matching the specified date
        /// </summary>
        /// <param name="aDate">Date for which jobs are required</param>
        /// <returns>Null if error, otherwise (potentially empty) list of jobs</returns>
        public List<Job> FetchJobs(DateTime aDate, bool aNew, bool aInteresting)
        {
            List<Job> matchingJobs = new List<Job>();

            // Query the jobs table to get the basic details
            string jobSql = string.Empty;
            if (aNew)
            {
                // Select new (unread) jobs, regardless of date
                jobSql = "SELECT * FROM jobs " +
                         "WHERE Viewed = 0 " +
                         "ORDER BY date_found, title";
            }
            else if ( aInteresting )
            {
                // Select interesting jobs for the specified date
                jobSql = string.Format("SELECT * FROM jobs " +
                                       "WHERE date_found = '{0}' AND interesting = 1 " +
                                       "ORDER BY title",
                                       SqliteDate(aDate));
            }
            else
            {
                // Select all jobs for the specified date
                jobSql = string.Format("SELECT * FROM jobs " +
                                       "WHERE date_found = '{0}' " +
                                       "ORDER BY title",
                                       SqliteDate(aDate));
            }

            if (QueryBegin(jobSql))
            {
                IDataReader record;
                while (QueryGetNextRecord(out record))
                {
                    string source = record["source"].ToString();
                    Job thisJob = new Job(source, aDate);
                    thisJob.Title = record["title"].ToString();
                    thisJob.Id = (int)(long)record["id"];
                    thisJob.FirstSeen = (DateTime)record["first_posting"];
                    thisJob.LastSeen = (DateTime)record["last_posting"];
                    thisJob.Type = record["type"].ToString();
                    thisJob.Salary = record["salary"].ToString();
                    thisJob.Location = record["location"].ToString();
                    thisJob.Count = (int)(long)record["count"];
                    thisJob.Interesting = (((int)(long)record["interesting"]) > 0);
                    thisJob.Link = record["link"].ToString();

                    matchingJobs.Add(thisJob);
                }

                QueryEnd();
            }
            else
            {
                // iDiagnostic should give clue to problem
                return null;
            }

            // Now query the summary table to add detail to the jobs
            foreach (Job job in matchingJobs)
            {
                string detailSql = string.Format("SELECT line FROM summary " +
                                                 "WHERE jobid = '{0}' " +
                                                 "ORDER BY id",
                                                 job.Id);

                if (QueryBegin(detailSql))
                {
                    IDataReader record;
                    while (QueryGetNextRecord(out record))
                    {
                        job.AddDescription(record["line"].ToString());
                    }

                    QueryEnd();
                }
                else
                {
                    // iDiagnostic should give clue to problem
                    return null;
                }
            }

            return matchingJobs;
        }

        /// <summary>
        /// Update the state of the Interesting flag in the database for the supplied job
        /// </summary>
        /// <param name="aJob"></param>
        /// <returns>True if successful, false if database error</returns>
        public bool SaveInterest(Job aJob)
        {
            string sql = string.Format("UPDATE jobs SET interesting = {0} " +
                                       "WHERE id = {1}",
                                       (aJob.Interesting ? 1 : 0), aJob.Id);
            int rowsAffected = RunNonQuery(sql);

            // If something went wrong, diagnostic should be set
            return (rowsAffected == 1);
        }

        /// <summary>
        /// Update the state of the Viewed flag in the database for the supplied job
        /// (indicates that the job has now been viewed)
        /// </summary>
        /// <param name="aJob"></param>
        /// <returns>True if successful, false if database error</returns>
        public bool ShowRead(Job aJob)
        {
            string sql = string.Format("UPDATE jobs SET viewed = 1 WHERE id = {0}", aJob.Id);
            int rowsAffected = RunNonQuery(sql);

            // If something went wrong, diagnostic should be set
            return (rowsAffected == 1);
        }

        /// <summary>
        /// Given a source text string, return a list of descriptors within the string that
        /// filters dictate should be coloured red, and a list of descriptors that should be
        /// coloured green. Each descriptors comprises the index of the filtered word and its 
        /// length. Note: the returned "words" may be phrases rather than words.
        /// </summary>
        /// <param name="aText"></param>
        /// <param name="aRedWords"></param>
        /// <param name="aGreenWords"></param>
        public void GetFilterColours(string aText, out List<FilterResult> aRedWords,
                                     out List<FilterResult> aGreenWords)
        {
            aRedWords = new List<FilterResult>();
            aGreenWords = new List<FilterResult>();

            foreach (string word in iJobFilters.Keys)
            {
                // "word" may be multiple words, but use \b to insist on whole words
                Regex regexp = new Regex("\\b" + word + "\\b", RegexOptions.IgnoreCase);
                foreach (Match match in regexp.Matches(aText))
                {
                    aRedWords.Add(new FilterResult(match.Index, match.Length));
                }
            }

            foreach (string word in iBadLocFilters.Keys)
            {
                Regex regexp = new Regex("\\b" + word + "\\b", RegexOptions.IgnoreCase);
                foreach (Match match in regexp.Matches(aText))
                {
                    aRedWords.Add(new FilterResult(match.Index, match.Length));
                }
            }

            foreach (string word in iGoodLocFilters.Keys)
            {
                Regex regexp = new Regex("\\b" + word + "\\b", RegexOptions.IgnoreCase);
                foreach (Match match in regexp.Matches(aText))
                {
                    aGreenWords.Add(new FilterResult(match.Index, match.Length));
                }
            }
        }

        /// <summary>
        /// Return the list of current filter items of the specified type.
        /// Logically this might be achieved through a query on the database,
        /// but we already hold the items in memory!
        /// </summary>
        /// <param name="aFilterName"></param>
        /// <returns></returns>
        public List<string> GetFilterItems(string aFilterName)
        {
            List<string> items = null;

            if (aFilterName == "badloc")
            {
                items = new List<string>(iBadLocFilters.Keys);
            }
            else if (aFilterName == "goodloc")
            {
                items = new List<string>(iGoodLocFilters.Keys);
            }
            else if (aFilterName == "job")
            {
                items = new List<string>(iJobFilters.Keys);
            }
            else
            {
                // Unrecognised filter type
                items = new List<string>();
            }

            // Return items in alphabetical order
            items.Sort();
            return items;
        }

        /// <summary>
        /// Add a new item to the specified filter
        /// </summary>
        /// <param name="aFilter">The filter type to be extended</param>
        /// <param name="aItem">The item to be added to the filter</param>
        public bool AddFilterItem(string aFilterName, string aItem)
        {
            // Update the in-memory filter
            if (aFilterName == "badloc")
            {
                iBadLocFilters.Add(aItem, 0);
            }
            else if (aFilterName == "goodloc")
            {
                iGoodLocFilters.Add(aItem, 0);
            }
            else if (aFilterName == "job")
            {
                iJobFilters.Add(aItem, 0);
            }
            else
            {
                iDiagnostic = "Unrecognised filter: " + aFilterName;
                return false;
            }

            // Update the filters in the database
            string sql = string.Format("INSERT INTO filters " +
                                       "(type, value, count, last_used) " +
                                       "VALUES ('{0}', '{1}', 0, '{2}')",
                                       aFilterName, aItem, SqliteDate(DateTime.Now));
            int rowsAffected = RunNonQuery(sql);
            if (rowsAffected != 1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Remove an item from the specified filter
        /// </summary>
        /// <param name="aFilter">The filter type to be revised</param>
        /// <param name="aItem">The item to be removed from the filter</param>
        public bool DeleteFilterItem(string aFilterName, string aItem)
        {
            // Update the in-memory filters
            if ((aFilterName == "badloc") && iBadLocFilters.ContainsKey(aItem))
            {
                iBadLocFilters.Remove(aItem);
            }
            else if ((aFilterName == "goodloc") && iGoodLocFilters.ContainsKey(aItem))
            {
                iGoodLocFilters.Remove(aItem);
            }
            else if ((aFilterName == "job") && iJobFilters.ContainsKey(aItem))
            {
                iJobFilters.Remove(aItem);
            }

            // Update the filters in the database
            string sql = string.Format("DELETE FROM filters WHERE type = '{0}' AND value = '{1}'",
                                       aFilterName, aItem);
            int rowsAffected = RunNonQuery(sql);
            if (rowsAffected != 1)
            {
                return false;
            }

            return true;
        }

        public bool GetStats(out int aTotalJobs, out int aUnreadJobs)
        {
            aTotalJobs = RunScalar("SELECT COUNT(*) FROM Jobs");
            aUnreadJobs = RunScalar("SELECT COUNT(*) FROM Jobs WHERE Viewed = 0");

            // If RunScalar fails, it returns a negative result and iDiagnostic is loaded with details
            return ((aTotalJobs >= 0) && (aUnreadJobs >= 0));
        }

        public int GetCountOfJobsOlderThan(int aNumberOfDays)
        {
            string sql = string.Format("SELECT COUNT(*) " +
                                       "FROM Jobs " +
                                       "WHERE (julianday(current_date) - julianday(last_posting)) > {0}", 
                                       aNumberOfDays);
            return RunScalar(sql);
        }

        public void DeleteJobsOlderThan(int aNumberOfDays)
        {
            // Learn the threshold ID to be deleted
            string sql = string.Format("SELECT MIN(ID) " +
                                       "FROM JOBS " +
                                       "WHERE (julianday(current_date) - julianday(last_posting)) <= {0}",
                                       aNumberOfDays);
            int thresholdId = RunScalar(sql);

            // Delete from supporting tables: this is likely to leave some orphan records in those tables,
            // as we are not deleting a contiguous set of IDs from the Jobs table - orphans will be
            // deleted in subsequent maintenance.
            sql = string.Format("DELETE FROM SUMMARY WHERE JOBID < {0}", thresholdId);
            RunNonQuery(sql);
            sql = string.Format("DELETE FROM DETAIL WHERE JOBID < {0}", thresholdId);
            RunNonQuery(sql);

            // Delete from the main table
            sql = string.Format("DELETE FROM JOBS WHERE (julianday(current_date) - julianday(last_posting)) > {0}", 
                                aNumberOfDays);
            RunNonQuery(sql);
        }

        #endregion Public methods


        #region Private methods

        // Use this method to get a connection to the Sqlite database
        private IDbConnection CreateConnection()
        {
            string connectionString = 
                string.Format(@"Data Source={0};", Properties.Settings.Default.SqlitePath);
            return new SQLiteConnection(connectionString);
        }

        // Use this method to open the connection to the Sqlite database, and
        // handle the potential for the database not being there.
        private bool OpenConnection(IDbConnection aCon)
        {
            try
            {
                aCon.Open();
                return true;
            }
            catch (Exception e)
            {
                iDiagnostic = string.Format("JobDatabase: no connection ({0})", e.Message);
                return false;
            }
        }

        // Begin a MySql query
        private bool QueryBegin(string aSqlStatement)
        {
            iCon = CreateConnection();
            if (!OpenConnection(iCon))
            {
                // No database connection: iDiagnostic is set
                return false;
            }
            else
            {
                using (IDbCommand command = iCon.CreateCommand())
                {
                    command.CommandText = aSqlStatement;

                    // Assign results to DataReader
                    try
                    {
                        iQueryResults = command.ExecuteReader();
                    }
                    catch (SQLiteException e)
                    {
                        iDiagnostic = string.Format("QueryTableBegin: did not like the SQL\n" +
                                                    "{0}\n{1}\n", e.ToString(), command.CommandText);
                        return false;
                    }
                }

                return true;
            }
        }

        // Get next record from current MySql query
        private bool QueryGetNextRecord(out IDataReader aRecord)
        {
            // Is there another record to return?
            if (iQueryResults.Read())
            {
                aRecord = iQueryResults;
                return true;
            }
            else
            {
                aRecord = null;
                return false;
            }
        }

        // End a MySql query - free resources
        private void QueryEnd()
        {
            iQueryResults.Close();
            iCon.Close();
        }

        // Non-query - no results
        private int RunNonQuery(string aSql)
        {
            using (IDbConnection con = CreateConnection())
            {
                if (!OpenConnection(con))
                {
                    // No database connection: iDiagnostic is set
                    return -1;
                }
                else
                {
                    using (IDbCommand command = con.CreateCommand())
                    {
                        command.CommandText = aSql;

                        try
                        {
                            int result = command.ExecuteNonQuery();
                            return result;
                        }
                        catch (SQLiteException e)
                        {
                            iDiagnostic =
                                string.Format("RunNonQuery: did not like the SQL\n{0}\n{1}\n",
                                              e.ToString(), command.CommandText);
                            return -1;
                        }
                    }
                }
            }
        }

        // Query with single (integer) result
        private int RunScalar(string aSql)
        {
            using (IDbConnection con = CreateConnection())
            {
                if (!OpenConnection(con))
                {
                    // No database connection: iDiagnostic is set
                    return -1;
                }
                else
                {
                    using (IDbCommand command = con.CreateCommand())
                    {
                        command.CommandText = aSql;

                        try
                        {
                            // Normal casts fail here: use Convert
                            int result = Convert.ToInt32(command.ExecuteScalar());
                            return result;
                        }
                        catch (SQLiteException e)
                        {
                            iDiagnostic = string.Format("RunScalar: did not like the SQL\n" +
                                                        "{0}\n{1}\n", e.ToString(), command.CommandText);
                            return -1;
                        }
                    }
                }
            }
        }


        // Load data from the MySQL filters table into memory
        private bool LoadFilter(out Dictionary<string, int> aDictionaryFilter, string aName)
        {
            aDictionaryFilter = new Dictionary<string, int>();

            string sql = string.Format("SELECT value FROM Filters WHERE type = '{0}'", aName);
            if (QueryBegin(sql))
            {
                IDataReader record;
                while (QueryGetNextRecord(out record))
                {
                    if (!record.IsDBNull(0))
                    {
                        aDictionaryFilter.Add(record.GetString(0), 0);
                    }
                }

                QueryEnd();
                return true;
            }
            else
            {
                // iDiagnostic should give clue to problem
                return false;
            }
        }

        private bool LoadFilter(out List<string> aListFilter, string aName)
        {
            aListFilter = new List<string>();

            string sql = string.Format("SELECT value FROM Filters WHERE type = '{0}'", aName);
            if (QueryBegin(sql))
            {
                IDataReader record;
                while (QueryGetNextRecord(out record))
                {
                    if (!record.IsDBNull(0))
                    {
                        aListFilter.Add(record.GetString(0));
                    }
                }

                QueryEnd();
                return true;
            }
            else
            {
                // iDiagnostic should give clue to problem
                return false;
            }
        }

        // Update the MySQL filters table from memory
        private bool UpdateFilter(Dictionary<string, int> aDictionaryFilter, string aName)
        {
            foreach (KeyValuePair<string, int> kvp in aDictionaryFilter)
            {
                // Check the number of times that the filter has come into play this session.
                // Ignore if zero.
                if (kvp.Value > 0)
                {
                    string sql = string.Format("UPDATE Filters " +
                                               "SET Count = Count + {0}, " +
                                               "    Last_Used = '{1}' " +
                                               "WHERE type = '{2}' AND value = '{3}'",
                                               kvp.Value, SqliteDate(DateTime.Now), aName, kvp.Key);
                    int rowsAffected = RunNonQuery(sql);

                    if (rowsAffected != 1)
                    {
                        // Something went wrong: diagnostic should be set
                        return false;
                    }
                }
            }

            return true;
        }

        private string SqlSanitise(string aString)
        {
            // Duplicate quotes ready for SQL query
            string doubleQuotesHandled = aString.Replace("\"", "\"\"");
            string singleQuotesHandled = doubleQuotesHandled.Replace("'", "''");
            return singleQuotesHandled;
        }

        private string SqliteDate(DateTime aDate)
        {
            return aDate.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Break a potentially long line into lines of up to 70 characters
        /// </summary>
        /// <param name="aFullLine"></param>
        /// <returns></returns>
        private List<string> GetShortLines(string aFullLine)
        {
            List<string> shortLines = new List<string>();
            string remaining = aFullLine;
            while (remaining.Length > 70)
            {
                int spacePos = remaining.LastIndexOf(' ', 70);
                if (spacePos < 0)
                {
                    // No suitable space: have to break on 70
                    shortLines.Add(remaining.Substring(0, 70));
                    remaining = remaining.Substring(70);
                }
                else
                {
                    // Break on the space, don't include the space in the shortLines
                    shortLines.Add(remaining.Substring(0, spacePos));
                    remaining = remaining.Substring(spacePos + 1);
                }
            }

            if (remaining.Length > 0)
            {
                shortLines.Add(remaining);
            }

            return shortLines;
        }

        #endregion Private methods
    }
}
