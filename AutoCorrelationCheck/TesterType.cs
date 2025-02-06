using System;
using System.Windows.Forms;

namespace AutoCorrelationCheck
{
    public partial class TesterType : Form
    {
        public string selectedType;

        public TesterType()
        {
            InitializeComponent();
        }

        private void Okbutton_Click(object sender, EventArgs e)
        {
            selectedType = TesterList.SelectedItem.ToString();
            Dispose();
        }
    }
}