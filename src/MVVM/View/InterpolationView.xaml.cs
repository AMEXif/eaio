using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;
using Microsoft.WindowsAPICodePack.Shell;
using System.Collections.Generic;

namespace eaio.MVVM.View
{
    /// <summary>
    /// Interaction logic for TranscoderView.xaml
    /// </summary>
    public partial class InterpolationView : UserControl
    {
        public InterpolationView()
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

            int multiplierId = MultiplierBox.SelectedIndex;
            int multiplier = 2;

            switch (multiplierId)
            {
                case 0:
                    multiplier = 2;
                    break;
                case 1:
                    multiplier = 4;
                    break;
                case 2:
                    multiplier = 8;
                    break;
            }

            if (dialog == true)
            {
                string InputFileName = fileDialog.FileName;

                InputFileTextBox.Text = InputFileName;
            }

            outFps(InputFileTextBox.Text, multiplier);
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
            string rifePath = $"{program_files_path}\\EAIO\\Rife\\rife-ncnn-vulkan.exe";
            string ffmPath = $"{program_files_path}\\EAIO\\FFmpeg\\ffmpeg.exe";
            bool ffInstalled = File.Exists(ffmPath);
            string temp_folder = $"{program_files_path}\\EAIO\\temp";
            string input_frames = $"{temp_folder}\\input_frames";
            string output_frames = $"{temp_folder}\\output_frames";
            string dedup_video = $"{temp_folder}\\dedup\\dedup.mp4";

            

            string rifeArgs = $"-i \"{input_frames}\" -m rife-v4 -o \"{output_frames}\"";
            float OutFps = 0;

            int multiplierId = MultiplierBox.SelectedIndex;
            int multiplier = 2;

            switch (multiplierId)
            {
                case 0:
                    multiplier = 2;
                    break;
                case 1:
                    multiplier = 4;
                    break;
                case 2:
                    multiplier = 8;
                    break;
            }

            if (String.IsNullOrEmpty(InputFile))
            {
                cmdOutputBox.Text = "CannotStartConversionError: No Input File";
                return;
            }
            else
            {
                string Videofps = getFps(InputFile);
                OutFps = Single.Parse(Videofps) * multiplier;
                Debug.WriteLine(Videofps);
                Debug.WriteLine(OutFps);
            }

            string ffmArgs2 = $"-y -hwaccel auto -framerate {OutFps} -i \"{output_frames}\\%08d.png\" -crf 18 -c:v libx264 -pix_fmt yuv420p \"{OutputFile}\"";

