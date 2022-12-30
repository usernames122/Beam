using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
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
using System.Windows.Threading;
namespace Beam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public void DownFile(Uri url, string outputFilePath)
        {
            try
            {
                const int BUFFER_SIZE = 16 * 1024;
                using (var outputFileStream = File.Create(outputFilePath, BUFFER_SIZE))
                {
                    var req = WebRequest.Create(url);
                    using (var response = req.GetResponse())
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            var buffer = new byte[BUFFER_SIZE];
                            int bytesRead;
                            do
                            {
                                bytesRead = responseStream.Read(buffer, 0, BUFFER_SIZE);
                                outputFileStream.Write(buffer, 0, bytesRead);
                            } while (bytesRead > 0);
                        }
                    }
                }
            } catch (System.Net.WebException ex)
            {

            }
            GC.Collect();
        }
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        public Button? dButton;
        public string CurrentLibrary = "C:\\BeamLibrary\\";
        public int CurrentGameId = 0;
        public int[] Games = new int[65535];
        public void OpenSite(string url)
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).webView.Visibility = Visibility.Visible;
            ((MainWindow)System.Windows.Application.Current.MainWindow).closeButton.Visibility = Visibility.Visible;
            ((MainWindow)System.Windows.Application.Current.MainWindow).webView.Source = new Uri(url);
        }
        public MainWindow()
        {
            AllocConsole();
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            InitializeComponent();
            ((MainWindow)System.Windows.Application.Current.MainWindow).webView.Visibility = Visibility.Hidden;
            ((MainWindow)System.Windows.Application.Current.MainWindow).closeButton.Visibility = Visibility.Hidden;
        }
        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //dButton.Content = e.ProgressPercentage + "%";
        }
        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            Button? someButton = sender as Button;
            if (someButton != null)
            {
                string CurrentLibraryl = CurrentLibrary; // Lock Current library so people can change current library without breaking downloads
                int CurrentGameIdl = CurrentGameId; // Lock Current game so people can change current game without breaking downloads
                bool OwnsGame = false;
                someButton.IsEnabled = false;
                someButton.Content = "Downloading...";
                ((MainWindow)System.Windows.Application.Current.MainWindow).UpdateLayout();
                await Task.Delay(1000);
                if (!OwnsGame)
                {
                    someButton.IsEnabled = true;
                    someButton.Content = "Download";
                    OpenSite("http://steampowered.com");
                    return;
                }
                var url = @"http://localhost:80/BeamGames/7z.php?f=" + CurrentGameIdl + ".7z";
                DownFile(new Uri(url), CurrentLibraryl + CurrentGameIdl + ".7z");

                try
                {
                    var handle = GetConsoleWindow();
                    ShowWindow(handle, SW_SHOW);
                    Process cmd = new Process();
                    cmd.StartInfo.FileName = "cmd.exe";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.CreateNoWindow = false;
                    cmd.StartInfo.UseShellExecute = false;
                    cmd.Start();

                    cmd.StandardInput.WriteLine("set \"sevenzip=%cd%\\7zr.exe\"");
                    cmd.StandardInput.WriteLine("cd /d \"" + CurrentLibraryl + "\"");
                    cmd.StandardInput.WriteLine("mkdir " + CurrentGameIdl);
                    cmd.StandardInput.WriteLine("cd " + CurrentGameIdl);
                    cmd.StandardInput.WriteLine("%sevenzip% e ../" + CurrentGameIdl + ".7z");
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    cmd.WaitForExit();
                    Console.WriteLine(cmd.StandardOutput.ReadToEnd());
                    File.Delete(CurrentLibraryl + CurrentGameIdl + ".7z");
                    GC.Collect();
                    await Task.Delay(4000);
                    ShowWindow(handle, SW_HIDE);
                } catch (System.IO.FileNotFoundException ex)
                {
                    someButton.Content = "Error with unzipping: " + ex.Message;
                    await Task.Delay(4000);
                } catch (System.IO.InvalidDataException ex)
                {
                    someButton.Content = "Error with unzipping: " + ex.Message;
                    await Task.Delay(4000);
                }
                await Task.Delay(1000);
                someButton.Content = "Download";
                ((MainWindow)System.Windows.Application.Current.MainWindow).UpdateLayout();
                someButton.IsEnabled = true;
            }

        }

        private void lstGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox? lb = sender as ListBox;

            if (lb != null)
            {
                ListBoxItem? lbi = lb.SelectedItem as ListBoxItem;

                if (lbi != null)
                {
                    TextBlock tb = (TextBlock)lbi.Content;
                }
            }
        }

        private async void btnAdd_Copy_Click(object sender, RoutedEventArgs e)
        {
            Button? someButton = sender as Button;
            if (someButton != null)
            {
                string CurrentLibraryl = CurrentLibrary; // Lock Current library so people can change current library without breaking downloads
                int CurrentGameIdl = CurrentGameId; // Lock Current game so people can change current game without breaking downloads
                if (!Directory.Exists(CurrentLibraryl + CurrentGameIdl))
                {
                    someButton.Content = "Current game is not downloaded. please download it";
                    await Task.Delay(1000);
                    someButton.Content = "Play";
                    return;
                }
                string[] files = Directory.GetFiles(@CurrentLibraryl + CurrentGameIdl, "*.exe", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        Process.Start(file);
                        //someButton.Content = file;
                    }
            }
        }

        private void CloseSite_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).webView.Visibility = Visibility.Hidden;
            ((MainWindow)System.Windows.Application.Current.MainWindow).closeButton.Visibility = Visibility.Hidden;
        }
    }
}
