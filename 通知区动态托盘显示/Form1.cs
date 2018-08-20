using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;


namespace 通知区动态托盘显示
{
   

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            StartUp("2");
          
        }

        //改变窗体上的关闭按钮
        protected override void WndProc(ref   Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;
            if (m.Msg == WM_SYSCOMMAND && (int)m.WParam == SC_CLOSE)
            {
                this.WindowState = FormWindowState.Minimized;

                return;
            }
            base.WndProc(ref m);
         }


        private void Form1_Load(object sender, EventArgs e)
        {
                 
            //隐藏窗体
            this.WindowState = FormWindowState.Minimized;
            //this.richTextBox1.Enabled = true;
            int time = Convert.ToInt32(GetAppConfig("Time"));
            if (time == 0) {
                time = 1;
            }
            this.timer1.Interval =time*60*1000;
           
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
                this.notifyIcon.Visible = true;
            }
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {

            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.Show();

            type = 0;

            
        }

        public int type = 1;
                    
        int c = 0;

        //请求定时器
        private void timer1_Tick(object sender, EventArgs e)
        {

            string url = GetAppConfig("GetUrl");
            string userId = GetAppConfig("UserId");

           

            if (url == "" || userId=="")
            {
                MessageBox.Show("请在配置文件中配置请求url和用户UserId");
            }
            else
            {
                
                string responseVal = HttpGet(url);

                if (responseVal == "") {
                   // MessageBox.Show("请求异常");
                    return;
                }
                
                ToJsonMy obj = ToObj(responseVal);

                if (obj.code != "00") {
                   // MessageBox.Show("请求异常");
                    return;
                }

                if (obj.data.total == "0")
                {
                    type = 0;
                    this.richTextBox1.Text = "";
                    this.richTextBox1.AppendTextColorful("暂无待办", Color.Black, 16);
                }
                else
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer();
                    player.SoundLocation = Application.StartupPath + "\\yinpin.wav";
                    player.LoadAsync();
                    player.PlaySync();  

                    type = 1;

                    this.timer.Enabled = true;

                    this.richTextBox1.Text = "";
                  
                    string msg = "总消息数：" + obj.data.total+ "\r\n" ;

                    
                    //this.richTextBox1.AppendTextColorful(msg, Color.Black, 16);

                    int index = 1;
                    List<ToJsonItem> items = obj.data.items;
                   
                    foreach (ToJsonItem item in items)
                    {

                        msg += index + "、" + item.title + "\r\n";
                       // this.richTextBox1.AppendTextColorful(index+"、" + item.title, Color.Black, 16);

                        //this.richTextBox1.AppendTextColorful("申请人：" + item.apply + "    时间：" + item.date, Color.Black, 14);
                        msg += "申请人：" + item.apply + "    时间：" + item.date + "\r\n";
                        index++;

                    }
                    this.richTextBox1.AppendTextColorful(msg, Color.Black, 14);

                    richTextBox1.SelectionStart = 0;
                    richTextBox1.Focus();

                }
            }

        }

    
        //图标闪动定时器
        private void timer_Tick(object sender, EventArgs e)
        {
                        
                
                //根据指定的类型依次设置相应的图标实现动态效果
                switch (type)
                {
  
                    //notifyIcon 指定在通知区域中创建图标的组件。 此类不能被继承。

                    case 1:
                        if (c == 0) { notifyIcon.Icon = Properties.Resources.stop; c++; }
                        else { c = 0; notifyIcon.Icon = Properties.Resources._null; }
                        break;

                    case 0:
                        notifyIcon.Icon = Properties.Resources.stop;
                        break;

                }
           
            
        }

      


        //post请求
        //public static string HttpPost()
        //{

        //    string strURL = "http://localhost/WinformSubmit.php";
        //    System.Net.HttpWebRequest request;
        //    request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);
        //    //Post请求方式  
        //    request.Method = "POST";
        //    // 内容类型  
        //    request.ContentType = "application/x-www-form-urlencoded";
        //    // 参数经过URL编码  
        //    string paraUrlCoded = System.Web.HttpUtility.UrlEncode("keyword");
        //    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode("多月");
        //    byte[] payload;
        //    //将URL编码后的字符串转化为字节  
        //    payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
        //    //设置请求的 ContentLength   
        //    request.ContentLength = payload.Length;
        //    //获得请 求流  
        //    System.IO.Stream writer = request.GetRequestStream();
        //    //将请求参数写入流  
        //    writer.Write(payload, 0, payload.Length);
        //    // 关闭请求流  
        //    writer.Close();
        //    System.Net.HttpWebResponse response;
        //    // 获得响应流  
        //    response = (System.Net.HttpWebResponse)request.GetResponse();
        //    System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
        //    string responseText = myreader.ReadToEnd();
        //    myreader.Close();
        //  //  MessageBox.Show(responseText);  
        //    return responseText;
        
        //}


        public static  ToJsonMy ToObj(string responeString) {

            ToJsonMy tjData = new ToJsonMy();

            JavaScriptSerializer js = new JavaScriptSerializer();   //实例化一个能够序列化数据的类
            tjData = js.Deserialize<ToJsonMy>(responeString);

            return tjData;
        }

         //get请求
        public static string HttpGet(string url) {

            string responseText = "";

            try {

                url += "?userID=" + GetAppConfig("UserId");

                System.Net.HttpWebRequest request;
                // 创建一个HTTP请求  
                request = (System.Net.HttpWebRequest)WebRequest.Create(url);
                //request.Method="get";  
                System.Net.HttpWebResponse response;
                response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
                 responseText = myreader.ReadToEnd();
                myreader.Close();
                return responseText;
            
            }catch(Exception e){
                
                return responseText;
            }
            

           // return "{\"status\":\"xx\",\"msg\":\"xxx\",\"data\":{\"total\":2,\"items\":[{\"title\":\"11\",\"date\":\"11\",\"apply\":\"apply1\"},{\"title\":\"22\",\"date\":\"222\",\"apply\":\"apply2\"}]}}";
            //return "";
           

        }
        


        ///<summary> 
        ///返回*.exe.config文件中appSettings配置节的value项  
        ///</summary> 
        ///<param name="strKey"></param> 
        ///<returns></returns> 
        public static string GetAppConfig(string strKey)
        {
            string file = System.Windows.Forms.Application.ExecutablePath;
            Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                if (key == strKey)
                {
                    return config.AppSettings.Settings[strKey].Value.ToString();
                }
            }
            return null;
        }

        ///<summary>  
        ///在*.exe.config文件中appSettings配置节增加一对键值对  
        ///</summary>  
        ///<param name="newKey"></param>  
        ///<param name="newValue"></param>  
        public static void UpdateAppConfig(string newKey, string newValue)
        {
            
            string file = System.Windows.Forms.Application.ExecutablePath;
            Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            bool exist = false;
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                if (key == newKey)
                {
                    exist = true;
                }
            }
            if (exist)
            {
                config.AppSettings.Settings.Remove(newKey);
            }
            config.AppSettings.Settings.Add(newKey, newValue);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }


        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>  
        /// 修改程序在注册表中的键值  
        /// </summary>  
        /// <param name="flag">1:开机启动</param>  
        private void StartUp(string flag)
        {
            try
            {

                string path = Application.StartupPath;

                //当前用户
                Microsoft.Win32.RegistryKey Rkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (flag.Equals("1"))
                {
                    if (Rkey == null)
                    {
                        //当前用户
                       Rkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");

                    }
                    Rkey.SetValue("通知区动态托盘显示", path);
                    
                }
                else
                {
                    if (Rkey != null)
                    {
                        Rkey.DeleteValue("通知区动态托盘显示", false);
                    }
                }
          
            
            }
            catch(Exception ex) { 
            }

        }    
 
    }

    public static class RichTextBoxExtension
    {
        public static void AppendTextColorful(this RichTextBox rtBox, string text, Color color, int fontSize, bool addNewLine = true)
        {
            if (addNewLine)
            {
                text += Environment.NewLine;
            }
            rtBox.SelectionStart = rtBox.TextLength;
            rtBox.SelectionLength = 0;
            rtBox.SelectionColor = color;
            fontSize = fontSize > 0 ? fontSize : 10;
            rtBox.SelectionFont = new Font(rtBox.Font.Name, fontSize);
            rtBox.AppendText(text);
            rtBox.SelectionColor = rtBox.ForeColor;
        }

        public static void TextColorful(this RichTextBox rtBox, string text, Color color, int fontSize, bool addNewLine = true)
        {
            if (addNewLine)
            {
                text += Environment.NewLine;
            }
            rtBox.SelectionStart = rtBox.TextLength;
            rtBox.SelectionLength = 0;
            rtBox.SelectionColor = color;
            fontSize = fontSize > 0 ? fontSize : 10;
            rtBox.SelectionFont = new Font(rtBox.Font.Name, fontSize);
            rtBox.Text = text;
            rtBox.SelectionColor = rtBox.ForeColor;
        }
    }
}
