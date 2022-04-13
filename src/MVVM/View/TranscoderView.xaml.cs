using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;

namespace eaio.MVVM.View
{
    /// <summary>
    /// Interaction logic for TranscoderView.xaml
    /// </summary>
    public partial class TranscoderView : UserControl
    {
        public TranscoderView()
        {
            InitializeComponent();
        }
        public void btnIn_Click(object sender, RoutedEventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            fileDialog.Filter = "MKV files (*.mkv)|*.mkv|AVI files (*.avi)|*.avi|All files|*.*";
            fileDialog.DefaultExt = ".mkv";
            Nullable<bool> dialog = fileDialog.ShowDialog();

            if (dialog == true)
            {
                string InputFileName = fileDialog.FileName;

                InputFileTextBox.Text = InputFileName;
            }
        }

        public void btnOut_Click(object sender, RoutedEventArgs e)
        {
            {
                FileDialog fileDialog = new SaveFileDialog();
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                fileDialog.Filter = "MP4 file (*.mp4)|*.mp4";
                fileDialog.DefaultExt = ".mp4";
                Nullable<bool> dialog = fileDialog.ShowDialog();

                if (dialog == true)
                {
                    string OutputFileName = fileDialog.FileName;

                    OutputFileTextBox.Text = OutputFileName;
                }
            }
        }
        
        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder cmd_output = new StringBuilder();

            string InputFile = InputFileTextBox.Text;
            string OutputFile = OutputFileTextBox.Text;
            string program_files_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string ffmPath = $"{program_files_path}\\EAIO\\FFmpeg\\ffmpeg.exe";
            string Args = $"-y -hwaccel auto -i \"{InputFile}\" -map 0:v:0 -c:v copy -c:a copy -map 0:a -sn -strict -2 \"{OutputFile}\"";
            bool ffInstalled = File.Exists(ffmPath);

            if (ffInstalled == false)   {
                cmdOutputBox.Text = "CannotStartConversionError: FFmpeg not installed";
                return;
            }

            if (String.IsNullOrEmpty(InputFile) && String.IsNullOrEmpty(OutputFile))    {
                cmdOutputBox.Text = "CannotStartConversionError: No Input or Output File";
                return;
            }

            if (String.IsNullOrEmpty(InputFile))    {
                cmdOutputBox.Text = "CannotStartConversionError: No Input File";
                return;
            }

            if (String.IsNullOrEmpty(OutputFile))   {
                cmdOutputBox.Text = "CannotStartConversionError: No Output File";
                return;
            }

            else

            {
            cmdOutputBox.Text = "Starting Conversion...";

            Thread thread1 = new Thread(() =>
            {
                Stopwatch TimeTaken = new Stopwatch();

                Process ffm = new Process();
                ffm.StartInfo.FileName = ffmPath;
                ffm.StartInfo.Arguments = Args;
                ffm.StartInfo.CreateNoWindow = true;
                ffm.StartInfo.RedirectStandardOutput = true;
                ffm.StartInfo.RedirectStandardError = true;
                ffm.StartInfo.UseShellExecute = false;

                ffm.EnableRaisingEvents = true;
                ffm.OutputDataReceived += (s, ea) => { 

                    Debug.WriteLine($"STD: {ea.Data}"); 
                };
                ffm.ErrorDataReceived += (s, ea) => {

                    cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}")); 
                };
                TimeTaken.Start();
                ffm.Start();
                ffm.BeginOutputReadLine();
                ffm.BeginErrorReadLine();
                ffm.WaitForExit();
                ffm.Close();
                TimeTaken.Stop();

                TimeSpan ts = TimeTaken.Elapsed;

                cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Conversion finished in ~{ts.Minutes:00}:{ts.Seconds:00} Seconds."));
            });

            thread1.Start();
            }
        }

    }
    
}
