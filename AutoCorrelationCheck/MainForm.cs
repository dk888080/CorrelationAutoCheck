using ClosedXML.Excel;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;

namespace AutoCorrelationCheck
{
    public partial class MainForm : Form
    {
        public int LastColumnUsedIndex = 0;
        public int ParaRowNum = 0;

        // Create a new DataTable
        public DataTable datatable = new DataTable();

        // Declare workbook and worksheet as class-level variables
        private XLWorkbook workbook;

        private IXLWorksheet CorrelationRF1, CorrelationRF2, CorrelationNFR;
        //private Manage DataManager = new Manage();
        //private BindingSource bindingSource = new BindingSource();

        public string TesterTypeStr = "";
        private Version Version = new Version(0, 0, 1);

        public MainForm()
        {
            InitializeComponent();

            this.Text = $"NPI-ACC. Auto Correlation Check v{Version.ToString()}";
            this.MinimumSize = this.Size;

            DoubleBuffered = true;
            advancedDataGridView1.SetDoubleBuffered();

            SetupBindingSource();
            spcMain.Panel1Collapsed = true;
            //spcMain.Panel2Collapsed = true;

            MainForm_SizeChanged(null, null);
        }

        private void SetupBindingSource()
        {
            bindingSource1.ListChanged += (s, e) => UpdateRowStyles();
        }

        public class PackageHelper
        {
            private float poutLimitDelta = 0.5f;
            private float RxGainminLSL = 5;
            private float RxGainmaxUSL = 20;
            private const double minLSL = -999999999999999;
            private const double maxUSL = 999999999999999;
            public double benchhighL = 9999999;
            public double benchlowL = -9999999;

            private List<string> m_cfIgnoreItems = new List<string>();
            private List<string> m_benchTLIgnoreItems = new List<string>();
            private List<string> m_plIncludeRuleForTx = new List<string>();
            private List<string> m_plExcludeRuleForTx = new List<string>();
            private List<string> m_plIncludeRuleForRx = new List<string>();
            private List<string> m_plExcludeRuleForRx = new List<string>();
            private List<GUBench> BenchRules = new List<GUBench>();
            private List<List<string>> m_guAddItems = new List<List<string>>();
            private List<List<string>> m_guMultiplyItems = new List<List<string>>();

            private SplitContainer spmain;

            public PackageHelper()
            {
                string FilePath = "";
                // Ask if user wants to load a config file
                DialogResult dialogResult = MessageBox.Show("Would you like to load the PackageGenConfig file?", "Load Config", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    // Open file dialog for selecting a XML file
                    OpenFileDialog XMLFileDialog = new OpenFileDialog();
                    XMLFileDialog.Filter = "XML Files|*.xml";
                    if (XMLFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        FilePath = XMLFileDialog.FileName;
                    }
                }
                else
                {
                    // Load the default config file from the program's directory
                    FilePath = System.IO.Path.Combine(Application.StartupPath, "Config", "PackageGenConfig.xml");
                }

                XDocument xdoc = XDocument.Load(FilePath);

                XElement xmlRoot = xdoc.Root;
                var xelPoutLimit = xmlRoot.Element("TL_PoutLimit");
                if (xelPoutLimit != null)
                {
                    var xelDeltaValue = xelPoutLimit.Element("DeltaValue");
                    if (xelDeltaValue != null && xelDeltaValue.Value.TryToDouble(out double tempLimit))
                    {
                        poutLimitDelta = (float)tempLimit;
                    }

                    var xelRxGainMinValueValue = xelPoutLimit.Element("RxGainMinValue");
                    if (xelRxGainMinValueValue != null && xelRxGainMinValueValue.Value.TryToDouble(out double tempGainMinLimit))
                    {
                        RxGainminLSL = (float)tempGainMinLimit;
                    }

                    var xelRxGainMaxValueValue = xelPoutLimit.Element("RxGainMaxValue");
                    if (xelRxGainMaxValueValue != null && xelRxGainMaxValueValue.Value.TryToDouble(out double tempGainMaxLimit))
                    {
                        RxGainmaxUSL = (float)tempGainMaxLimit;
                    }

                    var xelignoreList = xelPoutLimit.Element("IgnoreCF");
                    if (xelignoreList != null)
                    {
                        var ignoreList = xelignoreList.Value.SplitToArray(',');
                        if (ignoreList.Length > 0)
                        {
                            m_cfIgnoreItems.Clear();
                            m_cfIgnoreItems.AddRange(ignoreList);
                        }
                    }

                    var xelPoutLimitConditions = xelPoutLimit.Element("PoutLimitConditions");
                    if (xelPoutLimitConditions != null)
                    {
                        var includeRuleForTx = xelPoutLimitConditions.Element("IncludeRuleForTx").Value.SplitToArray(',');
                        var excludeRuleForTx = xelPoutLimitConditions.Element("ExcludeRuleForTx").Value.SplitToArray(',');

                        var includeRuleForRx = xelPoutLimitConditions.Element("IncludeRuleForRx").Value.SplitToArray(',');
                        var excludeRuleForRx = xelPoutLimitConditions.Element("ExcludeRuleForRx").Value.SplitToArray(',');

                        if (includeRuleForTx.Length > 0)
                        {
                            m_plIncludeRuleForTx.Clear();
                            m_plIncludeRuleForTx.AddRange(includeRuleForTx);
                        }

                        if (excludeRuleForTx.Length > 0)
                        {
                            m_plExcludeRuleForTx.Clear();
                            m_plExcludeRuleForTx.AddRange(excludeRuleForTx);
                        }

                        if (includeRuleForRx.Length > 0)
                        {
                            m_plIncludeRuleForRx.Clear();
                            m_plIncludeRuleForRx.AddRange(includeRuleForRx);
                        }

                        if (excludeRuleForRx.Length > 0)
                        {
                            m_plExcludeRuleForRx.Clear();
                            m_plExcludeRuleForRx.AddRange(excludeRuleForRx);
                        }
                    }
                }

                var xelGuPackage = xmlRoot.Element("GuPacakge");
                if (xelGuPackage != null)
                {
                    var xelGuCorr = xelGuPackage.Element("GuCorr");
                    var xelGuBench = xelGuPackage.Element("GuBench");

                    foreach (var item in xelGuCorr.Element("ADD").Elements("Item"))
                    {
                        m_guAddItems.Add(new List<string>(item.Value.SplitToArray(',').ToList()));
                    }

                    foreach (var item in xelGuCorr.Element("Multiply").Elements("Item"))
                    {
                        m_guMultiplyItems.Add(new List<string>(item.Value.SplitToArray(',').ToList()));
                    }

                    var guCommonSpec = xelGuBench.Element("Specs");
                    if (guCommonSpec != null)
                    {
                        if (guCommonSpec.Element("HighL").Value.TryToDouble(out double v1))
                            benchhighL = v1;
                        if (guCommonSpec.Element("LowL").Value.TryToDouble(out double v2))
                            benchlowL = v2;
                    }

                    foreach (var item in xelGuBench.Elements("Item"))
                    {
                        GUBench bench = new GUBench();

                        if (item.Element("Limit").Value.TryToDouble(out double lim))
                        {
                            bench.Limit = lim;
                        }

                        foreach (var include in item.Elements("Include"))
                        {
                            bench.PushToIncludes(new List<string>(include.Value.SplitToArray(',')));
                        }

                        foreach (var exclude in item.Elements("Exclude"))
                        {
                            bench.PushToExcludes(new List<string>(exclude.Value.SplitToArray(',')));
                        }

                        BenchRules.Add(bench);
                    }
                }
            }

            public class GUBench
            {
                public double Limit;
                public List<List<string>> Includes = new List<List<string>>();
                public List<List<string>> Excludes = new List<List<string>>();

                public void PushToIncludes(List<string> item)
                {
                    Includes.Add(item.Select(x => x.ToUpper()).ToList());
                }

                public void PushToExcludes(List<string> item)
                {
                    Excludes.Add(item.Select(x => x.ToUpper()).ToList());
                }
            }

            public void GeneratePackage(List<string> header, List<string> factorAdd, List<string> factorMultiply, SplitContainer SPForm)
            {
                Dictionary<string, string> OffsetFactor = new Dictionary<string, string>();

                for (int i = 0; i < header.Count(); i++)
                {
                    if (string.IsNullOrEmpty(header[i])) header[i] = "NA" + i.ToString();
                    OffsetFactor.Add(header[i].ToUpper(), factorAdd[i] + "," + factorMultiply[i]);
                }

                spmain = SPForm;
                //string ProjectInfo = "ENGR-8276-AP1-RF1";
                //string BuildInfo = "PROTO2A";
                //string RevInfo = "A2A";
                string ProjectInfo = spmain.Panel1.Controls["ProjectInfoText"].Text;
                string BuildInfo = spmain.Panel1.Controls["BuildInfoText"].Text;
                string RevInfo = spmain.Panel1.Controls["RevInfoText"].Text;

                string TargetFileName = string.Format("{0}_{1}_{2}", ProjectInfo, BuildInfo, RevInfo);

                string CombinedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"PackageFiles");
                Directory.CreateDirectory(CombinedPath);

                string cfPath = Path.Combine(@"C:\Avago.ATF.Common.x64\CorrelationFiles\Development", $"{TargetFileName}_CF_Rev9999.csv");
                string tsfPath = Path.Combine(CombinedPath, $"{TargetFileName}_TSF_Rev9999.csv");
                string corrTemplatePath = Path.Combine(CombinedPath, $"{TargetFileName}_GuCorrTemplate_Rev9999.csv");
                string BenchTemplatePath = Path.Combine(CombinedPath, $"{TargetFileName}_GuBenchDataFile_Rev9999.csv");

