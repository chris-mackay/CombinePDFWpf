using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;

namespace CombinePDFWpf
{
    /// <summary>
    /// Interaction logic for frmSettings.xaml
    /// </summary>
    public partial class frmSettings : Window
    {
        public frmSettings()
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
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string dir = XMLSettings.GetSettingsValue("DefaultDirectory");
            txtDirectory.Text = dir;

            bool alwaysOverwrite = bool.Parse(XMLSettings.GetSettingsValue("AlwaysOverwrite"));
            ckbAlwaysOverwrite.IsChecked = alwaysOverwrite;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            string dir = txtDirectory.Text;
            bool alwaysOverwrite = ckbAlwaysOverwrite.IsChecked.Value;

            if (alwaysOverwrite)
                XMLSettings.SetSettingsValue("AlwaysOverwrite", "true");
            else
                XMLSettings.SetSettingsValue("AlwaysOverwrite", "false");

            XMLSettings.SetSettingsValue("DefaultDirectory", dir);
        }

        private void txtDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            string dir = txtDirectory.Text;

            if (Directory.Exists(dir))
                btnSave.IsEnabled = true;
            else if (dir == string.Empty)
                btnSave.IsEnabled = true;
            else
                btnSave.IsEnabled = false;
        }
    }
}
