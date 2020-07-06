using DataCore;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ApplicationEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ApplicationDataMgr appMgr;
        public MainWindow()
        {
            InitializeComponent();

            string path = @"C:\Spm1\Application Modes\ApplicationModes.txt";
            if (File.Exists(path))
            {
                appMgr = new ApplicationDataMgr();

                appMgr.ReadFiles(path);
                AppList.ItemsSource = appMgr.AppsDic.Keys;

                AppList.SelectedIndex = 0;

                DataContext = appMgr;

                appMgr.EnableCells = new System.Action(EnableCells);
            }
            else
                MessageBox.Show($"File '{path}' does not exist!", "Import Error");
        }

        /// <summary>
        /// Handle Selection Change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void App_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppList.SelectedIndex != -1 && sender == AppList)
            {
                AppDataGrid.ItemsSource = null;
                string appName = AppList.SelectedItem as string;
                AppDataGrid.ItemsSource = appMgr.GetEditList(appName);
                Title = "Application Editor -- " + appName;

                await Task.Delay(1000);

                EnableCells();
            }
            else
                await Task.Delay(10);
        }

        private void EnableCells()
        {
            AppDataGrid.IsReadOnly = appMgr.EditMode == EditModes.ReadOnly;
            bool bsRO = appMgr.EditMode == EditModes.ValueEdit || !appMgr.IsAdvanced;
            AppDataGrid.Columns[0].IsReadOnly = bsRO;
            AppDataGrid.Columns[1].IsReadOnly = bsRO;
            AppDataGrid.Columns[2].IsReadOnly = bsRO;
            AppDataGrid.Columns[4].IsReadOnly = bsRO;
        }

        private void Button_Click_Exit(object sender, RoutedEventArgs e)
        {
            if (appMgr != null && MessageBox.Show("Save changes before close?", "Close", MessageBoxButton.YesNo)
                == MessageBoxResult.Yes)
            {
                appMgr.CommandSave.Execute(null);
            }

            this.Close();
        }

        /// <summary>
        /// Clone the selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_C(object sender, RoutedEventArgs e)
        {
            if (appMgr != null)
            {
                NewAppDlg dlg = new NewAppDlg();
                string appName = AppList.SelectedItem as string;
                dlg.textName.Text = appName;
                dlg.Title = $"Clone An Application From the '{appName}'";
                if (!dlg.ShowDialog().Value)
                    return;

                appName = dlg.textName.Text;

                // Check the duplication
                foreach (var key in appMgr.AppsDic.Keys)
                {
                    if (key == appName)
                    {
                        MessageBox.Show($"The name '{appName}' already exists in the custom list!", "Duplicated Name Error");
                        return;
                    }
                }

                appMgr.CommandClone.Execute(appName);

                await Task.Delay(500);

                AppList.ItemsSource = null;
                AppList.ItemsSource = appMgr.AppsDic.Keys;

                await Task.Delay(500);

                //switch to the new added at the end
                AppList.SelectedIndex = appMgr.AppsDic.Count - 1;
            }
        }

        /// <summary>
        /// Delete the selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_Del(object sender, RoutedEventArgs e)
        {
            int currentIndex = AppList.SelectedIndex;
            if (appMgr != null && currentIndex > -1)
            {     
                if (currentIndex < appMgr.nBuildInAppCount)
                    MessageBox.Show("The Build-In application cannot be deleted.", "Delete Limit");
                else
                {
                    if(MessageBox.Show($"Sure to delete the '{AppList.SelectedItem.ToString()}' ?",
                    "Delete Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        appMgr.DeleteSelctedApp();
                        await Task.Delay(500);

                        AppList.ItemsSource = null;
                        AppList.ItemsSource = appMgr.AppsDic.Keys;

                        await Task.Delay(500);
                        AppList.SelectedIndex = currentIndex - 1;
                    }                      
                } 
            }
        }
    }
}
