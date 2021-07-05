using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DialogMessage;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;

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

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            string dir = string.Empty;
            dir = txtDirectory.Text;

            if (dir != string.Empty)
            {
                lstFiles.Items.Clear();

                string[] files = Files(dir);

                foreach (string file in files)
                {
                    lstFiles.Items.Add(file);
                }
            }
            else
            {
                DMessage.ShowMessage("Combine PDF", 
                                     "No directory specified", 
                                     DMessage.MsgButtons.OK, 
                                     DMessage.MsgIcons.Info, 
                                     "Click Browse to specify a directory before refreshing");
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedItems.Count > 0)
            {
                int i = lstFiles.SelectedIndex;
                int j = lstFiles.SelectedIndex + 1;

                string itemToMove = lstFiles.Items[i].ToString();

                lstFiles.Items.RemoveAt(i);
                lstFiles.Items.Insert(j, itemToMove);
            }
        }

        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedItems.Count > 0)
            {
                int i = lstFiles.SelectedIndex;
                int j = lstFiles.SelectedIndex - 1;

                string itemToMove = lstFiles.Items[i].ToString();

                lstFiles.Items.RemoveAt(i);
                lstFiles.Items.Insert(j, itemToMove);

            }
        }

        private void btnCombine_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.Items.Count > 1)
            {
                if (DMessage.ShowMessage("Combine PDF",
                     "Are you sure you want to combine the files?",
                     DMessage.MsgButtons.YesNo,
                     DMessage.MsgIcons.None,
                     "") == System.Windows.Forms.DialogResult.Yes)
                {
                    PdfDocument outputDocument = new PdfDocument();
                    string dir = txtDirectory.Text;

                    foreach (string file in lstFiles.Items)
                    {
                        PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                        int count = inputDocument.PageCount;
                        for (int idx = 0; idx < count; idx++)
                        {
                            PdfPage page = inputDocument.Pages[idx];
                            outputDocument.AddPage(page);
                        }
                    }

                    frmInput input = new frmInput();
                    input.lblPrompt.Content = "Combined file name";
                    input.Title = "Combine PDF";
                    input.ShowDialog();

                    if (input.DialogResult.HasValue && input.DialogResult.Value)
                    {
                        string name = input.txtInput.Text + ".pdf";
                        string filename = System.IO.Path.Combine(dir, name);

                        if (!File.Exists(filename))
                        {
                            outputDocument.Save(filename);

                            if (DMessage.ShowMessage("Combine PDF",
                                                     "Files have been combined successfully",
                                                     DMessage.MsgButtons.YesNo,
                                                     DMessage.MsgIcons.Info,
                                                     "Would you like the open the combined file now?") == System.Windows.Forms.DialogResult.Yes)
                            {
                                Process.Start(filename);
                            }
                        }
                        else
                        {
                            bool alwaysOverwrite = bool.Parse(XMLSettings.GetSettingsValue("AlwaysOverwrite"));

                            if (alwaysOverwrite)
                            {
                                File.Delete(filename);
                                outputDocument.Save(filename);

                                if (DMessage.ShowMessage("Combine PDF",
                                                     "Files have been combined successfully",
                                                     DMessage.MsgButtons.YesNo,
                                                     DMessage.MsgIcons.Info,
                                                     "Would you like the open the combined file now?") == System.Windows.Forms.DialogResult.Yes)
                                {
                                    Process.Start(filename);
                                }
                            }
                            else
                            {

                                if (DMessage.ShowMessage("Combine PDF",
                                                     "A file with the provided name already exists in this location",
                                                     DMessage.MsgButtons.YesNo,
                                                     DMessage.MsgIcons.Info,
                                                     "Do you want to overwrite the file?") == System.Windows.Forms.DialogResult.Yes)
                                {

                                    File.Delete(filename);
                                    outputDocument.Save(filename);

                                    if (DMessage.ShowMessage("Combine PDF",
                                                     "Files have been combined successfully",
                                                     DMessage.MsgButtons.YesNo,
                                                     DMessage.MsgIcons.Info,
                                                     "Would you like the open the combined file now?") == System.Windows.Forms.DialogResult.Yes)
                                    {
                                        Process.Start(filename);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedItems.Count > 0)
            {
                string selectedFile = lstFiles.SelectedItem.ToString();
                int index = lstFiles.SelectedIndex;

                if (DMessage.ShowMessage("Combine PDF",
                                         "Are you sure you want to delete the selected file?",
                                         DMessage.MsgButtons.YesNo,
                                         DMessage.MsgIcons.Warning,
                                         "THE FILE WILL BE PERMANENTLY DELETED FROM YOUR COMPUTER\n\n" + selectedFile) == System.Windows.Forms.DialogResult.Yes)
                {
                    File.Delete(selectedFile);
                    lstFiles.Items.RemoveAt(index);
                }
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedItems.Count > 0)
            {
                string selectedFile = lstFiles.SelectedItem.ToString();
                int index = lstFiles.SelectedIndex;

                if (DMessage.ShowMessage("Combine PDF",
                                         "Are you sure you want to remove the selected file from the list?",
                                         DMessage.MsgButtons.YesNo,
                                         DMessage.MsgIcons.None,
                                         "This will not delete the actual file\n\n" + selectedFile) == System.Windows.Forms.DialogResult.Yes)
                {
                    lstFiles.Items.RemoveAt(index);
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Title = "Select a file to add";
            ofd.Filter = "PDF files (*.pdf)|*.pdf";
            ofd.InitialDirectory = XMLSettings.GetSettingsValue("DefaultDirectory");

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = ofd.FileName;

                if (!lstFiles.Items.Contains(file))
                {
                    lstFiles.Items.Add(ofd.FileName);
                }
                else
                {
                    DMessage.ShowMessage("Combine PDF",
                                         "File already exists in this list",
                                         DMessage.MsgButtons.OK,
                                         DMessage.MsgIcons.Warning,
                                         file);
                }
            }
        }

        private void txtDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            string dir = txtDirectory.Text;

            if (!DrawingDirectoryIsDefault(dir))
            {
                ckbDefault.IsChecked = false;
                ckbDefault.IsEnabled = true;
            }
            else
            {
                ckbDefault.IsChecked = true;
                ckbDefault.IsEnabled = false;
            }
        }

        private void ckbDefault_Checked(object sender, RoutedEventArgs e)
        {
            string dir = txtDirectory.Text;
            bool isChecked = ckbDefault.IsChecked.Value;

            if (isChecked)
                if (dir != string.Empty && System.IO.Directory.Exists(dir))
                {
                    XMLSettings.SetSettingsValue("DefaultDirectory", dir);
                    ckbDefault.IsEnabled = false;
                }
        }
    }
}
