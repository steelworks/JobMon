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
    public partial class EditorForm : Form
    {
        JobDatabase.JobDatabase iJobDb;
        string iFilterType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="aJobDb">Interface to Job database</param>
        /// <param name="aName">Name of item which may be added to the filter</param>
        /// <param name="aFilter">The filter type</param>
        public EditorForm(JobDatabase.JobDatabase aJobDb, string aName, string aFilter)
        {
            InitializeComponent();

            iJobDb = aJobDb;
            iFilterType = aFilter;
            this.Text = "Edit " + iFilterType;
            newTextBox.Text = aName.Trim();
            List<string> existingItems = iJobDb.GetFilterItems(aFilter);
            existingListBox.Items.AddRange(existingItems.ToArray());
        }

        /// <summary>
        /// Add the text in the text box as a new item for the filter
        /// </summary>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string item = newTextBox.Text;
            if (!iJobDb.AddFilterItem(iFilterType, item))
            {
                string diagnostic = string.Format("Failed to add item {0} to filter {1}\n{2}",
                                                  item, iFilterType, iJobDb.Diagnostic);
                MessageBox.Show(diagnostic, "JobMon");
                return;
            }

            // Easiest way of refreshing the list box to include the new item
            existingListBox.Items.Clear();
            List<string> existingItems = iJobDb.GetFilterItems(iFilterType);
            existingListBox.Items.AddRange(existingItems.ToArray());

            int itemNumber = existingListBox.Items.IndexOf(item);
            if ((itemNumber >= 0) && (itemNumber < existingListBox.Items.Count))
            {
                existingListBox.SelectedIndex = itemNumber;
            }
        }

        private void existingListBox_MouseDown(object sender, MouseEventArgs e)
        {
            // Mouse button selects the current item
            int itemNumber = existingListBox.IndexFromPoint(e.X, e.Y);
            if ((itemNumber >= 0) && (itemNumber < existingListBox.Items.Count))
            {
                existingListBox.SelectedIndex = itemNumber;
                existingListBox.Refresh();

                // For right-click, show the context menu
                if (e.Button == MouseButtons.Right)
                {
                    int listX = e.X + existingListBox.ClientRectangle.Left;
                    int listY = e.Y + existingListBox.ClientRectangle.Top;
                    editMenuStrip.Show(new Point(listX, listY));
                }
            }
        }

        // Want to recognise right-click rather than left-click
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int itemNumber = existingListBox.SelectedIndex;
            if ((itemNumber >= 0) && (itemNumber < existingListBox.Items.Count))
            {
                string item = existingListBox.Items[itemNumber].ToString();
                DialogResult res = MessageBox.Show("Delete <" + item + ">?", iFilterType, 
                                                   MessageBoxButtons.OKCancel);
                if (res == DialogResult.OK)
                {
                    if (!iJobDb.DeleteFilterItem(iFilterType, item))
                    {
                        string diagnostic = 
                            string.Format("Failed to delete item {0} from filter {1}\n{2}",
                                          item, iFilterType, iJobDb.Diagnostic);
                        MessageBox.Show(diagnostic, "JobMon");
                        return;
                    }

                    // Easiest way of refreshing the list box to exclude the deleted item
                    existingListBox.Items.Clear();
                    List<string> existingItems = iJobDb.GetFilterItems(iFilterType);
                    existingListBox.Items.AddRange(existingItems.ToArray());

                    if (itemNumber < existingListBox.Items.Count)
                    {
                        existingListBox.SelectedIndex = itemNumber;
                    }
                }
            }
        }

        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