            if (deDuplicate == false)
            {
                if (File.Exists($"{temp_folder}\\audio.m4a"))
                {
                    ffmArgs2 = $"-y -hwaccel auto -framerate {OutFps} -i \"{output_frames}\\%08d.png\" -i audio.m4a -c:a copy -crf 18 -c:v libx264 -pix_fmt yuv420p \"{OutputFile}\"";
                    Debug.WriteLine("deDuplication is false");
                }
                else
                {
                    ffmArgs2 = $"-y -hwaccel auto -framerate {OutFps} -i \"{output_frames}\\%08d.png\" -crf 18 -c:v libx264 -pix_fmt yuv420p \"{OutputFile}\"";
                }
               
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

            if (Directory.Exists(temp_folder))
            { Directory.Delete(temp_folder, true); }

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
                cmdOutputBox.Text = "Starting Interpolation...";
                bool Processing = false;
                Thread ProgressLoop = new Thread(() =>
                {
                    Stopwatch ElapsedTime = new Stopwatch();
                    ElapsedTime.Start();
                    TimeSpan ts = ElapsedTime.Elapsed;
                    while (Processing == true)
                    {
                        ts = ElapsedTime.Elapsed;
                        ElapsedTimeText.Dispatcher.BeginInvoke((Action)(() => ElapsedTimeText.Text = $"{ts.Minutes}:{ts.Seconds}.{ts.Milliseconds}"));
                        Thread.Sleep(75);
                    }
                    ElapsedTime.Stop();
                });

                Thread PercentageLoop = new Thread(() =>
                {
                    
                    decimal iFrames = Directory.GetFiles(input_frames).Length;
                    decimal oFrames = Directory.GetFiles(output_frames).Length;
                    decimal FullFrames = iFrames * multiplier;
                    decimal PercentageR = 0;
                    decimal PercentageP = 0;

                    while (Processing == true)
                    {
                        oFrames = Directory.GetFiles(output_frames).Length;
                        PercentageP = oFrames / FullFrames;
                        PercentageR = PercentageP * 100;

                        Percentage.Dispatcher.BeginInvoke((Action)(() => Percentage.Text = $"{Decimal.Round(PercentageR)}%"));
                        Thread.Sleep(125);
                    }
                });

                Thread ExtractFrames2x = new Thread(() =>
                {
                    if (deDuplicate == false)
                    {
                        Processing = true;
                        ProgressLoop.Start();
                        setState("Extracting frames...");
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
                        PercentageLoop.Start();
                        setState("Extracting audio...");
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
                        ffmExAud.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmExAud.ErrorDataReceived += (s, ea) =>
                        {
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
                        setState("Processing...");
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
                        Rife.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation in progress..."));
                        };
                        Rife.ErrorDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation in progress..."));
                        };

                        TimeTaken2.Start();
                        Rife.Start();
                        Rife.BeginOutputReadLine();
                        Rife.BeginErrorReadLine();
                        Rife.WaitForExit();
                        Rife.Close();
                        TimeTaken2.Stop();

                        TimeSpan ts2 = TimeTaken2.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation finished in ~{ts2.Minutes:00}:{ts2.Seconds:00} seconds. Encoding..."));
                        setState("Saving...");
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
                        ffmD.OutputDataReceived += (s, ea) =>
                        {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmD.ErrorDataReceived += (s, ea) =>
                        {
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
                        Processing = false;
                        setState("Done");
                    }
                    if (deDuplicate == true)
                    {
                        Processing = true;
                        ProgressLoop.Start();
                        setState("De-duplicating...");
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
                        setState("Extracting frames...");
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
                        PercentageLoop.Start();
                        setState("Processing...");
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
                        Rife.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation in progress..."));
                        };
                        Rife.ErrorDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation in progress..."));
                        };

                        TimeTaken2.Start();
                        Rife.Start();
                        Rife.BeginOutputReadLine();
                        Rife.BeginErrorReadLine();
                        Rife.WaitForExit();
                        Rife.Close();
                        TimeTaken2.Stop();

                        TimeSpan ts2 = TimeTaken2.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation finished in ~{ts2.Minutes:00}:{ts2.Seconds:00} seconds. Encoding..."));
                        setState("Saving...");
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
                        ffmD.OutputDataReceived += (s, ea) =>
                        {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmD.ErrorDataReceived += (s, ea) =>
                        {
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
                        Processing = false;
                        setState("Done");
                    }
                });

                Thread ExtractFrames4x = new Thread(() =>
                {
                    if (deDuplicate == false)
                    {
                        Processing = true;
                        ProgressLoop.Start();
                        setState("Extracting frames...");
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
                        PercentageLoop.Start();
                        setState("Extracting audio...");
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
                        ffmExAud.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmExAud.ErrorDataReceived += (s, ea) =>
                        {
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
                        setState("Processing (1/2)...");
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
                        Rife.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation in progress..."));
                        };
                        Rife.ErrorDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation in progress..."));
                        };

                        TimeTaken2.Start();
                        Rife.Start();
                        Rife.BeginOutputReadLine();
                        Rife.BeginErrorReadLine();
                        Rife.WaitForExit();
                        Rife.Close();
                        TimeTaken2.Stop();

                        TimeSpan ts2 = TimeTaken2.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"First run finished in ~{ts2.Minutes:00}:{ts2.Seconds:00} seconds. Encoding..."));

                        DirectoryInfo di = new DirectoryInfo(input_frames);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }

                        String directoryName = input_frames;
                        DirectoryInfo dirInfo = new DirectoryInfo(directoryName);
                        if (dirInfo.Exists == false)
                            Directory.CreateDirectory(directoryName);

                        List<string> first_run_frames = Directory.GetFiles(output_frames, "*.*", SearchOption.AllDirectories).ToList();

                        foreach (string file in first_run_frames)
                        {
                            FileInfo mFile = new FileInfo(file);
                            // to remove name collisions
                            if (new FileInfo(dirInfo + "\\" + mFile.Name).Exists == false)
                            {
                                mFile.MoveTo(dirInfo + "\\" + mFile.Name);
                            }
                        }
                        setState("Processing (2/2)...");
                        Stopwatch TimeTakenSecond = new Stopwatch();
                        Process Rife4x = new Process();
                        Rife4x.StartInfo.FileName = rifePath;
                        Rife4x.StartInfo.Arguments = rifeArgs;
                        Rife4x.StartInfo.WorkingDirectory = temp_folder;
                        Rife4x.StartInfo.CreateNoWindow = true;
                        Rife4x.StartInfo.RedirectStandardOutput = true;
                        Rife4x.StartInfo.RedirectStandardError = true;
                        Rife4x.StartInfo.UseShellExecute = false;

                        Rife4x.EnableRaisingEvents = true;
                        Rife4x.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Re-running Rife for 4x interpolation..."));
                        };
                        Rife4x.ErrorDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Re-running Rife for 4x interpolation..."));
                        };

