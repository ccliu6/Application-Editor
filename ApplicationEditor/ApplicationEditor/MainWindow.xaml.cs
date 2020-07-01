using DataCore;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
            if (sender is ListBox listBox && listBox.SelectedIndex != -1)
            {
                AppDataGrid.ItemsSource = null;
                AppDataGrid.ItemsSource = appMgr.GetEditList(listBox.SelectedItem.ToString());

                await Task.Delay(1000);

                if (!appMgr.IsAdvanced)
                {
                    AppDataGrid.Columns[0].IsReadOnly = true;
                    AppDataGrid.Columns[1].IsReadOnly = true;
                    AppDataGrid.Columns[2].IsReadOnly = true;
                }
            }
            else
                await Task.Delay(10);
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
                //pop-up a dialog with old name to get the new name

                string newName = "-Custom";

                // Check the duplication
                foreach (var key in appMgr.AppsDic.Keys)
                {
                    if (key == newName)
                    {
                        MessageBox.Show($"The name '{newName}' already exists in the custom list!", "Duplicated Name Error");
                        return;
                    }
                }

                appMgr.CommandClone.Execute(newName);

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
            if (appMgr != null)
            {
                int currentIndex = AppList.SelectedIndex;

                if (appMgr.DeleteSelctedApp(currentIndex))
                {
                    await Task.Delay(500);

                    AppList.ItemsSource = null;
                    AppList.ItemsSource = appMgr.AppsDic.Keys;

                    await Task.Delay(500);
                    AppList.SelectedIndex = currentIndex - 1;
                }
                else
                    MessageBox.Show("A Build-In application cannot be deleted.", "Delete Warming");
            }
        }
    }
}
