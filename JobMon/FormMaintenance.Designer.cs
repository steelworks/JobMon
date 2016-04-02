namespace JobMon
{
    partial class FormMaintenance
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.iButtonExit = new System.Windows.Forms.Button();
            this.iTrackBarAge = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label0 = new System.Windows.Forms.Label();
            this.label1000 = new System.Windows.Forms.Label();
            this.iLabelTotalJobs = new System.Windows.Forms.Label();
            this.iButtonDelete = new System.Windows.Forms.Button();
            this.iLabelOlderJobs = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.iTrackBarAge)).BeginInit();
            this.SuspendLayout();
            // 
            // iButtonExit
            // 
            this.iButtonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.iButtonExit.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.iButtonExit.Location = new System.Drawing.Point(440, 174);
            this.iButtonExit.Name = "iButtonExit";
            this.iButtonExit.Size = new System.Drawing.Size(75, 23);
            this.iButtonExit.TabIndex = 0;
            this.iButtonExit.Text = "Exit";
            this.iButtonExit.UseVisualStyleBackColor = true;
            // 
            // iTrackBarAge
            // 
            this.iTrackBarAge.LargeChange = 50;
            this.iTrackBarAge.Location = new System.Drawing.Point(29, 49);
            this.iTrackBarAge.Maximum = 1000;
            this.iTrackBarAge.Name = "iTrackBarAge";
            this.iTrackBarAge.Size = new System.Drawing.Size(471, 45);
            this.iTrackBarAge.SmallChange = 5;
            this.iTrackBarAge.TabIndex = 1;
            this.iTrackBarAge.Value = 365;
            this.iTrackBarAge.ValueChanged += new System.EventHandler(this.TrackBarAge_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select age of jobs in database";
            // 
            // label0
            // 
            this.label0.AutoSize = true;
            this.label0.Location = new System.Drawing.Point(25, 81);
            this.label0.Name = "label0";
            this.label0.Size = new System.Drawing.Size(38, 13);
            this.label0.TabIndex = 3;
            this.label0.Text = "0 days";
            // 
            // label1000
            // 
            this.label1000.AutoSize = true;
            this.label1000.Location = new System.Drawing.Point(459, 81);
            this.label1000.Name = "label1000";
            this.label1000.Size = new System.Drawing.Size(56, 13);
            this.label1000.TabIndex = 4;
            this.label1000.Text = "1000 days";
            // 
            // iLabelTotalJobs
            // 
            this.iLabelTotalJobs.AutoSize = true;
            this.iLabelTotalJobs.Location = new System.Drawing.Point(246, 115);
            this.iLabelTotalJobs.Name = "iLabelTotalJobs";
            this.iLabelTotalJobs.Size = new System.Drawing.Size(44, 13);
            this.iLabelTotalJobs.TabIndex = 5;
            this.iLabelTotalJobs.Text = "Statistic";
            // 
            // iButtonDelete
            // 
            this.iButtonDelete.Location = new System.Drawing.Point(31, 174);
            this.iButtonDelete.Name = "iButtonDelete";
            this.iButtonDelete.Size = new System.Drawing.Size(75, 23);
            this.iButtonDelete.TabIndex = 6;
            this.iButtonDelete.Text = "Delete";
            this.iButtonDelete.UseVisualStyleBackColor = true;
            this.iButtonDelete.Click += new System.EventHandler(this.ButtonDelete_Click);
            // 
            // iLabelOlderJobs
            // 
            this.iLabelOlderJobs.AutoSize = true;
            this.iLabelOlderJobs.Location = new System.Drawing.Point(246, 137);
            this.iLabelOlderJobs.Name = "iLabelOlderJobs";
            this.iLabelOlderJobs.Size = new System.Drawing.Size(44, 13);
            this.iLabelOlderJobs.TabIndex = 5;
            this.iLabelOlderJobs.Text = "Statistic";
            // 
            // FormMaintenance
            // 
            this.AcceptButton = this.iButtonExit;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(537, 218);
            this.ControlBox = false;
            this.Controls.Add(this.iButtonDelete);
            this.Controls.Add(this.iLabelOlderJobs);
            this.Controls.Add(this.iLabelTotalJobs);
            this.Controls.Add(this.label1000);
            this.Controls.Add(this.label0);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.iTrackBarAge);
            this.Controls.Add(this.iButtonExit);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMaintenance";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Maintenance";
            ((System.ComponentModel.ISupportInitialize)(this.iTrackBarAge)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button iButtonExit;
        private System.Windows.Forms.TrackBar iTrackBarAge;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label0;
        private System.Windows.Forms.Label label1000;
        private System.Windows.Forms.Label iLabelTotalJobs;
        private System.Windows.Forms.Button iButtonDelete;
        private System.Windows.Forms.Label iLabelOlderJobs;
    }
}