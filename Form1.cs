using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Runtime.InteropServices;

namespace Launcher
{
    /// <summary>
    /// 2022-06-30
    /// curClientVer : 현재 클라이언트 버전 비교용
    /// s3Downloader : 파일 다운로드 받을 AWS S3 SDK 클래스
    /// </summary>

    public partial class Form1 : Form
    {
        public string curClientVer;
        AmazonS3Downloader s3Downloader = new AmazonS3Downloader();
        List<Control> loginInfos = new List<Control>();
        string folderPath = "";
        private Point mousePointer;

        private byte setScreenMode = 3;

        bool isLogin = false;


        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            // UI세팅 변경 (투명도 등)
            // 이미지 부모 설정 및 이미지에 클릭이벤트 추가
            setUI();
            //다운로드 스타트
            downloadSetting();

        }

        #region UI 관련 함수

        public void setUI()
        {
            ///데코
            GameLogo.Parent = bg;
            GameRates.Parent = bg;
            imgTop.Parent = bg;
            imgTop.BackColor = Color.FromArgb(217, 0, 0, 0);
            noticeBox.Parent = bg;
            loginBox.Parent = bg;

            ///오버레이박스 (notice)
            pic_notice.Parent = noticeBox;
            pic_notice.Location = new Point(33, 45);

            ///오버레이박스 (login)
            string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //ver = ver.Remove(ver.Length - 7, 6);

            txt_ver.Text = " Ver. " + ver;
            txt_ver.Parent = loginBox;
            txt_ver.Location = new Point(10, 0);
            txt_all.Parent = loginBox;
            txt_all.Location = new Point(15, 55);

            ID.Parent = loginBox;
            ID.Location = new Point(82, 118);
            PW.Parent = loginBox;
            PW.Location = new Point(109, 157);

            bar_AllBorder.Parent = loginBox;
            bar_AllBorder.Location = new Point(60, 45);
            bar_All.Parent = bar_AllBorder;
            bar_All.Location = new Point(0, 0);
            txt_AllProcess.Parent = loginBox;
            txt_AllProcess.Location = new Point(bar_AllBorder.Width + txt_AllProcess.Width + 10, bar_AllBorder.Height + txt_AllProcess.Height - 8);

            bt_window.Parent = loginBox;
            bt_window.Location = new Point(10, 187);
            bt_window_pushed.Parent = bt_window;
            bt_window_pushed.Location = new Point(0, 0);
            txt_window.Parent = loginBox;
            txt_window.Location = new Point(38, 187);

            bt_fullscreen.Parent = loginBox;
            bt_fullscreen.Location = new Point(105, 187);
            bt_fullscreen_pushed.Parent = bt_fullscreen;
            bt_fullscreen_pushed.Location = new Point(0, 0);
            txt_fullscreen.Parent = loginBox;
            txt_fullscreen.Location = new Point(131, 187);

            bt_setLoginInfo.Parent = loginBox;
            bt_setLoginInfo.Location = new Point(433, 187);
            bt_setLoginInfo_pushed.Parent = bt_setLoginInfo;
            bt_setLoginInfo_pushed.Location = new Point(0, 0);
            txt_setLoginInfo.Parent = loginBox;
            txt_setLoginInfo.Location = new Point(459, 187);
            txt_afterLoginInfo.Parent = loginBox;
            txt_afterLoginInfo.Location = new Point(74, 121);

            update_state.Parent = loginBox;
            update_state.Location = new Point(160, 8);
            //pic_all = loginBox.Parent;

            ///상단바
            companyLogo.Parent = imgTop;
            bt_homepage.Parent = imgTop;
            bt_cash.Parent = imgTop;
            bt_signup.Parent = imgTop;
            bt_logout.Parent = imgTop;
            bt_setting.Parent = imgTop;
            bt_minimize.Parent = imgTop;
            bt_appquit.Parent = imgTop;

            ///이벤트 추가
            //bt_logout.MouseClick +=  
            //bt_setting.MouseClick +=
            //bt_fullscreen.MouseClick +=
            imgTop.MouseDown += new MouseEventHandler((o, e) => mousePointer = new Point(e.X, e.Y));
            imgTop.MouseMove += BarMouseMove;
            bt_homepage.MouseClick += new MouseEventHandler((o, e) => Process.Start("https://www.borngame.co.kr/"));
            bt_cash.MouseClick += new MouseEventHandler((o, e) => Process.Start("https://www.borngame.co.kr/itemshop/item_cash_daou.php"));
            bt_signup.MouseClick += new MouseEventHandler((o, e) => Process.Start("https://www.borngame.co.kr/member/agreement.php"));
            bt_minimize.MouseClick += new MouseEventHandler((o, e) => this.WindowState = FormWindowState.Minimized);
            bt_appquit.MouseClick += new MouseEventHandler((o, e) => Environment.Exit(0));
            txt_id.GotFocus += new EventHandler((o, e) => txt_id.SelectAll());
            txt_pw.GotFocus += new EventHandler((o, e) => txt_pw.SelectAll());
            


            bt_window.Click += setWindowScreen;
            bt_fullscreen.Click += setFullScreen;
            bt_setLoginInfo.Click += SaveLoginInfo;
            bt_setLoginInfo_pushed.Click += SaveLoginInfo;
            bt_login.Click += OnClickLogin;
            bt_logout.Click += OnClickLogout;
            bt_GameStart.Click += OnClickGameStart;
            timer.Tick += UpdateComplete;
            
            

            loginInfos.Add(ID);
            loginInfos.Add(PW);
            loginInfos.Add(Pic_ID);
            loginInfos.Add(Pic_PW);
            loginInfos.Add(txt_id);
            loginInfos.Add(txt_pw);
            loginInfos.Add(bt_setLoginInfo);
            loginInfos.Add(bt_setLoginInfo_pushed);
            loginInfos.Add(txt_setLoginInfo);

            this.Load += LoadPropertis;
        }
 
