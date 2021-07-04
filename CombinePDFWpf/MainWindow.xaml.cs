using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CombinePDFWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            string def = XMLSettings.GetSettingsValue("DefaultDirectory");

            if (def == "")
            {
                dialog.InitialDirectory = "C:\\";
            }
            else
            {
                dialog.InitialDirectory = def;
            }

            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string dir = dialog.FileName;
                txtDirectory.Text = dir;

                lstFiles.Items.Clear();

                string[] files = Files(dir);
                foreach (string file in files)
                {
                    lstFiles.Items.Add(file);
                }
            }
        }

        private string[] Files(string dir)
        {
            string[] fInfos = Directory.GetFiles(dir, "*.pdf", SearchOption.TopDirectoryOnly);

            return fInfos;
        }

        private bool DrawingDirectoryIsDefault(string dir)
        {
            bool flag = false;
            dir = txtDirectory.Text;

            string savedDir = XMLSettings.GetSettingsValue("DefaultDirectory");

            if (dir != string.Empty && System.IO.Directory.Exists(dir))
            {
                if (dir == savedDir)
                    flag = true;
                else
                    flag = false;
            }

            return flag;
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();

            settings.Add("DefaultDirectory", "");
            settings.Add("AlwaysOverwrite", "False");

            XMLSettings.AppSettingsFile = "Settings.xml";
            XMLSettings.InitializeSettings(settings);

            string dir = XMLSettings.GetSettingsValue("DefaultDirectory");
            txtDirectory.Text = dir;
            txtDirectory.Select(dir.Length + 1, 0);

            bool isChecked = DrawingDirectoryIsDefault(dir);
            ckbDefault.IsChecked = isChecked;

            if (isChecked)
            {
                lstFiles.Items.Clear();

                string[] files = Files(dir);
                foreach (string file in files)
                {
                    lstFiles.Items.Add(file);
                }
            }
        }
    }
}
