using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9800VideoTest
{
    internal class LoginInfo
    {
        public string ServiceIP { get; set; }
        public int Port { get; set; }
        public string PlatformType { get; set; }
        public string UserName { get; set; }
        public int NetZoneID { get; set; }
        public string TGT { get; set; }
        public int UserID { get; set; }
        public int IsHttps { get; set; }
        public string OcxThemeType { get; set; }
        public int HttpsPort { get; set; }

        public LoginInfo()
        {
            ServiceIP = "";
            Port = 80;
            PlatformType = "";
            UserName = "";
            NetZoneID = 0;
            TGT = "";
            UserID = 0;
            IsHttps = 0;//https 1,http 0
            HttpsPort = 443;
        }
        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("ServiceIP:" + this.ServiceIP);
            text.AppendLine("Port:" + this.Port);
            text.AppendLine("PlatformType:" + this.PlatformType);
            text.AppendLine("UserName:" + this.UserName);
            text.AppendLine("TGT:" + this.TGT);
            text.AppendLine("UserID:" + this.UserID);
            text.AppendLine("IsHttps:" + this.IsHttps);
            text.AppendLine("OcxThemeType:" + this.OcxThemeType);
            text.AppendLine("HttpsPort:" + this.HttpsPort);
            return text.ToString();
        }
    }
}
