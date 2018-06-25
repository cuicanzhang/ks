﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace k3
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

            getZhcwJson();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(updateTime), null);
            this.Dispatcher.BeginInvoke(new Action(InitTimer), null);
        }

        private async void updateTime()
        {
            while (true)
            {
                await Task.Run(() => Thread.Sleep(900));
                DateTimeLB.Content = DateTime.Now.ToString();
                await Task.Delay(100);
            }
        }
        private async void InitTimer()
        {
            while (true)
            {
                await Task.Run(() => Thread.Sleep(900));

                JObject init_result = JsonConvert.DeserializeObject(zhcw()) as JObject;
                var qh = int.Parse(init_result["list"][0]["issue"].ToString().Substring(7, 2));
                var qh_next = int.Parse(init_result["issue"].ToString().Substring(7, 2));
                DateTime startTime = Convert.ToDateTime(init_result["startTime"].ToString());
                DateTime endTime = Convert.ToDateTime(init_result["endTime"].ToString());
                nextIssueLB.Content = "距" + init_result["issue"].ToString() + "期开奖剩余：";
                for (int i = 0; i <= 5; i++)
                {
                    string qhlbName = "qh" + i + "LB";
                    string jhlbName = "jh" + i + "LB";

                    Label qhLB = this.FindName(qhlbName) as Label;
                    Label jhLB = this.FindName(jhlbName) as Label;
                    if (qhLB != null && jhLB != null)
                    {
                        qhLB.Content = init_result["list"][i]["issue"].ToString();
                        jhLB.Content = init_result["list"][i]["awardNum"].ToString().Replace(",", "");
                    }
                }

                TimeSpan subTime = endTime - startTime;
              
                    
                    while (startTime<endTime)
                    {
                        endTime = Convert.ToDateTime(init_result["endTime"].ToString());
                        startTime = Convert.ToDateTime(DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"));
                        subTime = endTime - startTime;

                        await Task.Run(() => Thread.Sleep(900));
                        nextIssueLB.Content = "距" + init_result["issue"].ToString() + "期开奖剩余：" + subTime.Minutes.ToString().PadLeft(2, '0') + ":" + subTime.Seconds.ToString().PadLeft(2, '0');
                        await Task.Delay(100);
                    }
                
                    nextIssueLB.Content = "距" + init_result["issue"].ToString() + "期开奖剩余：获取中...";
                    JObject init_new_result = JsonConvert.DeserializeObject(zhcw()) as JObject;
                    while (startTime > endTime)
                    {
                        init_new_result = JsonConvert.DeserializeObject(zhcw()) as JObject;
                        qh = int.Parse(init_result["list"][0]["issue"].ToString().Substring(7, 2));
                        qh_next = int.Parse(init_result["issue"].ToString().Substring(7, 2));
                        if (qh_next - qh == 1)
                        {
                            await Task.Run(() => Thread.Sleep(4900));
                            await Task.Delay(100);
                            break;
                        }
                    }
              
                await Task.Delay(100);
            }


        }

        private async void getZhcwJson()
        {
            JObject init_result = JsonConvert.DeserializeObject(zhcw()) as JObject;
            
            DateTime startTime = Convert.ToDateTime(init_result["startTime"].ToString());
            DateTime endTime = Convert.ToDateTime(init_result["endTime"].ToString());

            nextIssueLB.Content = "距" + init_result["issue"].ToString() + "期开奖剩余：" ;

            for (int i = 0; i <= 5; i++)
            {
                string qhlbName = "qh" + i + "LB";
                string jhlbName = "jh" + i + "LB";
                
                Label qhLB = this.FindName(qhlbName) as Label;
                Label jhLB = this.FindName(jhlbName) as Label;
                if (qhLB != null && jhLB!=null)
                {
                    qhLB.Content = init_result["list"][i]["issue"].ToString();
                    jhLB.Content = init_result["list"][i]["awardNum"].ToString().Replace(",", "");
                }
            }

            while (true)
            {
                await Task.Run(() => Thread.Sleep(900));
                jh.Content = DateTime.Now.ToString();
                await Task.Delay(100);
            }

        }
        private string zhcw()
        {

            string url = "http://data.zhcw.com/k3/index.php?act=kstb&provinceCode=22";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.AllowAutoRedirect = false;
            req.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            string cookies = res.Headers.Get("Set-Cookie");

            string jsonUrl = "http://data.zhcw.com/port/client_json.php?transactionType=10130307";
            HttpWebRequest req1 = (HttpWebRequest)WebRequest.Create(jsonUrl);
            req1.Method = "GET";
            req1.AllowAutoRedirect = false;
            req1.ContentType = "application/x-www-form-urlencoded";
            req1.CookieContainer = new CookieContainer();
            req1.CookieContainer.SetCookies(req1.RequestUri, cookies);
            HttpWebResponse res1 = (HttpWebResponse)req1.GetResponse();
            string html = new StreamReader(res1.GetResponseStream()).ReadToEnd();
            return html;
        }

    }
}
