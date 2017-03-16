using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using VideoTest.WebServiceInterface;
using System.Runtime.Serialization;
using CommandLine;
using _9800Client;
using System.Configuration;

namespace _9800VideoTest
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
           
 #if debug //如果是debug则写日志
            LogListener listener = new LogListener();
            Trace.Listeners.Add(listener);
#endif
            Trace.TraceInformation("初始化参数");
            string Params = "";
            foreach (string temp in args)
            {
                Params += temp + " ";
            }
            Params = Params.Substring(Params.LastIndexOf(':') + 1);
            Trace.TraceInformation("执行参数："+Params);
            args = Params.Split(' ');
            var options = new Options();
            Parser parser = new Parser();
            var result = parser.ParseArguments(args, options);
            if (options.Register)
            {
                Regist();
                System.Environment.Exit(0);
            }
            else if (options.UnRegister)
            {
                UnRegist();
                System.Environment.Exit(0);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
           
           

            Login();
            Trace.TraceInformation("初始化主窗口");
            Application.Run(new MainForm(options));
            Trace.TraceInformation("主窗口关闭");
        }
        private static VmsSdkWebServicePortTypeClient _ws;
        const int MAX_SIZE = 2147483647;
        public static LoginInfo LoginInfo { get; private set; }
        public static PlatformService Service { get; set; }
        public static void Login()
        {
            Trace.TraceInformation("读取配置文件并尝试连接服务");
            ServerConfig config = GetAppConfig();
            string xml = "";
            LoginInfo = new LoginInfo();
            Program.Service = new PlatformService(config.ServeiceIp,config.Port);
            xml = PlatformService.sdkLogin("3h", PlatformService.SHA256Encrypt(config.PassWord), config.ServeiceIp,"" ,"");
            if (xml == "")
            {
                return ;
            }
            ServiceResult sr = ServiceResult.Parse(xml);
            if (sr.ResultCode != 0)
            {
                Trace.TraceInformation("登录失败：{0}", sr.ResultMsg);
            }
            if (sr.Rows[0]["tgt"].Length == 0)
            {
                Trace.TraceInformation("登录失败：{0}", "用户名或密码错误");
            }
            Program.LoginInfo.IsHttps =config.IsHttps;
            Program.LoginInfo.ServiceIP = config.ServeiceIp;
            Program.LoginInfo.Port = config.Port;
            Program.LoginInfo.UserName =config.UserName;
            Program.LoginInfo.TGT = sr.Rows[0]["tgt"];
            Program.LoginInfo.NetZoneID = int.Parse(sr.Rows[0]["netzoneid"]);
            Program.LoginInfo.PlatformType = sr.Rows[0]["type"];
            Program.LoginInfo.UserID = int.Parse(sr.Rows[0]["userid"]);
            Program.LoginInfo.OcxThemeType = "blue";
            //GetResource("172.16.236.14", 80);
        }
        private static ServerConfig GetAppConfig()
        {
            try
            {
                string ip = ConfigurationManager.AppSettings["ServiceIp"];
                string user = ConfigurationManager.AppSettings["UserName"];
                string pwd = ConfigurationManager.AppSettings["PassWord"];
                int port = int.Parse(ConfigurationManager.AppSettings["Port"]);
                int isHttp = int.Parse(ConfigurationManager.AppSettings["IsHttp"]);
                return new ServerConfig(ip, user, pwd, port, isHttp);
               
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("EXE配置文件有误，请检查后重试！程序退出由于" + ex.Message);
                return null;
            }
        }
        public static void GetResource(string ip,int port)
        {
            Trace.TraceInformation("初始化连接");
            BasicHttpBinding bhb = new BasicHttpBinding()
            {
                MaxBufferSize = MAX_SIZE,
                MaxReceivedMessageSize = MAX_SIZE,
                ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas() { MaxStringContentLength = MAX_SIZE }
            };
            string url = string.Format("http://{0}:{1}/vms/services/VmsSdkWebService.VmsSdkWebServiceHttpSoap11Endpoint/", ip, port);
            _ws = new VmsSdkWebServicePortTypeClient(bhb, new EndpointAddress(url));
        }
        private static void Regist()
        {
            string protocal = "SH3H.WSMP.Client.Video9800";
            string execute = Application.ExecutablePath;

            try
            {
                // 查看注册表键值是否已经存在
                Register reg = new Register(protocal, RegDomain.ClassesRoot);
                if (reg.IsSubKeyExist())
                {
                    reg.DeleteSubKey();
                }

                // [HKEY_CLASSES_ROOT\SH3H.WAP.Client]
                reg.CreateSubKey();
                reg.WriteRegeditKey(string.Format("URL:{0} Protocol", protocal));
                reg.WriteRegeditKey("URL Protocol", "");

                // [HKEY_CLASSES_ROOT\SH3H.WAP.Client\DefaultIcon]
                reg = new Register(string.Format("{0}\\DefaultIcon", protocal), RegDomain.ClassesRoot);
                reg.CreateSubKey();
                reg.WriteRegeditKey(execute);

                // [HKEY_CLASSES_ROOT\SH3H.WAP.Client\Shell]
                reg = new Register(string.Format("{0}\\Shell", protocal), RegDomain.ClassesRoot);
                reg.CreateSubKey();

                // [HKEY_CLASSES_ROOT\SH3H.WAP.Client\Shell\Open]
                reg = new Register(string.Format("{0}\\Shell\\Open", protocal), RegDomain.ClassesRoot);
                reg.CreateSubKey();

                // [HKEY_CLASSES_ROOT\SH3H.WAP.Client\Shell\Open\command]
                reg = new Register(string.Format("{0}\\Shell\\Open\\command", protocal), RegDomain.ClassesRoot);
                reg.CreateSubKey();
                reg.WriteRegeditKey(string.Format("\"{0}\" \"%1\"", execute));
                Trace.TraceInformation("添加注册表完毕");
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("添加注册表失败：" + ex.Message);
            }
        }
        private static void UnRegist()
        {

            string protocal = "SH3H.WSMP.Client.Video9800";
            string execute = Application.ExecutablePath;

            try
            {
                // 查看注册表键值是否已经存在
                Register reg = new Register(protocal, RegDomain.ClassesRoot);
                if (reg.IsSubKeyExist())
                {
                    if (reg.DeleteSubKey())

                        Trace.TraceInformation("卸载完毕");
                    else
                        Trace.TraceInformation("卸载失败");
                }

                else return;
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("卸载失败:" + ex.Message);
            }

        }

    }
    public class ServerConfig {
       public string ServeiceIp { get; set; }
       public string UserName { get; set; }
        public string PassWord { get; set; }
         public int Port { get; set; }
        public int IsHttps { get; set; }
        public ServerConfig(string ip,string user,string pwd,int port,int isHttp)
        {
            this.ServeiceIp = ip;
            this.UserName = user;
            this.PassWord = pwd;
            this.Port = port;
            this.IsHttps = isHttp;
        }
    }
}
