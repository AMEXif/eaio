using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Text;
using Chilkat;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Microsoft.Win32;
using System.Threading;
using System.Windows.Media.Animation;
using Microsoft.WindowsAPICodePack.Shell;
using System.Collections.Generic;
using System.Linq;

namespace eaio.MVVM.View
{
    /// <summary>
    /// Interaction logic for InterpolationView.xaml
    /// </summary>

    public partial class RecognitionView : UserControl
    {

        public RecognitionView()
        {
            InitializeComponent();
        }
        public Dictionary<int, string> parseTrace(string response)
        {
            Dictionary<int, string> data = new Dictionary<int, string>();

            Chilkat.JsonObject json = new Chilkat.JsonObject();
            json.Load(response);

            int anilist;
            string filename = "";
            int episode = 1;
            string from = "";
            string v_to = "";
            string similarity;
            string video = "";
            string image = "";

            int frameCount = json.IntOf("frameCount");
            string error = json.StringOf("error");
            int i = 0;
            int count_i = json.SizeOfArray("result");
            while (i == 0)
            {
                json.I = i;
                anilist = json.IntOf("result[i].anilist");
                filename = json.StringOf("result[i].filename");
                episode = json.IntOf("result[i].episode");
                from = json.StringOf("result[i].from");
                v_to = json.StringOf("result[i].to");
                similarity = json.StringOf("result[i].similarity");
                video = json.StringOf("result[i].video");
                image = json.StringOf("result[i].image");
                i = i + 1;
            }

            data.Add(0, filename);
            data.Add(1, episode.ToString());
            data.Add(2, image);
            data.Add(3, from);
            data.Add(4, v_to);
            return data;
        }

        public string parseJson(string response)
        {
            Chilkat.JsonObject json = new Chilkat.JsonObject();
            json.Load(response);

            string hash;
            string name;
            string url = "";
            string size;

            bool success = json.BoolOf("success");
            int i = 0;
            int count_i = json.SizeOfArray("files");
            while (i < count_i)
            {
                json.I = i;
                hash = json.StringOf("files[i].hash");
                name = json.StringOf("files[i].name");
                url = json.StringOf($"files[i].url");
                size = json.StringOf("files[i].size");
                i = i + 1;
            }

            Debug.WriteLine(url);
            header("Anime Recognition - Processing...");
            string urltext = $"https://trace.moe/?url={url}";
            AllResults.Dispatcher.BeginInvoke((Action)(() => AllResults.Text = urltext));

            return url;
        }
        public string uploadImage(string path)
        {
            Chilkat.Rest rest = new Chilkat.Rest();
            bool success;
            Thread headerT = new Thread(() =>
            {
                header("Anime Recognition - Processing...");
            });
            headerT.Start();

            // URL: https://uguu.se/upload.php
            bool bTls = true;
            int port = 443;
            bool bAutoReconnect = true;
            success = rest.Connect("uguu.se", port, bTls, bAutoReconnect);
            if (success != true)
            {
                Debug.WriteLine("ConnectFailReason: " + Convert.ToString(rest.ConnectFailReason));
                Debug.WriteLine(rest.LastErrorText);
            }

            rest.PartSelector = "1";
            Chilkat.Stream fileStream1 = new Chilkat.Stream();
            fileStream1.SourceFile = path;
            rest.AddHeader("Content-Disposition", "form-data; name=\"files[]\"; filename=\"yourfile.jpeg\"");
            rest.AddHeader("Content-Type", "image/jpeg");
            rest.SetMultipartBodyStream(fileStream1);

            rest.PartSelector = "0";

            rest.AddHeader("Expect", "100-continue");

            rest.PartSelector = "0";
            rest.AddHeader("Content-Type", "multipart/form-data");

            string strResponseBody = rest.FullRequestMultipart("POST", "/upload.php");
            if (rest.LastMethodSuccess != true)
            {
                Debug.WriteLine(rest.LastErrorText);
            }

            int respStatusCode = rest.ResponseStatusCode;
            Debug.WriteLine("response status code = " + Convert.ToString(respStatusCode));
            if (respStatusCode <= 400)
            {
                Debug.WriteLine("Response Status Code = " + Convert.ToString(respStatusCode));
                Debug.WriteLine("Response Header:");
                Debug.WriteLine(rest.ResponseHeader);
                Debug.WriteLine("Response Body:");
                Debug.WriteLine(strResponseBody);
            }

            Chilkat.JsonObject jsonResponse = new Chilkat.JsonObject();


            string Ourl = parseJson(strResponseBody);
            header("Anime Recognition - Processing...");
            return Ourl;
        }

