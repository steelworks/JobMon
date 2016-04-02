namespace JobMon
{
    partial class DetailsForm
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
            this.exitButton = new System.Windows.Forms.Button();
            this.titleLabel = new System.Windows.Forms.Label();
            this.jobDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.jobCountLabel = new System.Windows.Forms.Label();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.topPanel = new System.Windows.Forms.Panel();
            this.iAllRadioButton = new System.Windows.Forms.RadioButton();
            this.iInterestingRadioButton = new System.Windows.Forms.RadioButton();
            this.iNewRadioButton = new System.Windows.Forms.RadioButton();
            this.jobWebBrowser = new System.Windows.Forms.WebBrowser();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.interestingCheckBox = new System.Windows.Forms.CheckBox();
            this.nextButton = new System.Windows.Forms.Button();
            this.prevButton = new System.Windows.Forms.Button();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.badLocButton = new System.Windows.Forms.Button();
            this.badJobButton = new System.Windows.Forms.Button();
            this.goodLocButton = new System.Windows.Forms.Button();
            this.maintenanceButton = new System.Windows.Forms.Button();
            this.summaryRichTextBox = new System.Windows.Forms.RichTextBox();
            this.mainTableLayoutPanel.SuspendLayout();
            this.topPanel.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.exitButton.Location = new System.Drawing.Point(778, 505);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 23);
            this.exitButton.TabIndex = 1;
            this.exitButton.Text = "Exit";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Location = new System.Drawing.Point(36, 5);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(37, 13);
            this.titleLabel.TabIndex = 2;
            this.titleLabel.Text = "Show:";
            // 
            // jobDateTimePicker
            // 
            this.jobDateTimePicker.Location = new System.Drawing.Point(375, 1);
            this.jobDateTimePicker.Name = "jobDateTimePicker";
            this.jobDateTimePicker.Size = new System.Drawing.Size(159, 20);
            this.jobDateTimePicker.TabIndex = 3;
            this.jobDateTimePicker.ValueChanged += new System.EventHandler(this.DateTimePickerOrRadioButton_ValueChanged);
            // 
            // jobCountLabel
            // 
            this.jobCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.jobCountLabel.AutoSize = true;
            this.jobCountLabel.Location = new System.Drawing.Point(802, 0);
            this.jobCountLabel.Name = "jobCountLabel";
            this.jobCountLabel.Size = new System.Drawing.Size(51, 13);
            this.jobCountLabel.TabIndex = 4;
            this.jobCountLabel.Text = "job count";
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 2;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.mainTableLayoutPanel.Controls.Add(this.jobCountLabel, 1, 0);
            this.mainTableLayoutPanel.Controls.Add(this.exitButton, 1, 3);
            this.mainTableLayoutPanel.Controls.Add(this.topPanel, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.jobWebBrowser, 0, 2);
            this.mainTableLayoutPanel.Controls.Add(this.bottomPanel, 0, 3);
            this.mainTableLayoutPanel.Controls.Add(this.buttonPanel, 1, 1);
            this.mainTableLayoutPanel.Controls.Add(this.summaryRichTextBox, 0, 1);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 4;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(856, 531);
            this.mainTableLayoutPanel.TabIndex = 5;
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.iAllRadioButton);
            this.topPanel.Controls.Add(this.iInterestingRadioButton);
            this.topPanel.Controls.Add(this.iNewRadioButton);
            this.topPanel.Controls.Add(this.titleLabel);
            this.topPanel.Controls.Add(this.jobDateTimePicker);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topPanel.Location = new System.Drawing.Point(3, 3);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(750, 24);
            this.topPanel.TabIndex = 5;
            // 
            // iAllRadioButton
            // 
            this.iAllRadioButton.AutoSize = true;
            this.iAllRadioButton.Location = new System.Drawing.Point(285, 3);
            this.iAllRadioButton.Name = "iAllRadioButton";
            this.iAllRadioButton.Size = new System.Drawing.Size(73, 17);
            this.iAllRadioButton.TabIndex = 6;
            this.iAllRadioButton.TabStop = true;
            this.iAllRadioButton.Text = "All jobs for";
            this.iAllRadioButton.UseVisualStyleBackColor = true;
            this.iAllRadioButton.CheckedChanged += new System.EventHandler(this.DateTimePickerOrRadioButton_ValueChanged);
            // 
            // iInterestingRadioButton
            // 
            this.iInterestingRadioButton.AutoSize = true;
            this.iInterestingRadioButton.Location = new System.Drawing.Point(163, 3);
            this.iInterestingRadioButton.Name = "iInterestingRadioButton";
            this.iInterestingRadioButton.Size = new System.Drawing.Size(96, 17);
            this.iInterestingRadioButton.TabIndex = 5;
            this.iInterestingRadioButton.TabStop = true;
            this.iInterestingRadioButton.Text = "Interesting jobs";
            this.iInterestingRadioButton.UseVisualStyleBackColor = true;
            this.iInterestingRadioButton.CheckedChanged += new System.EventHandler(this.DateTimePickerOrRadioButton_ValueChanged);
            // 
            // iNewRadioButton
            // 
            this.iNewRadioButton.AutoSize = true;
            this.iNewRadioButton.Checked = true;
            this.iNewRadioButton.Location = new System.Drawing.Point(79, 3);
            this.iNewRadioButton.Name = "iNewRadioButton";
            this.iNewRadioButton.Size = new System.Drawing.Size(69, 17);
            this.iNewRadioButton.TabIndex = 4;
            this.iNewRadioButton.TabStop = true;
            this.iNewRadioButton.Text = "New jobs";
            this.iNewRadioButton.UseVisualStyleBackColor = true;
            this.iNewRadioButton.CheckedChanged += new System.EventHandler(this.DateTimePickerOrRadioButton_ValueChanged);
            // 
            // jobWebBrowser
            // 
            this.mainTableLayoutPanel.SetColumnSpan(this.jobWebBrowser, 2);
            this.jobWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jobWebBrowser.Location = new System.Drawing.Point(3, 283);
            this.jobWebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.jobWebBrowser.Name = "jobWebBrowser";
            this.jobWebBrowser.ScriptErrorsSuppressed = true;
            this.jobWebBrowser.Size = new System.Drawing.Size(850, 215);
            this.jobWebBrowser.TabIndex = 6;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.interestingCheckBox);
            this.bottomPanel.Controls.Add(this.nextButton);
            this.bottomPanel.Controls.Add(this.prevButton);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomPanel.Location = new System.Drawing.Point(3, 504);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(750, 24);
            this.bottomPanel.TabIndex = 7;
            // 
            // interestingCheckBox
            // 
            this.interestingCheckBox.AutoSize = true;
            this.interestingCheckBox.Location = new System.Drawing.Point(182, 4);
            this.interestingCheckBox.Name = "interestingCheckBox";
            this.interestingCheckBox.Size = new System.Drawing.Size(75, 17);
            this.interestingCheckBox.TabIndex = 2;
            this.interestingCheckBox.Text = "Interesting";
            this.interestingCheckBox.UseVisualStyleBackColor = true;
            this.interestingCheckBox.CheckedChanged += new System.EventHandler(this.interestingCheckBox_CheckedChanged);
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(87, 1);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(75, 23);
            this.nextButton.TabIndex = 1;
            this.nextButton.Text = "Next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // prevButton
            // 
            this.prevButton.Location = new System.Drawing.Point(6, 1);
            this.prevButton.Name = "prevButton";
            this.prevButton.Size = new System.Drawing.Size(75, 23);
            this.prevButton.TabIndex = 0;
            this.prevButton.Text = "Prev";
            this.prevButton.UseVisualStyleBackColor = true;
            this.prevButton.Click += new System.EventHandler(this.prevButton_Click);
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.badLocButton);
            this.buttonPanel.Controls.Add(this.badJobButton);
            this.buttonPanel.Controls.Add(this.goodLocButton);
            this.buttonPanel.Controls.Add(this.maintenanceButton);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPanel.Location = new System.Drawing.Point(759, 33);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(94, 244);
            this.buttonPanel.TabIndex = 9;
            // 
            // badLocButton
            // 
            this.badLocButton.Location = new System.Drawing.Point(3, 3);
            this.badLocButton.Name = "badLocButton";
            this.badLocButton.Size = new System.Drawing.Size(88, 23);
            this.badLocButton.TabIndex = 3;
            this.badLocButton.Text = "Bad loc";
            this.badLocButton.UseVisualStyleBackColor = true;
            this.badLocButton.Click += new System.EventHandler(this.badLocButton_Click);
            // 
            // badJobButton
            // 
            this.badJobButton.Location = new System.Drawing.Point(3, 61);
            this.badJobButton.Name = "badJobButton";
            this.badJobButton.Size = new System.Drawing.Size(88, 23);
            this.badJobButton.TabIndex = 2;
            this.badJobButton.Text = "Bad Job";
            this.badJobButton.UseVisualStyleBackColor = true;
            this.badJobButton.Click += new System.EventHandler(this.badJobButton_Click);
            // 
            // goodLocButton
            // 
            this.goodLocButton.Location = new System.Drawing.Point(3, 32);
            this.goodLocButton.Name = "goodLocButton";
            this.goodLocButton.Size = new System.Drawing.Size(88, 23);
            this.goodLocButton.TabIndex = 1;
            this.goodLocButton.Text = "Good loc";
            this.goodLocButton.UseVisualStyleBackColor = true;
            this.goodLocButton.Click += new System.EventHandler(this.goodLocButton_Click);
            // 
            // maintenanceButton
            // 
            this.maintenanceButton.Location = new System.Drawing.Point(3, 90);
            this.maintenanceButton.Name = "maintenanceButton";
            this.maintenanceButton.Size = new System.Drawing.Size(88, 23);
            this.maintenanceButton.TabIndex = 0;
            this.maintenanceButton.Text = "Maintenance";
            this.maintenanceButton.UseVisualStyleBackColor = true;
            this.maintenanceButton.Click += new System.EventHandler(this.maintenanceButton_Click);
            // 
            // summaryRichTextBox
            // 
            this.summaryRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.summaryRichTextBox.Location = new System.Drawing.Point(3, 33);
            this.summaryRichTextBox.Name = "summaryRichTextBox";
            this.summaryRichTextBox.ReadOnly = true;
            this.summaryRichTextBox.Size = new System.Drawing.Size(750, 244);
            this.summaryRichTextBox.TabIndex = 10;
            this.summaryRichTextBox.Text = "";
            // 
            // DetailsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(856, 531);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Name = "DetailsForm";
            this.Text = "details";
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.mainTableLayoutPanel.PerformLayout();
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.DateTimePicker jobDateTimePicker;
        private System.Windows.Forms.Label jobCountLabel;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.WebBrowser jobWebBrowser;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.CheckBox interestingCheckBox;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button prevButton;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button badLocButton;
        private System.Windows.Forms.Button badJobButton;
        private System.Windows.Forms.Button goodLocButton;
        private System.Windows.Forms.Button maintenanceButton;
        private System.Windows.Forms.RichTextBox summaryRichTextBox;
        private System.Windows.Forms.RadioButton iAllRadioButton;
        private System.Windows.Forms.RadioButton iInterestingRadioButton;
        private System.Windows.Forms.RadioButton iNewRadioButton;

    }
}