using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using JobDatabase;
using JobRss;

namespace JobMon
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            // Append CVS version to the main form title
            string version = "$Name: Rev_1_001 $";
            string[] components = version.Split(' ');
            if ((components.Length >= 3) && !version.Contains("1_001"))
            {
                this.Text += (" " + components[1]);
            }
            else
            {
	            // Show the version number in the form.
	            // This will only work properly if the correct Major/Minor version number was set
	            // when publishing the installer.
	            this.Text += (" " + PublishVersion());
            }

            // Open the database
            iJobDb = new JobDatabase.JobDatabase();
            string info;
            if (iJobDb.IsValid(out info))
            {
                textBoxInfo.Text = string.Format("Job database loaded successfully\r\n{0}", info);

                // Check for jobs on regular clock tick processing
                iStatsUpdateRequired = true;            // No stats known as yet
                iTimer = new Timer();
                iTimer.Interval = 5000;
                iTimer.Tick += TimedCheckForUpdates;
                iTimer.Start();
            }
            else
            {
                // No database connection: no processing possible
                textBoxInfo.Text = string.Format("Job database failed to load\r\n{0}", info);
            }

            // Populate the RSS feeds
            iRssFeeds = new Dictionary<string, bool>();
            foreach ( var feed in Properties.Settings.Default.RssFeeds)
            {
                // False: we have not yet successfully processed this feed
                iRssFeeds.Add(feed, false);
            }
        }

        #region Event handlers

        /// <summary>
        /// Regular clock tick processing: check for any new job emails
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimedCheckForUpdates(object sender, EventArgs e)
        {
            // Checking may take time: stop the timer whilst processing
            iTimer.Stop();

            toolStripCheckIndicator.Visible = !toolStripCheckIndicator.Visible;
            foreach (string email in GetEmails())
            {
                textBoxInfo.Text += string.Format("Found <{0}>\r\n", email);
                if (!ProcessEmail(email))
                {
                    // Failed to process an email: alert the user, return before restarting the timer
                    textBoxInfo.ForeColor = Color.Red;
                    return;
                }

                // Processed a job email - stats are out of date
                iStatsUpdateRequired = true;
            }

            // Update the info panel - only when required
            // (otherwise ShowJobStats queries the database needlessly)
            if ( iStatsUpdateRequired )
            {
                textBoxInfo.Select(textBoxInfo.Text.Length - 1, 1);
                textBoxInfo.ScrollToCaret();

                // Update the job counts in the top panel of the main form
                ShowJobStats();

                // Updated the stats - update no longer required
                iStatsUpdateRequired = false;
            }

            // Process each RSS feed once successfully.
            // Ideally we would like to process each at regular intervals to check for new jobs,
            // but we really don't want to process the whole feed again.
            // We are also processing the feed for one day - if JobMon has not been running for a few days
            // we should process the feed for intervening days. If JobMon is run multiple times in a day
            // we should recognise it and avoid processing the same feed.
            var allFeeds = new List<string>(iRssFeeds.Keys);
            foreach ( var feed in allFeeds )
            {
                // Process the feeds that we have not yet processed successfully
                if (!iRssFeeds[feed])
                {
                    // Crude parsing of the Job source from the RSS URL: the word following www.
                    var source = "Unknown";
                    var wwwPos = feed.ToLower().IndexOf("www.");
                    if (wwwPos >= 0)
                    {
                        var domainPos = feed.IndexOf(".", wwwPos + 4);
                        if ( domainPos > 0)
                        {
                            source = feed.Substring(wwwPos + 4, domainPos - wwwPos - 4);
                        }
                    }
                    RssReader rss = new RssReader(source, feed, iJobDb);
                    string problem = string.Empty;
                    if (rss.GetJobs(richTextBoxTrace, out problem))
                    {
                        textBoxInfo.Text += string.Format("Processed RSS {0}\r\n", feed);
                        iRssFeeds[feed] = true;         // Successfully processed - don't try this feed again
                    }
                    else
                    {
                        // Not very good - if we're offline, it will repeatedly trace a failure
                        textBoxInfo.Text += string.Format("RSS processing error:\r\n{0}", problem);
                        break;
                    }
                }

                if (textBoxInfo.Text.Length > 0)
                {
                    textBoxInfo.Select(textBoxInfo.Text.Length - 1, 1);
                    textBoxInfo.ScrollToCaret();
                }
            }

            // Do another check shortly
            iTimer.Start();
        }

        /// <summary>
        /// Update the job counts in the top panel of the main form
        /// </summary>
        private void ShowJobStats()
        {
            int totalJobs = 0;
            int unreadJobs = 0;
            if (iJobDb.GetStats(out totalJobs, out unreadJobs))
            {
                iTotalJobsLabel.Text = "Total jobs in database: " + totalJobs.ToString();
                iUnreadJobsLabel.Text = "New jobs in database: " + unreadJobs.ToString();
                iUnreadJobsLabel.ForeColor = (unreadJobs > 0) ? Color.Red : Color.Black;
            }
            else
            {
                iTotalJobsLabel.Text = "Failed to get stats from database";
                iUnreadJobsLabel.Text = iJobDb.Diagnostic;
                iUnreadJobsLabel.Visible = true;
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            if (!iJobDb.Close())
            {
                MessageBox.Show(iJobDb.Diagnostic, "Jobs database update failure", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Close();
        }

        #endregion Event handlers

        #region Private methods

        /// <summary>
        /// Check the working folder for new emails
        /// </summary>
        /// <returns>list of paths to such emails</returns>
        List<string> GetEmails()
        {
            string workingFolder = Path.Combine(Properties.Settings.Default.WorkingFolder, "Jobs");
            if (Directory.Exists(workingFolder))
            {
                string[] emails = Directory.GetFiles(workingFolder, "*.tmp");
                return new List<string>(emails);
            }
            else
            {
                // Working folder does not exist: should post a warning rather
                // than simply keep checking for where no files will ever appear.
                return new List<string>();
            }
        }

        /// <summary>
        /// Process one jobs email
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns>True if successsful, false if error</returns>
        bool ProcessEmail(string aPath)
        {
            JobEmail jobEmail = JobEmail.Create(aPath, iJobDb);

            if (jobEmail == null)
            {
                textBoxInfo.Text += string.Format("Ignoring file {0}\r\n", aPath);
            }
            else
            {
                string problem = string.Empty;
                if (jobEmail.GetJobs(richTextBoxTrace, out problem))
                {
                    textBoxInfo.Text += string.Format("Processed {0}\r\n", jobEmail.ToString());
                    File.Delete(aPath);
                }
                else
                {
                    textBoxInfo.Text += string.Format("Job processing error:\r\n{0}", problem);
                    return false;
                }
            }

            if (textBoxInfo.Text.Length > 0)
            {
                textBoxInfo.Select(textBoxInfo.Text.Length - 1, 1);
                textBoxInfo.ScrollToCaret();
            }

            return true;
        }

        void reportButton_Click(object sender, EventArgs e)
        {
            DetailsForm detailsForm = new DetailsForm(DateTime.Today, iJobDb);
            System.Windows.Forms.DialogResult res = detailsForm.ShowDialog();

            // Viewed the jobs: this changes the stats
            iStatsUpdateRequired = true;
        }

        string PublishVersion()
        {
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                Version ver = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
                return string.Format("{0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
            }
            else
            {
                return "Beta";
            }
        }

        #endregion Private methods

        JobDatabase.JobDatabase iJobDb;
        Timer iTimer;
        bool iStatsUpdateRequired;
        Dictionary<string, bool> iRssFeeds;
    }
}
