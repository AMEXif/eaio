using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chilkat;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Web;
using System.Security.Cryptography;
using System.Net;

namespace eaio.MVVM.View
{
    class ACRCloudExtrTool
    {
        private int filter_energy_min_ = 50;
        private int silence_energy_threshold_ = 100;
        private float silence_rate_threshold_ = 0.99f;

        public ACRCloudExtrTool()
        {
            acr_init();
        }

        public ACRCloudExtrTool(int filter_energy_min, int silence_energy_threshold, float silence_rate_threshold)
        {
            this.filter_energy_min_ = filter_energy_min;
            this.silence_energy_threshold_ = silence_energy_threshold;
            this.silence_rate_threshold_ = silence_rate_threshold;

            acr_init();
        }

        public byte[] CreateFingerprint(byte[] pcmBuffer, int pcmBufferLen, bool isDB)
        {
            byte[] fpBuffer = null;
            if (pcmBuffer == null || pcmBufferLen <= 0)
            {
                return fpBuffer;
            }
            if (pcmBufferLen > pcmBuffer.Length)
            {
                pcmBufferLen = pcmBuffer.Length;
            }
            byte tIsDB = (isDB) ? (byte)1 : (byte)0;
            IntPtr pFpBuffer = IntPtr.Zero;
            int fpBufferLen = create_fingerprint(pcmBuffer, pcmBufferLen, tIsDB, this.filter_energy_min_, this.silence_energy_threshold_, this.silence_rate_threshold_, ref pFpBuffer);
            if (fpBufferLen <= 0)
            {
                return fpBuffer;
            }

            fpBuffer = new byte[fpBufferLen];
            Marshal.Copy(pFpBuffer, fpBuffer, 0, fpBufferLen);
            acr_free(pFpBuffer);

            return fpBuffer;
        }

        public byte[] CreateHummingFingerprint(byte[] pcmBuffer, int pcmBufferLen)
        {
            byte[] fpBuffer = null;
            if (pcmBuffer == null || pcmBufferLen <= 0)
            {
                return fpBuffer;
            }
            if (pcmBufferLen > pcmBuffer.Length)
            {
                pcmBufferLen = pcmBuffer.Length;
            }

            IntPtr pFpBuffer = IntPtr.Zero;
            int fpBufferLen = create_humming_fingerprint(pcmBuffer, pcmBufferLen, ref pFpBuffer);
            if (fpBufferLen <= 0)
            {
                return fpBuffer;
            }

            fpBuffer = new byte[fpBufferLen];
            Marshal.Copy(pFpBuffer, fpBuffer, 0, fpBufferLen);
            acr_free(pFpBuffer);

            return fpBuffer;
        }

        public byte[] CreateFingerprintByFile(string filePath, int startTimeSeconds, int audioLenSeconds, bool isDB)
        {
            byte[] fpBuffer = null;

            byte tIsDB = (isDB) ? (byte)1 : (byte)0;
            IntPtr pFpBuffer = IntPtr.Zero;
            int fpBufferLen = create_fingerprint_by_file(filePath, startTimeSeconds, audioLenSeconds, tIsDB, this.filter_energy_min_, this.silence_energy_threshold_, this.silence_rate_threshold_, ref pFpBuffer);
            switch (fpBufferLen)
            {
                case -1:
                    throw new Exception(filePath + " is not readable!");
                case -2:
                    throw new Exception(filePath + " can not be decoded audio data!");
            }
            if (fpBufferLen == 0)
            {
                return fpBuffer;
            }

            fpBuffer = new byte[fpBufferLen];
            Marshal.Copy(pFpBuffer, fpBuffer, 0, fpBufferLen);
            acr_free(pFpBuffer);

            return fpBuffer;
        }

