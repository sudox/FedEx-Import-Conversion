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
using System.IO;

namespace HDImport
{
    /// <summary>
    /// Interaction logic for EditModels.xaml
    /// </summary>
    public partial class EditModels : Window
    {
        public List<Model> models;
        public EditModels()
        {
            InitializeComponent();
            Okay.IsEnabled = false;
            Cancel.IsEnabled = false;
        }

        public EditModels(List<Model> m)
        {
            InitializeComponent();
            if (m != null)
            {
                foreach (var i in m)
                {
                    this.ModelListBox.Items.Add(i);
                }
            }
            Okay.IsEnabled = false;
            Cancel.IsEnabled = false;
        }
        private void clearValues()
        {
            Code.Text = "";
            Weight.Text = "";
            Length.Text = "";
            Width.Text = "";
            Height.Text = "";
            Items.Text = "";
        }

        private void addClick(object sender, RoutedEventArgs e)
        {
            Okay.IsEnabled = true;
            Cancel.IsEnabled = true;
            clearValues();
            ModelListBox.SelectedItem = null;
            ModelListBox.IsEnabled = false;
            AddButon.IsEnabled = false;
            EditButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
        }

        private void editClick(object sender, RoutedEventArgs e)
        {
            if (ModelListBox.SelectedItem == null)
            {
                MessageBox.Show("Select an item first");
            }
            else
            {
                clearValues();
                ModelListBox.IsEnabled = false;
                Okay.IsEnabled = true;
                Cancel.IsEnabled = true;
                AddButon.IsEnabled = false;
                EditButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;

                Model m = (Model)ModelListBox.SelectedItem;
                Code.Text = m.ToString();
                Weight.Text = m.getWeight();
                Length.Text = m.getLength();
                Width.Text = m.getWidth();
                Height.Text = m.getHeight();
                if (m.getOversize().Equals("Y"))
                {
                    Oversize.IsChecked = true;
                }

                foreach (var i in m.getItems())
                {
                    Items.Text += i + "\n";
                }
            }
        }

        private void deleteClick(object sender, RoutedEventArgs e)
        {
            // If there is nothing selected check to see if they want to clear the list, otherwise double check to confirm delete
            if (ModelListBox.SelectedIndex == -1)
            {
                if (MessageBoxResult.Yes == MessageBox.Show("Are you sure you want to delete ALL items?", "Delete All?", MessageBoxButton.YesNo))
                {
                    for (int i = ModelListBox.Items.Count - 1; i >= 0; i--)
                    {
                        ModelListBox.Items.RemoveAt(i);
                    }
                }
            }
            else
            {
                if (MessageBoxResult.Yes == MessageBox.Show("Are you sure you would like to delete " + ModelListBox.SelectedItem.ToString() + "?",
                    "Delete Model?", MessageBoxButton.YesNo))
                {
                    ModelListBox.Items.RemoveAt(ModelListBox.SelectedIndex);
                }
            }
        }

        private void okayClick(object sender, RoutedEventArgs e)
        {
            if (Code.Text.Equals("") || Weight.Text.Equals("") || Length.Text.Equals("") || Width.Text.Equals("") || Height.Text.Equals("") || Items.Text.Equals(""))
            {
                MessageBox.Show("Please enter a value in all fields");
            }

            else
            {
                Items.Text = Items.Text.Replace("\r", "");
                List<string> tempList = Items.Text.Split('\n').ToList<string>();
                List<string> list = new List<string>();

                foreach (var s in tempList)
                {
                    list.Add(s.ToUpper());
                }

                for (int i = 0; i < list.Count; i++)
                {
                    list.Remove("");
                }

                Model m = new Model(Code.Text.ToUpper(), list, Weight.Text, Length.Text, Width.Text, Height.Text, (bool)Oversize.IsChecked);
                if (ModelListBox.SelectedItem != null)
                {
                    ModelListBox.Items.Remove(ModelListBox.SelectedItem);
                }

                ModelListBox.Items.Add(m);

                clearValues();
                Okay.IsEnabled = false;
                Cancel.IsEnabled = false;
                AddButon.IsEnabled = true;
                EditButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                ModelListBox.IsEnabled = true;
            }
        }

        private void cancelClick(object sender, RoutedEventArgs e)
        {
            clearValues();
            ModelListBox.IsEnabled = true;
            AddButon.IsEnabled = true;
            EditButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;
            Okay.IsEnabled = false;
            Cancel.IsEnabled = false;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            List<Model> list = new List<Model>();
            foreach (Model m in ModelListBox.Items)
            {
                list.Add(m);
            }
            models = list;
        }
    }
}
