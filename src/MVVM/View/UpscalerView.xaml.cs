using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;
using Microsoft.WindowsAPICodePack.Shell;

namespace eaio.MVVM.View
{
    /// <summary>
    /// Interaction logic for InterpolationView.xaml
    /// </summary>
    public partial class UpscalerView : UserControl
    {
        public UpscalerView()
        {
            InitializeComponent();
        }
        public void btnIn_Click(object sender, RoutedEventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            fileDialog.Filter = "MP4 file (*.mp4)|*.mp4|MOV files (*.mov)|*.mov|All files|*.*";
            fileDialog.DefaultExt = ".mp4";
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


            string InputFile = InputFileTextBox.Text;
            string OutputFile = OutputFileTextBox.Text;
            bool? deDuplicate = ChkProcess.IsChecked;

            string program_files_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string rifePath = $"{program_files_path}\\EAIO\\Real-CUGAN\\realcugan-ncnn-vulkan.exe";
            string ffmPath = $"{program_files_path}\\EAIO\\FFmpeg\\ffmpeg.exe";
            bool ffInstalled = File.Exists(ffmPath);
            string temp_folder = $"{program_files_path}\\EAIO\\temp";
            string input_frames = $"{temp_folder}\\input_frames";
            string output_frames = $"{temp_folder}\\output_frames";
            string dedup_video = $"{temp_folder}\\dedup\\dedup.mp4";

            string rifeArgs = $"-i \"{input_frames}\" -s 3 -n 3 -j 2:2:2 -o \"{output_frames}\"";
            int OutFps = 0;

            if (String.IsNullOrEmpty(InputFile))
            {
                cmdOutputBox.Text = "CannotStartConversionError: No Input File";
                return;
            }
            else
            {
                string Videofps = getFps(InputFile);
                OutFps = Int32.Parse(Videofps) * 2;
                Debug.WriteLine(Videofps);
                Debug.WriteLine(OutFps);
            }

            string ffmArgs2 = $"-y -hwaccel auto -framerate {OutFps} -i \"{output_frames}\\%08d.png\" -crf 18 -c:v libx264 -pix_fmt yuv420p \"{OutputFile}\"";

            if (deDuplicate == false)
            {
                ffmArgs2 = $"-y -hwaccel auto -framerate {OutFps} -i \"{output_frames}\\%08d.png\" -i audio.m4a -c:a copy -crf 18 -c:v libx264 -pix_fmt yuv420p \"{OutputFile}\"";
                Debug.WriteLine("deDuplication is false");
            }
            if (deDuplicate == true)
            {
                ffmArgs2 = $"-y -hwaccel auto -framerate {OutFps} -i \"{output_frames}\\%08d.png\" -crf 18 -c:v libx264 -pix_fmt yuv420p \"{OutputFile}\"";
                Debug.WriteLine("deDuplication is true");
            }

            string ffmArgs = $"-y -hwaccel auto -i \"{InputFile}\" \"{input_frames}\\frame_%08d.png\"";
            string ffmArgsEE = $"-y -hwaccel auto -i \"{dedup_video}\" \"{input_frames}\\frame_%08d.png\"";
            string ffmArgsE = $"-y -hwaccel auto -i \"{InputFile}\" -vf mpdecimate,setpts=N/FRAME_RATE/TB -crf 15 -an \"{dedup_video}\"";
            string ffmArgs1 = $"-y -i \"{InputFile}\" -vn -c:a copy \"audio.m4a\"";


            Directory.CreateDirectory(temp_folder);
            Directory.CreateDirectory(input_frames);
            Directory.CreateDirectory(output_frames);

            if (ffInstalled == false)
            {
                cmdOutputBox.Text = "CannotStartConversionError: FFmpeg not installed";
                return;
            }
            if (String.IsNullOrEmpty(InputFile) && String.IsNullOrEmpty(OutputFile))
            {
                cmdOutputBox.Text = "CannotStartConversionError: No Input or Output File";
                return;
            }
            if (String.IsNullOrEmpty(InputFile))
            {
                cmdOutputBox.Text = "CannotStartConversionError: No Input File";
                return;
            }
            if (String.IsNullOrEmpty(OutputFile))
            {
                cmdOutputBox.Text = "CannotStartConversionError: No Output File";
                return;
            }
            else

            {
                cmdOutputBox.Text = "Starting Conversion...";

                Thread ExtractFrames = new Thread(() =>
                {
                    if (deDuplicate == false)
                    {
                        Stopwatch TimeTaken = new Stopwatch();
                        Process ffm = new Process();
                        ffm.StartInfo.FileName = ffmPath;
                        ffm.StartInfo.Arguments = ffmArgs;
                        ffm.StartInfo.CreateNoWindow = true;
                        ffm.StartInfo.RedirectStandardOutput = true;
                        ffm.StartInfo.RedirectStandardError = true;
                        ffm.StartInfo.UseShellExecute = false;

                        ffm.EnableRaisingEvents = true;
                        ffm.OutputDataReceived += (s, ea) =>
                        {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffm.ErrorDataReceived += (s, ea) =>
                        {
                            Debug.WriteLine($"{ea.Data}");
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
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Frame extraction finished in ~{ts.Minutes:00}:{ts.Seconds:00} seconds."));

                        Stopwatch TimeTaken1 = new Stopwatch();
                        Process ffmExAud = new Process();
                        ffmExAud.StartInfo.FileName = ffmPath;
                        ffmExAud.StartInfo.Arguments = ffmArgs1;
                        ffmExAud.StartInfo.WorkingDirectory = temp_folder;
                        ffmExAud.StartInfo.CreateNoWindow = true;
                        ffmExAud.StartInfo.RedirectStandardOutput = true;
                        ffmExAud.StartInfo.RedirectStandardError = true;
                        ffmExAud.StartInfo.UseShellExecute = false;

                        ffmExAud.EnableRaisingEvents = true;
                        ffmExAud.OutputDataReceived += (s, ea) => {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmExAud.ErrorDataReceived += (s, ea) => {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };

                        TimeTaken1.Start();
                        ffmExAud.Start();
                        ffmExAud.BeginOutputReadLine();
                        ffmExAud.BeginErrorReadLine();
                        ffmExAud.WaitForExit();
                        ffmExAud.Close();
                        TimeTaken1.Stop();

                        TimeSpan ts1 = TimeTaken1.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Audio extraction finished in ~{ts1.Minutes:00}:{ts1.Seconds:00} seconds."));

                        Stopwatch TimeTaken2 = new Stopwatch();
                        Process Rife = new Process();
                        Rife.StartInfo.FileName = rifePath;
                        Rife.StartInfo.Arguments = rifeArgs;
                        Rife.StartInfo.WorkingDirectory = temp_folder;
                        Rife.StartInfo.CreateNoWindow = true;
                        Rife.StartInfo.RedirectStandardOutput = true;
                        Rife.StartInfo.RedirectStandardError = true;
                        Rife.StartInfo.UseShellExecute = false;

                        Rife.EnableRaisingEvents = true;
                        Rife.OutputDataReceived += (s, ea) => {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Upscaling in progress..."));
                        };
                        Rife.ErrorDataReceived += (s, ea) => {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Upscaling in progress..."));
                        };

                        TimeTaken2.Start();
                        Rife.Start();
                        Rife.BeginOutputReadLine();
                        Rife.BeginErrorReadLine();
                        Rife.WaitForExit();
                        Rife.Close();
                        TimeTaken2.Stop();

                        TimeSpan ts2 = TimeTaken2.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Upscaling finished in ~{ts2.Minutes:00}:{ts2.Seconds:00} seconds. Encoding..."));

                        Stopwatch TimeTaken3 = new Stopwatch();
                        Process ffmD = new Process();
                        ffmD.StartInfo.FileName = ffmPath;
                        ffmD.StartInfo.Arguments = ffmArgs2;
                        ffmD.StartInfo.WorkingDirectory = temp_folder;
                        ffmD.StartInfo.CreateNoWindow = true;
                        ffmD.StartInfo.RedirectStandardOutput = true;
                        ffmD.StartInfo.RedirectStandardError = true;
                        ffmD.StartInfo.UseShellExecute = false;

                        ffmD.EnableRaisingEvents = true;
                        ffmD.OutputDataReceived += (s, ea) => {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmD.ErrorDataReceived += (s, ea) => {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };

                        TimeTaken3.Start();
                        ffmD.Start();
                        ffmD.BeginOutputReadLine();
                        ffmD.BeginErrorReadLine();
                        ffmD.WaitForExit();
                        ffmD.Close();
                        TimeTaken3.Stop();

                        TimeSpan ts3 = TimeTaken3.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Encoding finished in ~{ts3.Minutes:00}:{ts3.Seconds:00} Seconds."));

                        Directory.Delete(temp_folder, true);
                    }
                    if (deDuplicate == true)
                    {
                        Directory.CreateDirectory($"{temp_folder}\\dedup");

                        Stopwatch TimeTaken = new Stopwatch();
                        Process ffmE = new Process();
                        ffmE.StartInfo.FileName = ffmPath;
                        ffmE.StartInfo.Arguments = ffmArgsE;
                        ffmE.StartInfo.CreateNoWindow = true;
                        ffmE.StartInfo.RedirectStandardOutput = true;
                        ffmE.StartInfo.RedirectStandardError = true;
                        ffmE.StartInfo.UseShellExecute = false;

                        ffmE.EnableRaisingEvents = true;
                        ffmE.OutputDataReceived += (s, ea) =>
                        {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmE.ErrorDataReceived += (s, ea) =>
                        {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };

                        TimeTaken.Start();
                        ffmE.Start();
                        ffmE.BeginOutputReadLine();
                        ffmE.BeginErrorReadLine();
                        ffmE.WaitForExit();
                        ffmE.Close();
                        TimeTaken.Stop();

                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Removed duplicate frames. Extracting leftover frames..."));

                        Process ffm = new Process();
                        ffm.StartInfo.FileName = ffmPath;
                        ffm.StartInfo.Arguments = ffmArgsEE;
                        ffm.StartInfo.CreateNoWindow = true;
                        ffm.StartInfo.RedirectStandardOutput = true;
                        ffm.StartInfo.RedirectStandardError = true;
                        ffm.StartInfo.UseShellExecute = false;

                        ffm.EnableRaisingEvents = true;
                        ffm.OutputDataReceived += (s, ea) =>
                        {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffm.ErrorDataReceived += (s, ea) =>
                        {
                            Debug.WriteLine($"{ea.Data}");
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
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Frame extraction finished in ~{ts.Minutes:00}:{ts.Seconds:00} seconds."));

                        Stopwatch TimeTaken2 = new Stopwatch();
                        Process Rife = new Process();
                        Rife.StartInfo.FileName = rifePath;
                        Rife.StartInfo.Arguments = rifeArgs;
                        Rife.StartInfo.WorkingDirectory = temp_folder;
                        Rife.StartInfo.CreateNoWindow = true;
                        Rife.StartInfo.RedirectStandardOutput = true;
                        Rife.StartInfo.RedirectStandardError = true;
                        Rife.StartInfo.UseShellExecute = false;

                        Rife.EnableRaisingEvents = true;
                        Rife.OutputDataReceived += (s, ea) => {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Upscaling in progress..."));
                        };
                        Rife.ErrorDataReceived += (s, ea) => {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Upscaling in progress..."));
                        };

                        TimeTaken2.Start();
                        Rife.Start();
                        Rife.BeginOutputReadLine();
                        Rife.BeginErrorReadLine();
                        Rife.WaitForExit();
                        Rife.Close();
                        TimeTaken2.Stop();

                        TimeSpan ts2 = TimeTaken2.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Upscaling finished in ~{ts2.Minutes:00}:{ts2.Seconds:00} seconds. Encoding..."));

                        Stopwatch TimeTaken3 = new Stopwatch();
                        Process ffmD = new Process();
                        ffmD.StartInfo.FileName = ffmPath;
                        ffmD.StartInfo.Arguments = ffmArgs2;
                        ffmD.StartInfo.WorkingDirectory = temp_folder;
                        ffmD.StartInfo.CreateNoWindow = true;
                        ffmD.StartInfo.RedirectStandardOutput = true;
                        ffmD.StartInfo.RedirectStandardError = true;
                        ffmD.StartInfo.UseShellExecute = false;

                        ffmD.EnableRaisingEvents = true;
                        ffmD.OutputDataReceived += (s, ea) => {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmD.ErrorDataReceived += (s, ea) => {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };

                        TimeTaken3.Start();
                        ffmD.Start();
                        ffmD.BeginOutputReadLine();
                        ffmD.BeginErrorReadLine();
                        ffmD.WaitForExit();
                        ffmD.Close();
                        TimeTaken3.Stop();

                        TimeSpan ts3 = TimeTaken3.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Encoding finished in ~{ts3.Minutes:00}:{ts3.Seconds:00} Seconds."));

                        Directory.Delete(temp_folder, true);
                    }
                });

                ExtractFrames.Start();
            }
        }

        public string getFps(string sourceFile)
        {
            ShellFile shellFile = ShellFile.FromFilePath(sourceFile);
            return (shellFile.Properties.System.Video.FrameRate.Value / 1000).ToString();
        }
    }
}