        public byte[] CreateHummingFingerprintByFile(string filePath, int startTimeSeconds, int audioLenSeconds)
        {
            byte[] fpBuffer = null;

            IntPtr pFpBuffer = IntPtr.Zero;
            int fpBufferLen = create_humming_fingerprint_by_file(filePath, startTimeSeconds, audioLenSeconds, ref pFpBuffer);
            switch (fpBufferLen)
            {
                case -1:
                    throw new Exception(filePath + " is not readable!");
                case -2:
                    throw new Exception(filePath + " can not be decoded audio data!");
            }
            if (fpBufferLen == 0)
            {
                return fpBuffer;
            }

            fpBuffer = new byte[fpBufferLen];
            Marshal.Copy(pFpBuffer, fpBuffer, 0, fpBufferLen);
            acr_free(pFpBuffer);

            return fpBuffer;
        }
        public byte[] CreateFingerprintByFileBuffer(byte[] fileBuffer, int fileBufferLen, int startTimeSeconds, int audioLenSeconds, bool isDB)
        {
            byte[] fpBuffer = null;
            if (fileBufferLen > fileBuffer.Length)
            {
                fileBufferLen = fileBuffer.Length;
            }

            byte tIsDB = (isDB) ? (byte)1 : (byte)0;
            IntPtr pFpBuffer = IntPtr.Zero;
            int fpBufferLen = create_fingerprint_by_filebuffer(fileBuffer, fileBufferLen, startTimeSeconds, audioLenSeconds, tIsDB, this.filter_energy_min_, this.silence_energy_threshold_, this.silence_rate_threshold_, ref pFpBuffer);
            switch (fpBufferLen)
            {
                case -1:
                    throw new Exception("fileBuffer is not audio/video data!");
                case -2:
                    throw new Exception("fileBuffer can not be decoded audio data!");
            }
            if (fpBufferLen == 0)
            {
                return fpBuffer;
            }

            fpBuffer = new byte[fpBufferLen];
            Marshal.Copy(pFpBuffer, fpBuffer, 0, fpBufferLen);
            acr_free(pFpBuffer);
            return fpBuffer;
        }
        public byte[] CreateHummingFingerprintByFileBuffer(byte[] fileBuffer, int fileBufferLen, int startTimeSeconds, int audioLenSeconds)
        {
            byte[] fpBuffer = null;
            if (fileBufferLen > fileBuffer.Length)
            {
                fileBufferLen = fileBuffer.Length;
            }

            IntPtr pFpBuffer = IntPtr.Zero;
            int fpBufferLen = create_humming_fingerprint_by_filebuffer(fileBuffer, fileBufferLen, startTimeSeconds, audioLenSeconds, ref pFpBuffer);
            switch (fpBufferLen)
            {
                case -1:
                    throw new Exception("fileBuffer is not audio/video data!");
                case -2:
                    throw new Exception("fileBuffer can not be decoded audio data!");
            }
            if (fpBufferLen == 0)
            {
                return fpBuffer;
            }

            fpBuffer = new byte[fpBufferLen];
            Marshal.Copy(pFpBuffer, fpBuffer, 0, fpBufferLen);
            acr_free(pFpBuffer);
            return fpBuffer;
        }
        public byte[] DecodeAudioByFile(string filePath, int startTimeSeconds, int audioLenSeconds)
        {
            byte aa = 1;
            acr_set_debug(aa);
            byte[] audioBuffer = null;

            IntPtr pAudioBuffer = IntPtr.Zero;
            int fpBufferLen = decode_audio_by_file(filePath, startTimeSeconds, audioLenSeconds, ref pAudioBuffer);
            switch (fpBufferLen)
            {
                case -1:
                    throw new Exception(filePath + " is not readable!");
                case -2:
                    throw new Exception(filePath + " can not be decoded audio data!");
            }
            if (fpBufferLen == 0)
            {
                return audioBuffer;
            }

            audioBuffer = new byte[fpBufferLen];
            Marshal.Copy(pAudioBuffer, audioBuffer, 0, fpBufferLen);
            acr_free(pAudioBuffer);

            return audioBuffer;
        }
        public byte[] DecodeAudioByFileBuffer(byte[] fileBuffer, int fileBufferLen, int startTimeSeconds, int audioLenSeconds)
        {
            byte[] audioBuffer = null;

            if (fileBufferLen > fileBuffer.Length)
            {
                fileBufferLen = fileBuffer.Length;
            }
            IntPtr pAudioBuffer = IntPtr.Zero;
            int fpBufferLen = decode_audio_by_filebuffer(fileBuffer, fileBufferLen, startTimeSeconds, audioLenSeconds, ref pAudioBuffer);
            switch (fpBufferLen)
            {
                case -1:
                    throw new Exception("fileBuffer is not audio/video data!");
                case -2:
                    throw new Exception("fileBuffer can not be decoded audio data!");
            }
            if (fpBufferLen == 0)
            {
                return audioBuffer;
            }

            audioBuffer = new byte[fpBufferLen];
            Marshal.Copy(pAudioBuffer, audioBuffer, 0, fpBufferLen);
            acr_free(pAudioBuffer);
            return audioBuffer;
        }
        public int GetDurationMillisecondByFile(string filePath)
        {
            return get_duration_ms_by_file(filePath);
        }

