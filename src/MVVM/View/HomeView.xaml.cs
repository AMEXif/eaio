using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace eaio.MVVM.View
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        const string github = "https://github.com/AMEXif/eaio";
        const string discord = "https://discord.gg/ppeFYx9ke3";
        const string duong = "https://discord.io/imduong2k6";
        const string carrd = "https://amexif.carrd.co/";
        const string clips = "https://www.animeclips.online/";
        const string plugins = "https://drive.google.com/drive/folders/1ncEM7yFtJduFSY0bVRk4TzqzZX1K_eGT?usp=sharing";

        public void openDiscord(object sender, MouseButtonEventArgs e)
        {

            string InvLink = discord;
            string command = $"/C start {InvLink}";

            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = @"C:\";
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.Arguments = command;
            process.Start();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void openGithub(object sender, MouseButtonEventArgs e)
        {
            string InvLink = github;
            string command = $"/C start {InvLink}";

            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = @"C:\";
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.Arguments = command;
            process.Start();
        }

        private void openCarrd(object sender, MouseButtonEventArgs e)
        {
            string InvLink = carrd;
            string command = $"/C start {InvLink}";

            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = @"C:\";
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.Arguments = command;
            process.Start();
        }

        public void openPlugins(object sender, MouseButtonEventArgs e)
        {

            string Link = plugins;
            string command = $"/C start {Link}";

            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = @"C:\";
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.Arguments = command;
            process.Start();
        }

        public void openClips(object sender, MouseButtonEventArgs e)
        {

            string Link = clips;
            string command = $"/C start {Link}";

            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = @"C:\";
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.Arguments = command;
            process.Start();
        }

        public void openDuong(object sender, MouseButtonEventArgs e)
        {

            string Link = duong;
            string command = $"/C start {Link}";

            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = @"C:\";
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.Arguments = command;
            process.Start();
        }

    }
}