                CheckBox cbGUCorr = (CheckBox)spmain.Panel1.Controls["GUCorrFile"];
                CheckBox cbGUBen = (CheckBox)spmain.Panel1.Controls["GUBenchFile"];
                CheckBox cbPoutLimit = (CheckBox)spmain.Panel1.Controls["PoutLimitFile"];
                try
                {
                    if (cbGUCorr.Checked)
                    {
                        string[] CFHeader = new string[] { "ParameterName", "Factor_Add_site1", "Factor_Add_LowLimit", "Factor_Add_HighLimit", "Factor_Multiply_site1", "Factor_Multiply_LowLimit", "Factor_Multiply_HighLimit" };

                        // StringBuilder를 사용하여 문자열을 하나로 결합
                        var cfContent = new StringBuilder();
                        var corrTemplateContent = new StringBuilder();

                        // CFHeader와 TSFHeader 추가
                        cfContent.AppendLine(string.Join(",", CFHeader));
                        corrTemplateContent.AppendLine(string.Join(",", CFHeader));

                        for (int cfindex = 0; cfindex < header.Count; cfindex++)
                        {
                            string paraName = header[cfindex];

                            cfContent.AppendLine(string.Format("{0},0,-999999,999999,0,-999999,999999", paraName));

                            if (m_guAddItems.Any(t => paraName.CIvContainsAllOf(t.ToArray())))
                            {
                                corrTemplateContent.AppendLine(string.Format("{0},0.1,-999999,999999,0,-999999,999999", paraName));
                            }
                            else if (m_guMultiplyItems.Any(t => paraName.CIvContainsAllOf(t.ToArray())))
                            {
                                corrTemplateContent.AppendLine(string.Format("{0},0,-999999,999999,0.1,-999999,999999", paraName));
                            }
                            else
                            {
                                corrTemplateContent.AppendLine(string.Format("{0},0,-999999,999999,0,-999999,999999", paraName));
                            }
                        }

                        File.WriteAllText(cfPath, cfContent.ToString());
                        File.WriteAllText(corrTemplatePath, corrTemplateContent.ToString());
                    }
                    if (cbPoutLimit.Checked)
                    {
                        var tsfContent = new StringBuilder();

                        tsfContent
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#HEADER", "", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "TestMode", "Production", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "Title", TargetFileName, ""))
                        .AppendLine(string.Format("{0},'{1:dd/MM/yyyy},{2},,,,,,,,,,,,", "Date", DateTime.Now, ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "Author", "Seoul NPI", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "Description", "Automated PoutLimit", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "SpecVersion", "1", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#END", "", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#", "", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#CONTROL_PARAMETERS", "", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "TotalBinYieldAlarmLimit", "70", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "StopAfterBinLimitFail", "0", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "StopAfterAnyParaLimitFail", "0", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "StopRequiredUnitsCount", "9999999", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "ContinuousUnitsPassAlarmLimit", "9999999", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "ContinuousUnitsFailAlarmLimit", "9999999", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "MoveToQABinUnitsCount", "9999999", ""))
                        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "MoveToQAHwBinNum", "9999999", ""));

                        //if ((ClothoDataObject.Instance.ClothoVersion.Major >= 3 && ClothoDataObject.Instance.ClothoVersion.Minor >= 3) || ClothoDataObject.Instance.ClothoVersion.Major >= 4)
                        //{
                        //    tsfContent.AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "MoveToQABinUnitsLimit", "99999", ""))
                        //        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "ExpediteLotTraceUnitsCount", "0", ""))
                        //        .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "ExpediteLotTraceUnitsLimit", "0", ""));
                        //}
                        tsfContent.AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "MoveToQABinUnitsLimit", "99999", ""))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "ExpediteLotTraceUnitsCount", "0", ""))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "ExpediteLotTraceUnitsLimit", "0", ""));

                        tsfContent.AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#END", "", ""))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#", "", ""))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#HWBIN_DEFINITION", "", ""))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "1", "PASS_ALL+", "1"))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "2", "PASS_A", "2"))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "3", "PASS_B", "3"))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "4", "FAIL_A", "4"))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "5", "FAIL_ALL+", "5"))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#END", "", ""))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#", "", ""))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#SWBIN_DEFINITION", "", ""))
                            .AppendLine(string.Format("{0},{1},{2},{3},,,,,,,,,,,", "1", "PASS_ALL+", "OR", "1"))
                            .AppendLine(string.Format("{0},{1},{2},{3},,,,,,,,,,,", "2", "PASS_A", "OR", "2"))
                            .AppendLine(string.Format("{0},{1},{2},{3},,,,,,,,,,,", "3", "PASS_B", "OR", "3"))
                            .AppendLine(string.Format("{0},{1},{2},{3},,,,,,,,,,,", "4", "FAIL_A", "OR", "4"))
                            .AppendLine(string.Format("{0},{1},{2},{3},,,,,,,,,,,", "5", "FAIL_ALL+", "OR", "5"))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#END", "", ""))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#", "", ""))
                            .AppendLine(string.Format("{0},{1},{2},,,,,,,,,,,,", "#SERIAL_DEFINITION", "", ""))
                            .AppendLine(string.Format(",,,,,1,,2,,3,,4,,5,"))
                            .AppendLine(string.Format("TestNumber,TestParameter,ColumnDisplayFlag,ChartDisplayFlag,FailThresPercent,Min,Max,Min,Max,Min,Max,Min,Max,Min,Max"));

                        for (int cfindex = 0; cfindex < header.Count; cfindex++)
                        {
                            string paraName = header[cfindex];

                            float lsl = -999999f, usl = 999999f;

                            if (paraName.CIvContainsAllOf(m_plIncludeRuleForTx.ToArray()) && !paraName.CIvContainsAnyOf(m_plExcludeRuleForTx.ToArray()))
                            {
                                var isMatched = paraName.SplitToArray('_').FirstOrDefault(t => t.CIvEndsWith("dBm"));

                                lsl = float.Parse(isMatched.Replace("dBm", "")) - poutLimitDelta;
                                usl = float.Parse(isMatched.Replace("dBm", "")) + poutLimitDelta;

                                tsfContent.AppendLine(string.Format("{0},{1},0,0,0,{2},{3},{2},{3},{2},{3},{2},{3},{4},{5}", cfindex + 1, paraName, lsl, usl, minLSL, maxUSL));
                            }
                            else if (paraName.CIvContainsAllOf(m_plIncludeRuleForRx.ToArray()) && !paraName.CIvContainsAnyOf(m_plExcludeRuleForRx.ToArray()))
                            {
                                lsl = RxGainminLSL;
                                usl = RxGainmaxUSL;

                                tsfContent.AppendLine(string.Format("{0},{1},0,0,0,{2},{3},{2},{3},{2},{3},{2},{3},{4},{5}", cfindex + 1, paraName, lsl, usl, minLSL, maxUSL));
                            }
                            else
                            {
                                tsfContent.AppendLine(string.Format("{0},{1},0,0,0,{2},{3},{2},{3},{2},{3},{2},{3},{2},{3}", cfindex + 1, paraName, minLSL, maxUSL));
                            }
                        }

                        File.WriteAllText(tsfPath, tsfContent.ToString());
                    }
                    if (cbGUBen.Checked)
                    {
                        bool VrfyLimitAuto = false;
                        DialogResult dialogResult = MessageBox.Show("Would you like to generate the Auto GU Vrfy limit? \n If yes, It will be generated 'auto' to the HighL and LowL", "GU Vrfy Limit version", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {
                            VrfyLimitAuto = true;
                        }
                        List<string> Header = new List<string>();
                        List<string> TestNum = new List<string>();
                        List<string> Unit = new List<string>();
                        List<string> HighL = new List<string>();
                        List<string> LowL = new List<string>();

                        BenchInputForm benchinputform = new BenchInputForm();

                        benchinputform.ShowDialog();

                        int sampleC = benchinputform.SampleCount;
                        int numOfRepeat = benchinputform.RepetitionCount;

                        string InputFilePath = OpenFileDialog();
                        string outputFilePath = BenchTemplatePath;

                        using (StreamWriter writer = new StreamWriter(outputFilePath))
                        {
                            try
                            {
                                StringBuilder sb = new StringBuilder();

                                var lines = File.ReadLines(InputFilePath).ToList();
                                Dictionary<string, Dictionary<string, List<string>>> Rawdata = new Dictionary<string, Dictionary<string, List<string>>>();
                                int PIDNum = 0;

                                for (int rowIndex = 0; rowIndex < lines.Count; rowIndex++)
                                {
                                    var CellValue = lines[rowIndex].Split(',');
                                    var index0 = CellValue[0];
                                    if (index0.CIvEquals("PARAMETER"))
                                    {
                                        Header = CellValue.ToList();
                                        sb.AppendLine(lines[rowIndex]);
                                    }
                                    else if (index0.CIvEquals("TESTS#"))
                                    {
                                        TestNum = CellValue.ToList();
                                        sb.AppendLine(lines[rowIndex]);
                                    }
                                    else if (index0.CIvEquals("UNIT"))
                                    {
                                        for (int i = 0; i < TestNum.Count; i++)
                                        {
                                            if (!string.IsNullOrEmpty(TestNum[i]) && string.IsNullOrEmpty(CellValue[i]))
                                                Unit.Add("NA");
                                            else
                                                Unit.Add(CellValue[i]);
                                        }

                                        string UnitString = string.Join(",", Unit);
                                        sb.AppendLine(UnitString);
                                    }
                                    else if (index0.Contains("PID-"))
                                    {
                                        int sampleNum = ((PIDNum % sampleC) + 1);
                                        string sampleKey = $"Sample#{sampleNum}";

                                        int repeatIndex = (PIDNum / sampleC) + 1;
                                        string repeatKey = $"Repeat#{repeatIndex}";

                                        if (!Rawdata.ContainsKey(repeatKey))
                                        {
                                            Rawdata[repeatKey] = new Dictionary<string, List<string>>();
                                        }

                                        if (!Rawdata[repeatKey].ContainsKey(sampleKey))
                                        {
                                            Rawdata[repeatKey][sampleKey] = new List<string>();
                                        }
                                        Rawdata[repeatKey][sampleKey] = CellValue.ToList();
                                        PIDNum++;
                                    }
                                }

                                HighL.Add("HighL");
                                LowL.Add("LowL");

                                for (int i = 1; i < TestNum.Count; i++)
                                {
                                    string param = Header[i].ToUpper();
                                    if (string.IsNullOrEmpty(TestNum[i]))
                                    {
                                        HighL.Add("");
                                        LowL.Add("");
                                    }
                                    else
                                    {
                                        if (VrfyLimitAuto)
                                        {
                                            HighL.Add("auto");
                                            LowL.Add("auto");
                                        }
                                        else
                                        {
                                            bool Included = false;
                                            bool Excluded = true;
                                            bool Vrfyflag = false;

                                            foreach (var VrfyLimit in BenchRules)
                                            {
                                                Included = VrfyLimit.Includes.Any(group => group.All(item => param.Contains(item)));
                                                Excluded = VrfyLimit.Excludes.Any(group => group.Any(item => param.Contains(item)));

                                                if (Included && !Excluded)
                                                {
                                                    string highL = VrfyLimit.Limit.ToString();
                                                    string lowL = (VrfyLimit.Limit * -1).ToString();
                                                    HighL.Add(highL);
                                                    LowL.Add(lowL);

                                                    Included = false;
                                                    Excluded = true;
                                                    Vrfyflag = true;

                                                    break;
                                                }
                                            }
                                            if (!Vrfyflag)
                                            {
                                                HighL.Add(benchhighL.ToString());
                                                LowL.Add(benchlowL.ToString());
                                            }
                                        }
                                    }
                                }

                                sb.AppendLine(string.Join(",", HighL))
                                    .AppendLine(string.Join(",", LowL));

                                for (int sampleNum = 1; sampleNum <= sampleC; sampleNum++)
                                {
                                    string sampleKey = $"Sample#{sampleNum}";

                                    List<string> avgList = new List<string>();

                                    for (int paramIndex = 0; paramIndex < Rawdata.First().Value.First().Value.Count; paramIndex++)
                                    {
                                        List<double> values = new List<double>();

                                        if (!string.IsNullOrEmpty(TestNum[paramIndex]) && paramIndex != 0)
                                        {
                                            for (int repeat = 1; repeat <= numOfRepeat; repeat++)
                                            {
                                                string repeatKey = $"Repeat#{repeat}";
                                                if (Rawdata.ContainsKey(repeatKey) && Rawdata[repeatKey].ContainsKey(sampleKey))
                                                {
                                                    values.Add(double.Parse(Rawdata[repeatKey][sampleKey][paramIndex]));
                                                }
                                            }

                                            if (values.Count > 2)
                                            {
                                                double maxVal = values.Max();
                                                double minVal = values.Min();

                                                int maxIndex = values.IndexOf(maxVal);
                                                if (maxIndex >= 0) values.RemoveAt(maxIndex);

                                                int minIndex = values.IndexOf(minVal);
                                                if (minIndex >= 0) values.RemoveAt(minIndex);
                                            }

                                            string avg = values.Average().ToString();

                                            string paramstring = Header[paramIndex].ToUpper();
                                            bool OffsetFactorIncluded = OffsetFactor.ContainsKey(paramstring);
                                            if (OffsetFactorIncluded)
                                            {
                                                string OffsetValue = OffsetFactor[paramstring];
                                                var OffsetValArry = OffsetValue.Split(',');
                                                bool AddOffset = Convert.ToDouble(OffsetValArry[0]) != 0 ? true : false;
                                                bool MultiplyOffset = Convert.ToDouble(OffsetValArry[1]) != 1 ? true : false;
                                                if (AddOffset || MultiplyOffset)
                                                {
                                                    if (AddOffset)
                                                    {
                                                        avg = (Convert.ToDouble(avg) + Convert.ToDouble(OffsetValArry[0])).ToString();
                                                    }
                                                    else if (MultiplyOffset)
                                                    {
                                                        avg = (Convert.ToDouble(avg) * Convert.ToDouble(OffsetValArry[1])).ToString();
                                                    }
                                                }
                                            }

                                            avgList.Add(avg);
                                        }
                                        else if (paramIndex == 0)
                                            avgList.Add("PID-" + sampleNum);
                                        else
                                            avgList.Add("");
                                    }

                                    sb.AppendLine(string.Join(",", avgList));
                                }

                                writer.Write(sb.ToString());
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }

                MessageBox.Show("Package files successfully saved!");

                if (!cbGUCorr.Checked && !cbGUBen.Checked && !cbPoutLimit.Checked)
                {
                    MessageBox.Show("Please check which files you want to generate.");
                }
            }

            private string OpenFileDialog()
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                    openFileDialog.Title = "Select a CSV File";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        return openFileDialog.FileName;
                    }
                }
                return null;
            }

            private string ModifyVersionPattern(string fileName)
            {
                string pattern = @"Rev\d+(p\d*)?";
                string replacement = "Rev9999";

                string result = Regex.Replace(fileName, pattern, replacement);
                if (!Regex.IsMatch(fileName, pattern))
                {
                    result += "_Rev9999";
                }

                return result;
            }

            private string ConvertToValidClassName(string fileName)
            {
                string validName = fileName.Replace('-', '_').Replace(' ', '_');

                if (char.IsDigit(validName[0]))
                    validName = "_" + validName;

                validName = Regex.Replace(validName, @"[^a-zA-Z0-9_]", "");

                if (!validName.CIvStartsWith("_"))
                    validName = "_CS_" + validName;

                return validName;
            }
        }

        //Class to handle XML configuration file loading
        public class XmlParameterLoader
        {
            public string defaultPath;

            public void CreateXmlParameterLoader()
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                defaultPath = Path.Combine(basePath, "Config", "ParameterConfig.xml");

                string configFolder = Path.Combine(basePath, "Config");
                if (!Directory.Exists(configFolder))
                {
                    Directory.CreateDirectory(configFolder);
                }

                if (!File.Exists(defaultPath))
                {
                    CreateDefaultXmlFile();
                }
            }

            private void CreateDefaultXmlFile()
            {
                var xDoc = new XDocument(
                    new XElement("Groups",
                        new XElement("Group", new XAttribute("name", "RF1"),
                            new XElement("BigOffset",
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_P2dB"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_x"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR1"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR2"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PT_E-ACLR"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PT_Txleakage"), new XAttribute("value", "5")),
                                new XElement("Parameter", new XAttribute("name", "PT_H"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PR_IM3"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_x"), new XAttribute("value", "1"))
                            ),
                            new XElement("Trend",
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_P2dB"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_x"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR1"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR2"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PT_E-ACLR"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PT_Txleakage"), new XAttribute("value", "5")),
                                new XElement("Parameter", new XAttribute("name", "PT_H"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PR_IM3"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_x"), new XAttribute("value", "1"))
                            ),
                            new XElement("CustomLimit",
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_P2dB"), new XAttribute("LSL", "25"), new XAttribute("USL", "35")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_x"), new XAttribute("LSL", "0"), new XAttribute("USL", "35")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR1"), new XAttribute("LSL", "-999"), new XAttribute("USL", "-39")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR2"), new XAttribute("LSL", "-999"), new XAttribute("USL", "-42")),
                                new XElement("Parameter", new XAttribute("name", "PT_E-ACLR"), new XAttribute("LSL", "-999"), new XAttribute("USL", "-36")),
                                new XElement("Parameter", new XAttribute("name", "PT_Txleakage"), new XAttribute("LSL", "-50"), new XAttribute("USL", "0")),
                                new XElement("Parameter", new XAttribute("name", "PT_H"), new XAttribute("LSL", "-999"), new XAttribute("USL", "-50")),
                                new XElement("Parameter", new XAttribute("name", "PR_IM3"), new XAttribute("LSL", "-999"), new XAttribute("USL", "-65")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_x"), new XAttribute("LSL", "-20"), new XAttribute("USL", "20"))
                            ),
                            new XElement("Addkeyword",
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR")),
                                new XElement("Parameter", new XAttribute("name", "PT_E-ACLR")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_x")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_P2dB")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_MaxGain")),
                                new XElement("Parameter", new XAttribute("name", "PT_H")),
                                new XElement("Parameter", new XAttribute("name", "PT_PAE")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_P2dB")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_MaxGain")),
                                new XElement("Parameter", new XAttribute("name", "PT_TxLeakage")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_x")),
                                new XElement("Parameter", new XAttribute("name", "PR_Pin_P1dB")),
                                new XElement("Parameter", new XAttribute("name", "PR_Pout_P1dB")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_P1dB")),
                                new XElement("Parameter", new XAttribute("name", "PR_Pout_PSAT")),
                                new XElement("Parameter", new XAttribute("name", "PR_Pout_x")),
                                new XElement("Parameter", new XAttribute("name", "PR_IIP3")),
                                new XElement("Parameter", new XAttribute("name", "PR_IM3")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pin_x")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pin_P2dB")),
                                new XElement("Parameter", new XAttribute("name", "PT_PIN_MaxGain")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_atPin12dBm")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_atPin10dBm"))
                            ),
                            new XElement("MultiplyKeyword",
                                new XElement("Parameter", new XAttribute("name", "PT_Icc_x")),
                                new XElement("Parameter", new XAttribute("name", "PT_Icc2_x")),
                                new XElement("Parameter", new XAttribute("name", "PT_Icc_P2dB")),
                                new XElement("Parameter", new XAttribute("name", "PT_Icc2_P2dB")),
                                new XElement("Parameter", new XAttribute("name", "PT_Itotal_P2dB")),
                                new XElement("Parameter", new XAttribute("name", "PT_Ieff_x")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pcon_x")),
                                new XElement("Parameter", new XAttribute("name", "PT_EVM_x"))
                            ),
                            new XElement("ExceptionKeyword",
                                new XElement("Parameter", new XAttribute("name", "PDM_")),
                                new XElement("Parameter", new XAttribute("name", "CW_x_FixedPin")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_Delta")),
                                new XElement("Parameter", new XAttribute("name", "PT_CAP")),
                                new XElement("Parameter", new XAttribute("name", "G6")),
                                new XElement("Parameter", new XAttribute("name", "PT_TxLeakageTRX"))
                            )
                        ),
                        new XElement("Group", new XAttribute("name", "RF2"),
                            new XElement("BigOffset",
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_P2dB"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_x"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR1"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR2"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PT_E-ACLR"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PT_Txleakage"), new XAttribute("value", "5")),
                                new XElement("Parameter", new XAttribute("name", "PT_H"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PR_IM3"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_x"), new XAttribute("value", "1"))
                            ),
                            new XElement("Trend",
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_P2dB"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_x"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR1"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR2"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PT_E-ACLR"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PT_Txleakage"), new XAttribute("value", "5")),
                                new XElement("Parameter", new XAttribute("name", "PT_H"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PR_IM3"), new XAttribute("value", "10")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_x"), new XAttribute("value", "1"))
                            ),
                            new XElement("CustomLimit",
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_P2dB"), new XAttribute("LSL", "25"), new XAttribute("USL", "35")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_x"), new XAttribute("LSL", "0"), new XAttribute("USL", "35")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR1"), new XAttribute("LSL", "-999"), new XAttribute("USL", "-39")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR2"), new XAttribute("LSL", "-999"), new XAttribute("USL", "-42")),
                                new XElement("Parameter", new XAttribute("name", "PT_E-ACLR"), new XAttribute("LSL", "-999"), new XAttribute("USL", "-36")),
                                new XElement("Parameter", new XAttribute("name", "PT_Txleakage"), new XAttribute("LSL", "-50"), new XAttribute("USL", "0")),
                                new XElement("Parameter", new XAttribute("name", "PT_H"), new XAttribute("LSL", "-999"), new XAttribute("USL", "-50")),
                                new XElement("Parameter", new XAttribute("name", "PR_IM3"), new XAttribute("LSL", "-999"), new XAttribute("USL", "-65")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_x"), new XAttribute("LSL", "-20"), new XAttribute("USL", "20"))
                            ),
                            new XElement("Addkeyword",
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR")),
                                new XElement("Parameter", new XAttribute("name", "PT_E-ACLR")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_x")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_P2dB")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_MaxGain")),
                                new XElement("Parameter", new XAttribute("name", "PT_H")),
                                new XElement("Parameter", new XAttribute("name", "PT_PAE")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_P2dB")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_MaxGain")),
                                new XElement("Parameter", new XAttribute("name", "PT_TxLeakage")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_x")),
                                new XElement("Parameter", new XAttribute("name", "PR_Pin_P1dB")),
                                new XElement("Parameter", new XAttribute("name", "PR_Pout_P1dB")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_P1dB")),
                                new XElement("Parameter", new XAttribute("name", "PR_Pout_PSAT")),
                                new XElement("Parameter", new XAttribute("name", "PR_Pout_x")),
                                new XElement("Parameter", new XAttribute("name", "PR_IIP3")),
                                new XElement("Parameter", new XAttribute("name", "PR_IM3")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pin_x")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pin_P2dB")),
                                new XElement("Parameter", new XAttribute("name", "PT_PIN_MaxGain")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_atPin12dBm")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pout_atPin10dBm"))
                            ),
                            new XElement("MultiplyKeyword",
                                new XElement("Parameter", new XAttribute("name", "PT_Icc_x")),
                                new XElement("Parameter", new XAttribute("name", "PT_Icc2_x")),
                                new XElement("Parameter", new XAttribute("name", "PT_Icc_P2dB")),
                                new XElement("Parameter", new XAttribute("name", "PT_Icc2_P2dB")),
                                new XElement("Parameter", new XAttribute("name", "PT_Itotal_P2dB")),
                                new XElement("Parameter", new XAttribute("name", "PT_Ieff_x")),
                                new XElement("Parameter", new XAttribute("name", "PT_Pcon_x")),
                                new XElement("Parameter", new XAttribute("name", "PT_EVM_x"))
                            ),
                            new XElement("ExceptionKeyword",
                                new XElement("Parameter", new XAttribute("name", "PDM_")),
                                new XElement("Parameter", new XAttribute("name", "CW_x_FixedPin")),
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_Delta")),
                                new XElement("Parameter", new XAttribute("name", "PT_CAP")),
                                new XElement("Parameter", new XAttribute("name", "G6")),
                                new XElement("Parameter", new XAttribute("name", "PT_TxLeakageTRX"))
                            )
                        )
                    )
                );

                xDoc.Save(defaultPath);
            }

            public void LoadParametersFromXml(string userSelectedPath = null)
            {
                string filePath = string.IsNullOrEmpty(userSelectedPath) ? defaultPath : userSelectedPath;

                try
                {
                    XDocument xDoc = XDocument.Load(filePath);

                    var groups = xDoc.Descendants("Group");
                    foreach (var group in groups)
                    {
                        string groupName = group.Attribute("name")?.Value;
                        if (string.IsNullOrEmpty(groupName))
                            throw new Exception("Group name is missing in XML.");

                        var bigOffsetParameters = group.Descendants("BigOffset").Elements("Parameter");
                        var trendParameters = group.Descendants("Trend").Elements("Parameter");
                        var CustomLimitParameters = group.Descendants("CustomLimit").Elements("Parameter");
                        var AddkeywordParameters = group.Descendants("Addkeyword").Elements("Parameter");
                        var MultiplyKeywordParameters = group.Descendants("MultiplyKeyword").Elements("Parameter");
                        var ExceptionKeywordParameters = group.Descendants("ExceptionKeyword").Elements("Parameter");

                        // Initialize dictionaries if they don't exist
                        if (!Manage.ParameterConfig.ContainsKey(groupName))
                        {
                            Manage.ParameterConfig[groupName] = new Dictionary<string, Dictionary<string, double>>();
                        }

                        // Initialize dictionaries if they don't exist
                        if (!Manage.ParameterConfigLimit.ContainsKey(groupName))
                        {
                            Manage.ParameterConfigLimit[groupName] = new Dictionary<string, Dictionary<string, string>>();
                        }

                        // Initialize dictionaries if they don't exist
                        if (!Manage.ParameterCorr.ContainsKey(groupName))
                        {
                            Manage.ParameterCorr[groupName] = new Dictionary<string, List<string>>();
                        }

                        // BigOffset group
                        if (!Manage.ParameterConfig[groupName].ContainsKey("BigOffset"))
                        {
                            Manage.ParameterConfig[groupName]["BigOffset"] = new Dictionary<string, double>();
                        }

                        foreach (var parameter in bigOffsetParameters)
                        {
                            string paramName = parameter.Attribute("name")?.Value;
                            string paramValueStr = parameter.Attribute("value")?.Value;
                            double paramValue = double.Parse(paramValueStr);

                            // Add to BigOffset group
                            Manage.ParameterConfig[groupName]["BigOffset"][paramName.ToUpper()] = paramValue;
                        }

                        // Trend group
                        if (!Manage.ParameterConfig[groupName].ContainsKey("Trend"))
                        {
                            Manage.ParameterConfig[groupName]["Trend"] = new Dictionary<string, double>();
                        }

                        foreach (var parameter in trendParameters)
                        {
                            string paramName = parameter.Attribute("name")?.Value;
                            string paramValueStr = parameter.Attribute("value")?.Value;
                            double paramValue = double.Parse(paramValueStr);

                            // Add to Trend group
                            Manage.ParameterConfig[groupName]["Trend"][paramName.ToUpper()] = paramValue;
                        }

                        // CustomLimit group
                        if (!Manage.ParameterConfigLimit[groupName].ContainsKey("CustomLimit"))
                        {
                            Manage.ParameterConfigLimit[groupName]["CustomLimit"] = new Dictionary<string, string>();
                        }

                        foreach (var parameter in CustomLimitParameters)
                        {
                            string paramName = parameter.Attribute("name")?.Value;
                            string paramLSLStr = parameter.Attribute("LSL")?.Value;
                            string paramUSLStr = parameter.Attribute("USL")?.Value;
                            string paramLimit = "LSL:" + paramLSLStr + "@" + "USL:" + paramUSLStr;

                            // Add to CustomLimit group
                            Manage.ParameterConfigLimit[groupName]["CustomLimit"][paramName.ToUpper()] = paramLimit;
                        }

                        // Addkeyword group
                        if (!Manage.ParameterCorr[groupName].ContainsKey("Addkeyword"))
                        {
                            Manage.ParameterCorr[groupName]["Addkeyword"] = new List<string>();
                        }
                        foreach (var parameter in AddkeywordParameters)
                        {
                            string paramName = parameter.Attribute("name")?.Value.ToUpper();

                            Manage.ParameterCorr[groupName]["Addkeyword"].Add(paramName);
                        }
                        // MultiplyKeyword group
                        if (!Manage.ParameterCorr[groupName].ContainsKey("MultiplyKeyword"))
                        {
                            Manage.ParameterCorr[groupName]["MultiplyKeyword"] = new List<string>();
                        }
                        foreach (var parameter in MultiplyKeywordParameters)
                        {
                            string paramName = parameter.Attribute("name")?.Value.ToUpper();

                            Manage.ParameterCorr[groupName]["MultiplyKeyword"].Add(paramName);
                        }
                        // ExceptionKeyword group
                        if (!Manage.ParameterCorr[groupName].ContainsKey("ExceptionKeyword"))
                        {
                            Manage.ParameterCorr[groupName]["ExceptionKeyword"] = new List<string>();
                        }
                        foreach (var parameter in ExceptionKeywordParameters)
                        {
                            string paramName = parameter.Attribute("name")?.Value.ToUpper();

                            Manage.ParameterCorr[groupName]["ExceptionKeyword"].Add(paramName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading XML: {ex.Message}");
                    MessageBox.Show($"Error reading XML: {ex.Message}");
                    throw;
                }
            }
        }

        public class Manage
        {
            private static Manage _instance;

            public Dictionary<string, int> UniqueKey = new Dictionary<string, int>();
            public Dictionary<int, string> KeyByIndex = new Dictionary<int, string>();
            public List<string> ParaList = new List<string>();

            // Static dictionary to store parameters in a 3-level structure
            public static Dictionary<string, Dictionary<string, Dictionary<string, double>>> ParameterConfig =
                new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();

            public static Dictionary<string, Dictionary<string, Dictionary<string, string>>> ParameterConfigLimit =
                new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            public static Dictionary<string, Dictionary<string, List<string>>> ParameterCorr =
                new Dictionary<string, Dictionary<string, List<string>>>();

            public DataSet NPIGroup = new DataSet() { DType = "NPI" };
            public DataSet RNDGroup = new DataSet() { DType = "R&D" };
            public StasticSet DeltaGroup = new StasticSet() { DType = "Delta" };

            public static Manage GetInstance()
            {
                if (_instance == null)
                {
                    _instance = new Manage();
                }
                return _instance;
            }

            public class StasticSet
            {
                public string DType = "Delta";
                public List<MathStastic> MyList = new List<MathStastic>();

                public class MathStastic
                {
                    public bool bInit = false;
                    public List<double> delta;
                    public double avg, sum, max, min, delta_max_min, Correlation;
                    public int numcount;
                    public List<double> groupRND;
                    public List<double> groupNPI;
                    public string BigOffset;
                    public string Trend;
                    public string Limit;
                    public string CorrFactorAdd = "0";
                    public string CorrFactorMultiply = "1";
                }

                public void DeltaTrendCheck(string Ttype, string param = "")
                {
                    try
                    {
                        foreach (var Bigoffset in ParameterConfig[Ttype]["BigOffset"].Keys)
                        {
                            if (param.ToUpper().Contains(Bigoffset.ToString()))
                            {
                                foreach (var deltaval in MyList.Last().delta)
                                {
                                    double criteria = ParameterConfig[Ttype]["BigOffset"][Bigoffset];

                                    if (Math.Abs(deltaval) > criteria)
                                    {
                                        MyList.Last().BigOffset = "V";
                                        break;
                                    }
                                }
                            }
                        }
                        foreach (var Trend in ParameterConfig[Ttype]["Trend"].Keys)
                        {
                            if (param.ToUpper().Contains(Trend.ToString()))
                            {
                                double criteria = ParameterConfig[Ttype]["Trend"][Trend];

                                if (Math.Abs(MyList.Last().delta_max_min) > criteria)
                                {
                                    MyList.Last().Trend = "V";
                                    break;
                                }
                            }
                        }
                        foreach (var CustomLimit in ParameterConfigLimit[Ttype]["CustomLimit"].Keys)
                        {
                            if (param.ToUpper().Contains(CustomLimit.ToString()))
                            {
                                string Limit = ParameterConfigLimit[Ttype]["CustomLimit"][CustomLimit];
                                var UpperLowerLimit = Limit.Split('@');
                                var LSL = UpperLowerLimit[0].Split(':');
                                var USL = UpperLowerLimit[1].Split(':');
                                double LSLVal = Convert.ToDouble(LSL[1]);
                                double USLVal = Convert.ToDouble(USL[1]);

                                for (int i = 0; i < MyList.Last().groupNPI.Count; i++)
                                {
                                    if (MyList.Last().groupNPI[i] < LSLVal || MyList.Last().groupNPI[i] > USLVal)
                                    {
                                        MyList.Last().Limit = "V";
                                        break;
                                    }
                                    if (MyList.Last().groupRND[i] < LSLVal || MyList.Last().groupRND[i] > USLVal)
                                    {
                                        MyList.Last().Limit = "V";
                                        break;
                                    }
                                }
                            }
                        }

                        if (ParameterCorr[Ttype]["Addkeyword"].Any(s => param.ToUpper().Contains(s)))
                        {
                            if (ParameterCorr[Ttype]["ExceptionKeyword"].Any(s => param.ToUpper().Contains(s)))
                            {
                                MyList.Last().CorrFactorAdd = "0";
                            }
                            else
                            {
                                MyList.Last().CorrFactorAdd = ((MyList.Last().delta.Sum() - MyList.Last().delta.Max() - MyList.Last().delta.Min()) / (MyList.Last().delta.Count() - 2)).ToString();
                            }
                        }
                        else if (ParameterCorr[Ttype]["MultiplyKeyword"].Any(s => param.ToUpper().Contains(s)))
                        {
                            if (ParameterCorr[Ttype]["ExceptionKeyword"].Any(s => param.ToUpper().Contains(s)))
                            {
                                MyList.Last().CorrFactorMultiply = "1";
                            }
                            else
                            {
                                MyList.Last().CorrFactorMultiply = ((MyList.Last().delta.Sum() - MyList.Last().delta.Max() - MyList.Last().delta.Min()) / (MyList.Last().delta.Count() - 2)).ToString();
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                public void AddDelta(string Ttype, string param, double value1, double value2, bool bNewRow = false)
                {
                    try
                    {
                        if (bNewRow)
                        {
                            if (MyList.Count > 0)
                            {
                                //var tt = MyList.Last().delta_max_min;
                                try
                                {
                                    //MyList.Last().CorrFactorAdd = "0";
                                    //MyList.Last().CorrFactorMultiply = "1";

                                    MyList.Last().avg = ((MyList.Last().delta.Sum() - MyList.Last().delta.Max() - MyList.Last().delta.Min()) / (MyList.Last().delta.Count() - 2));

                                    var lastrow = MyList.Last();
                                    var distinctValues = lastrow.delta.Distinct().OrderByDescending(x => x).ToList();

                                    if (lastrow.delta == null || lastrow.delta.Count < 2)
                                    {
                                        MyList.Last().delta_max_min = Double.NaN;
                                    }
                                    else if (distinctValues.Count < 2)
                                    {
                                        MyList.Last().delta_max_min = Double.NaN;
                                    }
                                    else
                                    {
                                        double secndMax = lastrow.delta.Distinct().OrderByDescending(x => x).Skip(1).First();

                                        MyList.Last().delta_max_min = Math.Abs(secndMax - lastrow.min);
                                    }
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.ToString());
                                }

                                if (Double.IsNaN(Correlation.Pearson(MyList.Last().groupRND, MyList.Last().groupNPI))) { }
                                else
                                {
                                    try
                                    {
                                        double maxRND = MyList.Last().groupRND.Max();
                                        double maxNPI = MyList.Last().groupNPI.Max();

                                        List<double> filteredRND = new List<double>();
                                        bool removed = false;
                                        foreach (var num in MyList.Last().groupRND)
                                        {
                                            if (num == maxRND && !removed)
                                            {
                                                removed = true;
                                                continue;
                                            }
                                            filteredRND.Add(num);
                                        }

                                        List<double> filteredNPI = new List<double>();
                                        removed = false;
                                        foreach (var num in MyList.Last().groupNPI)
                                        {
                                            if (num == maxNPI && !removed)
                                            {
                                                removed = true;
                                                continue;
                                            }
                                            filteredNPI.Add(num);
                                        }

                                        MyList.Last().Correlation = Correlation.Pearson(filteredRND, filteredNPI);
                                        //MyList.Last().Correlation = Correlation.Pearson(MyList.Last().groupRND, MyList.Last().groupNPI);
                                    }
                                    catch (Exception e)
                                    {
                                        MessageBox.Show(e.ToString());
                                    }
                                }
                            }
                            MyList.Add(new MathStastic() { delta = new List<double>(), groupRND = new List<double>(), groupNPI = new List<double>() });
                        }

                        var LastRow = MyList.Last();

                        double delta = 0;

                        if (ParameterCorr[Ttype]["MultiplyKeyword"].Any(s => param.ToUpper().Contains(s)))
                        {
                            delta = value1 / value2;
                        }
                        else
                        {
                            delta = value1 - value2;
                        }

                        LastRow.groupRND.Add(value1);
                        LastRow.groupNPI.Add(value2);
                        LastRow.delta.Add(delta);
                        LastRow.numcount += 1;

                        if (LastRow.bInit)
                        {
                            LastRow.sum += delta;
                            LastRow.avg = LastRow.sum / LastRow.numcount;
                            LastRow.max = LastRow.max > delta ? LastRow.max : delta;
                            LastRow.min = LastRow.min < delta ? LastRow.min : delta;
                            LastRow.delta_max_min = Math.Abs(LastRow.max - LastRow.min);
                        }
                        else
                        {
                            LastRow.max = LastRow.min = LastRow.delta_max_min = LastRow.sum = LastRow.avg = delta;
                            LastRow.bInit = true;
                            //LastRow.BigOffset = LastRow.Trend = "";
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                }
            }

            public class DataSet
            {
                public string DType = "NPI";
                public Dictionary<string, List<double>> MyList = new Dictionary<string, List<double>>();

                public void Add(string PID, double value)
                {
                    if (MyList.ContainsKey(PID))
                        MyList[PID].Add(value);
                    else
                        MyList.Add(PID, new List<double>() { value });
                }
            }
        }

        // Load Excel button click event
        private void LoadExcel_Click(object sender, EventArgs e)
        {
            TesterType TesterTypeform = new TesterType();

            TesterTypeform.ShowDialog();
            TesterTypeStr = TesterTypeform.selectedType;

            if (TesterTypeStr != null)
            {
                // Open the Excel file dialog
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    DereferenceLinks = false,  // 네트워크 드라이브 문제 방지
                    CheckFileExists = false,  // 존재 여부 검사 비활성화
                    Filter = "Excel Files|*.xlsx"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Manage DataManager = Manage.GetInstance();
                        XmlParameterLoader xmlLoader = new XmlParameterLoader();
                        int dataStartRow = 0;
                        int dataStartColumn = 0;
                        int parameterColumn = 0;
                        int NPIPIDColumn = 0;
                        int NPIPIDRow = 0;
                        int RNDPIDColumn = 0;
                        int RNDPIDRow = 0;

                        // Open the Excel file
                        using (workbook = new XLWorkbook(openFileDialog.FileName))
                        {
                            // Find the sheet with "RAWDATA" in its name
                            CorrelationRF1 = workbook.Worksheets.FirstOrDefault(ws => ws.Name.ToUpper().Contains("CORRELATION-" + TesterTypeStr));
                            //var keywordSheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.ToUpper().Contains("KEYWORD"));

                            //if (CorrelationRF1 == null || keywordSheet == null)
                            if (CorrelationRF1 == null)
                            {
                                MessageBox.Show("Required sheets ('Correlation-RF1' or 'KEYWORD') not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            // Assuming 'sheet' is your Excel worksheet
                            var noCell = CorrelationRF1.CellsUsed().FirstOrDefault(cell => cell.Value.ToString().ToUpper() == "NO");

                            if (noCell != null)
                            {
                                dataStartRow = noCell.Address.RowNumber + 1;  // Row just below "No" cell
                                dataStartColumn = noCell.Address.ColumnNumber;  // Column "No" cell
                                parameterColumn = noCell.Address.ColumnNumber + 1;  // Column right of "No"
                            }
                            else
                            {
                                MessageBox.Show("'No' cell not found in the sheet.");
                            }

                            int LastRow = CorrelationRF1.LastRowUsed().RowNumber();
                            for (int i = dataStartRow; LastRow >= i; i++)
                            {
                                string key = CorrelationRF1.Cell(i, dataStartColumn).Value.ToString() + "@" + CorrelationRF1.Cell(i, parameterColumn).Value.ToString();
                                DataManager.UniqueKey.Add(key, i);
                                DataManager.KeyByIndex.Add(i, key);
                                DataManager.ParaList.Add(key);
                            }

                            var NPICell = CorrelationRF1.CellsUsed().FirstOrDefault(cell => cell.Value.ToString().ToUpper().Contains("NPI"));
                            var RNDCell = CorrelationRF1.CellsUsed().FirstOrDefault(cell => cell.Value.ToString().ToUpper().Contains("R&D"));

                            NPIPIDColumn = NPICell.Address.ColumnNumber;
                            NPIPIDRow = NPICell.Address.RowNumber + 1;
                            RNDPIDColumn = RNDCell.Address.ColumnNumber;
                            RNDPIDRow = RNDCell.Address.RowNumber + 1;

                            while (!string.IsNullOrEmpty(CorrelationRF1.Cell(NPIPIDRow, NPIPIDColumn).Value.ToString()))
                            {
                                string value = CorrelationRF1.Cell(NPIPIDRow, NPIPIDColumn).Value.ToString();
                                if (value.Contains("PID"))
                                {
                                    List<double> DataValue = new List<double>();

                                    foreach (var rownum in DataManager.UniqueKey)
                                    {
                                        double result = double.TryParse(CorrelationRF1.Cell(rownum.Value, NPIPIDColumn).Value.ToString(), out double parsedValue) ? parsedValue : -999;
                                        DataValue.Add(result);
                                    }
                                    DataManager.NPIGroup.MyList.Add(value, DataValue);
                                }
                                NPIPIDColumn++;
                            }
                            NPIPIDColumn = NPICell.Address.ColumnNumber;

                            while (!string.IsNullOrEmpty(CorrelationRF1.Cell(RNDPIDRow, RNDPIDColumn).Value.ToString()))
                            {
                                string value = CorrelationRF1.Cell(RNDPIDRow, RNDPIDColumn).Value.ToString();
                                if (value.Contains("PID"))
                                {
                                    List<double> DataValue = new List<double>();

                                    foreach (var rownum in DataManager.UniqueKey)
                                    {
                                        double result = double.TryParse(CorrelationRF1.Cell(rownum.Value, RNDPIDColumn).Value.ToString(), out double parsedValue) ? parsedValue : -999;
                                        DataValue.Add(result);
                                    }
                                    DataManager.RNDGroup.MyList.Add(value, DataValue);
                                }
                                RNDPIDColumn++;
                            }
                            RNDPIDColumn = RNDCell.Address.ColumnNumber;

                            // Ask if user wants to load a config file
                            DialogResult dialogResult = MessageBox.Show("Would you like to load the Parameter config file?", "Load Config", MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                            {
                                // Open file dialog for selecting a config file
                                OpenFileDialog configFileDialog = new OpenFileDialog();
                                configFileDialog.Filter = "XML Files|*.xml";
                                if (configFileDialog.ShowDialog() == DialogResult.OK)
                                {
                                    // Load the selected XML config file
                                    xmlLoader.LoadParametersFromXml(configFileDialog.FileName);
                                }
                            }
                            else
                            {
                                // Load the default config file from the program's directory
                                string defaultConfigFile = System.IO.Path.Combine(Application.StartupPath, "Config", "ParameterConfig.xml");
                                xmlLoader.CreateXmlParameterLoader();
                                xmlLoader.LoadParametersFromXml(defaultConfigFile);
                            }

                            var cNum = DataManager.UniqueKey.Count();
                            ParaRowNum = 0;
                            for (int i = 0; i < cNum; i++)
                            {
                                try
                                {
                                    bool bInitRow = true;
                                    string[] Param;
                                    Param = DataManager.ParaList[ParaRowNum].Split('@');

                                    foreach (var PID in DataManager.NPIGroup.MyList)
                                    {
                                        var value1 = DataManager.RNDGroup.MyList[PID.Key][i];
                                        var value2 = DataManager.NPIGroup.MyList[PID.Key][i];

                                        DataManager.DeltaGroup.AddDelta(TesterTypeStr, Param[1], value1, value2, bInitRow);
                                        bInitRow = false;
                                    }

                                    DataManager.DeltaGroup.DeltaTrendCheck(TesterTypeStr, Param[1]);
                                    ParaRowNum++;
                                }
                                catch
                                {
                                }
                            }
                        }
                        //Convert to DataTable type
                        ConvertToDataTable(DataManager.DeltaGroup.MyList, DataManager.ParaList);

                        lblStatus.Text = "Excel file successfully Loaded! If you want to generate package files, Press the 'F7' button to change the form.";
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = $"Error occurred: {ex.Message}";
                        ParaRowNum = 0;
                        MessageBox.Show("Excel file Loading failed!\n" + ex.ToString());
                    }
                }
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Export_ExcelFile_Click(object sender, EventArgs e)
        {
            Manage dataManager = Manage.GetInstance();

            var deltaData = dataManager.DeltaGroup.MyList;
            var parameter = dataManager.ParaList;

            try
            {
                ExportExcelFile(deltaData, parameter);
                MessageBox.Show("Excel file successfully saved!");
            }
            catch
            {
                MessageBox.Show("Failed to save the excel file, please check the error!");
            }
        }

        private void ExportExcelFile(List<Manage.StasticSet.MathStastic> MyList, List<string> parameterdic)
        {
            // Create a new workbook
            var workbook = new XLWorkbook();

            // Create a worksheet for the data
            var ExportWorksheet = workbook.AddWorksheet("ExportedData");

            // Set header row
            ExportWorksheet.Cell(1, 1).Value = "No";
            ExportWorksheet.Cell(1, 2).Value = "Parameter";

            int currentColumn = 3; // Track current column position
            int SampleCount = MyList[0].delta.Count; // Track the maximum sample count across all rows

            // Add headers dynamically for groupNPI, groupRND, and delta
            for (int i = 0; i < SampleCount; i++)
            {
                ExportWorksheet.Cell(1, currentColumn).Value = $"RND-{i + 1}";
                currentColumn++;
            }

            for (int i = 0; i < SampleCount; i++)
            {
                ExportWorksheet.Cell(1, currentColumn).Value = $"NPI-{i + 1}";
                currentColumn++;
            }

            for (int i = 0; i < SampleCount; i++)
            {
                ExportWorksheet.Cell(1, currentColumn).Value = $"Delta-{i + 1}";
                currentColumn++;
            }

            // Add headers for the remaining fixed columns
            ExportWorksheet.Cell(1, currentColumn++).Value = "Delta Avg";
            ExportWorksheet.Cell(1, currentColumn++).Value = "Delta Max";
            ExportWorksheet.Cell(1, currentColumn++).Value = "Delta Min";
            ExportWorksheet.Cell(1, currentColumn++).Value = "Delta Max-Min";
            ExportWorksheet.Cell(1, currentColumn++).Value = "Correlation";
            ExportWorksheet.Cell(1, currentColumn++).Value = "BigOffset";
            ExportWorksheet.Cell(1, currentColumn++).Value = "Trend";
            ExportWorksheet.Cell(1, currentColumn++).Value = "CustomLimit";
            ExportWorksheet.Cell(1, currentColumn++).Value = "CorrFactor_Add";
            ExportWorksheet.Cell(1, currentColumn++).Value = "CorrFactor_Multiply";

            // Start filling data from row 2
            int currentRow = 2;
            int index = 0;
            foreach (var list in MyList)
            {
                try
                {
                    // No and Parameter row
                    ExportWorksheet.Cell(currentRow, 1).Value = (index + 1).ToString();
                    string[] param = parameterdic[index].Split('@');
                    ExportWorksheet.Cell(currentRow, 2).Value = param[1].ToString();
                    currentColumn = 3; // Reset to the first column for each row

                    // Write groupRND values
                    for (int i = 0; i < SampleCount; i++)
                    {
                        ExportWorksheet.Cell(currentRow, currentColumn).Value = i < list.groupRND.Count ? list.groupRND[i] : 999;
                        currentColumn++;
                    }
                    // Write groupNPI values
                    for (int i = 0; i < SampleCount; i++)
                    {
                        ExportWorksheet.Cell(currentRow, currentColumn).Value = i < list.groupNPI.Count ? list.groupNPI[i] : 999;
                        currentColumn++;
                    }
                    // Write Delta values
                    for (int i = 0; i < SampleCount; i++)
                    {
                        ExportWorksheet.Cell(currentRow, currentColumn).Value = i < list.delta.Count ? list.delta[i] : 999;
                        currentColumn++;
                    }
                    // Write the remaining values
                    ExportWorksheet.Cell(currentRow, currentColumn++).Value = list.avg.ToString();
                    ExportWorksheet.Cell(currentRow, currentColumn++).Value = list.max.ToString();
                    ExportWorksheet.Cell(currentRow, currentColumn++).Value = list.min.ToString();
                    ExportWorksheet.Cell(currentRow, currentColumn++).Value = list.delta_max_min.ToString();
                    ExportWorksheet.Cell(currentRow, currentColumn++).Value = list.Correlation.ToString();
                    ExportWorksheet.Cell(currentRow, currentColumn++).Value = list.BigOffset == null ? "" : list.BigOffset;
                    ExportWorksheet.Cell(currentRow, currentColumn++).Value = list.Trend == null ? "" : list.Trend;
                    ExportWorksheet.Cell(currentRow, currentColumn++).Value = list.Limit == null ? "" : list.Limit;
                    ExportWorksheet.Cell(currentRow, currentColumn++).Value = list.CorrFactorAdd == null ? "" : list.CorrFactorAdd;
                    ExportWorksheet.Cell(currentRow, currentColumn++).Value = list.CorrFactorMultiply == null ? "" : list.CorrFactorMultiply;

                    index++;
                    currentRow++;
                }
                catch
                {
                }
            }

            // Add borders to all cells
            ExportWorksheet.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ExportWorksheet.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Set the first row to bold
            ExportWorksheet.Row(1).Style.Font.Bold = true;

            // After processing, display Save As dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xlsx";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Save as a new file
                workbook.SaveAs(saveFileDialog.FileName);
                ExportWorksheet.Clear();

                lblStatus.Text = "Excel file successfully saved.";
            }
        }

        private void advancedDataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            Manage dataManager = Manage.GetInstance();
            var temp = advancedDataGridView1.SelectedRows;

            if (advancedDataGridView1.SelectedRows.Count > 0)
            {
                var selectedRows = advancedDataGridView1.SelectedRows;

                if (selectedRows.Count == 1)
                {
                    // Get the filtered DataRowView
                    DataRowView dataRowView = (DataRowView)bindingSource1[e.RowIndex];

                    // Get the actual DataRow
                    DataRow dataRow = dataRowView.Row;
                    int sampleCount = dataManager.DeltaGroup.MyList[0].delta.Count();
                    // Use dataRow to plot the graph
                    PlotGraph(dataRow, sampleCount);
                }
                else
                {
                    DialogResult result = MessageBox.Show(
                        "Do you want to plot each row individually (Yes) or combine all rows into one window (No)?",
                        "Plot Options",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        foreach (DataGridViewRow selectedRow in advancedDataGridView1.SelectedRows)
                        {
                            // Get the filtered DataRowView
                            DataRowView dataRowView = (DataRowView)bindingSource1[selectedRow.Index];

                            // Get the actual DataRow
                            DataRow dataRow = dataRowView.Row;
                            int sampleCount = dataManager.DeltaGroup.MyList[0].delta.Count();
                            // Use dataRow to plot the graph
                            PlotGraph(dataRow, sampleCount);
                        }
                    }
                    else if (result == DialogResult.No)
                    {
                        List<List<double>> allGroupRND = new List<List<double>>();
                        List<List<double>> allGroupNPI = new List<List<double>>();
                        List<string> parameters = new List<string>();

                        foreach (DataGridViewRow selectedRow in selectedRows)
                        {
                            DataRowView dataRowView = (DataRowView)selectedRow.DataBoundItem;
                            DataRow dataRow = dataRowView.Row;

                            List<double> groupRNDList = new List<double>();
                            List<double> groupNPIList = new List<double>();
                            string parameterName = dataRow["Parameter"].ToString();

                            for (int i = 0; i < dataManager.DeltaGroup.MyList[0].delta.Count(); i++)
                            {
                                groupRNDList.Add(Convert.ToDouble(dataRow[$"RND-{i + 1}"]));
                                groupNPIList.Add(Convert.ToDouble(dataRow[$"NPI-{i + 1}"]));
                            }

                            allGroupRND.Add(groupRNDList);
                            allGroupNPI.Add(groupNPIList);
                            parameters.Add(parameterName);
                        }

                        PlotCombinedGraphAsRows(allGroupRND, allGroupNPI, parameters);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please choose the row you want to plot");
            }
        }

        private void PlotCombinedGraphAsRows(List<List<double>> allGroupRND, List<List<double>> allGroupNPI, List<string> parameters)
        {
            Form combinedForm = new Form
            {
                Text = "Combined Graphs",
                AutoScroll = true,
                Width = 800,
                Height = 600,
            };

            TableLayoutPanel panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                AutoScroll = true,
                BackColor = Color.LightGray
            };

            panel.RowCount = (int)Math.Ceiling(allGroupRND.Count / 3.0);

            for (int i = 0; i < panel.ColumnCount; i++)
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 567));

            for (int i = 0; i < panel.RowCount; i++)
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 567));

            for (int i = 0; i < allGroupRND.Count; i++)
            {
                Chart chart = new Chart
                {
                    Dock = DockStyle.None,
                    // Set the minimum size to 120mm x 120mm
                    Width = (int)(150 * 3.77953),  // 120mm in pixels
                    Height = (int)(150 * 3.77953), // 120mm in pixels
                };

                var chartArea = new ChartArea
                {
                    AxisX = { Title = "Sample #" },
                    AxisY = { Title = "Value" }
                };

                var seriesRND = new Series("RND")
                {
                    ChartType = SeriesChartType.Line,  // Line chart for the trend
                    BorderWidth = 3,
                    Color = Color.Blue,
                    MarkerStyle = MarkerStyle.Circle,  // Add markers to the line
                    MarkerSize = 10  // Size of the markers
                };

                var seriesNPI = new Series("NPI")
                {
                    ChartType = SeriesChartType.Line,  // Line chart for the trend
                    BorderWidth = 3,
                    Color = Color.Red,
                    MarkerStyle = MarkerStyle.Square,  // Add markers to the line
                    MarkerSize = 10  // Size of the markers
                };

                for (int j = 0; j < allGroupRND[i].Count; j++)
                {
                    seriesRND.Points.AddXY(j + 1, allGroupRND[i][j]);
                    seriesNPI.Points.AddXY(j + 1, allGroupNPI[i][j]);
                }
                chart.ChartAreas.Add(chartArea);

                chartArea.AxisY.IsStartedFromZero = false;
                chart.ChartAreas[0].AxisY.Minimum = Double.NaN;
                chart.ChartAreas[0].AxisY.Maximum = Double.NaN;
                chart.ChartAreas[0].RecalculateAxesScale();

                // Set X-axis and Y-axis titles
                chart.ChartAreas[0].AxisX.Title = "X-Axis (Sample#)";
                chart.ChartAreas[0].AxisY.Title = "Y-Axis (Value)";

                // Customize axis title font
                chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 10, FontStyle.Bold);
                chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 10, FontStyle.Bold);

                // Add a legend to the chart
                var legend = new Legend
                {
                    Docking = Docking.Right, // Position the legend at the top
                    Alignment = StringAlignment.Far, // Center align the legend
                    Font = new Font("Arial", 8) // Set legend font size
                };

                chart.Legends.Add(legend);

                var title = new Title
                {
                    Text = parameters[i], // Include the parameter name in the title
                    Font = new Font("Arial", 10, FontStyle.Bold), // Set default font style
                    Docking = Docking.Top, // Position the title at the top of the chart
                    Alignment = ContentAlignment.TopLeft, // Align the title to the center
                    IsDockedInsideChartArea = false // Position the title outside the chart area
                };
                // Adjust font size for long parameter names
                if (parameters[i].Length > 50)
                {
                    title.Font = new Font("Arial", 8, FontStyle.Bold); // Reduce font size for long names
                }

                // Clear existing titles and add the new one
                chart.Titles.Clear();
                chart.Titles.Add(title);

                // Adjust chart area to ensure the title does not overlap with the graph
                chart.ChartAreas[0].Position = new ElementPosition(5, 10, 90, 80); // Adjust margins

                chart.Series.Add(seriesRND);
                chart.Series.Add(seriesNPI);

                // Assign series to legend
                chart.Series["RND"].LegendText = "RND";
                chart.Series["NPI"].LegendText = "NPI";

                panel.Controls.Add(chart);
            }
            combinedForm.Controls.Add(panel);

            combinedForm.Show();
        }

        private void PlotGraph(List<double> groupRNDList, List<double> groupNPIList, string parameterName)
        {
            ChartForm form2 = new ChartForm();

            // Create a new chart for comparing the trends
            var chart = new Chart();
            chart.Dock = DockStyle.Fill;  // Fill the form with the chart

            var chartArea = new ChartArea();

            chartArea.AxisY.IsStartedFromZero = false;
            chartArea.AxisY.Minimum = Double.NaN;
            chartArea.AxisY.Maximum = Double.NaN;

            var seriesRND = new Series("RND")
            {
                ChartType = SeriesChartType.Line,  // Line chart for the trend
                BorderWidth = 3,
                Color = Color.Blue,
                MarkerStyle = MarkerStyle.Circle,  // Add markers to the line
                MarkerSize = 10  // Size of the markers
            };

            var seriesNPI = new Series("NPI")
            {
                ChartType = SeriesChartType.Line,  // Line chart for the trend
                BorderWidth = 3,
                Color = Color.Red,
                MarkerStyle = MarkerStyle.Square,  // Add markers to the line
                MarkerSize = 10  // Size of the markers
            };

            // Add data points to the series
            for (int i = 0; i < groupRNDList.Count; i++)
            {
                seriesRND.Points.AddXY(i + 1, groupRNDList[i]);  // X is index, Y is value
                seriesNPI.Points.AddXY(i + 1, groupNPIList[i]);   // Same X for comparison
            }

            chart.ChartAreas.Add(chartArea);

            // Set X-axis and Y-axis titles
            chart.ChartAreas[0].AxisX.Title = "X-Axis (Sample#)";
            chart.ChartAreas[0].AxisY.Title = "Y-Axis (Value)";

            // Customize axis title font
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 10, FontStyle.Bold);
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 10, FontStyle.Bold);

            // Add a legend to the chart
            var legend = new Legend
            {
                Docking = Docking.Right, // Position the legend at the top
                Alignment = StringAlignment.Far, // Center align the legend
                Font = new Font("Arial", 8) // Set legend font size
            };

            chart.Legends.Add(legend);

            var title = new Title
            {
                Text = parameterName, // Include the parameter name in the title
                Font = new Font("Arial", 10, FontStyle.Bold), // Set default font style
                Docking = Docking.Top, // Position the title at the top of the chart
                Alignment = ContentAlignment.TopLeft, // Align the title to the center
                IsDockedInsideChartArea = false // Position the title outside the chart area
            };
            // Adjust font size for long parameter names
            if (parameterName.Length > 50)
            {
                title.Font = new Font("Arial", 8, FontStyle.Bold); // Reduce font size for long names
            }

            // Clear existing titles and add the new one
            chart.Titles.Clear();
            chart.Titles.Add(title);

            // Adjust chart area to ensure the title does not overlap with the graph
            chart.ChartAreas[0].Position = new ElementPosition(5, 10, 90, 80); // Adjust margins

            chart.Series.Add(seriesRND);
            chart.Series.Add(seriesNPI);

            // Assign series to legend
            chart.Series["RND"].LegendText = "RND";
            chart.Series["NPI"].LegendText = "NPI";

            chart.Dock = DockStyle.Fill;

            //Plot location
            int fixedX = Screen.PrimaryScreen.WorkingArea.Width / 2 - 200;
            int fixedY = Screen.PrimaryScreen.WorkingArea.Height / 2 - 150;
            form2.StartPosition = FormStartPosition.Manual;
            form2.Location = new Point(fixedX, fixedY);

            // Add chart to the form
            form2.SetChart(chart);
            form2.Show();
        }

        private void PlotGraph(DataRow datarow, int SampleCount)
        {
            // Extract data from the DataRow and use it to plot the graph
            List<double> groupRNDList = new List<double>();
            List<double> groupNPIList = new List<double>();
            string parameterName = datarow["Parameter"].ToString();

            for (int i = 0; i < SampleCount; i++)
            {
                groupRNDList.Add(Convert.ToDouble(datarow[$"RND-{i + 1}"]));
                groupNPIList.Add(Convert.ToDouble(datarow[$"NPI-{i + 1}"]));
            }

            // Call the existing graph plotting method
            PlotGraph(groupRNDList, groupNPIList, parameterName);
        }

        private void advancedDataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            advancedDataGridView1.SuspendLayout();
            // Perform any updates if needed
            advancedDataGridView1.ResumeLayout();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7)
            {
                if (spcMain.Panel1Collapsed == true)
                {
                    spcMain.Panel1Collapsed = false;
                }
                else
                {
                    spcMain.Panel1Collapsed = true;
                }
            }
        }

        private void GenPackage_Click(object sender, EventArgs e)
        {
            PackageHelper nPack = new PackageHelper();
            List<string> para = new List<string>();
            List<string> FactorAdd = new List<string>();
            List<string> FactorMultiply = new List<string>();

            foreach (DataRow Row in datatable.Rows)
            {
                var parameter = Row["Parameter"];
                var CorrFactorAdd = Row["CorrFactor_Add"];
                var CorrFactorMultiply = Row["CorrFactor_Multiply"];
                para.Add(parameter.ToString());
                FactorAdd.Add(CorrFactorAdd.ToString());
                FactorMultiply.Add(CorrFactorMultiply.ToString());
            }

            nPack.GeneratePackage(para, FactorAdd, FactorMultiply, spcMain);
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            lblDataSource.Location = new Point((advancedDataGridView1.Width - lblDataSource.Width) / 2, (advancedDataGridView1.Height - lblDataSource.Height) / 2);
        }

        private void ConvertToDataTable(List<Manage.StasticSet.MathStastic> MyList, List<string> parameterdic)
        {
            // Create a new DataTable
            //DataTable datatable = new DataTable();

            // Set header row
            datatable.Columns.Add("No", typeof(int));
            datatable.Columns.Add("Parameter", typeof(string));
            datatable.Columns.Add("Parameter2nd", typeof(string));

            int SampleCount = MyList[0].delta.Count; // Track the maximum sample count across all rows
            // Add headers dynamically for groupNPI, groupRND, and delta
            for (int i = 0; i < SampleCount; i++)
            {
                datatable.Columns.Add($"RND-{i + 1}", typeof(string));
            }
            for (int i = 0; i < SampleCount; i++)
            {
                datatable.Columns.Add($"NPI-{i + 1}", typeof(string));
            }
            for (int i = 0; i < SampleCount; i++)
            {
                datatable.Columns.Add($"Delta-{i + 1}", typeof(string));
            }

            datatable.Columns.Add("Delta Avg", typeof(string));
            datatable.Columns.Add("Delta Max", typeof(string));
            datatable.Columns.Add("Delta Min", typeof(string));
            datatable.Columns.Add("Delta Max-Min", typeof(string));
            datatable.Columns.Add("Correlation", typeof(string));
            datatable.Columns.Add("Big Offset", typeof(string));
            datatable.Columns.Add("Trend", typeof(string));
            datatable.Columns.Add("CustomLimit", typeof(string));
            datatable.Columns.Add("CorrFactor_Add", typeof(string));
            datatable.Columns.Add("CorrFactor_Multiply", typeof(string));

            //Start filling data from row 2
            int index = 0;
            foreach (var list in MyList)
            {
                try
                {
                    string[] param = parameterdic[index].Split('@');

                    var row = datatable.NewRow();

                    row["No"] = (index + 1);
                    row["Parameter"] = param[1];
                    row["Parameter2nd"] = param[1];
                    row["Delta Avg"] = list.avg.ToString();
                    row["Delta Max"] = list.max.ToString();
                    row["Delta Min"] = list.min.ToString();
                    row["Delta Max-Min"] = list.delta_max_min.ToString();
                    row["Correlation"] = list.Correlation.ToString();
                    row["Big Offset"] = list.BigOffset == null ? "" : list.BigOffset.ToString();
                    row["Trend"] = list.Trend == null ? "" : list.Trend.ToString();
                    row["CustomLimit"] = list.Limit == null ? "" : list.Limit.ToString();
                    row["CorrFactor_Add"] = list.CorrFactorAdd.ToString() == null ? "" : list.CorrFactorAdd.ToString();
                    row["CorrFactor_Multiply"] = list.CorrFactorMultiply.ToString() == null ? "" : list.CorrFactorMultiply.ToString();

                    for (int i = 0; i < SampleCount; i++)
                    {
                        row[$"RND-{i + 1}"] = list.groupRND[i].ToString();
                    }

                    for (int i = 0; i < SampleCount; i++)
                    {
                        row[$"NPI-{i + 1}"] = list.groupNPI[i].ToString();
                    }

                    for (int i = 0; i < SampleCount; i++)
                    {
                        row[$"Delta-{i + 1}"] = list.delta[i].ToString();
                    }

                    datatable.Rows.Add(row);

                    index++;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
            // Remove automatically added columns and rows
            advancedDataGridView1.Columns.Clear(); // Removes all columns
            advancedDataGridView1.Rows.Clear();    // Removes all rows
            advancedDataGridView1.AllowUserToAddRows = true;
            advancedDataGridView1.MultiSelect = true;

            // Bind the datatable to the BindingSource and then to the AdvancedDataGridView
            bindingSource1.DataSource = datatable;
            advancedDataGridView1.DataSource = bindingSource1;
            advancedDataGridView1.SetFilterChecklistNodesMax(10);

            this.ClientSize = advancedDataGridView1.PreferredSize;

            foreach (DataGridViewColumn column in advancedDataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                column.Resizable = DataGridViewTriState.True; // Allow resizing for all columns
            }
            advancedDataGridView1.AllowUserToResizeRows = true; // Allow users to resize rows

            // Change row style based on 'Big Offset' and 'Trend' column values
            foreach (DataGridViewRow row in advancedDataGridView1.Rows)
            {
                if (row.Cells["Big Offset"].Value?.ToString() == "V" | row.Cells["Trend"].Value?.ToString() == "V")
                {
                    row.DefaultCellStyle.ForeColor = Color.Red; // Change text color to red
                    row.DefaultCellStyle.BackColor = Color.LightYellow; // Change background color to light yellow
                }
                if (row.Cells["CustomLimit"].Value?.ToString() == "V")
                {
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    row.DefaultCellStyle.BackColor = Color.LightPink;
                }
            }
            lblDataSource.Hide();
        }

        private void UpdateRowStyles()
        {
            foreach (DataGridViewRow row in advancedDataGridView1.Rows)
            {
                if (row.Cells["Big Offset"].Value?.ToString() == "V" | row.Cells["Trend"].Value?.ToString() == "V")
                {
                    row.DefaultCellStyle.ForeColor = Color.Red; // Change text color to red
                    row.DefaultCellStyle.BackColor = Color.LightYellow; // Change background color to light yellow
                }
                if (row.Cells["CustomLimit"].Value?.ToString() == "V")
                {
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    row.DefaultCellStyle.BackColor = Color.LightPink;
                }
            }
        }
    }
}