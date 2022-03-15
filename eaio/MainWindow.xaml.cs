using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace eaio
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

        private void closeApp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.25));
                doubleAnimation.Completed += (s, _) => this.Close();
                this.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void minimizeApp(object sender, MouseButtonEventArgs e)
        {

            try
            {
                this.WindowState = WindowState.Minimized;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        public void DownloadStateChange(string state)
        {

            DownloadProgress.Dispatcher.BeginInvoke((Action)(() => DownloadProgress.Text = $"{state}"));

        }

        private void DownloadProgress_Loaded(object sender, RoutedEventArgs e)
        {
            string Original_text = "Made by @amexif on Instagram";
            DownloadStateChange(Original_text);

            Thread loader = new Thread(() =>
            {
                DownloadStateChange("Checking assets...");
                CheckForDependencies();
                Thread.Sleep(1000);
                DownloadStateChange("Loading assets...");
                Thread.Sleep(200);
                DownloadStateChange(Original_text);
            });
            loader.Start();
        }
        public void CheckForDependencies()
        {
            string program_files_path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string FFmpeg_path = $"{program_files_path}\\EAIO\\FFmpeg";
            string rife_path = $"{program_files_path}\\EAIO\\Rife";
            string Real_CUGAN_path = $"{program_files_path}\\EAIO\\Real-CUGAN";

            bool FFmpegInstalled = Directory.Exists(FFmpeg_path);
            bool RifeInstalled = Directory.Exists(rife_path);
            bool CUGANInstalled = Directory.Exists(Real_CUGAN_path);

            string InstalledDepends = null;
            bool MissedDepends = false;

            if (FFmpegInstalled)
            {
                Debug.WriteLine("FFmpeg found!");
            }
            if (RifeInstalled)
            {
                Debug.WriteLine("Rife found!");
            }
            if (CUGANInstalled)
            {
                Debug.WriteLine("Real-CUGAN found!");
            }
            if (FFmpegInstalled == false)
            {
                Debug.WriteLine("FFmpeg not found!");
                InstalledDepends = "FFmpeg ";
                MissedDepends = true;
            }
            if (RifeInstalled == false)
            {
                Debug.WriteLine("Rife not found!");
                InstalledDepends = InstalledDepends + "Rife ";
                MissedDepends = true;
            }
            if (CUGANInstalled == false)
            {
                Debug.WriteLine("Real-CUGAN not found!");
                InstalledDepends = InstalledDepends + "CUGAN ";
                MissedDepends = true;
            }
            if (MissedDepends)
            {
                DownloadStateChange("Loading failed...");
                MessageBox.Show($"One or more dependencies are not installed! \nThey will automatically install after you press OK, some features may not work during this time. \n\nMissing components : {InstalledDepends}:", "Dependencies not found!");
                DownloadStateChange("Downloading assets...");
                Download_Components();
            }
        }
        public void Download_Components()
        {
            Thread.Sleep(5000);
        }

    }
}