        public string Recognize(string path)
        {

            string url = uploadImage(path);
            Chilkat.Rest rest = new Chilkat.Rest();
            bool success;
            Thread headerT = new Thread(() =>
            {
                header("Anime Recognition - Processing...");
            });
            headerT.Start();

            // URL: https://api.trace.moe/search?url=https://images.plurk.com/32B15UXxymfSMwKGTObY5e.jpg
            bool bTls = true;
            int port = 443;
            bool bAutoReconnect = true;
            success = rest.Connect("api.trace.moe", port, bTls, bAutoReconnect);
            if (success != true)
            {
                Debug.WriteLine("ConnectFailReason: " + Convert.ToString(rest.ConnectFailReason));
                Debug.WriteLine(rest.LastErrorText);
            }

            Chilkat.StringBuilder sbResponseBody = new Chilkat.StringBuilder();
            success = rest.FullRequestNoBodySb("GET", $"/search?url={url}", sbResponseBody);
            if (success != true)
            {
                Debug.WriteLine(rest.LastErrorText);
            }

            int respStatusCode = rest.ResponseStatusCode;
            Debug.WriteLine("response status code = " + Convert.ToString(respStatusCode));
            if (respStatusCode >= 400)
            {
                Debug.WriteLine(rest.ResponseHeader); ;
                Debug.WriteLine(sbResponseBody.GetAsString());
            }

            Chilkat.JsonObject jsonResponse = new Chilkat.JsonObject();
            jsonResponse.LoadSb(sbResponseBody);

            jsonResponse.EmitCompact = false;
            Debug.WriteLine(jsonResponse.Emit());

            var results = parseTrace(sbResponseBody.ToString());
            changeStates(results);

            return sbResponseBody.ToString();
        }

        private void StackPanel_Drop(object sender, DragEventArgs e)
        {
            DropText.Dispatcher.Invoke((Action)(() => DropText.FontSize = 15));
            DropText.Dispatcher.Invoke((Action)(() => DropText.Foreground = new SolidColorBrush(Colors.White) { Opacity = 1 }));
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Debug.WriteLine(Path.GetExtension(files[0]));
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (Path.GetExtension(files[0]) == ".jpg")
                {
                    string filename = Path.GetFileName(files[0]);
                    string path = Path.GetFullPath(files[0]);

                    var img = System.Drawing.Image.FromFile(path);
                    string resolution = img.Width + "x" + img.Height;

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path);
                    bitmap.EndInit();

                    DropText.Dispatcher.Invoke((Action)(() => DropText.Text = "Processing..."));
                    FileName.Dispatcher.Invoke((Action)(() => FileName.Text = filename));
                    Resolution.Dispatcher.Invoke((Action)(() => Resolution.Text = resolution));
                    DropBG.Dispatcher.Invoke((Action)(() => DropBG.Source = bitmap));
                    DropBG.Dispatcher.Invoke((Action)(() => DropBG.Opacity = 0.5));
                    Thread thread = new Thread(() =>
                    {
                        Recognize(path);
                    });
                    thread.Start();
                    return;
                }
                if (Path.GetExtension(files[0]) == ".png")
                {

                    string filename = Path.GetFileName(files[0]);
                    string path = Path.GetFullPath(files[0]);

                    var img = System.Drawing.Image.FromFile(path);
                    string resolution = img.Width + "x" + img.Height;

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path);
                    bitmap.EndInit();
                    DropText.Dispatcher.Invoke((Action)(() => DropText.Text = "Processing..."));

