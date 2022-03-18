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
            string program_files_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string EAIOPath = $"{program_files_path}\\EAIO";
            bool EAIOInstalled = Directory.Exists(EAIOPath);

            if (EAIOInstalled == false)
            {
                Directory.CreateDirectory(EAIOPath);
                Debug.WriteLine($"EAIO Folder Created at {EAIOPath}");
            }
            else
            {
                Debug.WriteLine("EAIO exists");
            }

        }
    }
}
