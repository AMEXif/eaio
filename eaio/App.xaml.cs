using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.IO;
using eaio.Core;
using eaio;

namespace eaio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            Thread thread = new Thread(() =>
            {
                Debug.WriteLine("Started");
                CheckForDependencies();
            });
            thread.Start();
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
                InstalledDepends = "FFmpeg ";
            }
            if (RifeInstalled)
            {
                Debug.WriteLine("Rife found!");
                InstalledDepends = InstalledDepends + "Rife ";
            }
            if (CUGANInstalled)
            {
                Debug.WriteLine("Real-CUGAN found!");
                InstalledDepends = InstalledDepends + "CUGAN";
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
                InstalledDepends = InstalledDepends + "CUGAN";
                MissedDepends = true;
            }
            if (MissedDepends)
            {
                MessageBox.Show("One or more of the following dependencies are not installed: FFmpeg, Rife and Real-CUGAN. \n\nThey will automatically install right now, some features may not work during this time.", "Dependencies not found!");
            }
        }
    }
}