                    FileName.Dispatcher.Invoke((Action)(() => FileName.Text = filename));
                    Resolution.Dispatcher.Invoke((Action)(() => Resolution.Text = resolution));
                    DropBG.Dispatcher.Invoke((Action)(() => DropBG.Source = bitmap));
                    DropBG.Dispatcher.Invoke((Action)(() => DropBG.Opacity = 0.5));
                    Thread thread = new Thread(() =>
                    {
                        Recognize(path);
                    });
                    thread.Start();
                    return;
                }
                if (Path.GetExtension(files[0]) == ".gif")
                {

                    string filename = Path.GetFileName(files[0]);
                    string path = Path.GetFullPath(files[0]);

                    var img = System.Drawing.Image.FromFile(path);
                    string resolution = img.Width + "x" + img.Height;

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path);
                    bitmap.EndInit();

                    DropText.Dispatcher.Invoke((Action)(() => DropText.Text = "Processing..."));
                    FileName.Dispatcher.Invoke((Action)(() => FileName.Text = filename));
                    Resolution.Dispatcher.Invoke((Action)(() => Resolution.Text = resolution));
                    DropBG.Dispatcher.Invoke((Action)(() => DropBG.Source = bitmap));
                    DropBG.Dispatcher.Invoke((Action)(() => DropBG.Opacity = 0.5));
                    Thread thread = new Thread(() =>
                    {
                        Recognize(path);
                    });
                    thread.Start();
                    return;
                }
                else
                {
                    Header.Dispatcher.Invoke((Action)(() => Header.Text = "Anime Recognition - Invalid File Type"));
                    DropText.Dispatcher.Invoke((Action)(() => DropText.Text = "Invalid File Type"));
                    DropText.Dispatcher.Invoke((Action)(() => DropText.Foreground = new SolidColorBrush(Colors.Red) { Opacity = 1 }));
                }

            }
        }
        private void changeStates(Dictionary<int, string> results)
        {
            string program_files_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string EAIOPath = $"{program_files_path}\\EAIO\\imgs";
            Debug.WriteLine(results[0]);
            Debug.WriteLine(results[1]);
            float x = Convert.ToSingle(results[3]);
            TimeSpan span = TimeSpan.FromSeconds((double)(new decimal(x)));

            var Randname = RandomString(18);
            string filePath = $"{EAIOPath}\\{Randname}.png";

            Anime.Dispatcher.Invoke((Action)(() => Anime.Text = results[0]));
            Episode.Dispatcher.Invoke((Action)(() => Episode.Text = results[1]));
            Timestamp.Dispatcher.Invoke((Action)(() => Timestamp.Text = span.ToString(@"hh\:mm\:ss")));

            Thread ResultPic = new Thread(() =>
            {
                using (WebClient webClient = new WebClient())
                {
                    switch (results[2] == null)
                    {
                        case false:
                            var dataArr = webClient.DownloadData(results[2]);
                            using (MemoryStream mem = new MemoryStream(dataArr))
                            {
                                using (var yourImage = System.Drawing.Image.FromStream(mem))
                                {
                                    yourImage.Save(filePath, ImageFormat.Png);

                                }
                            }

                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(filePath);
                            bitmap.EndInit();
                            bitmap.Freeze();

                            DropText.Dispatcher.Invoke((Action)(() => DropText.Text = "Done"));
                            ResultBG.Dispatcher.Invoke((Action)(() => ResultBG.Source = bitmap));
                            DropText.Dispatcher.Invoke((Action)(() => DropText.FontSize = 15));
                            break;

                        case true:
                            DropText.Dispatcher.Invoke((Action)(() => DropText.Text = "Downloading result information"));
                            DropText.Dispatcher.Invoke((Action)(() => DropText.FontSize = 11));
                            break;
                    }
                }
            });

            ResultPic.Start();

            Header.Dispatcher.Invoke((Action)(() => Header.Text = "Anime Recognition"));


        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void header(string text)
        {
            Header.Dispatcher.BeginInvoke((Action)(() => Header.Text = $"Anime Recognition"));
        }

        private void AllResults_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = AllResults.Text;

            if (url == " - -")
            {
                return;
            }
            else
            {
                string command = $"/C start {url}";

                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.WorkingDirectory = @"C:\";
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.StartInfo.Arguments = command;
                process.Start();
            }
        }
    }
}
