namespace AutoCorrelationCheck
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.LoadExcel = new System.Windows.Forms.Button();
            this.Exit = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column17 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Export_ExcelFile = new System.Windows.Forms.Button();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.advancedDataGridView1 = new Zuby.ADGV.AdvancedDataGridView();
            this.lblDataSource = new System.Windows.Forms.Label();
            this.spcMain = new System.Windows.Forms.SplitContainer();
            this.PoutLimitFile = new System.Windows.Forms.CheckBox();
            this.GUBenchFile = new System.Windows.Forms.CheckBox();
            this.GUCorrFile = new System.Windows.Forms.CheckBox();
            this.RevInfo = new System.Windows.Forms.Label();
            this.BuildInfo = new System.Windows.Forms.Label();
            this.ProjectInfo = new System.Windows.Forms.Label();
            this.RevInfoText = new System.Windows.Forms.TextBox();
            this.BuildInfoText = new System.Windows.Forms.TextBox();
            this.ProjectInfoText = new System.Windows.Forms.TextBox();
            this.GenPackage = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.advancedDataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spcMain)).BeginInit();
            this.spcMain.Panel1.SuspendLayout();
            this.spcMain.Panel2.SuspendLayout();
            this.spcMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // LoadExcel
            // 
            this.LoadExcel.Location = new System.Drawing.Point(12, 12);
            this.LoadExcel.Name = "LoadExcel";
            this.LoadExcel.Size = new System.Drawing.Size(75, 23);
            this.LoadExcel.TabIndex = 0;
            this.LoadExcel.Text = "LoadExcel";
            this.LoadExcel.UseVisualStyleBackColor = true;
            this.LoadExcel.Click += new System.EventHandler(this.LoadExcel_Click);
            // 
            // Exit
            // 
            this.Exit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Exit.Location = new System.Drawing.Point(397, 12);
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(75, 23);
            this.Exit.TabIndex = 1;
            this.Exit.Text = "Exit";
            this.Exit.UseVisualStyleBackColor = true;
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Location = new System.Drawing.Point(93, 17);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(177, 13);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Please load a corrleation data";
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Column1";
            this.Column1.Name = "Column1";
            this.Column1.Width = 73;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Column2";
            this.Column2.Name = "Column2";
            this.Column2.Width = 73;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Column3";
            this.Column3.Name = "Column3";
            this.Column3.Width = 73;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Column4";
            this.Column4.Name = "Column4";
            this.Column4.Width = 73;
            // 
            // Column5
            // 
            this.Column5.HeaderText = "Column5";
            this.Column5.Name = "Column5";
            this.Column5.Width = 73;
            // 
            // Column6
            // 
            this.Column6.HeaderText = "Column6";
            this.Column6.Name = "Column6";
            this.Column6.Width = 73;
            // 
            // Column7
            // 
            this.Column7.HeaderText = "Column7";
            this.Column7.Name = "Column7";
            this.Column7.Width = 73;
            // 
            // Column8
            // 
            this.Column8.HeaderText = "Column8";
            this.Column8.Name = "Column8";
            this.Column8.Width = 73;
            // 
            // Column9
            // 
            this.Column9.HeaderText = "Column9";
            this.Column9.Name = "Column9";
            this.Column9.Width = 73;
            // 
            // Column10
            // 
            this.Column10.HeaderText = "Column10";
            this.Column10.Name = "Column10";
            this.Column10.Width = 79;
            // 
            // Column11
            // 
            this.Column11.HeaderText = "Column11";
            this.Column11.Name = "Column11";
            this.Column11.Width = 79;
            // 
            // Column12
            // 
            this.Column12.HeaderText = "Column12";
            this.Column12.Name = "Column12";
            this.Column12.Width = 79;
            // 
            // Column13
            // 
            this.Column13.HeaderText = "Column13";
            this.Column13.Name = "Column13";
            this.Column13.Width = 79;
            // 
            // Column14
            // 
            this.Column14.HeaderText = "Column14";
            this.Column14.Name = "Column14";
            this.Column14.Width = 79;
            // 
            // Column15
            // 
            this.Column15.HeaderText = "Column15";
            this.Column15.Name = "Column15";
            this.Column15.Width = 79;
            // 
            // Column16
            // 
            this.Column16.HeaderText = "Column16";
            this.Column16.Name = "Column16";
            this.Column16.Width = 79;
            // 
            // Column17
            // 
            this.Column17.HeaderText = "Column17";
            this.Column17.Name = "Column17";
            this.Column17.Width = 79;
            // 
            // Export_ExcelFile
            // 
            this.Export_ExcelFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Export_ExcelFile.Location = new System.Drawing.Point(287, 12);
            this.Export_ExcelFile.Name = "Export_ExcelFile";
            this.Export_ExcelFile.Size = new System.Drawing.Size(102, 23);
            this.Export_ExcelFile.TabIndex = 4;
            this.Export_ExcelFile.Text = "Export_ExcelFile";
            this.Export_ExcelFile.UseVisualStyleBackColor = true;
            this.Export_ExcelFile.Click += new System.EventHandler(this.Export_ExcelFile_Click);
            // 
            // advancedDataGridView1
            // 
            this.advancedDataGridView1.AllowUserToOrderColumns = true;
            this.advancedDataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            this.advancedDataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.advancedDataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.advancedDataGridView1.FilterAndSortEnabled = true;
            this.advancedDataGridView1.FilterStringChangedInvokeBeforeDatasourceUpdate = true;
            this.advancedDataGridView1.Location = new System.Drawing.Point(0, 0);
            this.advancedDataGridView1.MaxFilterButtonImageHeight = 23;
            this.advancedDataGridView1.Name = "advancedDataGridView1";
            this.advancedDataGridView1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.advancedDataGridView1.Size = new System.Drawing.Size(460, 297);
            this.advancedDataGridView1.SortStringChangedInvokeBeforeDatasourceUpdate = true;
            this.advancedDataGridView1.TabIndex = 5;
            this.advancedDataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.advancedDataGridView1_CellDoubleClick);
            this.advancedDataGridView1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.advancedDataGridView1_Scroll);
            // 
            // lblDataSource
            // 
            this.lblDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDataSource.AutoSize = true;
            this.lblDataSource.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.lblDataSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDataSource.Location = new System.Drawing.Point(173, 135);
            this.lblDataSource.Name = "lblDataSource";
            this.lblDataSource.Size = new System.Drawing.Size(118, 24);
            this.lblDataSource.TabIndex = 6;
            this.lblDataSource.Text = "DataSource";
            // 
            // spcMain
            // 
            this.spcMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.spcMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.spcMain.Location = new System.Drawing.Point(12, 41);
            this.spcMain.Name = "spcMain";
            this.spcMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcMain.Panel1
            // 
            this.spcMain.Panel1.Controls.Add(this.PoutLimitFile);
            this.spcMain.Panel1.Controls.Add(this.GUBenchFile);
            this.spcMain.Panel1.Controls.Add(this.GUCorrFile);
            this.spcMain.Panel1.Controls.Add(this.RevInfo);
            this.spcMain.Panel1.Controls.Add(this.BuildInfo);
            this.spcMain.Panel1.Controls.Add(this.ProjectInfo);
            this.spcMain.Panel1.Controls.Add(this.RevInfoText);
            this.spcMain.Panel1.Controls.Add(this.BuildInfoText);
            this.spcMain.Panel1.Controls.Add(this.ProjectInfoText);
            this.spcMain.Panel1.Controls.Add(this.GenPackage);
            // 
            // spcMain.Panel2
            // 
            this.spcMain.Panel2.Controls.Add(this.lblDataSource);
            this.spcMain.Panel2.Controls.Add(this.advancedDataGridView1);
            this.spcMain.Size = new System.Drawing.Size(460, 408);
            this.spcMain.SplitterDistance = 107;
            this.spcMain.TabIndex = 7;
            // 
            // PoutLimitFile
            // 
            this.PoutLimitFile.AutoSize = true;
            this.PoutLimitFile.Location = new System.Drawing.Point(14, 67);
            this.PoutLimitFile.Name = "PoutLimitFile";
            this.PoutLimitFile.Size = new System.Drawing.Size(85, 17);
            this.PoutLimitFile.TabIndex = 9;
            this.PoutLimitFile.Text = "PoutLimitFile";
            this.PoutLimitFile.UseVisualStyleBackColor = true;
            // 
            // GUBenchFile
            // 
            this.GUBenchFile.AutoSize = true;
            this.GUBenchFile.Location = new System.Drawing.Point(14, 41);
            this.GUBenchFile.Name = "GUBenchFile";
            this.GUBenchFile.Size = new System.Drawing.Size(89, 17);
            this.GUBenchFile.TabIndex = 8;
            this.GUBenchFile.Text = "GUBenchFile";
            this.GUBenchFile.UseVisualStyleBackColor = true;
            // 
            // GUCorrFile
            // 
            this.GUCorrFile.AutoSize = true;
            this.GUCorrFile.Location = new System.Drawing.Point(14, 15);
            this.GUCorrFile.Name = "GUCorrFile";
            this.GUCorrFile.Size = new System.Drawing.Size(77, 17);
            this.GUCorrFile.TabIndex = 7;
            this.GUCorrFile.Text = "GUCorrFile";
            this.GUCorrFile.UseVisualStyleBackColor = true;
            // 
            // RevInfo
            // 
            this.RevInfo.AutoSize = true;
            this.RevInfo.Location = new System.Drawing.Point(126, 68);
            this.RevInfo.Name = "RevInfo";
            this.RevInfo.Size = new System.Drawing.Size(45, 13);
            this.RevInfo.TabIndex = 6;
            this.RevInfo.Text = "RevInfo";
            // 
            // BuildInfo
            // 
            this.BuildInfo.AutoSize = true;
            this.BuildInfo.Location = new System.Drawing.Point(123, 42);
            this.BuildInfo.Name = "BuildInfo";
            this.BuildInfo.Size = new System.Drawing.Size(48, 13);
            this.BuildInfo.TabIndex = 5;
            this.BuildInfo.Text = "BuildInfo";
            // 
            // ProjectInfo
            // 
            this.ProjectInfo.AutoSize = true;
            this.ProjectInfo.Location = new System.Drawing.Point(113, 16);
            this.ProjectInfo.Name = "ProjectInfo";
            this.ProjectInfo.Size = new System.Drawing.Size(58, 13);
            this.ProjectInfo.TabIndex = 4;
            this.ProjectInfo.Text = "ProjectInfo";
            // 
            // RevInfoText
            // 
            this.RevInfoText.Location = new System.Drawing.Point(177, 65);
            this.RevInfoText.Name = "RevInfoText";
            this.RevInfoText.Size = new System.Drawing.Size(100, 20);
            this.RevInfoText.TabIndex = 3;
            // 
            // BuildInfoText
            // 
            this.BuildInfoText.Location = new System.Drawing.Point(177, 39);
            this.BuildInfoText.Name = "BuildInfoText";
            this.BuildInfoText.Size = new System.Drawing.Size(100, 20);
            this.BuildInfoText.TabIndex = 2;
            // 
            // ProjectInfoText
            // 
            this.ProjectInfoText.Location = new System.Drawing.Point(177, 13);
            this.ProjectInfoText.Name = "ProjectInfoText";
            this.ProjectInfoText.Size = new System.Drawing.Size(100, 20);
            this.ProjectInfoText.TabIndex = 1;
            // 
            // GenPackage
            // 
            this.GenPackage.Location = new System.Drawing.Point(335, 13);
            this.GenPackage.Name = "GenPackage";
            this.GenPackage.Size = new System.Drawing.Size(102, 39);
            this.GenPackage.TabIndex = 0;
            this.GenPackage.Text = "GenPackage";
            this.GenPackage.UseVisualStyleBackColor = true;
            this.GenPackage.Click += new System.EventHandler(this.GenPackage_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.spcMain);
            this.Controls.Add(this.Export_ExcelFile);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.Exit);
            this.Controls.Add(this.LoadExcel);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.advancedDataGridView1)).EndInit();
            this.spcMain.Panel1.ResumeLayout(false);
            this.spcMain.Panel1.PerformLayout();
            this.spcMain.Panel2.ResumeLayout(false);
            this.spcMain.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcMain)).EndInit();
            this.spcMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoadExcel;
        private System.Windows.Forms.Button Exit;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column8;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column10;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column11;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column12;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column13;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column14;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column15;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column16;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column17;
        private System.Windows.Forms.Button Export_ExcelFile;
        private System.Windows.Forms.BindingSource bindingSource1;
        private Zuby.ADGV.AdvancedDataGridView advancedDataGridView1;
        private System.Windows.Forms.Label lblDataSource;
        private System.Windows.Forms.SplitContainer spcMain;
        private System.Windows.Forms.Button GenPackage;
        private System.Windows.Forms.Label RevInfo;
        private System.Windows.Forms.Label BuildInfo;
        private System.Windows.Forms.Label ProjectInfo;
        private System.Windows.Forms.TextBox RevInfoText;
        private System.Windows.Forms.TextBox BuildInfoText;
        private System.Windows.Forms.TextBox ProjectInfoText;
        private System.Windows.Forms.CheckBox PoutLimitFile;
        private System.Windows.Forms.CheckBox GUBenchFile;
        private System.Windows.Forms.CheckBox GUCorrFile;
    }
}

