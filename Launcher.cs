using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;
using System.IO.Compression;
using Amazon;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;


namespace Launcher
{
    // 런처 메인
    public class Launcher
    {
    }

    /// <summary>
    /// 아마존 다운로더
    /// </summary>
    public class AmazonS3Downloader
    {
        /// <summary>
        /// 버킷 정보 기입
        /// </summary>
        private string bucketName = "";
        private const string accessKey = "AKIAUY5XCCPCKMBFYFEV";
        private const string secretKey = "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";
        private const string rootPath = "";

        delegate void StrToArg(string text);

        Form1 form1;
        string folderPath;
        IAmazonS3 s3Client;
        string currentLauncherVer;

        string currentClientVer;

        bool isLauncherUpdate = false;
        bool isClientUpdate = false;

        public bool updateCompite = false;
        public bool isWork = false;

        public void Start(string folderPath, ref Form1 form1)
        {

#if DEBUG
            uploadLauncherVerToServer();
#endif


            this.folderPath = folderPath;
            this.form1 = form1;

            //런처 현재 정보 받아오기
            currentLauncherVer = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //클라이언트 현재 정보 받아오기
            getClientVer();

            Thread downThread = new Thread(new ThreadStart(DirectoryInfo));
            downThread.Start();
        }



        /// <summary>
        /// 다운로드 스레드
        /// </summary>
        private void DirectoryInfo()
        {
            AmazonS3Config cfg = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.APNortheast2
            };

            // s3버킷 세팅
            s3Client = new AmazonS3Client(accessKey, secretKey, cfg);

            ListBucketsResponse buckets = s3Client.ListBuckets();

            //버전 비교
            S3DirectoryInfo versioning = new S3DirectoryInfo(s3Client, buckets.Buckets[0].BucketName, rootPath);

            compareVersion(versioning);
            //



            foreach (var val in buckets.Buckets)
            {
                /// 1. 버전 비교
                /// 2. 런처 업데이트
                /// 3. 클라 업데이트
                if (val.BucketName.Equals("josun-application-versioning")) continue;
                if (!isLauncherUpdate && val.BucketName.Equals("josun-launcher")) continue;
                if (!isClientUpdate && val.BucketName.Equals("josun-remaster")) continue;

                MessageBox.Show("업데이트를 진행합니다", "알림", MessageBoxButtons.OK);

                Console.WriteLine(val.BucketName);
                bucketName = val.BucketName;
                S3DirectoryInfo dir = new S3DirectoryInfo(s3Client, bucketName, rootPath);

                DirSearch(dir);

                if (isLauncherUpdate) Application.Restart();

            }
            if (form1.update_state.InvokeRequired)
                form1.update_state.Invoke(new MethodInvoker(delegate { form1.update_state.Text = "업데이트 완료. 게임을 실행해주세요."; }));
            if (form1.bar_All.InvokeRequired)
                form1.bar_All.Invoke(new MethodInvoker(delegate { form1.bar_All.Width = form1.bar_AllBorder.Width; }));
            updateCompite = true;

        }


        #region versioning 관련


        /// <summary>
        /// 버전 비교
        /// </summary>
        /// <param name="s3DirectoryInfo"></param>

