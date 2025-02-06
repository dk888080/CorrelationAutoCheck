using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCorrelationCheck
{
    public partial class BenchInputForm : Form
    {
        public int SampleCount, RepetitionCount;
        public BenchInputForm()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (int.TryParse(this.NumOfSampleText.Text, out int sampleCount) && sampleCount > 0 &&
                int.TryParse(this.NumOfRepeatText.Text, out int repetitionCount) && repetitionCount > 0)
            {
                SampleCount = sampleCount;
                RepetitionCount = repetitionCount;
                DialogResult = DialogResult.OK;  // OK 버튼 클릭 시 폼 종료
                this.Close();  // 폼을 닫음
            }
            else
            {
                MessageBox.Show("Please enter valid positive numbers.");
            }
        }
    }
}
