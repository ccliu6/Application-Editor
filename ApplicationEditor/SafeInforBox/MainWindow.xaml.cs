using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace SafeInforBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string strPath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadCypher_Click(object sender, RoutedEventArgs e)
        {
            var ofdlg = new OpenFileDialog();
            ofdlg.DefaultExt = "*.bin";
            if (ofdlg.ShowDialog() == true)
            {
                strPath = ofdlg.FileName;

                string cipherText = File.ReadAllText(strPath);

                TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd)
                {
                    Text = AesOperation.DecryptString(cipherText)
                };
            }
        }

        private void SaveCypher_Click(object sender, RoutedEventArgs e)
        {
            var sf = new SaveFileDialog();
            sf.FileName = strPath;
            sf.DefaultExt = "*.bin";

            if(sf.ShowDialog() == true)
            {
                TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                string cipherText = AesOperation.EncryptString(range.Text);
                File.WriteAllText(sf.FileName, cipherText);
            }
        }
    }
}
