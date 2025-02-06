namespace AutoCorrelationCheck
{
    partial class BenchInputForm
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
            this.NumOfSampleText = new System.Windows.Forms.TextBox();
            this.NumOfSample = new System.Windows.Forms.Label();
            this.NumOfRepeatText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OkButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // NumOfSampleText
            // 
            this.NumOfSampleText.Location = new System.Drawing.Point(33, 38);
            this.NumOfSampleText.Name = "NumOfSampleText";
            this.NumOfSampleText.Size = new System.Drawing.Size(100, 20);
            this.NumOfSampleText.TabIndex = 0;
            // 
            // NumOfSample
            // 
            this.NumOfSample.AutoSize = true;
            this.NumOfSample.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NumOfSample.Location = new System.Drawing.Point(30, 22);
            this.NumOfSample.Name = "NumOfSample";
            this.NumOfSample.Size = new System.Drawing.Size(108, 13);
            this.NumOfSample.TabIndex = 1;
            this.NumOfSample.Text = "Number of sample";
            // 
            // NumOfRepeatText
            // 
            this.NumOfRepeatText.Location = new System.Drawing.Point(33, 89);
            this.NumOfRepeatText.Name = "NumOfRepeatText";
            this.NumOfRepeatText.Size = new System.Drawing.Size(100, 20);
            this.NumOfRepeatText.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(30, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(203, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Number of repeated measurements";
            // 
            // OkButton
            // 
            this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OkButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OkButton.Location = new System.Drawing.Point(248, 134);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 6;
            this.OkButton.Text = "Ok";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // BenchInputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(335, 169);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NumOfRepeatText);
            this.Controls.Add(this.NumOfSample);
            this.Controls.Add(this.NumOfSampleText);
            this.Name = "BenchInputForm";
            this.Text = "Bench_Input_Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox NumOfSampleText;
        private System.Windows.Forms.Label NumOfSample;
        private System.Windows.Forms.TextBox NumOfRepeatText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OkButton;
    }
}