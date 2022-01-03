using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace wnacg2
{
    public class DownloadImages
    {
        public string url { get; set; }
        public string caption { get; set; }
    }

    class Program
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

        static void Main(string[] args)
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
            string DataImagesUrl = "https://www.wnacg.org/photos-gallery-aid-" + DataNumber + ".html";

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
                        DownloadTitle = DownloadTitle.Replace("[中国翻訳]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[中国翻译]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[中國翻譯]", "").Trim();

                        //移除漢化組名稱
                        DownloadTitle = DownloadTitle.Replace("[羅莎莉亞漢化]", "").Trim();
                        DownloadTitle = DownloadTitle.Replace("[翻车汉化组]", "").Trim();

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

                        if (Directory.Exists(DirectoryPath))
                        {
                            dynamic DI = JsonConvert.DeserializeObject(DataJson);

                            int ImgCount = 1;

                            foreach (var img in DI.DownloadImages)
                            {
                                string ImgURL = img.url;
                                string ImgCaption = img.caption;

                                //下載URL
                                string DownloadImagesUrl = "https://" + ImgURL;

                                //取得主檔名
                                string DownloadImagesName = ImgCaption.Trim();
                                DownloadImagesName = DownloadImagesName.Replace("[", "");
                                DownloadImagesName = DownloadImagesName.Replace("]", "");

                                //取得副檔名
                                string[] ImgSplit = ImgURL.Split('.');
                                string DownloadImagesExt = ImgSplit[ImgURL.Split('.').Length - 1];

                                //儲存檔案名稱
                                string MainDownloadFile = DirectoryPath + DownloadImagesName + "." + DownloadImagesExt;

                                if (!DownloadImagesUrl.Contains("themes"))
                                {
                                    if (File.Exists(MainDownloadFile))
                                    {
                                        Console.WriteLine("檔案已存在 : " + DownloadImagesUrl);
                                    }
                                    else
                                    {
                                        using (WebClient DataClient = new WebClient())
                                        {
                                            try
                                            {
                                                DataClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko");

                                                DataClient.DownloadFile(DownloadImagesUrl, MainDownloadFile);

                                                Console.WriteLine("下載 : " + DownloadImagesUrl + " [" + ImgCount + "]");

                                                ImgCount += 1;
                                            }
                                            catch
                                            {
                                                Console.WriteLine("下載失敗 : " + DownloadImagesUrl + " [" + ImgCount + "]");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.Write("Press any key to close the app...");
            Console.ReadKey();
        }
    }
}