        [DllImport("libacrcloud_extr_tool.dll")]
        private static extern int create_fingerprint(byte[] pcm_buffer, int pcm_buffer_len, byte is_db_fingerprint, int filter_energy_min, int silence_energy_threshold, float silence_rate_threshold, ref IntPtr fps_buffer);
        [DllImport("libacrcloud_extr_tool.dll")]
        private static extern int create_humming_fingerprint(byte[] pcm_buffer, int pcm_buffer_len, ref IntPtr fps_buffer);
        [DllImport("libacrcloud_extr_tool.dll")]
        private static extern int create_fingerprint_by_file(string file_path, int start_time_seconds, int audio_len_seconds, byte is_db_fingerprint, int filter_energy_min, int silence_energy_threshold, float silence_rate_threshold, ref IntPtr fps_buffer);
        [DllImport("libacrcloud_extr_tool.dll")]
        private static extern int create_humming_fingerprint_by_file(string file_path, int start_time_seconds, int audio_len_seconds, ref IntPtr fps_buffer);
        [DllImport("libacrcloud_extr_tool.dll")]
        private static extern int create_fingerprint_by_filebuffer(byte[] file_buffer, int file_buffer_len, int start_time_seconds, int audio_len_seconds, byte is_db_fingerprint, int filter_energy_min, int silence_energy_threshold, float silence_rate_threshold, ref IntPtr fps_buffer);
        [DllImport("libacrcloud_extr_tool.dll")]
        private static extern int create_humming_fingerprint_by_filebuffer(byte[] file_buffer, int file_buffer_len, int start_time_seconds, int audio_len_seconds, ref IntPtr fps_buffer);
        [DllImport("libacrcloud_extr_tool.dll")]
        private static extern int decode_audio_by_file(string file_path, int start_time_seconds, int audio_len_seconds, ref IntPtr audio_buffer);
        [DllImport("libacrcloud_extr_tool.dll")]
        private static extern int decode_audio_by_filebuffer(byte[] file_buffer, int file_buffer_len, int start_time_seconds, int audio_len_seconds, ref IntPtr audio_buffer);
        [DllImport("libacrcloud_extr_tool.dll")]
        private static extern void acr_free(IntPtr buffer);
        [DllImport("libacrcloud_extr_tool.dll")]
        private static extern int get_duration_ms_by_file(string file_path);
        [DllImport("libacrcloud_extr_tool.dll")]
        public static extern void acr_set_debug(byte is_debug);
        [DllImport("libacrcloud_extr_tool.dll")]
        private static extern void acr_init();
    }

