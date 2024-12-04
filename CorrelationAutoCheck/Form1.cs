using ClosedXML.Excel;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CorrelationAutoCheck
{
    public partial class Form1 : Form
    {
        public int LastColumnUsedIndex = 0;
        public int ParaRowNum = 0;
        // Declare workbook and worksheet as class-level variables
        private XLWorkbook workbook;
        private IXLWorksheet CorrelationRF1, CorrelationRF2, CorrelationNFR;
        //private Manage DataManager = new Manage();

        public Form1()
        {
            InitializeComponent();

        }

        // Class to handle XML configuration file loading
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
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_x"), new XAttribute("value", "0.5")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR1"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR2"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_E-ACLR"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_Txleakage"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_H"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PR_IM3"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_x"), new XAttribute("value", "0.5"))
                            ),
                            new XElement("Trend",
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_x"), new XAttribute("value", "0.5")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR1"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR2"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_E-ACLR"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_Txleakage"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_H"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PR_IM3"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_x"), new XAttribute("value", "0.5"))
                            )
                        ),
                        new XElement("Group", new XAttribute("name", "RF2"),
                            new XElement("BigOffset",
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_x"), new XAttribute("value", "0.5")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR1"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR2"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_E-ACLR"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_Txleakage"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_H"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PR_IM3"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_x"), new XAttribute("value", "0.5"))
                            ),
                            new XElement("Trend",
                                new XElement("Parameter", new XAttribute("name", "PT_Gain_x"), new XAttribute("value", "0.5")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR1"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_ACLR2"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_E-ACLR"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_Txleakage"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PT_H"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PR_IM3"), new XAttribute("value", "1")),
                                new XElement("Parameter", new XAttribute("name", "PR_Gain_x"), new XAttribute("value", "0.5"))
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

                        // Initialize dictionaries if they don't exist
                        if (!Manage.ParameterConfig.ContainsKey(groupName))
                        {
                            Manage.ParameterConfig[groupName] = new Dictionary<string, Dictionary<string, double>>();
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
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading XML: {ex.Message}");
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
                }

                public void DeltaTrendCheck(string param = "")
                {
                    foreach (var Bigoffset in ParameterConfig["RF1"]["BigOffset"].Keys)
                    {
                        if (param.ToUpper().Contains(Bigoffset.ToString()))
                        {
                            foreach (var deltaval in MyList.Last().delta)
                            {
                                double criteria = ParameterConfig["RF1"]["BigOffset"][Bigoffset];

                                if (Math.Abs(deltaval) > criteria)
                                {
                                    MyList.Last().BigOffset = "V";
                                    break;
                                }
                            }

                        }
                    }
                    foreach (var Trend in ParameterConfig["RF1"]["Trend"].Keys)
                    {
                        if (param.ToUpper().Contains(Trend.ToString()))
                        {
                            double criteria = ParameterConfig["RF1"]["Trend"][Trend];

                            if (Math.Abs(MyList.Last().delta_max_min) > criteria)
                            {
                                MyList.Last().Trend = "V";
                                break;
                            }
                        }
                    }
                }

                public void AddDelta(double value1, double value2, bool bNewRow = false)
                {
                    try
                    {
                        if (bNewRow)
                        {
                            if (MyList.Count > 0)
                            {
                                if (Double.IsNaN(Correlation.Pearson(MyList.Last().groupRND, MyList.Last().groupNPI))) { }
                                else
                                {
                                    MyList.Last().Correlation = Correlation.Pearson(MyList.Last().groupRND, MyList.Last().groupNPI);
                                }
                            }
                            MyList.Add(new MathStastic() { delta = new List<double>(), groupRND = new List<double>(), groupNPI = new List<double>() });
                        }

                        var LastRow = MyList.Last();

                        var delta = value1 - value2;

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
                    catch
                    {

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
            // Open the Excel file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xlsx";
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
                        CorrelationRF1 = workbook.Worksheets.FirstOrDefault(ws => ws.Name.ToUpper().Contains("CORRELATION-RF1"));
                        var keywordSheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.ToUpper().Contains("KEYWORD"));

                        if (CorrelationRF1 == null || keywordSheet == null)
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
                        DialogResult dialogResult = MessageBox.Show("Would you like to load the Config file?", "Load Config", MessageBoxButtons.YesNo);
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

                                foreach (var PID in DataManager.NPIGroup.MyList)
                                {

                                    var value1 = DataManager.RNDGroup.MyList[PID.Key][i];
                                    var value2 = DataManager.NPIGroup.MyList[PID.Key][i];

                                    DataManager.DeltaGroup.AddDelta(value1, value2, bInitRow);
                                    bInitRow = false;

                                }

                                Param = DataManager.ParaList[ParaRowNum].Split('@');

                                DataManager.DeltaGroup.DeltaTrendCheck(Param[1]);
                                ParaRowNum++;
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = $"Error occurred: {ex.Message}";
                    ParaRowNum = 0;
                }

                lblStatus.Text = "Excel file successfully Loaded!";
                MessageBox.Show("Excel file successfully Loaded!");
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

            // Start filling data from row 2
            int currentRow = 2;
            int index = 0;
            foreach (var list in MyList)
            {
                try
                {
                    // No and Parameter row
                    ExportWorksheet.Cell(currentRow, 1).Value = index.ToString();
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
                    ExportWorksheet.Cell(currentRow, currentColumn++).Value = list.Trend == null ? "" : list.BigOffset;

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
    }
}