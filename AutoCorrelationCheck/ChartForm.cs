using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

namespace AutoCorrelationCheck
{
    public partial class ChartForm : Form
    {
        public ChartForm()
        {
            InitializeComponent();
            chart1.Dock = DockStyle.Fill;
        }
        public void SetChart(Chart chart)
        {
            Controls.Clear();

            this.chart1 = chart;
            Controls.Add(chart1);  // Add the chart to the form's controls
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.Manual;
            // Set the minimum size to 120mm x 120mm
            int minWidth = (int)(150 * 3.77953);  // 120mm in pixels
            int minHeight = (int)(150 * 3.77953); // 120mm in pixels
            this.MinimumSize = new Size(minWidth, minHeight);
            this.chart1.Titles[0].Font = new Font("Arial", 10, FontStyle.Bold);
            if (this.chart1.Titles[0].Text.Length > 50)
            {
                this.chart1.Titles[0].Font = new Font("Arial", 8, FontStyle.Bold); // Reduce font size for long names
            }
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            // Adjust chart title font size based on form size
            float newFontSize = Math.Max(5, this.ClientSize.Width / 50);
            this.chart1.ChartAreas[0].AxisX.TitleFont = new Font("Arial", newFontSize, FontStyle.Bold);
            this.chart1.ChartAreas[0].AxisY.TitleFont = new Font("Arial", newFontSize, FontStyle.Bold);
            this.chart1.Titles[0].Font = new Font("Arial", newFontSize, FontStyle.Bold);
        }
    }
}
