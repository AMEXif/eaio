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
using SharpCompress.Archives.Rar;
using SharpCompress.Archives;
using SharpCompress.Common.Rar;
using SharpCompress.Common;
using SharpCompress.Readers.Rar;
using SharpCompress.Writers;

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

        WebClient client = new WebClient();

        private void closeApp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                try
                {
                    string program_files_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string thumb_path = $"{program_files_path}\\EAIO\\imgs";
                    bool imgs_exist = Directory.Exists(thumb_path);
                    switch (imgs_exist)
                    {
                        case true:
                            Directory.Delete(thumb_path, true);
                            Directory.CreateDirectory(thumb_path);
                            break;
                        case false:
                            Directory.CreateDirectory(thumb_path);
                            break;
                    }
                }
                catch { }

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
        public void DownloadProgress_Loaded(object sender, RoutedEventArgs e)
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

            Console.WriteLine($"Height: {Content.ActualHeight} Width: {Content.ActualWidth}");
        }

        public void CheckForDependencies()
        {
            string program_files_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string FFmpeg_path = $"{program_files_path}\\EAIO\\FFmpeg";
            string rife_path = $"{program_files_path}\\EAIO\\Rife";
            string Real_CUGAN_path = $"{program_files_path}\\EAIO\\Real-CUGAN";
            string thumb_path = $"{program_files_path}\\EAIO\\imgs";

            string requiredSpace = "98.8 MB";
            decimal leftSpaceC = 0;

            bool FFmpegInstalled = Directory.Exists(FFmpeg_path);
            bool RifeInstalled = Directory.Exists(rife_path);
            bool CUGANInstalled = Directory.Exists(Real_CUGAN_path);
            bool imgs_exist = Directory.Exists(thumb_path);

            string InstalledDepends = null;
            bool MissedDepends = false;

            DriveInfo[] Drives = DriveInfo.GetDrives();

            foreach (DriveInfo d in Drives)
            {
                var DriveName = d.Name;
                Console.WriteLine(DriveName);
                float GB = 1024 * 1024 * 1024;

                if (d.IsReady == true & DriveName == "C:\\")
                {
                    long leftSpace = d.TotalFreeSpace;
                    float Spaceleft = Convert.ToSingle(leftSpace);
                    float leftGigs = Spaceleft / GB;
                    leftSpaceC = Math.Round((decimal)leftGigs, 2);

                    Console.WriteLine($"{DriveName} - {leftSpaceC} GB");
                }
            }

            switch (imgs_exist)
            {
                case true:
                    Directory.Delete(thumb_path, true);
                    Directory.CreateDirectory(thumb_path);
                    break;
                case false:
                    Directory.CreateDirectory(thumb_path);
                    break;
            }


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
                MessageBox.Show($"One or more dependencies are not installed! \nThey will automatically install after you press OK, some features may not work during this time. \n\nRequired space: {requiredSpace}\nSpace left: {leftSpaceC} GB\nMissing components : {InstalledDepends}:", "Dependencies not found!");
                DownloadStateChange("Downloading archives...");
                Download_Components();
            }
        }
        public void Download_Components()
        {
            string program_files_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string FilePath = $"{program_files_path}\\EAIO";

            string ffmUrl = "https://raw.githubusercontent.com/AMEXif/eaio/main/Packages/eaio_FFmpeg.rar";
            string RifeUrl = "https://raw.githubusercontent.com/AMEXif/eaio/main/Packages/eaio_Rife.rar";
            string CUGANUrl = "https://raw.githubusercontent.com/AMEXif/eaio/main/Packages/eaio_Real-CUGAN.rar";

            Uri uri2 = new Uri(CUGANUrl);
            string fileName2 = System.IO.Path.GetFileName(uri2.AbsolutePath);
            client.DownloadFile(uri2, $"{FilePath}\\{fileName2}");
            Debug.WriteLine($"downloaded a file {FilePath}\\{fileName2}");
            
            Uri uri = new Uri(ffmUrl);
            string fileName = System.IO.Path.GetFileName(uri.AbsolutePath);
            client.DownloadFile(uri, $"{FilePath}\\{fileName}");
            Debug.WriteLine($"Downloaded a file {FilePath}\\{fileName}");

            Uri uri1 = new Uri(RifeUrl);
            string fileName1 = System.IO.Path.GetFileName(uri1.AbsolutePath);
            client.DownloadFile(uri1, $"{FilePath}\\{fileName1}");
            Debug.WriteLine($"downloaded a file {FilePath}\\{fileName1}");


            string Path1 = $"{FilePath}\\{fileName}";
            string Path2 = $"{FilePath}\\{fileName1}";
            string Path3 = $"{FilePath}\\{fileName2}";

            Extract_packages(Path1, Path2, Path3);
        }
        public void Extract_packages(string Path1, string Path2, string Path3)
        {
            string program_files_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string FilePath = $"{program_files_path}\\EAIO";
            DownloadStateChange("Extracting archives...");


            using (var archive2 = RarArchive.Open(Path3))
            {
                foreach (var entry in archive2.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(FilePath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
            using (var archive1 = RarArchive.Open(Path2))
            {
                foreach (var entry in archive1.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(FilePath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
            using (var archive = RarArchive.Open(Path1))
            {
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(FilePath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
            Cleanup(Path1, Path2, Path3);
        }
        public void Cleanup(string Path1, string Path2, string Path3)
        {
            DownloadStateChange("Cleaning up...");

            File.Delete(Path1);
            File.Delete(Path2);
            File.Delete(Path3);
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
