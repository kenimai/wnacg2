using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace wnacg2
{
    public class DownloadImages
    {
        public string url { get; set; }
        public string caption { get; set; }
    }

    public class Program
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static IntPtr ThisConsole = GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;

        static async Task Main(string[] args)
        {
            //Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            ShowWindow(ThisConsole, MAXIMIZE);

            //OUTPUT設定
            System.Console.OutputEncoding = Encoding.UTF8;

            string FilePath = @"K:\";

            string HtmlTitle = "";
            string HtmlBoby = "";

            string DownloadTitle = "";

            //輸入作品編號 
            Console.WriteLine("請輸入作品編號 : ");
            string DataNumber = Console.ReadLine().Trim();

            //作品標題網頁
            string DataTitleUrl = "https://www.wnacg.org/photos-slide-aid-" + DataNumber + ".html";

            //作品圖片網頁
            //string DataImagesUrl = "https://www.wnacg.org/photos-gallery-aid-" + DataNumber + ".html";
            string DataImagesUrl = "https://www.wnacg.org/photos-webp-aid-" + DataNumber + ".html";

            //取得作品標題
            using (WebClient DataTitleClient = new WebClient())
            {
                DataTitleClient.Encoding = Encoding.UTF8;

                // 指定 WebClient 的 Content-Type header
                //DataClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                HtmlTitle = DataTitleClient.DownloadString(DataTitleUrl);

                if (HtmlTitle.Length > 0)
                {
                    //string DataTitlePattern = @"<title>\s*(.+?)\s*</title>";
                    string DataTitlePattern = @"(?<=<title>)(.*)(?=</title>)";

                    Regex r = new Regex(DataTitlePattern);

                    Match MatchTitle = r.Match(HtmlTitle);

                    if (MatchTitle.Success)
                    {
                        //作品名稱  
                        DownloadTitle = MatchTitle.Groups[0].Value;
                        DownloadTitle = DownloadTitle.Replace("- 列表 - 紳士漫畫-專註分享漢化本子&#124;邪惡漫畫", "").Trim();

                        //移除不必要標題內容
                        DownloadTitle = DownloadTitle.Replace("[中国翻訳]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[中国翻译]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[中國翻譯]", "").Trim();


                        //移除漢化組名稱
                        DownloadTitle = DownloadTitle.Replace("[羅莎莉亞漢化]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[翻车汉化组]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[绅士仓库汉化]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[靴下汉化组]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[無邪気漢化組]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[甜族星人x我不看本子个人汉化]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[黑锅汉化组]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[夢之行蹤漢化組]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[白杨汉化组]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[黎欧出资汉化]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[不咕鸟汉化组]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[君日本語本當上手漢化組]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[空気系☆漢化]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[黑条汉化]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[CE家族社]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[无毒汉化组]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[紫苑汉化组]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[新桥月白日语社汉化]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[大鸟可不敢乱转汉化]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[裸單騎漢化]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[流木个人汉化]", "").Trim();

                        Console.WriteLine("作品名稱 : " + DownloadTitle);
                    }
                }
            }

            using (WebClient DataClient = new WebClient())
            {
                // 指定 WebClient 的編碼
                DataClient.Encoding = Encoding.UTF8;

                // 指定 WebClient 的 Content-Type header
                //DataClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                // 從網路 url 上取得資料
                HtmlBoby = DataClient.DownloadString(DataImagesUrl);
            }

            if (HtmlBoby.Length > 0)
            {
                string DataJson = "";
                string DataPattern = @"(?<=var imglist =)(.*)";

                Regex r = new Regex(DataPattern);

                Match MatchData = r.Match(HtmlBoby);

                if (MatchData.Success)
                {
                    DataJson = MatchData.Groups[0].Value.Trim();
                    DataJson = DataJson.Replace(";\");", "");
                    //DataJson = DataJson.Replace("fast_img_host+\\\"", "\"https:");

                    DataJson = DataJson.Replace("fast_img_host+\\\"", "");
                    DataJson = DataJson.Replace("\\\\\\\"", "");

                    DataJson = DataJson.Replace("//", "\"");
                    DataJson = DataJson.Replace(@"\""", "\"");
                    DataJson = DataJson.Replace("url: /themes/weitu/", "url: \"themes/weitu/");

                    DataJson = DataJson.Replace("url:", "\"url\":");
                    DataJson = DataJson.Replace("caption:", "\"caption\":");

                    DataJson = "{ \"DownloadImages\" : " + DataJson + "}";

                    //寫入TXT
                    /*
                    string TxtFile = FilePath + DataNumber + ".txt";
                    StreamWriter sw = new StreamWriter(TxtFile, false, Encoding.UTF8);
                    sw.WriteLine(DataJson);
                    sw.Close();
                    */

                    if (DataJson.Length > 0)
                    {
                        //移除不合法字元
                        DownloadTitle = DownloadTitle.Replace(">", "");
                        DownloadTitle = DownloadTitle.Replace("<", "");
                        DownloadTitle = DownloadTitle.Replace(":", "");
                        DownloadTitle = DownloadTitle.Replace("|", "");
                        DownloadTitle = DownloadTitle.Replace("?", "");
                        DownloadTitle = DownloadTitle.Replace("*", "");

                        //作品目錄路徑
                        string DirectoryPath = FilePath + DownloadTitle + @"\";

                        //建立作品目錄
                        if (string.IsNullOrEmpty(DownloadTitle) == false)
                        {
                            Directory.CreateDirectory(DirectoryPath);
                        }

                        using (var client = new HttpClient())
                        {
                            if (Directory.Exists(DirectoryPath))
                            {
                                dynamic DI = JsonConvert.DeserializeObject(DataJson);

                                int ImgTotalCount = 0;
                                int ImgCount = 1;

                                if (((JArray)DI.DownloadImages).Count > 0)
                                {
                                    ImgTotalCount = ((JArray)DI.DownloadImages).Count;
                                }

                                foreach (var img in DI.DownloadImages)
                                {
                                    string ImgURL = img.url;
                                    string ImgWebp = img.webp;
                                    string ImgCaption = img.caption;
                                    string DownloadImagesName = "";

                                    //下載URL
                                    string DownloadImagesUrl = "https://" + ImgURL;

                                    //取得主檔名
                                    //string DownloadImagesName = ImgCaption.Trim();
                                    if (!string.IsNullOrEmpty(ImgWebp))
                                    {
                                        DownloadImagesName = ImgWebp.Trim();
                                        DownloadImagesName = DownloadImagesName.Replace("[", "");
                                        DownloadImagesName = DownloadImagesName.Replace("]", "");
                                    }

                                    //取得副檔名
                                    string[] ImgSplit = ImgURL.Split('.');
                                    string DownloadImagesExt = ImgSplit[ImgURL.Split('.').Length - 1];

                                    //儲存檔案名稱
                                    string MainDownloadFile = DirectoryPath + DownloadImagesName + "." + DownloadImagesExt;

                                    if (!DownloadImagesUrl.Contains("themes") && !string.IsNullOrEmpty(DownloadImagesName))
                                    {
                                        if (File.Exists(MainDownloadFile))
                                        {
                                            Console.WriteLine("檔案已存在 : " + DownloadImagesUrl);
                                        }
                                        else
                                        {
                                            Console.WriteLine("下載 : " + DownloadImagesUrl);
                                            Console.WriteLine("下載時間 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));

                                            try
                                            {
                                                using (var s = await client.GetStreamAsync(DownloadImagesUrl))
                                                {
                                                    using (var fs = new FileStream(MainDownloadFile, FileMode.CreateNew))
                                                    {
                                                        await s.CopyToAsync(fs);

                                                        //Console.WriteLine("完成 : " + DownloadImagesUrl + " KB : " + (fs.Length / 1000));
                                                        Console.WriteLine("完成 : " + DownloadImagesUrl + " " + ImgCount + "/" + ImgTotalCount);
                                                        Console.WriteLine("完成時間 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                                                        Console.WriteLine();

                                                        ImgCount += 1;
                                                    }
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                throw;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Console.Write("No data...");
            }

            Console.Write("Press any key to close the app...");
            Console.ReadKey();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
