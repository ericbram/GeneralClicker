using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.IO;

namespace GeneralClicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isActive = false;
        Thread oThread;
        private static List<Point> clicks;

        public MainWindow()
        {
            InitializeComponent();
            clicks = new List<Point>();
            tbPath.Text = Properties.Settings.Default.path;
            AddToDropdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ActionThread at = new ActionThread(tbPath.Text, cbGBG.IsChecked ?? false, txtGBGPosition.Text, tbGBGRepeat.Text);
            oThread = new Thread(new ThreadStart(at.MainRun));
            oThread.IsBackground = true;
            oThread.Start();
        }

        private void AddToDropdown()
        {
            cbAids.Items.Clear();
            cbAids.Visibility = Visibility.Hidden;
            if (!string.IsNullOrEmpty(tbPath.Text))
            {
                string path = System.IO.Path.GetDirectoryName(tbPath.Text);
                if (Directory.Exists(path))
                {
                    List<string> files = Directory.GetFiles(path, "*.txt").ToList();
                    foreach (string s in files)
                    {
                        cbAids.Items.Add(System.IO.Path.GetFileName(s));
                    }
                }
            }
            cbAids.Visibility = cbAids.Items.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void CbAids_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbPath.Text))
            {
                string path = System.IO.Path.GetDirectoryName(tbPath.Text);
                if (Directory.Exists(path))
                {
                    string fullPath = System.IO.Path.Combine(path, cbAids.SelectedItem.ToString());
                    if (File.Exists(fullPath))
                    {
                        tbPath.Text = fullPath;
                        SaveFileName(fullPath);
                    }
                }
            }
        }

        public static Point GetMousePositionWindowsForms()
        {
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
            return new Point(point.X, point.Y);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                oThread.Abort();
            }
            catch{};
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog { Multiselect = false };
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                SaveFileName(filename);
                tbPath.Text = filename;
                AddToDropdown();
            }
        }

        private void SaveFileName(string filename)
        {
            Properties.Settings.Default.path = filename;
            Properties.Settings.Default.Save();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            isActive = true;
            clicks = new List<Point>();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.G && isActive)
            {
                isActive = false;
                List<string> output = new List<string>();
                foreach (Point p in clicks)
                {
                    if (p.X == -10000 && p.Y == -10000)
                    {
                        output.Add($"DELAY 1500{Environment.NewLine}");
                    }
                    else
                    {
                        output.Add($"CLICK {p.X},{p.Y}{Environment.NewLine}");
                    }
                }
                NotepadHelper.ShowMessage(String.Concat(output.ToArray()), "Temp Clicks");
                clicks = new List<Point>();
            }
            if (e.Key == Key.P && isActive)
            {
                clicks.Add(GetMousePositionWindowsForms());
            }
            else if (e.Key == Key.O && isActive)
            {
                clicks.Add(new Point(-10000, -10000));
            } 
            else if (e.Key == Key.T && isActive)
            {
                txtGBGPosition.Text = GetMousePositionWindowsForms().ToString();
                isActive = false;
            }

            if (e.Key == Key.Q && oThread != null)
            {
                oThread.Abort();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            isActive = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string messageBoxText = "REPEAT <N> - MUST BE FIRST LINE - Repeats all subsquent commands N times\nCLICK <X,Y> - Click the coordinates X,Y on the screen.\nDELAY <SECONDS> - Delay a number of seconds\nSCRIPT <PATH> - Call another script -- Must be full path to script\nSHUTDOWN <SECONDS> - Shuts down the computer after a specified number of seconds";
            string caption = "Commands";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}