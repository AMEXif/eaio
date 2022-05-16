using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.Win32;
using System.Linq;
using System.Threading;

namespace eaio.MVVM.View
{
    /// <summary>
    /// Interaction logic for TranscoderView.xaml
    /// </summary>
    public partial class TranscoderView : UserControl
    {

        public string[] files;

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
                string[] InputFileNames = fileDialog.FileNames;
                files = fileDialog.FileNames;
                setOutput(files[0]);

                InputFileTextBox.Text = InputFileNames[0];
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

            string InputFileT = InputFileTextBox.Text;
            string OutputFileT = OutputFileTextBox.Text;
            string program_files_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string ffmPath = $"{program_files_path}\\EAIO\\FFmpeg\\ffmpeg.exe";

            bool ffInstalled = File.Exists(ffmPath);

            if (ffInstalled == false) {
                cmdOutputBox.Text = "CannotStartConversionError: FFmpeg not installed";
                return;
            }

            if (String.IsNullOrEmpty(InputFileT) && String.IsNullOrEmpty(OutputFileT)) {
                cmdOutputBox.Text = "CannotStartConversionError: No Input or Output File";
                return;
            }

            if (String.IsNullOrEmpty(InputFileT)) {
                cmdOutputBox.Text = "CannotStartConversionError: No Input File";
                return;
            }

            if (String.IsNullOrEmpty(OutputFileT)) {
                cmdOutputBox.Text = "CannotStartConversionError: No Output File";
                return;
            }

            else

            {
                cmdOutputBox.Text = "Starting Conversion...";

                Convert(files);
            }
        }

        private void btnIn_Drop(object sender, DragEventArgs e)
        {
            files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (Path.GetExtension(files[0]) == ".eaio")
            {
                var json = File.ReadAllText(files[0]);

                json = json.Replace("[", "");
                json = json.Replace("]", "");
                json = json.Replace('"', ' ');
                json = json.Replace(" ", "");
                json = json.Replace("\\\\", "\\");

                files = json.Split(',');

                foreach (string file in files)
                {
                    Debug.WriteLine(file);
                }
                InputFileTextBox.Text = files[0];
                setOutput(files[0]);
            }
            else
            {
                InputFileTextBox.Text = files[0];
                setOutput(files[0]);
            }
        }

        private void setOutput(string input)
        {
            string root = Path.GetDirectoryName(input);
            string filename = root + "\\" + Path.GetFileNameWithoutExtension(input);
            OutputFileTextBox.Text = filename + ".mp4";
            FileCount.Text = $"Files in queue: {files.Length}";

            List<string> filenames = new List<string>();

            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                filenames.Add(name);
            }
            if (files.Length > 1)
            {
                string inputfiles = "";

                foreach (string file in filenames)
                {
                    inputfiles = $"{inputfiles} {file}";
                }

                FileNames.Opacity = 1;
                FileNames.Text = $"All files:{inputfiles}";
            }
            else
            {
                FileNames.Opacity = 0;
            }
        }
        private string GetOutput(string input)
        {
            string root = Path.GetDirectoryName(input);
            string filename = root + "\\" + Path.GetFileNameWithoutExtension(input);
            string output = filename + ".mp4";

            return output;
        }

        private void Convert(string[] input)
        {

            Thread converter = new Thread(() =>
            {
                int i = files.Length + 1;
                int d = 0;

                foreach (string file in input)
                {
                    d += 1;
                    i = i - 1;
                    Status.Dispatcher.BeginInvoke((Action)(() => Status.Text = $"Running ({d}/{files.Length})"));

                    string InputFile = file;
                    string OutputFile = GetOutput(file);
                    string program_files_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string ffmPath = $"{program_files_path}\\EAIO\\FFmpeg\\ffmpeg.exe";
                    string Args = $"-y -hwaccel auto -i \"{InputFile}\" -map 0:v:0 -c:v copy -c:a copy -map 0:a -sn -strict -2 \"{OutputFile}\"";

                    if (InputFile == OutputFile)
                    {
                        OutputFile = OutputFile + $"_{RandomString(5)}{Path.GetExtension(OutputFile)}";
                        Args = $"-y -hwaccel auto -i \"{InputFile}\" -map 0:v:0 -c:v copy -c:a copy -map 0:a -sn -strict -2 \"{OutputFile}\"";
                    }

                    Debug.WriteLine(InputFile);
                    Debug.WriteLine(OutputFile);
                    Debug.WriteLine(Args);

                    Stopwatch TimeTaken = new Stopwatch();

                    Process ffm = new Process();
                    ffm.StartInfo.FileName = ffmPath;
                    ffm.StartInfo.Arguments = Args;
                    ffm.StartInfo.CreateNoWindow = true;
                    ffm.StartInfo.RedirectStandardOutput = true;
                    ffm.StartInfo.RedirectStandardError = true;
                    ffm.StartInfo.UseShellExecute = false;

                    ffm.EnableRaisingEvents = true;
                    ffm.OutputDataReceived += (s, ea) =>
                    {

                        Debug.WriteLine($"STD: {ea.Data}");
                    };
                    ffm.ErrorDataReceived += (s, ea) =>
                    {

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

                    cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Conversion finished in ~{ts.Minutes:00}:{ts.Seconds:00} Seconds. Files left: {i}"));
                }

                Status.Dispatcher.BeginInvoke((Action)(() => Status.Text = $"Done"));
                cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Done."));
            });

            converter.Start();
        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            fileDialog.Filter = "eaio file (*.eaio)|*.eaio";
            fileDialog.DefaultExt = ".eaio";
            Nullable<bool> dialog = fileDialog.ShowDialog();

            string InputFileName = "C:\\";

            if (dialog == true)
            {
                InputFileName = fileDialog.FileName;
            }

            var json = File.ReadAllText(InputFileName);

            json = json.Replace("[", "");
            json = json.Replace("]", "");
            json = json.Replace('"', ' ');
            json = json.Replace(" ", "");
            json = json.Replace("\\\\", "\\");

            files = json.Split(',');
            
            foreach (string file in files)
            {
                Debug.WriteLine(file);
            }
            InputFileTextBox.Text = files[0];
            setOutput(files[0]);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {

            FileDialog fileDialog = new SaveFileDialog();
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            fileDialog.Filter = "eaio file (*.eaio)|*.eaio";
            fileDialog.DefaultExt = ".eaio";
            Nullable<bool> dialog = fileDialog.ShowDialog();

            string OutputFileName = "C:\\";

            if (dialog == true)
            {
                OutputFileName = fileDialog.FileName;
            }

            var json = JsonSerializer.Serialize(files);

            if (OutputFileName != "C:\\")
            {
                File.WriteAllText(OutputFileName, json);
            }
        }
    }
}