    public class ACRCloudStatusCode
    {
        public static string HTTP_ERROR = "{\"status\":{\"msg\":\"Http Error\", \"code\":3000}}";
        public static string NO_RESULT = "{\"status\":{\"msg\":\"No Result\", \"code\":1001}}";
        public static string GEN_FP_ERROR = "{\"status\":{\"msg\":\"Gen Fingerprint Error\", \"code\":2004}}";
        public static string DECODE_AUDIO_ERROR = "{\"status\":{\"msg\":\"Can not decode audio data\", \"code\":2004}}";
        public static string RECORD_ERROR = "{\"status\":{\"msg\":\"Record Error\", \"code\":2000}}";
        public static string JSON_ERROR = "{\"status\":{\"msg\":\"json error\", \"code\":2002}}";
        public static string MUTE_ERROR = "{\"status\":{\"msg\":\"May Be Mute\", \"code\":2006}}";
    }

    class ACRCloudRecognizer
    {
        public enum RECOGNIZER_TYPE
        {
            acr_rec_type_audio, acr_rec_type_humming, acr_rec_type_both
        };
        private string host = "";
        private string accessKey = "";
        private string accessSecret = "";
        private int timeout = 5 * 1000; // ms
        private RECOGNIZER_TYPE rec_type = RECOGNIZER_TYPE.acr_rec_type_audio;
        private int filter_energy_min = 50;
        private int silence_energy_threshold = 50;
        private float silence_rate_threshold = 0.99f;

        private ACRCloudExtrTool acrTool = null;