                        TimeTakenSecond.Start();
                        Rife4x.Start();
                        Rife4x.BeginOutputReadLine();
                        Rife4x.BeginErrorReadLine();
                        Rife4x.WaitForExit();
                        Rife4x.Close();
                        TimeTakenSecond.Stop();

                        TimeSpan tsSecond = TimeTakenSecond.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Second run finished in ~{tsSecond.Minutes:00}:{tsSecond.Seconds:00} seconds. Encoding..."));
                        setState("Saving...");
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
                        ffmD.OutputDataReceived += (s, ea) =>
                        {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmD.ErrorDataReceived += (s, ea) =>
                        {
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
                        setState("Done");
                        Directory.Delete(temp_folder, true);
                        Processing = false;
                    }
                    if (deDuplicate == true)
                    {
                        setState("De-duplicating...");
                        Processing = true;
                        ProgressLoop.Start();
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
                        setState("Extracting frames...");
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
                        PercentageLoop.Start();
                        setState("Processing (1/2)...");
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
                        Rife.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation in progress..."));
                        };
                        Rife.ErrorDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation in progress..."));
                        };

                        TimeTaken2.Start();
                        Rife.Start();
                        Rife.BeginOutputReadLine();
                        Rife.BeginErrorReadLine();
                        Rife.WaitForExit();
                        Rife.Close();
                        TimeTaken2.Stop();

