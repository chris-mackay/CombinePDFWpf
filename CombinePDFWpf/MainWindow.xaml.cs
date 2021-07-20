using System.Collections.Generic;
using System.IO;
using System.Windows;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Controls;
using System.Diagnostics;

namespace CombinePDFWpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Events

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

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();

            ofd.Title = "Select a file to add";
            ofd.Filter = "PDF files (*.pdf)|*.pdf";
            ofd.InitialDirectory = XMLSettings.GetSettingsValue("DefaultDirectory");
            bool? result = ofd.ShowDialog();

            if ((bool)result)
            {
                string file = ofd.FileName;

                if (!lstFiles.Items.Contains(file))
                {
                    lstFiles.Items.Add(ofd.FileName);
                }
                else
                {
                    TaskDialog tdFileExists = new TaskDialog();
                    tdFileExists.StartupLocation = TaskDialogStartupLocation.CenterOwner;
                    tdFileExists.Caption = "Combine PDF";
                    tdFileExists.Icon = TaskDialogStandardIcon.Warning;
                    tdFileExists.StandardButtons = TaskDialogStandardButtons.Ok;
                    tdFileExists.InstructionText = "File already exists in this list";
                    tdFileExists.FooterText = file;

                    tdFileExists.Show();
                }
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedItems.Count > 0)
            {
                string selectedFile = lstFiles.SelectedItem.ToString();
                int index = lstFiles.SelectedIndex;

                TaskDialog td = new TaskDialog();
                td.StartupLocation = TaskDialogStartupLocation.CenterOwner;
                td.Caption = "Combine PDF";
                td.StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No;
                td.InstructionText = "Are you sure you want to remove the selected file from the list?";
                td.Text = "This will not delete the actual file";
                td.FooterText = selectedFile;

                if (td.Show() == TaskDialogResult.Yes)
                {
                    lstFiles.Items.RemoveAt(index);
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedItems.Count > 0)
            {
                string selectedFile = lstFiles.SelectedItem.ToString();
                int index = lstFiles.SelectedIndex;

                TaskDialog td = new TaskDialog();
                td.StartupLocation = TaskDialogStartupLocation.CenterOwner;
                td.Caption = "Combine PDF";
                td.Icon = TaskDialogStandardIcon.Warning;
                td.StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No;
                td.InstructionText = "Are you sure you want to delete the selected file?";
                td.Text = "THE FILE WILL BE PERMANENTLY DELETED FROM YOUR COMPUTER";
                td.FooterText = selectedFile;

                if (td.Show() == TaskDialogResult.Yes)
                {
                    File.Delete(selectedFile);
                    lstFiles.Items.RemoveAt(index);
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
                TaskDialog tdSpecifyDirectory = new TaskDialog();
                tdSpecifyDirectory.StartupLocation = TaskDialogStartupLocation.CenterOwner;
                tdSpecifyDirectory.Caption = "Combine PDF";
                tdSpecifyDirectory.Icon = TaskDialogStandardIcon.Information;
                tdSpecifyDirectory.StandardButtons = TaskDialogStandardButtons.Ok;
                tdSpecifyDirectory.InstructionText = "No directory specified";
                tdSpecifyDirectory.Text = "Click Browse to specify a directory before refreshing";

                tdSpecifyDirectory.Show();
            }
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
                TaskDialog tdConfirm = new TaskDialog();
                tdConfirm.StartupLocation = TaskDialogStartupLocation.CenterOwner;
                tdConfirm.Caption = "Combine PDF";
                tdConfirm.StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No;
                tdConfirm.InstructionText = "Are you sure you want to combine the files?";

                if (tdConfirm.Show() == TaskDialogResult.Yes)
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
                    input.Owner = Application.Current.MainWindow;
                    input.lblPrompt.Content = "Combined file name";
                    input.Title = "Combine PDF";
                    bool? result = input.ShowDialog();

                    if ((bool)result)
                    {
                        string name = input.txtInput.Text + ".pdf";
                        string filename = System.IO.Path.Combine(dir, name);

                        TaskDialog tdOpen = new TaskDialog();
                        tdOpen.StartupLocation = TaskDialogStartupLocation.CenterOwner;
                        tdOpen.Caption = "Combine PDF";
                        tdOpen.Icon = TaskDialogStandardIcon.Information;
                        tdOpen.StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No;
                        tdOpen.InstructionText = "Files have been combined successfully";
                        tdOpen.Text = "Would you like the open the combined file now?";
                        tdOpen.FooterText = filename;

                        if (!File.Exists(filename))
                        {
                            outputDocument.Save(filename);

                            if (tdOpen.Show() == TaskDialogResult.Yes)
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

                                if (tdOpen.Show() == TaskDialogResult.Yes)
                                {
                                    Process.Start(filename);
                                }
                            }
                            else
                            {
                                TaskDialog tdFileExists = new TaskDialog();
                                tdFileExists.StartupLocation = TaskDialogStartupLocation.CenterOwner;
                                tdFileExists.Caption = "Combine PDF";
                                tdFileExists.Icon = TaskDialogStandardIcon.Warning;
                                tdFileExists.StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No;
                                tdFileExists.InstructionText = "File already exists in this location";
                                tdFileExists.Text = "Overwrite file?";
                                tdFileExists.FooterText = filename;
                                tdFileExists.FooterCheckBoxText = "Always Overwrite?";
                                tdFileExists.FooterCheckBoxChecked = false;

                                if (tdFileExists.Show() == TaskDialogResult.Yes)
                                {
                                    if (tdFileExists.FooterCheckBoxChecked.Value)
                                    {
                                        XMLSettings.SetSettingsValue("AlwaysOverwrite", "true");
                                    }
                                    else
                                    {
                                        XMLSettings.SetSettingsValue("AlwaysOverwrite", "false");
                                    }

                                    File.Delete(filename);
                                    outputDocument.Save(filename);

                                    if (tdOpen.Show() == TaskDialogResult.Yes)
                                    {
                                        Process.Start(filename);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                TaskDialog tdAddFiles = new TaskDialog();
                tdAddFiles.StartupLocation = TaskDialogStartupLocation.CenterOwner;
                tdAddFiles.Caption = "Combine PDF";
                tdAddFiles.Icon = TaskDialogStandardIcon.Warning;
                tdAddFiles.StandardButtons = TaskDialogStandardButtons.Ok;
                tdAddFiles.InstructionText = "At least two files must be provided before combining";
                tdAddFiles.Text = "Click Add File to add more files";

                tdAddFiles.Show();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            frmSettings settings = new frmSettings();
            settings.Owner = Application.Current.MainWindow;
            bool? result = settings.ShowDialog();

            if((bool)result)
                txtDirectory.Text = settings.txtDirectory.Text;

        }

        #endregion

        #region HelperMethods

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

        #endregion

    }
}
