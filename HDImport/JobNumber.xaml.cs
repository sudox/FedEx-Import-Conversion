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
using System.Windows.Shapes;

namespace HDImport
{
    /// <summary>
    /// Interaction logic for JobNumber.xaml
    /// </summary>
    public partial class JobNumber : Window
    {
        public bool saved = false;
        public string job;

        public JobNumber()
        {
            InitializeComponent();
        }

        public JobNumber(string job)
        {
            InitializeComponent();
            if(job != null)
                this.jobNumber.Text = job.ToString();
        }

        public void saveClick(object sender, RoutedEventArgs e)
        {
            saved = true;
            job = this.jobNumber.Text;
            this.Close();
        }

        public void cancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