        /// <summary>
        /// 창모드 여부 설정
        /// </summary>
        private void setWindowScreen(object o, EventArgs e)
        {
            if (!bt_window_pushed.Visible)
            {
                bt_window_pushed.Visible = true;
                bt_fullscreen_pushed.Visible = false;
                setScreenMode = 3;
                Properties.Settings.Default.isFullscreen = false;
                Properties.Settings.Default.Save();
            }
            else return;
            
        }

        private void setFullScreen(object o, EventArgs e)
        {
            if (!bt_fullscreen_pushed.Visible)
            {
                bt_window_pushed.Visible = false;
                bt_fullscreen_pushed.Visible = true;
                setScreenMode = 1;
                Properties.Settings.Default.isFullscreen = true;
                Properties.Settings.Default.Save();
            }
            else return;
        }

        /// <summary>
        /// 계정 저장 여부
        /// </summary>
        private void SaveLoginInfo(object o, EventArgs e)
        {
            if(!bt_setLoginInfo_pushed.Visible)
            {
                bt_setLoginInfo_pushed.Visible = true;
            }
            else
            {
                bt_setLoginInfo_pushed.Visible = false;           
            }
           
        }

        private void SaveLoginInfo(bool isCheck)
        {
            if (isCheck)
            {
                Properties.Settings.Default.playerID = txt_id.Text;
                Properties.Settings.Default.playerPW = txt_pw.Text;
                Properties.Settings.Default.isSaveInfo = true;
            }
            else
            {
                Properties.Settings.Default.playerID = null;
                Properties.Settings.Default.playerPW = null;
                Properties.Settings.Default.isSaveInfo = false;
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 상단바 드래그 앤 드랍
        /// </summary>
        private void BarMouseMove(object o, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Location = new Point(Left - (mousePointer.X - e.X), Top - (mousePointer.Y - e.Y));
            }
        }

        /// <summary>
        /// 저장된 값 로드
        /// </summary>
        private void LoadPropertis(object o, EventArgs e)
        {
            txt_id.Text = Properties.Settings.Default.playerID;
            txt_pw.Text = Properties.Settings.Default.playerPW;
            if(Properties.Settings.Default.isFullscreen)
            {
                bt_window_pushed.Visible = false;
                bt_fullscreen_pushed.Visible = true;
                setScreenMode = 1;
            }    
            else
            {
                bt_window_pushed.Visible = true;
                bt_fullscreen_pushed.Visible = false;
                setScreenMode = 3;
            }
            if (Properties.Settings.Default.isSaveInfo) bt_setLoginInfo_pushed.Visible = true;
            else bt_setLoginInfo_pushed.Visible = false;

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 프로그레스바 진행
        /// </summary>
        public void WorkProgressbar(object o, EventArgs e)
        {
            float CalcTxt = (float)bar_All.Width / bar_AllBorder.Width;

            if(s3Downloader.isWork && bar_All.Width <= bar_AllBorder.Width * 0.98)
            {
                bar_All.Width += 3;
               txt_AllProcess.Text = string.Format("{0:P0}", CalcTxt);
            }
            else if ( !s3Downloader.isWork)
            {
                bar_All.Width = bar_AllBorder.Width;;
                txt_AllProcess.Text = string.Format("{0:P0}", CalcTxt);
            }
        }

        public void WorkProgressbar(int count, int index)
        {
            int CalcBar = bar_AllBorder.Width / count;
            float CalcTxt = (float)index / count;


            if (bar_All.InvokeRequired)
            {
                bar_All.Invoke(new MethodInvoker(delegate { bar_All.Width = CalcBar * index; }));
                if(count <= index)
                    bar_All.Invoke(new MethodInvoker(delegate { bar_All.Width = bar_AllBorder.Width; }));
                
            }

            if (txt_AllProcess.InvokeRequired)
            {
                txt_AllProcess.Invoke(new MethodInvoker(delegate { txt_AllProcess.Text = string.Format("{0:P0}", CalcTxt); }));
            }
            

            

        }

        /// <summary>
        /// 업데이트 완료시
        /// </summary>
        public void UpdateComplete(object o, EventArgs e)
        {
            if (s3Downloader.updateCompite && isLogin) bt_GameStart.Visible = true;
            else bt_GameStart.Visible = false;
        }

        /// <summary>
        /// 게임시작
        /// </summary>
        public void OnClickGameStart(object o, EventArgs e)
        {

            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.CreateNoWindow = false;
            psi.UseShellExecute = false;

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;

            p.StartInfo = psi;
            p.Start();

            /// 명령줄로 ID, PW 인자를 넘겨서 런처를 실행
            p.StandardInput.WriteLine(Application.StartupPath + @"\" + "josun-remaster.exe" + " -CustomArgs:ID=" + txt_id.Text + ";PW=" + txt_pw.Text);
            p.StandardInput.Close();
            p.WaitForExit();
            p.Close();

            //string txtPath = Application.StartupPath + "\\" + "player.info";
            //FileStream stream = File.Create(txtPath);
            //stream.Close();
            //File.WriteAllText(txtPath, txt_id.Text + "," + txt_pw.Text);
            
            
            //Process p = new Process();
            //p.StartInfo.FileName = Application.StartupPath + "\\" + "josun-remaster.exe";
            //p.StartInfo.Arguments = txt_id.Text + txt_pw.Text;
            //p.Start();

            Environment.Exit(0);
        }
        #endregion

        #region 로그인 관련 함수

        string webpage, httpResult;

        /// <summary>
        /// 로그인 클릭시
        /// </summary>
        private void OnClickLogin(object o, EventArgs e)
        {

            if (txt_id.Text == null || Pic_PW.Text == null)
            {
                MessageBox.Show("아이디나 비밀번호를 재확인해주세요", "알림", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                webpage = SetLoginInfo(txt_id.Text, txt_pw.Text);
                httpResult = HttpGet(webpage, 1);

                string result = httpResult.Replace("\0", "");

                string[] splStr; 
                splStr = result.Split('\a');

                if(splStr[0].Contains("하지 않는") || splStr[0].Contains("가 일치"))
                {
                    MessageBox.Show(splStr[0], "알림", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                SaveLoginInfo(bt_setLoginInfo_pushed.Visible);

                bt_signup.Visible = false;

                ///생성되어있는 캐릭터가 없을 때
                if(splStr[0].Equals("1"))
                {
                    txt_afterLoginInfo.Text = "캐릭터를 생성해주세요";
                }
                else if(splStr[0].Substring(0, 1).Equals("1"))
                {
                    string str = splStr[0].Remove(0, 1);
                    txt_afterLoginInfo.Text = str + "님 환영합니다";
                    
                }

                isLogin = true;
                txt_afterLoginInfo.Visible = true;

                for (int i = 0; i < loginInfos.Count; i++)
                {
                    loginInfos[i].Visible = false;
                }

            }
        }

        /// <summary>
        /// 로그아웃 클릭시
        /// </summary>
        private void OnClickLogout(object o, EventArgs e)
        {
            for (int i = 0; i < loginInfos.Count; i++)
            {
                loginInfos[i].Visible = true;
            }
            txt_afterLoginInfo.Visible = false;
            bt_GameStart.Visible = false;
            bt_signup.Visible = true;
            isLogin = false;
            
        }

        private string SetLoginInfo(string ID, string PW)
        {
            string str;
            str = string.Format(Constants._characterList, ID, PW, 0);
            return str;
        }

        /// <summary>
        /// C++ dll 
        /// </summary>
        [DllImport("wininet.dll")]
        public static extern IntPtr InternetOpenA(string strAgent, Int32 AccessType, string proxy, string ByPass, Int32 Flags);
        [DllImport("wininet.dll")]
        public static extern IntPtr InternetCloseHandle(IntPtr intPtr);
        [DllImport("wininet.dll")]
        public static extern IntPtr InternetSetOptionExA(IntPtr intPtr, Int32 option, IntPtr buffer, Int32 bufferLength, Int32 Flags);
        [DllImport("wininet.dll")]
        public static extern IntPtr InternetConnectA(IntPtr intPtr, string serverName, Int32 port, string userName, string userPass, Int32 Service, Int32 flags, IntPtr context);
        [DllImport("wininet.dll")]
        public static extern IntPtr HttpOpenRequestA(IntPtr intPtr, string Verb, string ObjectName, string version, string Ref, IntPtr AcceptTypes, Int32 flags, IntPtr context);
        [DllImport("wininet.dll")]
        public static extern IntPtr HttpSendRequestA(IntPtr intPtr, string header, Int32 headerLength, string optional, Int32 optionalLength);
        [DllImport("wininet.dll")]
        public static unsafe extern bool HttpQueryInfoA(IntPtr intPtr, Int32 InfoLevel, IntPtr Buffer, ref Int32 BufferLength, int Index);
        [DllImport("wininet.dll")]
        public static extern bool InternetReadFile(IntPtr intPtr, IntPtr data, Int32 NumberOfBytesToRead1, ref Int32 NumberOfBytesToRead2);


        /// <summary>
        /// 인터넷 결과 가져오기
        /// </summary>
        unsafe private string HttpGet(string page, uint pageindex)
        {
            string str = "";
            string param = "";
            string url = Constants._josun;
            int port = Constants._msgPort;


            int findIndex = page.IndexOf("?");
            if (findIndex != -1)
            {
                param = stringMid(page, findIndex + 1, page.Length - (findIndex + 1));
                page = stringLeft(page, findIndex);
            }

            var mstr_page = page;
            var mstr_url = url;
            var mstr_param = param;

            Int32 option = 1000 * 5;
            IntPtr optionPt = new IntPtr(option);
            IntPtr hServer = IntPtr.Zero;
            IntPtr hFile = IntPtr.Zero;
            Int32 buffer_size = 0;

            Int32 HttpRequestFlags = 0x000000002 | 0x04000000;
            string HttpResult = null;
            Int32 nRead = 0, Count = 0;

            IntPtr hSession = IntPtr.Zero;
            hSession = InternetOpenA("MSG Module", 0, null, null, 0);
            if (hSession == null)
            {
                str = "통신 모듈 초기화에 실패했습니다";
                MessageBox.Show(str, "알림", MessageBoxButtons.OK, MessageBoxIcon.Error);
                goto HTTPGET_END;
            }

            InternetSetOptionExA(hSession, 2, optionPt, sizeof(Int32), 0);
            option = 5;
            optionPt = new IntPtr(option);
            InternetSetOptionExA(hSession, 2, optionPt, sizeof(Int32), 0);

            // URL open
            try
            {
                hServer = InternetConnectA(hSession, mstr_url, port, null, null, 3, 0, IntPtr.Zero);
                if (hServer == null)
                {
                    string strError;
                    strError = string.Format("웹 서버에 접속할 수 없습니다. {0}:{1}", mstr_url, port);
                    MessageBox.Show(strError, "알림", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    goto HTTPGET_END;
                }
                hFile = HttpOpenRequestA(hServer, "POST", mstr_page, null, null, IntPtr.Zero, HttpRequestFlags, IntPtr.Zero);

                if (hFile == null)
                {
                    string strError;
                    strError = string.Format("{0}:pageindex = {1}", mstr_url, pageindex);
                    MessageBox.Show(strError, "알림", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    goto HTTPGET_END;
                }
                string hdrs = "Content-Type: application/x-www-form-urlencoded\r\nUser-Agent: MUSICSHAKE MSG Module\r\n";
                if (IntPtr.Zero == HttpSendRequestA(hFile, hdrs, hdrs.Length, mstr_param, mstr_param.Length))
                {
                    string strError;
                    strError = string.Format("{0}:pageindex = {1}", hdrs, pageindex);
                    MessageBox.Show(strError, "알림", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    goto HTTPGET_END;
                }

                Int32 ret = 0;
                byte[] buffer = new byte[80];
                buffer[0] = 0;
                
                GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                Int32 bufferLength = buffer.Length;

                fixed (byte* cArray = &buffer[0])
                {
                    if (HttpQueryInfoA(hFile, 19, gcHandle.AddrOfPinnedObject(), ref bufferLength, 0))
                    {
                        string parsingBuffer = ByteArrayToHexString(buffer);
                        ret = Int32.Parse(parsingBuffer);                      
                    }
                    if (ret == 200)
                    {
                        Int32 bufferLength_re = buffer.Length;
                        if (HttpQueryInfoA(hFile, 5, gcHandle.AddrOfPinnedObject(), ref bufferLength_re, 0))
                        {
                            string parsingBuffer = ByteArrayToHexString(buffer);
                            parsingBuffer = parsingBuffer.Replace("\00", "");
                            parsingBuffer = parsingBuffer.Replace("\0", "");
                            ret = Int32.Parse(parsingBuffer);

                            byte[] data = new byte[4097];
                            Array.Clear(data, 0, data.Length);
                            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

                            string ptr = HttpResult;
                            if (ptr != null)
                            {
                                string strError;
                                strError = string.Format("{0}:pageindex = {1}", mstr_url, pageindex);
                                MessageBox.Show(strError, "알림", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                goto HTTPGET_END;
                            }
                            while (true)
                            {
                                if (InternetReadFile(hFile, dataHandle.AddrOfPinnedObject(), 4096, ref nRead))
                                {
                                    data = cvtData(data);
                                    
                                }
                                if (nRead == 0) break;
                                string dst = ptr + Count.ToString();

                                Count += nRead;
                              
                            }

                            HttpResult = ByteArrayToHexString(data);

                        }
                        

                    }

                }
                
                
               


            }
            catch (ArgumentNullException err)
            {
                
            }
            catch (ArgumentException err)
            {

            }



        HTTPGET_END:
            InternetCloseHandle(hFile);
            InternetCloseHandle(hServer);
            InternetCloseHandle(hSession);

            mstr_url = null;
            mstr_page = null;
            mstr_param = null;
            


            return HttpResult;
        }

        /// <summary>
        /// Byte[] To Hex
        /// </summary>
        public string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            string str = "0123456789ABCDEF";

            foreach (var val in data)
            {
                sb.Append(str[(int)(val >> 4)]);
                sb.Append(str[(int)(val & 0xF)]);

            }
            string returnStr = sb.ToString();
            returnStr = Encoding.Default.GetString(data);

            return returnStr;
        }

        /// <summary>
        /// convert Data
        /// </summary>
        private byte[] cvtData(byte[] data)
        {

            char[] cpyData = new char[4097];
            Array.Copy(data, cpyData, 4096);

            byte x, y;

            int idx = 0;
            string bin = null;

            string str0 = null;
            string str1 = null;
            string newstr = null;

            List<byte> lByte = new List<byte>();

            for (int i = 0; i < cpyData.Length; i++)
            {
                x = 0;
                y = 0;

                idx = Convert.ToInt32(cpyData[i]);

                bin = Convert.ToString(idx, 2);

                bin = bin.PadLeft(8, '0');

            }
         
            return data;
        }

        private string stringLeft(string txt, int txtLength)
        {
            string str;

            if(txt.Length < txtLength)
            {
                txtLength = txt.Length;
            }
            str = txt.Substring(0, txtLength);
            return str;
        }

        private string stringMid(string txt, int Start, int End)
        {
            string str;
            if (Start < txt.Length || End < txt.Length)
            {
                str = txt.Substring(Start, End);
                return str;
            }
            else
                return txt;
        }

        #endregion


        public void downloadSetting()
        {

            // 디폴트 폴더 설정
            DirectoryInfo dir = new DirectoryInfo(Application.StartupPath);

            folderPath = dir.FullName;

            Form1 form1 = this;
            s3Downloader.Start(folderPath, ref form1);
        }
        


    }
    /// <summary>
    /// 기본 버킷 정보 설정
    /// </summary>
    public static class Constants
    {
        public const string _josun = "test.borngame.co.kr";
        public const int _msgPort = 7179;
        public const int _downloadPort = 80;

        public const string _characterList = "/msgs/GetCharacterList.yjd?id={0}&auth={1}&server={2}";
    }

}
