using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobDatabase
{
    public class Job
    {
        int iId;                        // Database id, if retrieved from database
        string iSource;                 // CW Jobs, JobServe, etc
        DateTime iDate;
        List<string> iLines;            // Raw details
        string iTitle;
        string iType;                   // Permanent / contract
        string iSalary;
        string iLocation;
        bool iInteresting;
        List<string> iDescription;      // Parsed description from within the email
        List<string> iDetail;           // Detail from the web link
        List<string> iRationalisedDescription;  // Description with long lines split up
        string iLink;
        int iCount;                     // Number of times seen
        DateTime iFirstSeen;
        DateTime iLastSeen;

        // Public properties
        public int Id
        {
            get { return iId; }
            set { iId = value; }
        }

        public string Source
        {
            get { return iSource; }
            set { iSource = value; }
        }

        public DateTime Date
        {
            get { return iDate; }
            set { iDate = value; }
        }

        public DateTime FirstSeen
        {
            get { return iFirstSeen; }
            set { iFirstSeen = value; }
        }

        public DateTime LastSeen
        {
            get { return iLastSeen; }
            set { iLastSeen = value; }
        }

        public string Title
        {
            get { return iTitle; }
            set { iTitle = value; }
        }

        public string Location
        {
            get { return iLocation; }
            set { iLocation = value; }
        }

        public string Salary
        {
            get { return iSalary; }
            set { iSalary = value; }
        }

        public string Type
        {
            get { return iType; }
            set { iType = value; }
        }

        public string Link
        {
            get { return iLink; }
            set { iLink = value; }
        }

        public int Count
        {
            get { return iCount; }
            set { iCount = value; }
        }

        public bool Interesting
        {
            get { return iInteresting; }
            set { iInteresting = value; }
        }

        public List<string> Description
        {
            get { return iDescription; }
            set 
            { 
                // Assign iDescription as-is
                iDescription = value;

                // Split long lines into shorter lines for rationalised description
                iRationalisedDescription = new List<string>();
                foreach (string line in iDescription)
                {
                    List<string> shortLines = GetShortLines(line);
                    iRationalisedDescription.AddRange(shortLines);
                }
            }
        }

        public List<string> RationalisedDescription
        {
            get { return iRationalisedDescription; }
        }

        public List<string> Detail
        {
            get { return iDetail; }
            set { iDetail = value; }
        }

        public bool IsComplete
        {
            // Job definition is complete if we have a link
            get { return iLink.Length > 0; }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="aSource">CW Jobs, JobServe, etc</param>
        /// <param name="aDate">Date of email in which job occurs</param>
        public Job(string aSource, DateTime aDate)
        {
            iId = -1;
            iSource = aSource;
            iDate = aDate;
            iFirstSeen = aDate;
            iLastSeen = aDate;

            iTitle = string.Empty;
            iType = string.Empty;
            iSalary = string.Empty;
            iLocation = string.Empty;
            iLink = string.Empty;

            iLines = new List<string>();
            iDescription = new List<string>();
            iRationalisedDescription = new List<string>();
            iDetail = new List<string>();
        }

        public void AddDescription(string aLine)
        {
            // Add one line to the description
            iDescription.Add(aLine);

            // Add potentially multiple lines to the rationalised description
            List<string> shortLines = GetShortLines(aLine);
            iRationalisedDescription.AddRange(shortLines);
        }

        #region Private methods

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