                        TimeSpan ts2 = TimeTaken2.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"First run finished in ~{ts2.Minutes:00}:{ts2.Seconds:00} seconds. Encoding..."));


                        DirectoryInfo di = new DirectoryInfo(input_frames);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }

                        String directoryName = input_frames;
                        DirectoryInfo dirInfo = new DirectoryInfo(directoryName);
                        if (dirInfo.Exists == false)
                            Directory.CreateDirectory(directoryName);

                        List<string> first_run_frames = Directory.GetFiles(output_frames, "*.*", SearchOption.AllDirectories).ToList();

                        foreach (string file in first_run_frames)
                        {
                            FileInfo mFile = new FileInfo(file);
                            // to remove name collisions
                            if (new FileInfo(dirInfo + "\\" + mFile.Name).Exists == false)
                            {
                                mFile.MoveTo(dirInfo + "\\" + mFile.Name);
                            }
                        }
                        setState("Processing (2/2)...");
                        Stopwatch TimeTakenSecond = new Stopwatch();
                        Process Rife4x = new Process();
                        Rife4x.StartInfo.FileName = rifePath;
                        Rife4x.StartInfo.Arguments = rifeArgs;
                        Rife4x.StartInfo.WorkingDirectory = temp_folder;
                        Rife4x.StartInfo.CreateNoWindow = true;
                        Rife4x.StartInfo.RedirectStandardOutput = true;
                        Rife4x.StartInfo.RedirectStandardError = true;
                        Rife4x.StartInfo.UseShellExecute = false;

                        Rife4x.EnableRaisingEvents = true;
                        Rife4x.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Re-running Rife for 4x interpolation..."));
                        };
                        Rife4x.ErrorDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Re-running Rife for 4x interpolation..."));
                        };

                        TimeTakenSecond.Start();
                        Rife4x.Start();
                        Rife4x.BeginOutputReadLine();
                        Rife4x.BeginErrorReadLine();
                        Rife4x.WaitForExit();
                        Rife4x.Close();
                        TimeTakenSecond.Stop();

                        TimeSpan tsSecond = TimeTakenSecond.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Second run finished in ~{tsSecond.Minutes:00}:{tsSecond.Seconds:00} seconds. Encoding..."));
                        setState("Saving...");
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
                        ffmD.OutputDataReceived += (s, ea) =>
                        {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmD.ErrorDataReceived += (s, ea) =>
                        {
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
                        setState("Done");
                        Directory.Delete(temp_folder, true);
                        Processing = false;
                    }
                });

                Thread ExtractFrames8x = new Thread(() =>
                {
                    if (deDuplicate == false)
                    {
                        setState("Extracting frames...");
                        Processing = true;
                        ProgressLoop.Start();
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
                        PercentageLoop.Start();
                        setState("Extracting audio...");
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
                        ffmExAud.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmExAud.ErrorDataReceived += (s, ea) =>
                        {
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
                        setState("Processing (1/3)...");
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
                        Rife.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation in progress..."));
                        };
                        Rife.ErrorDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation in progress..."));
                        };

                        TimeTaken2.Start();
                        Rife.Start();
                        Rife.BeginOutputReadLine();
                        Rife.BeginErrorReadLine();
                        Rife.WaitForExit();
                        Rife.Close();
                        TimeTaken2.Stop();

                        TimeSpan ts2 = TimeTaken2.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"First run finished in ~{ts2.Minutes:00}:{ts2.Seconds:00} seconds. Encoding..."));

                        DirectoryInfo di = new DirectoryInfo(input_frames);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }

                        String directoryName = input_frames;
                        DirectoryInfo dirInfo = new DirectoryInfo(directoryName);
                        if (dirInfo.Exists == false)
                            Directory.CreateDirectory(directoryName);

                        List<string> first_run_frames = Directory.GetFiles(output_frames, "*.*", SearchOption.AllDirectories).ToList();

                        foreach (string file in first_run_frames)
                        {
                            FileInfo mFile = new FileInfo(file);
                            // to remove name collisions
                            if (new FileInfo(dirInfo + "\\" + mFile.Name).Exists == false)
                            {
                                mFile.MoveTo(dirInfo + "\\" + mFile.Name);
                            }
                        }
                        setState("Processing (2/3)...");
                        Stopwatch TimeTakenSecond = new Stopwatch();
                        Process Rife4x = new Process();
                        Rife4x.StartInfo.FileName = rifePath;
                        Rife4x.StartInfo.Arguments = rifeArgs;
                        Rife4x.StartInfo.WorkingDirectory = temp_folder;
                        Rife4x.StartInfo.CreateNoWindow = true;
                        Rife4x.StartInfo.RedirectStandardOutput = true;
                        Rife4x.StartInfo.RedirectStandardError = true;
                        Rife4x.StartInfo.UseShellExecute = false;

                        Rife4x.EnableRaisingEvents = true;
                        Rife4x.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Re-running Rife for 4x interpolation..."));
                        };
                        Rife4x.ErrorDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Re-running Rife for 4x interpolation..."));
                        };

                        TimeTakenSecond.Start();
                        Rife4x.Start();
                        Rife4x.BeginOutputReadLine();
                        Rife4x.BeginErrorReadLine();
                        Rife4x.WaitForExit();
                        Rife4x.Close();
                        TimeTakenSecond.Stop();

                        TimeSpan tsSecond = TimeTakenSecond.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Second run finished in ~{tsSecond.Minutes:00}:{tsSecond.Seconds:00} seconds."));

                        DirectoryInfo diry = new DirectoryInfo(input_frames);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in diry.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                        setState("Processing (3/3)...");
                        String directoryName2 = input_frames;
                        DirectoryInfo dirInfo2 = new DirectoryInfo(directoryName);
                        if (dirInfo.Exists == false)
                            Directory.CreateDirectory(directoryName);

                        List<string> first_run_frames2 = Directory.GetFiles(output_frames, "*.*", SearchOption.AllDirectories).ToList();

                        foreach (string file in first_run_frames)
                        {
                            FileInfo mFile = new FileInfo(file);
                            // to remove name collisions
                            if (new FileInfo(dirInfo + "\\" + mFile.Name).Exists == false)
                            {
                                mFile.MoveTo(dirInfo + "\\" + mFile.Name);
                            }
                        }

                        Stopwatch TimeTakenThird = new Stopwatch();
                        Process Rife8x = new Process();
                        Rife8x.StartInfo.FileName = rifePath;
                        Rife8x.StartInfo.Arguments = rifeArgs;
                        Rife8x.StartInfo.WorkingDirectory = temp_folder;
                        Rife8x.StartInfo.CreateNoWindow = true;
                        Rife8x.StartInfo.RedirectStandardOutput = true;
                        Rife8x.StartInfo.RedirectStandardError = true;
                        Rife8x.StartInfo.UseShellExecute = false;

                        Rife8x.EnableRaisingEvents = true;
                        Rife8x.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Re-running Rife for 8x interpolation..."));
                        };
                        Rife8x.ErrorDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Re-running Rife for 8x interpolation..."));
                        };

                        TimeTakenThird.Start();
                        Rife8x.Start();
                        Rife8x.BeginOutputReadLine();
                        Rife8x.BeginErrorReadLine();
                        Rife8x.WaitForExit();
                        Rife8x.Close();
                        TimeTakenThird.Stop();

                        TimeSpan tsThird = TimeTakenThird.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Third run finished in ~{tsThird.Minutes:00}:{tsThird.Seconds:00} seconds. Encoding..."));
                        setState("Saving...");

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
                        ffmD.OutputDataReceived += (s, ea) =>
                        {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmD.ErrorDataReceived += (s, ea) =>
                        {
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
                        setState("Done");
                        Directory.Delete(temp_folder, true);
                        Processing = false;
                    }
                    if (deDuplicate == true)
                    {
                        Processing = true;
                        ProgressLoop.Start();
                        setState("De-duplicating...");
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
                        setState("Extracting frames...");
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
                        PercentageLoop.Start();
                        setState("Processing (1/3)...");
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
                        Rife.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation in progress... Do not close this window!"));
                        };
                        Rife.ErrorDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Interpolation in progress... Do not close this window!"));
                        };

                        TimeTaken2.Start();
                        Rife.Start();
                        Rife.BeginOutputReadLine();
                        Rife.BeginErrorReadLine();
                        Rife.WaitForExit();
                        Rife.Close();
                        TimeTaken2.Stop();

                        TimeSpan ts2 = TimeTaken2.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"First run finished in ~{ts2.Minutes:00}:{ts2.Seconds:00} seconds. Encoding..."));


                        DirectoryInfo di = new DirectoryInfo(input_frames);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }

                        String directoryName = input_frames;
                        DirectoryInfo dirInfo = new DirectoryInfo(directoryName);
                        if (dirInfo.Exists == false)
                            Directory.CreateDirectory(directoryName);

                        List<string> first_run_frames = Directory.GetFiles(output_frames, "*.*", SearchOption.AllDirectories).ToList();

                        foreach (string file in first_run_frames)
                        {
                            FileInfo mFile = new FileInfo(file);
                            // to remove name collisions
                            if (new FileInfo(dirInfo + "\\" + mFile.Name).Exists == false)
                            {
                                mFile.MoveTo(dirInfo + "\\" + mFile.Name);
                            }
                        }
                        setState("Processing (2/3)...");
                        Stopwatch TimeTakenSecond = new Stopwatch();
                        Process Rife4x = new Process();
                        Rife4x.StartInfo.FileName = rifePath;
                        Rife4x.StartInfo.Arguments = rifeArgs;
                        Rife4x.StartInfo.WorkingDirectory = temp_folder;
                        Rife4x.StartInfo.CreateNoWindow = true;
                        Rife4x.StartInfo.RedirectStandardOutput = true;
                        Rife4x.StartInfo.RedirectStandardError = true;
                        Rife4x.StartInfo.UseShellExecute = false;

                        Rife4x.EnableRaisingEvents = true;
                        Rife4x.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Re-running Rife for 4x interpolation... Do not close this window!"));
                        };
                        Rife4x.ErrorDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Re-running Rife for 4x interpolation... Do not close this window!"));
                        };

                        TimeTakenSecond.Start();
                        Rife4x.Start();
                        Rife4x.BeginOutputReadLine();
                        Rife4x.BeginErrorReadLine();
                        Rife4x.WaitForExit();
                        Rife4x.Close();
                        TimeTakenSecond.Stop();

                        TimeSpan tsSecond = TimeTakenSecond.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Second run finished in ~{tsSecond.Minutes:00}:{tsSecond.Seconds:00} seconds. Encoding..."));

                        DirectoryInfo diry = new DirectoryInfo(input_frames);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in diry.GetDirectories())
                        {
                            dir.Delete(true);
                        }

                        String directoryName2 = input_frames;
                        DirectoryInfo dirInfo2 = new DirectoryInfo(directoryName);
                        if (dirInfo.Exists == false)
                            Directory.CreateDirectory(directoryName);

                        List<string> first_run_frames2 = Directory.GetFiles(output_frames, "*.*", SearchOption.AllDirectories).ToList();

                        foreach (string file in first_run_frames)
                        {
                            FileInfo mFile = new FileInfo(file);
                            // to remove name collisions
                            if (new FileInfo(dirInfo + "\\" + mFile.Name).Exists == false)
                            {
                                mFile.MoveTo(dirInfo + "\\" + mFile.Name);
                            }
                        }
                        setState("Processing (3/3)...");
                        Stopwatch TimeTakenThird = new Stopwatch();
                        Process Rife8x = new Process();
                        Rife8x.StartInfo.FileName = rifePath;
                        Rife8x.StartInfo.Arguments = rifeArgs;
                        Rife8x.StartInfo.WorkingDirectory = temp_folder;
                        Rife8x.StartInfo.CreateNoWindow = true;
                        Rife8x.StartInfo.RedirectStandardOutput = true;
                        Rife8x.StartInfo.RedirectStandardError = true;
                        Rife8x.StartInfo.UseShellExecute = false;

                        Rife8x.EnableRaisingEvents = true;
                        Rife8x.OutputDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Re-running Rife for 8x interpolation... Do not close this window!"));
                        };
                        Rife8x.ErrorDataReceived += (s, ea) =>
                        {
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Re-running Rife for 8x interpolation... Do not close this window!"));
                        };

                        TimeTakenThird.Start();
                        Rife8x.Start();
                        Rife8x.BeginOutputReadLine();
                        Rife8x.BeginErrorReadLine();
                        Rife8x.WaitForExit();
                        Rife8x.Close();
                        TimeTakenThird.Stop();

                        TimeSpan tsThird = TimeTakenThird.Elapsed;
                        cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"Third run finished in ~{tsThird.Minutes:00}:{tsThird.Seconds:00} seconds. Encoding..."));
                        Stopwatch TimeTaken3 = new Stopwatch();
                        setState("Saving...");
                        Process ffmD = new Process();
                        ffmD.StartInfo.FileName = ffmPath;
                        ffmD.StartInfo.Arguments = ffmArgs2;
                        ffmD.StartInfo.WorkingDirectory = temp_folder;
                        ffmD.StartInfo.CreateNoWindow = true;
                        ffmD.StartInfo.RedirectStandardOutput = true;
                        ffmD.StartInfo.RedirectStandardError = true;
                        ffmD.StartInfo.UseShellExecute = false;

                        ffmD.EnableRaisingEvents = true;
                        ffmD.OutputDataReceived += (s, ea) =>
                        {
                            Debug.WriteLine($"{ea.Data}");
                            cmdOutputBox.Dispatcher.BeginInvoke((Action)(() => cmdOutputBox.Text = $"{ea.Data}"));
                        };
                        ffmD.ErrorDataReceived += (s, ea) =>
                        {
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
                        Processing = false;
                        setState("Done");
                    }
                });

                switch (multiplier)
                {
                    case 2:
                        ExtractFrames2x.Start();
                        break;
                    case 4:
                        ExtractFrames4x.Start();
                        break;
                    case 8:
                        ExtractFrames8x.Start();
                        
                        break;
                }
            }
        }

        public void outFps(string InputFile, int multiplier)
        {

            try
            {
                string Videofps = getFps(InputFile);
                float outFps = Single.Parse(Videofps) * multiplier;
                Debug.WriteLine(Videofps);
                Debug.WriteLine(outFps);
                OutFps.Dispatcher.BeginInvoke((Action)(() => OutFps.Text = $"{outFps}"));
            }
            catch
            {
                
            }

        }
        public string getFps(string sourceFile)
        {

            string Default = "x x x";

            try
            {
                ShellFile shellFile = ShellFile.FromFilePath(sourceFile);
                float fps = Convert.ToSingle(shellFile.Properties.System.Video.FrameRate.Value) / 1000;
                Debug.WriteLine(fps);
                return (fps).ToString();
            }
            catch
            {
                return Default;
            }
        }

        public void setState(string state)
        {
            State.Dispatcher.BeginInvoke((Action)(() => State.Text = state));
        }

        private void MultiplierBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int multiplierId = MultiplierBox.SelectedIndex;
            int multiplier = 2;

            switch (multiplierId)
            {
                case 0:
                    multiplier = 2;
                    break;
                case 1:
                    multiplier = 4;
                    break;
                case 2:
                    multiplier = 8;
                    break;
            }

            Debug.WriteLine("Click");
            outFps(InputFileTextBox.Text, multiplier);
        }
    }
}