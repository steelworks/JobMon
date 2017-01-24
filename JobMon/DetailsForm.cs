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
    public partial class DetailsForm : Form
    {
        JobDatabase.JobDatabase iDb;
        int iCurrentJob = 0;
        List<Job> iJobs = null;

        /// <summary>
        /// Constructor: populate form with first job for given date
        /// </summary>
        /// <param name="aDate"></param>
        /// <param name="aDb"></param>
        public DetailsForm(DateTime aDate, JobDatabase.JobDatabase aDb)
        {
            InitializeComponent();

            iDb = aDb;

            jobDateTimePicker.Value = aDate;
        }

        private void PopulateForm()
        {
            jobProgress.Minimum = 0;
            jobProgress.Maximum = iJobs.Count;
            if (iCurrentJob < iJobs.Count)
            {
                Job thisJob = iJobs[iCurrentJob];

                // Populate the summary text box
                string rawText =
                    string.Format("{0} ({1})\n\n" +
                                  "Job id {2} advertised {3} times from {4} to {5}\n" +
                                  "Salary: {6}\n" +
                                  "Location: {7}\n" +
                                  "Link: {8}\n\n",
                                  thisJob.Title, thisJob.Source,
                                  thisJob.Id, thisJob.Count, thisJob.FirstSeen, thisJob.LastSeen,
                                  thisJob.Salary,
                                  thisJob.Location,
                                  thisJob.Link);
                foreach (string line in thisJob.Description)
                {
                    rawText += (line + "\n");
                }

                // Learn the RTF formatting required for this job
                List<FilterResult> redWords = null;
                List<FilterResult> greenWords = null;
                iDb.GetFilterColours(rawText, out redWords, out greenWords);

                // Apply the rtf formatting
                summaryRichTextBox.Text = rawText;

                // Ensure we start with no colouring - not sure why, but occasionally get odd results
                // if this is not done.
                summaryRichTextBox.SelectAll();
                summaryRichTextBox.SelectionColor = System.Drawing.Color.Black;

                foreach (FilterResult redWord in redWords)
                {
                    summaryRichTextBox.Select(redWord.Index, redWord.Length);
                    summaryRichTextBox.SelectionColor = System.Drawing.Color.Red;
                }

                foreach (FilterResult greenWord in greenWords)
                {
                    summaryRichTextBox.Select(greenWord.Index, greenWord.Length);
                    summaryRichTextBox.SelectionColor = System.Drawing.Color.Green;
                }

                interestingCheckBox.Checked = thisJob.Interesting;

                jobCountLabel.Text = string.Format("Job {0} of {1}", iCurrentJob+1, iJobs.Count);
                jobProgress.Value = iCurrentJob + 1;

                // Populate the web browser control with the details.
                // This may take a second or two, so refresh the form first.
                Application.DoEvents();
                try
                {
                    jobWebBrowser.Navigate(new Uri(thisJob.Link));

                    // If details are successfully displayed, we mark the job as read
                    if (!iDb.ShowRead(thisJob))
                    {
                        summaryRichTextBox.Text = "Database error: " + iDb.Diagnostic;
                    }
                }
                catch
                {
                    jobWebBrowser.Stop();
                }
            }
            else
            {
                summaryRichTextBox.Text =
                    "No jobs for " + jobDateTimePicker.Value.ToShortDateString();
            }

            prevButton.Enabled = (iCurrentJob > 0);
            nextButton.Enabled = (iCurrentJob < (iJobs.Count - 1));
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void prevButton_Click(object sender, EventArgs e)
        {
            if (iCurrentJob > 0)
            {
                iCurrentJob--;
                PopulateForm();
            }
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if (iCurrentJob < (iJobs.Count - 1))
            {
                iCurrentJob++;
                PopulateForm();
            }
        }

        /// <summary>
        /// Add a new bad location to the filters, or allow editing of the bad location filters
        /// </summary>
        private void badLocButton_Click(object sender, EventArgs e)
        {
            if (iCurrentJob < iJobs.Count)
            {
                Job thisJob = iJobs[iCurrentJob];
                EditorForm badLocEditorForm = new EditorForm(iDb, thisJob.Location, "badloc");
                badLocEditorForm.ShowDialog();
            }
        }

        /// <summary>
        /// Add a new good location to the filters, or allow editing of the good location filters
        /// </summary>
        private void goodLocButton_Click(object sender, EventArgs e)
        {
            if (iCurrentJob < iJobs.Count)
            {
                Job thisJob = iJobs[iCurrentJob];
                EditorForm goodLocEditorForm = new EditorForm(iDb, thisJob.Location, "goodloc");
                goodLocEditorForm.ShowDialog();
            }
        }

        /// <summary>
        /// Add a new bad job to the filters, or allow editing of the bad job filters
        /// </summary>
        private void badJobButton_Click(object sender, EventArgs e)
        {
            if (iCurrentJob < iJobs.Count)
            {
                Job thisJob = iJobs[iCurrentJob];
                EditorForm badJobEditorForm = new EditorForm(iDb, thisJob.Title, "job");
                badJobEditorForm.ShowDialog();
            }

        }

        /// <summary>
        /// Database maintenance: typically removing old jobs
        /// </summary>
        private void maintenanceButton_Click(object sender, EventArgs e)
        {
            FormMaintenance maintenance = new FormMaintenance(iDb);
            maintenance.ShowDialog();
        }

        /// <summary>
        /// User toggles whether or not the current job is interesting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void interestingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (iCurrentJob < iJobs.Count)
            {
                // Update in memory
                Job thisJob = iJobs[iCurrentJob];
                thisJob.Interesting = interestingCheckBox.Checked;

                // Update in database
                if (!iDb.SaveInterest(thisJob))
                {
                    summaryRichTextBox.Text = "Database error: " + iDb.Diagnostic;
                }
            }
        }

        /// <summary>
        /// User changes the date for which the report is required, or a radio button
        /// specifying the choice of selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DateTimePickerOrRadioButton_ValueChanged(object sender, EventArgs e)
        {
            // Radio buttons dictate the choice of selection of jobs
            bool getNewJobs = iNewRadioButton.Checked;
            bool getInterestingJobs = iInterestingRadioButton.Checked;

            iJobs = iDb.FetchJobs(jobDateTimePicker.Value, getNewJobs, getInterestingJobs);
            iCurrentJob = 0;
            if (iJobs == null)
            {
                summaryRichTextBox.Text = "Database error: " + iDb.Diagnostic;
                prevButton.Enabled = false;
                nextButton.Enabled = false;
            }
            else
            {
                PopulateForm();
            }
        }
    }
}