        private void compareVersion(S3DirectoryInfo s3DirectoryInfo)
        {
            try
            {
                ///받아온 버전 파일과 현재 클라이언트 폴더 내부에 있는 버전 파일의 버전을 비교해서 다르면 업데이트, 아니면 스킵
                string folderPath = this.folderPath;
                bucketName = s3DirectoryInfo.Name;
                ListObjectsResponse fileList = s3Client.ListObjects(new ListObjectsRequest() { BucketName = bucketName });

                foreach (var val in fileList.S3Objects)
                {
                    GetObjectRequest req = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = val.Key
                    };

                    string filePath = val.BucketName.Replace(val.BucketName, folderPath);
                    GetObjectResponse res = s3Client.GetObject(req);

                    

                    res.WriteResponseStreamToFile(string.Format("{0}\\{1}", filePath, val.Key));


                    //현재 파일이랑 비교
                    compareVersion(filePath, val.Key);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 버전 비교하는 함수
        /// </summary>
       
        private void compareVersion(string filePath, string fileName)
        {
            string str = File.ReadAllText(filePath + "\\" + fileName);
            string[] splstr = str.Split('\n');

            str = splstr[splstr.Length - 1];

            if (fileName.Equals("app.info") && str.Equals(currentClientVer)) isClientUpdate = false;
            else if (fileName.Equals("app.info") && !str.Equals(currentClientVer)) isClientUpdate = true;

            if (fileName.Equals("launcher.info") && str.Equals(currentLauncherVer)) isLauncherUpdate = false;
            else if (fileName.Equals("launcher.info") && !str.Equals(currentLauncherVer)) isLauncherUpdate = true;

            File.Delete(filePath + "\\" + fileName);

        }


        /// <summary>
        /// 현재 클라이언트 버전 읽어오는 함수
        /// </summary>
        private void getClientVer()
        {
            ///디버그는 클라 빌드뽑아서 자주 쓰던 폴더로 해놨음 수정 필요
#if DEBUG
            StreamReader sr = new StreamReader("D:\\josun_re\\josun-remaster_Data\\app.info");
#else
            ///릴리즈는 설치파일을 통해 설치된 경로 통해서 읽어옴
            StreamReader sr = new StreamReader(Application.StartupPath + "\\josun-remaster_Data\\app.info");
#endif
            string str = sr.ReadToEnd();
            string[] splitStr = str.Split('\n');
            this.currentClientVer = splitStr[2];
        }

        /// <summary>
        /// 현재 런처 버전 업로드 (디버그 한정)
        /// </summary>
        private void uploadLauncherVerToServer()
        {
            string path = Application.StartupPath + "\\launcher.info";

            StreamWriter writer;
            writer = File.CreateText(path);
            writer.Write(Assembly.GetExecutingAssembly().GetName().Version.ToString());
            writer.Close();

            VersioningLauncher(path);
        }

        private bool VersioningLauncher(string path)
        {
            s3Client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.APNortheast2);

            FileInfo file = new FileInfo(path);
            string destPath = "launcher.info";
            PutObjectRequest req = new PutObjectRequest()
            {
                InputStream = file.OpenRead(),
                BucketName = "josun-application-versioning",
                Key = destPath
            };

            PutObjectResponse res = s3Client.PutObject(req);

            return true;
        }

        #endregion


        #region FileDownLoad 관련

        private void DirSearch(S3DirectoryInfo s3DirectoryInfo)
        {
            try
            {
                string folderPath = this.folderPath;
                ListObjectsResponse fileList = s3Client.ListObjects(new ListObjectsRequest() { BucketName = bucketName });


                //디렉토리 생성
                foreach (var val in s3DirectoryInfo.GetDirectories())
                {
                    string fullPath = val.FullName.Replace(val.Bucket.FullName, this.folderPath + "\\");
                    if (Directory.Exists(fullPath) == false)
                    {
                        Directory.CreateDirectory(fullPath);
                    }

                    DirSearch(val);
                }

                // 파일 생성
                foreach (var val in fileList.S3Objects)
                {
                    //파일 다운로드
                    GetObjectRequest req = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = val.Key
                    };

                    
                    
                    string filePath = val.BucketName.Replace(val.BucketName, folderPath);
                    GetObjectResponse res = s3Client.GetObject(req);

                    string fullPath = string.Format("{0}\\{1}", filePath, val.Key);

                    if (form1.update_state.InvokeRequired)
                        form1.update_state.Invoke(new MethodInvoker(delegate { form1.update_state.Text = "업데이트중입니다..."; }));

                    isWork = true;
                    form1.timer.Tick += form1.WorkProgressbar;

                    ///클라나 런처 받는
                    res.WriteResponseStreamToFile(fullPath);

                    isWork = false;
                    form1.timer.Tick -= form1.WorkProgressbar;

                    ///압축 해제후 알집파일 삭제
                    if (val.Key.Contains(".zip"))
                    {
                        ExtractZipByIO(fullPath, filePath);
                        File.Delete(fullPath);
                    }
                  
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            
        }

       /// <summary>
       /// 압축 해제하는 함수
       /// </summary>

        public void ExtractZipByIO(string zipPath, string extractionPath)
        {
            if (form1.update_state.InvokeRequired)
                form1.update_state.Invoke(new MethodInvoker(delegate { form1.update_state.Text = "파일 압축 해제중입니다..."; }));

            using (ZipArchive zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Read, null))
            {
                int idx = 0;
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    string fullPath = Path.GetFullPath(Path.Combine(extractionPath, entry.FullName));
                    idx++;
                    this.form1.WorkProgressbar(zipArchive.Entries.Count, idx);

                    if (Path.GetFileName(fullPath).Length != 0)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                        entry.ExtractToFile(fullPath, true);
                        
                    }
                    
                    
                }
            }
            //ZipFile.ExtractToDirectory(zipPath, extractionPath);
        }



#endregion

    }




}
