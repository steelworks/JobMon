using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JobDatabase;

namespace JobMon
{
    /// <summary>
    /// Allow user to perform maintenance operations on the database
    /// </summary>
    public partial class FormMaintenance : Form
    {
        /// <summary>
        /// Constructor: show initial statistics
        /// </summary>
        /// <param name="aDb"></param>
        public FormMaintenance(JobDatabase.JobDatabase aDb)
        {
            InitializeComponent();

            iDb = aDb;
            RefreshStatistics();
        }

        /// <summary>
        /// User has changed the number of days
        /// </summary>
        private void TrackBarAge_ValueChanged(object sender, EventArgs e)
        {
            RefreshStatistics();
        }

        /// <summary>
        /// User wants to delete jobs from the database
        /// </summary>
        private void ButtonDelete_Click(object sender, EventArgs e)
        {

            string message = "Delete " + iLabelOlderJobs.Text + "?";
            if (MessageBox.Show(message, "Maintenance", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                iDb.DeleteJobsOlderThan(iNumberOfDays);
                RefreshStatistics();
            }
        }

        private void RefreshStatistics()
        {
            iNumberOfDays = iTrackBarAge.Value;
            iNumberOfJobs = iDb.GetCountOfJobsOlderThan(iNumberOfDays);
            int totalJobs = 0;
            int unreadJobs = 0;
            if (iDb.GetStats(out totalJobs, out unreadJobs))
            {
                iLabelTotalJobs.Text = string.Format("{0} jobs in database", totalJobs);
                iLabelOlderJobs.Text = string.Format("{0} jobs older than {1} days", iNumberOfJobs, iNumberOfDays);
            }
            else
            {
                iLabelTotalJobs.Text = "Failed to get stats from database";
                iLabelOlderJobs.Text = iDb.Diagnostic;
            }
        }

        JobDatabase.JobDatabase iDb;
        int iNumberOfDays;
        int iNumberOfJobs;
    }
}