        public ACRCloudRecognizer(IDictionary<string, Object> config)
        {
            if (config.ContainsKey("host"))
            {
                this.host = (string)config["host"];
            }
            if (config.ContainsKey("access_key"))
            {
                this.accessKey = (string)config["access_key"];
            }
            if (config.ContainsKey("access_secret"))
            {
                this.accessSecret = (string)config["access_secret"];
            }
            if (config.ContainsKey("timeout"))
            {
                this.timeout = 1000 * (int)config["timeout"];
            }
            if (config.ContainsKey("rec_type"))
            {
                this.rec_type = (RECOGNIZER_TYPE)config["rec_type"];
            }
            if (config.ContainsKey("filter_energy_min"))
            {
                this.filter_energy_min = (int)config["filter_energy_min"];
            }
            if (config.ContainsKey("silence_energy_threshold"))
            {
                this.silence_energy_threshold = (int)config["silence_energy_threshold"];
            }
            if (config.ContainsKey("silence_rate_threshold"))
            {
                this.silence_rate_threshold = (float)config["silence_rate_threshold"];
            }

            this.acrTool = new ACRCloudExtrTool(this.filter_energy_min, this.silence_energy_threshold, this.silence_rate_threshold);
        }
        public string Recognize(byte[] wavAudioBuffer, int wavAudioBufferLen)
        {
            byte[] ext_fp = null;
            byte[] hum_fp = null;
            IDictionary<string, Object> query_data = new Dictionary<string, Object>();
            switch (this.rec_type)
            {
                case RECOGNIZER_TYPE.acr_rec_type_audio:
                    ext_fp = this.acrTool.CreateFingerprint(wavAudioBuffer, wavAudioBufferLen, false);
                    query_data.Add("ext_fp", ext_fp);
                    break;
                case RECOGNIZER_TYPE.acr_rec_type_humming:
                    hum_fp = this.acrTool.CreateHummingFingerprint(wavAudioBuffer, wavAudioBufferLen);
                    query_data.Add("hum_fp", hum_fp);
                    break;
                case RECOGNIZER_TYPE.acr_rec_type_both:
                    ext_fp = this.acrTool.CreateFingerprint(wavAudioBuffer, wavAudioBufferLen, false);
                    query_data.Add("ext_fp", ext_fp);
                    hum_fp = this.acrTool.CreateHummingFingerprint(wavAudioBuffer, wavAudioBufferLen);
                    query_data.Add("hum_fp", hum_fp);
                    break;
                default:
                    return ACRCloudStatusCode.NO_RESULT;
            }

            if (ext_fp == null && hum_fp == null)
            {
                return ACRCloudStatusCode.MUTE_ERROR;
            }

            return this.DoRecognize(query_data);
        }
        public String RecognizeByFile(string filePath, int startSeconds)
        {
            byte[] ext_fp = null;
            byte[] hum_fp = null;
            IDictionary<string, Object> query_data = new Dictionary<string, Object>();
            try
            {
                switch (this.rec_type)
                {
                    case RECOGNIZER_TYPE.acr_rec_type_audio:
                        ext_fp = this.acrTool.CreateFingerprintByFile(filePath, startSeconds, 12, false);
                        query_data.Add("ext_fp", ext_fp);
                        break;
                    case RECOGNIZER_TYPE.acr_rec_type_humming:
                        hum_fp = this.acrTool.CreateHummingFingerprintByFile(filePath, startSeconds, 12);
                        query_data.Add("hum_fp", hum_fp);
                        break;
                    case RECOGNIZER_TYPE.acr_rec_type_both:
                        ext_fp = this.acrTool.CreateFingerprintByFile(filePath, startSeconds, 12, false);
                        query_data.Add("ext_fp", ext_fp);
                        hum_fp = this.acrTool.CreateHummingFingerprintByFile(filePath, startSeconds, 12);
                        query_data.Add("hum_fp", hum_fp);
                        break;
                    default:
                        return ACRCloudStatusCode.NO_RESULT;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return ACRCloudStatusCode.DECODE_AUDIO_ERROR;
            }

            if (ext_fp == null && hum_fp == null)
            {
                return ACRCloudStatusCode.MUTE_ERROR;
            }
            return this.DoRecognize(query_data);
        }
        public String RecognizeAudioByFile(string filePath, int startSeconds, int audioLenSeconds = 20)
        {
            byte[] pcm_buffer = null;
            IDictionary<string, Object> query_data = new Dictionary<string, Object>();
            try
            {
                pcm_buffer = this.acrTool.DecodeAudioByFile(filePath, startSeconds, audioLenSeconds);
                query_data.Add("audio_pcm", pcm_buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return ACRCloudStatusCode.DECODE_AUDIO_ERROR;
            }

            return this.DoRecognize(query_data);
        }
        public String RecognizeByFileBuffer(byte[] fileBuffer, int fileBufferLen, int startSeconds)
        {
            byte[] ext_fp = null;
            byte[] hum_fp = null;
            IDictionary<string, Object> query_data = new Dictionary<string, Object>();
            try
            {
                switch (this.rec_type)
                {
                    case RECOGNIZER_TYPE.acr_rec_type_audio:
                        ext_fp = this.acrTool.CreateFingerprintByFileBuffer(fileBuffer, fileBufferLen, startSeconds, 12, false);
                        query_data.Add("ext_fp", ext_fp);
                        break;
                    case RECOGNIZER_TYPE.acr_rec_type_humming:
                        hum_fp = this.acrTool.CreateHummingFingerprintByFileBuffer(fileBuffer, fileBufferLen, startSeconds, 12);
                        query_data.Add("hum_fp", hum_fp);
                        break;
                    case RECOGNIZER_TYPE.acr_rec_type_both:
                        ext_fp = this.acrTool.CreateFingerprintByFileBuffer(fileBuffer, fileBufferLen, startSeconds, 12, false);
                        query_data.Add("ext_fp", ext_fp);
                        hum_fp = this.acrTool.CreateHummingFingerprintByFileBuffer(fileBuffer, fileBufferLen, startSeconds, 12);
                        query_data.Add("hum_fp", hum_fp);
                        break;
                    default:
                        return ACRCloudStatusCode.NO_RESULT;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return ACRCloudStatusCode.DECODE_AUDIO_ERROR;
            }

            if (ext_fp == null && hum_fp == null)
            {
                return ACRCloudStatusCode.MUTE_ERROR;
            }

            return this.DoRecognize(query_data);
        }

        private string PostHttp(string url, IDictionary<string, Object> postParams)
        {
            string result = "";

            string BOUNDARYSTR = "acrcloud***copyright***2015***" + DateTime.Now.Ticks.ToString("x");
            string BOUNDARY = "--" + BOUNDARYSTR + "\r\n";
            var ENDBOUNDARY = Encoding.ASCII.GetBytes("--" + BOUNDARYSTR + "--\r\n\r\n");

            var stringKeyHeader = BOUNDARY +
                           "Content-Disposition: form-data; name=\"{0}\"" +
                           "\r\n\r\n{1}\r\n";
            var filePartHeader = BOUNDARY +
                            "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                            "Content-Type: application/octet-stream\r\n\r\n";

            var memStream = new MemoryStream();
            foreach (var item in postParams)
            {
                if (item.Value is string)
                {
                    string tmpStr = string.Format(stringKeyHeader, item.Key, item.Value);
                    byte[] tmpBytes = Encoding.UTF8.GetBytes(tmpStr);
                    memStream.Write(tmpBytes, 0, tmpBytes.Length);
                }
                else if (item.Value is byte[])
                {
                    var header = string.Format(filePartHeader, item.Key, item.Key);
                    var headerbytes = Encoding.UTF8.GetBytes(header);
                    memStream.Write(headerbytes, 0, headerbytes.Length);
                    byte[] sample = (byte[])item.Value;
                    memStream.Write(sample, 0, sample.Length);
                    memStream.Write(Encoding.UTF8.GetBytes("\r\n"), 0, 2);
                }
            }
            memStream.Write(ENDBOUNDARY, 0, ENDBOUNDARY.Length);

            HttpWebRequest request = null;
            HttpWebResponse response = null;
            System.IO.Stream writer = null;
            StreamReader myReader = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = this.timeout;
                request.Method = "POST";
                request.ContentType = "multipart/form-data; boundary=" + BOUNDARYSTR;

                memStream.Position = 0;
                byte[] tempBuffer = new byte[memStream.Length];
                memStream.Read(tempBuffer, 0, tempBuffer.Length);

                writer = request.GetRequestStream();
                writer.Write(tempBuffer, 0, tempBuffer.Length);
                writer.Flush();
                writer.Close();
                writer = null;

                response = (HttpWebResponse)request.GetResponse();
                myReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = myReader.ReadToEnd();
            }
            catch (WebException e)
            {
                Console.WriteLine("timeout:\n" + e.ToString());
                result = ACRCloudStatusCode.HTTP_ERROR;
            }
            catch (Exception e)
            {
                Console.WriteLine("other excption:" + e.ToString());
                result = ACRCloudStatusCode.HTTP_ERROR;
            }
            finally
            {
                if (memStream != null)
                {
                    memStream.Close();
                    memStream = null;
                }
                if (writer != null)
                {
                    writer.Close();
                    writer = null;
                }
                if (myReader != null)
                {
                    myReader.Close();
                    myReader = null;
                }
                if (request != null)
                {
                    request.Abort();
                    request = null;
                }
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }

            return result;
        }

        private string EncryptByHMACSHA1(string input, string key)
        {
            HMACSHA1 hmac = new HMACSHA1(System.Text.Encoding.UTF8.GetBytes(key));
            byte[] stringBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashedValue = hmac.ComputeHash(stringBytes);
            return EncodeToBase64(hashedValue);
        }

        private string EncodeToBase64(byte[] input)
        {
            string res = Convert.ToBase64String(input, 0, input.Length);
            return res;
        }

        private string DoRecognize(IDictionary<string, Object> query_data)
        {
            byte[] ext_fp = null;
            byte[] hum_fp = null;
            byte[] audio_pcm = null;
            string method = "POST";
            string httpURL = "/v1/identify";
            string dataType = "fingerprint";
            string sigVersion = "1";
            string timestamp = ((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();

            string reqURL = "http://" + host + httpURL;

            string sigStr = method + "\n" + httpURL + "\n" + accessKey + "\n" + dataType + "\n" + sigVersion + "\n" + timestamp;
            string signature = EncryptByHMACSHA1(sigStr, this.accessSecret);

            var dict = new Dictionary<string, object>();
            dict.Add("access_key", this.accessKey);
            dict.Add("data_type", dataType);

            if (query_data.ContainsKey("ext_fp"))
            {
                ext_fp = (byte[])query_data["ext_fp"];
                if (ext_fp != null)
                {
                    dict.Add("sample_bytes", ext_fp.Length.ToString());
                    dict.Add("sample", ext_fp);
                }
            }
            if (query_data.ContainsKey("audio_pcm"))
            {
                audio_pcm = (byte[])query_data["audio_pcm"];
                if (audio_pcm != null)
                {
                    dict.Add("sample_bytes", audio_pcm.Length.ToString());
                    dict.Add("sample", audio_pcm);
                    dataType = "audio";
                    dict["data_type"] = dataType;
                    sigStr = method + "\n" + httpURL + "\n" + accessKey + "\n" + dataType + "\n" + sigVersion + "\n" + timestamp;
                    signature = EncryptByHMACSHA1(sigStr, this.accessSecret);
                }
            }
            if (query_data.ContainsKey("hum_fp"))
            {
                hum_fp = (byte[])query_data["hum_fp"];
                if (hum_fp != null)
                {
                    dict.Add("sample_hum_bytes", hum_fp.Length.ToString());
                    dict.Add("sample_hum", hum_fp);
                }
            }
            if (ext_fp == null && hum_fp == null && audio_pcm == null)
            {
                return ACRCloudStatusCode.NO_RESULT;
            }
            dict.Add("timestamp", timestamp);
            dict.Add("signature", signature);
            dict.Add("signature_version", sigVersion);

            string res = PostHttp(reqURL, dict);

            return res;
        }
    }

    
    public partial class MuRecognitionView : UserControl
    {
        public MuRecognitionView()
        {
            InitializeComponent();
        }

        public string spotifyLink = "";
        public string youtubeLink = "";
        public string Clipboard = "";

        public Dictionary<int, string> ParseMeBaby(string response)
        {
            Dictionary<int, string> data = new Dictionary<int, string>();

            Chilkat.JsonObject json = new Chilkat.JsonObject();
            json.Load(response);

            string title = "";
            string msg = json.StringOf("status.msg"); ;
            int j;
            string Id = "";
            int count_j;
            string name = "";

            int i = 0;
            int count_i = json.SizeOfArray("metadata.music");
            while (i < count_i)
            {
                json.I = i;
                title = json.StringOf("metadata.music[i].title");
                j = 0;
                Id = json.StringOf("metadata.music[i].external_metadata.spotify.track.id");
                count_j = json.SizeOfArray("metadata.music[i].artists");
                while (j < count_j)
                {
                    json.J = j;
                    name = json.StringOf("metadata.music[i].artists[j].name");
                    j = j + 1;
                }

                i = i + 1;
            }

            data.Add(0, title);
            data.Add(1, name);
            data.Add(2, Id);
            data.Add(3, msg);

            return data;
        }
        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Debug.WriteLine(files[0]);

            string filetype = Path.GetExtension(files[0]);
            string[] supportedFiles = { ".mp3", ".wav", ".m4a" };

            switch (supportedFiles.Contains(filetype))
            {
                case true:
                    PlayBtn.ContentStringFormat = "Play";
                    Media.Stop();
                    Media.Volume = 100;
                    DropText.Foreground = new SolidColorBrush(Colors.White);
                    Media.Source = new Uri(files[0]);
                    PlayBtn.Opacity = 1;
                    DropText.Text = Path.GetFileName(files[0]);
                    getInfo(files[0]);
                    Recognize(files[0]);
                    break;

                default:
                    DropText.Text = "Wrong file type";
                    DropText.Foreground = new SolidColorBrush(Colors.Red);
                    Media.Stop();
                    PlayBtn.Opacity = 0;
                    Media.Volume = 0;
                    break;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string state = PlayBtn.ContentStringFormat;
            switch (state)
            {
                case "Play":
                    Media.Play();
                    PlayBtn.ContentStringFormat = "Pause";
                    break;
                case "Pause":
                    Media.Pause();
                    PlayBtn.ContentStringFormat = "Play";
                    break;
            }
        }
        private void getInfo(string path)
        {
            FileInfo fi = new FileInfo(path);
            float MB = 1024 * 1024;
            float KB = 1024;

            filename.Text = $"File Name: {Path.GetFileName(path)}";
            float filesizeB = fi.Length / MB;

            if (filesizeB < 1)
            {
                filesizeB = fi.Length / KB;
                filesize.Text = $"File Size: {Math.Round(filesizeB, 0)} KB";
            }
            else
            {
                filesize.Text = $"File Size: {Math.Round(filesizeB, 1)} MB";
            }
        }
        private void Recognize(string path)
        {
            Thread Recognizer = new Thread(() =>
            {
                var config = new Dictionary<string, object>();

                config.Add("host", "");
                config.Add("access_key", "");
                config.Add("access_secret", "");
                config.Add("timeout", 10);

                ACRCloudRecognizer re = new ACRCloudRecognizer(config);

                string result = re.RecognizeByFile(path, 0);
                Dictionary<int, string> output = ParseMeBaby(result);
                Debug.WriteLine(result);
                Debug.WriteLine(output);

                switch (output[3])
                {
                    case "Success":
                        songname.Dispatcher.Invoke((Action)(() => songname.Text = $"Song name: {output[0]}"));
                        songname.Dispatcher.Invoke((Action)(() => songname.Opacity = 1));
                        songartist.Dispatcher.Invoke((Action)(() => songartist.Text = $"Artist: {output[1]}"));
                        songartist.Dispatcher.Invoke((Action)(() => songartist.Opacity = 1));
                        if (string.IsNullOrEmpty(output[2]) == false)
                        {
                            spotifyLink = $"https://open.spotify.com/track/{output[2]}";
                            spotify.Dispatcher.Invoke((Action)(() => spotify.Opacity = 1));
                            youtube.Dispatcher.Invoke((Action)(() => youtubeShift.X = 100));
                        }
                        if (string.IsNullOrEmpty(output[2]) == true)
                        {
                            spotifyLink = "";
                            spotify.Dispatcher.Invoke((Action)(() => spotify.Opacity = 0));
                            youtube.Dispatcher.Invoke((Action)(() => youtubeShift.X = 0));
                        }
                        Clipboard = $"{output[0]} - {output[1]}";
                        youtubeLink = $"https://www.youtube.com/results?search_query={Clipboard.Replace(" ", "+")}";
                        youtube.Dispatcher.Invoke((Action)(() => youtube.Opacity = 1));
                        CopyBtn.Dispatcher.Invoke((Action)(() => CopyBtn.Opacity = 1));
                        break;
                    default:
                        songname.Dispatcher.Invoke((Action)(() => songname.Text = $"No result"));
                        songname.Dispatcher.Invoke((Action)(() => songname.Opacity = 1));
                        songartist.Dispatcher.Invoke((Action)(() => songartist.Opacity = 0));
                        spotifyLink = "";
                        youtubeLink = "";
                        youtube.Dispatcher.Invoke((Action)(() => youtube.Opacity = 0));
                        spotify.Dispatcher.Invoke((Action)(() => spotify.Opacity = 0));
                        CopyBtn.Dispatcher.Invoke((Action)(() => CopyBtn.Opacity = 0));
                        Clipboard = $"";
                        break;
                }
            });
            Recognizer.Start();
        }

        private void spotify_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = spotifyLink;

            if (url == "")
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

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (string.IsNullOrEmpty(Clipboard))
            {
                case false:
                    System.Windows.Clipboard.SetText(Clipboard);
                    break;
                default:
                    break;
            }
            
        }

        private void youtube_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = youtubeLink;

            if (url == "")
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