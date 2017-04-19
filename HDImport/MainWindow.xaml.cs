using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;

namespace HDImport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HDSettings settings;

        private const string TOPLINE = "Service_Type,Billing_Option,Customer_ID,Reference_1,Ship_Name_1,Ship_Name_2,"
           + "Ship_Address_1,Ship_City,Ship_State,Ship_Zip,Ship_Country,Ship_Phone,Residential,"
           + "From_Name_1,From_Address_1,From_Country,From_Zip,From_City,From_State,Package_Type,"
           + "Weight,Length,Width,Height,Oversize,Reference_2,Bill_Name_1,Bill_Address_1,"
           + "Bill_Country,Bill_Zip,Bill_City,Bill_State,Account";

        //Private information censored
        private const string SERVICE_TYPE = "Ground";
        private const string BILLING_OPTION = "Bill To Third Party";
        private const string CUSTOMER_ID = "";
        private const string RESIDENTIAL = "";
        private const string FROM_NAME = "HD Fulfilment";
        private const string FROM_ADDRESS = "*";
        private const string FROM_COUNTRY = "*";
        private const string FROM_ZIP = "*";
        private const string FROM_CITY = "*";
        private const string FROM_STATE = "*";
        private const string PACKAGE_TYPE = "Package";
        private const string BILLING_NAME = "*";
        private const string BILLING_ADDRESS = "*";
        private const string BILLING_COUNTRY = "*";
        private const string BILLING_ZIP = "*";
        private const string BILLING_CITY = "*";
        private const string BILLING_STATE = "*";
        private const string ACCOUNT = "*";

        /// <summary>
        /// Saves the settings variable to file
        /// </summary>
        private void saveSettings()
        {
            try
            {
                System.IO.Stream file = File.OpenWrite("hdimport.settings");

                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(file, settings);
                file.Flush();
                file.Close();
                file.Dispose();
            }
            catch(Exception e)
            {
                MessageBox.Show("Unable to save models and job number. All settings will be lost.", "Write failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads the setting variable from file
        /// </summary>
        private void loadSettings()
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                FileStream file = File.Open("hdimport.settings", FileMode.Open);

                object settings = formatter.Deserialize(file);
                file.Flush();
                file.Close();
                file.Dispose();

                this.settings = (HDSettings)settings;
            }
            catch(Exception e)
            {
                MessageBox.Show("Unable to find settings file. Ensure you set the job number and build a model list!", "No settings found", MessageBoxButton.OK, MessageBoxImage.Warning);
                settings = new HDSettings();
            }
        }

        /// <summary>
        /// Parses the list of strings and determines which models it contains. If fails will close program.
        /// </summary>
        /// <param name="orderItems">List of model items</param>
        /// <returns>List of models contained</returns>
        private List<Model> modelContained(List<string> orderItems)
        {

            //TODO: Refactor this garbage...
            var modelList = new List<Model>();
            while (orderItems.Count != 0)
            {
                int tempLength = orderItems.Count;
                foreach (var m in settings.models)
                {
                    while(m.contains(orderItems))
                    {
                        modelList.Add(m);
                        foreach (var i in m.getItems())
                        {
                            orderItems.RemoveAt(orderItems.IndexOf(i));
                        }
                    }
                }
                // If the size didn't change then there was no match
                if (tempLength == orderItems.Count)
                {
                    MessageBox.Show("Failed to find a model. Failed to build list", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }
            }

            return modelList;
        }

        /// <summary>
        /// Process the files selected and output csv file for import to FedEx
        /// </summary>
        private void process()
        {
            try
            {
                if(settings.models.Count == 0 || settings.jobCode == null || settings.jobCode.Replace(" ", "") == "")
                {
                    MessageBox.Show("Missing required information. Ensure that the models list contains entries and a job code is set.",
                        "Missing Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.status.Content = "Failed";
                    return;
                }
                List<string> missingInfoOrders = new List<string>();

                if (importLine.Content.Equals("HDImport Output") || importLine.Content.Equals(""))
                {
                    importLine.Content = System.IO.Directory.GetCurrentDirectory() + "\\HDImport.csv";
                }

                string[] header = System.IO.File.ReadAllLines(@headerLine.Content.ToString());
                string[] item = System.IO.File.ReadAllLines(@itemLine.Content.ToString());
                var headers = new List<string[]>();
                var items = new List<string[]>();

                foreach (var s in header)
                {
                    string[] temp = s.Split('|');
                    string[] newstring = new string[9];
                    newstring[0] = temp[0].Remove(0, 2);        // Reference 1 Required
                    newstring[1] = temp[26];                    // Ship Name 1 Required
                    newstring[2] = temp[27];                    // Ship Name 2 Optional
                    newstring[3] = temp[29];                    // Ship Address Required
                    newstring[4] = temp[30];                    // Ship City Required
                    newstring[5] = temp[31];                    // Ship State Required
                    newstring[6] = temp[32];                    // Ship Zip Required
                    newstring[7] = temp[34];                    // Ship Country Required
                    newstring[8] = temp[35].Replace("-", "");    // Ship Phone Optional

                    // Check sanity
                    if ((new List<string> { newstring[0], newstring[1], newstring[3], newstring[4], newstring[5], newstring[6], newstring[7] }).Contains("", StringComparer.OrdinalIgnoreCase))
                    {
                        if (MessageBoxResult.No == MessageBox.Show("Order " + newstring[0] + " is missing crucial address information. Would you like to continue? "
                            + "If so you will need to manually remove or update this order.", "Missing address information", MessageBoxButton.YesNo, MessageBoxImage.Exclamation))
                        {
                            this.status.Content = "Cancelled";
                            return;
                        }
                        else
                        {
                            missingInfoOrders.Add(newstring[0]);
                        }
                    }
                    headers.Add(newstring);
                }

                foreach (var s in item)
                {
                    string[] temp = s.Split('|');
                    string[] newstring = new string[3];
                    newstring[0] = temp[0].Remove(0, 2);
                    newstring[1] = temp[2];
                    newstring[2] = temp[4].Replace(" ", "");
                    items.Add(newstring);
                }

                var orders = new List<string>();
                var orderItems = new List<List<string>>();

                foreach (string[] o in items)
                {
                    var len = Convert.ToInt32(o[2]);
                    if (orders.Contains(o[0]))
                    {
                        while (len > 0)
                        {
                            orderItems[orders.IndexOf(o[0])].Add(o[1]);
                            len--;
                        }
                    }
                    else
                    {
                        orders.Add(o[0]);
                        var tempI = new List<string>();
                        tempI.Add(o[1]);
                        orderItems.Add(tempI);
                        len--;

                        while (len > 0)
                        {
                            orderItems[orders.IndexOf(o[0])].Add(o[1]);
                            len--;
                        }
                    }
                }

                // Build HDImport file
                string currentLine = "";
                var writeLines = new List<string>();

                foreach (var h in headers)
                {
                    var mList = modelContained(orderItems[orders.IndexOf(h[0])]);
                    foreach (var m in mList)
                    {
                        currentLine += SERVICE_TYPE + ",";
                        currentLine += BILLING_OPTION + ",";
                        currentLine += CUSTOMER_ID + ",";

                        for (int i = 0; i < 9; i++)
                        {
                            if (h[i].Contains(','))
                            {
                                currentLine += "\"" + h[i] + "\",";
                            }
                            else
                            {
                                currentLine += h[i] + ",";
                            }
                        }

                        currentLine += RESIDENTIAL + ",";
                        currentLine += FROM_NAME + ",";
                        currentLine += FROM_ADDRESS + ",";
                        currentLine += FROM_COUNTRY + ",";
                        currentLine += FROM_ZIP + ",";
                        currentLine += FROM_CITY + ",";
                        currentLine += FROM_STATE + ",";
                        currentLine += PACKAGE_TYPE + ",";
                        var mLine = m.getItemLine(settings.jobCode);

                        currentLine += mLine[0] + ",";
                        currentLine += mLine[1] + ",";
                        currentLine += mLine[2] + ",";
                        currentLine += mLine[3] + ",";
                        currentLine += mLine[4] + ",";
                        currentLine += mLine[5] + ",";

                        currentLine += BILLING_NAME + ",";
                        currentLine += BILLING_ADDRESS + ",";
                        currentLine += BILLING_COUNTRY + ",";
                        currentLine += BILLING_ZIP + ",";
                        currentLine += BILLING_CITY + ",";
                        currentLine += BILLING_STATE + ",";
                        currentLine += ACCOUNT;

                        writeLines.Add(currentLine);
                        currentLine = "";
                    }
                }
                try
                {
                    // Seperate into a list of lists, sort, then combine back into csv strings
                    List<List<string>> temp = new List<List<string>>();
                    foreach (var w in writeLines)
                    {
                        temp.Add(w.Split(',').ToList());
                    }

                    temp = temp.OrderByDescending(l => l[23]).ThenBy(l => l[3]).ToList();
                    writeLines = new List<string>();

                    foreach (var t in temp)
                    {
                        string line = "";
                        foreach (var l in t)
                        {
                            line += l + ",";
                        }
                        writeLines.Add(line);
                    }

                    writeLines.Insert(0, TOPLINE);
                    System.IO.File.WriteAllLines(@importLine.Content.ToString(), writeLines.ToArray());

                    if(missingInfoOrders.Count > 0)
                    {
                        string error = "The following orders were created with missing address information:\n";
                        foreach(var s in missingInfoOrders)
                        {
                            error += s + "\n";
                        }
                        MessageBox.Show(error, "Missing address information", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (IOException e)
                {
                    MessageBox.Show("Unable to open file\n" + importLine.Content.ToString() + ".\nIt may be open or missing. Please try again.",
                        "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.status.Content = "Failed";
                }
                this.status.Content = "Done";
            }
            catch (IndexOutOfRangeException e)
            {
                MessageBox.Show("An error has occured. The input file is likely improperly formatted. Try again with different input.",
                    "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.status.Content = "Failed";
            }
        }

        public MainWindow()
        {
            loadSettings();
            InitializeComponent();
        }
        public void headerClick(object o, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.ShowDialog();
            headerLine.Content = dialog.FileName;
            this.status.Content = "";
        }

        public void itemClick(object o, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.ShowDialog();
            itemLine.Content = dialog.FileName;
            this.status.Content = "";
        }

        public void outputClick(object o, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.ShowDialog();
            importLine.Content = dialog.FileName;
            this.status.Content = "";
        }

        public void runClick(object o, RoutedEventArgs e)
        {
            if (headerLine.Content.Equals("Header File") || itemLine.Content.Equals("Item File"))
            {
                MessageBox.Show("Header and item files not selected");
            }
            else
            {
                if (importLine.Content.Equals("HDImport Output") || importLine.Content.Equals(""))
                {
                    if (MessageBox.Show("No header import file selected. \nDo you want to continue? HDImport will be saved at\n" + System.IO.Directory.GetCurrentDirectory() + "\\HDImport.csv", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                process();
            }
        }

        public void editModels(object o, RoutedEventArgs e)
        {
            EditModels editModels = new EditModels(this.settings.models);
            editModels.ShowDialog();
            List<Model> modelChanges = editModels.models;
            settings.models = modelChanges;
        }

        public void jobNumber(object o, RoutedEventArgs e)
        {
            JobNumber job = new JobNumber(settings.jobCode);
            job.ShowDialog();
            if(job.saved)
            {
                settings.jobCode = job.job;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            saveSettings();
        }
    }
}
