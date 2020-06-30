using DataCore;
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
            appMgr = new ApplicationDataMgr();
            appMgr.ReadFiles(@"C:\Spm1\Application Modes\ApplicationModes.txt");

            InitializeComponent();

            BuildInList.ItemsSource = appMgr.Apps_BuildIn.Keys;
            CustomList.ItemsSource = appMgr.Apps_Custom.Keys;

            BuildInList.SelectedIndex = 0;

            DataContext = appMgr;
        }

        private async void App_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.Items.Count > 0)
            {
                AppDataGrid.ItemsSource = null;
                AppDataGrid.ItemsSource = appMgr.GetEditList(listBox.SelectedItem.ToString(), listBox == BuildInList);

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Button_Click_C(object sender, RoutedEventArgs e)
        {
            //pop-up a dialog to get the new name

            string newName = "NewApp2";

            // Check the duplication
            foreach(var key in appMgr.Apps_Custom.Keys)
            {
                if (key == newName)
                {
                    MessageBox.Show($"The name '{newName}' already exists in the custom list!");
                    return;
                }
            }


            appMgr.CommandClone.Execute(newName);

            await Task.Delay(500);

            CustomList.ItemsSource = null;
            CustomList.ItemsSource = appMgr.Apps_Custom.Keys;

            await Task.Delay(500);

            //switch to the new added at the end
            CustomList.SelectedIndex = appMgr.Apps_Custom.Count - 1;

        }
    }
}
