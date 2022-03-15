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

    public void openDiscord(object sender, MouseButtonEventArgs e)
        {

            string InvLink = "https://discord.gg/qnHAxyxcV5";
            string command = $"/C start {InvLink}";

            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = @"C:\";
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.Arguments = command;
            process.Start();
        }
    }
}

