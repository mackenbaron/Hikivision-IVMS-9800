using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace _9800VideoTest
{
    public struct StrParam
    {
        public string m_snapPath;
        public string m_snapType;
        public string m_recordPath;
        public string m_recordSizeIdx;
        public string m_decode;
    };

    class PreviewGlobalParam
    {
        public static StrParam GetParam()
        {
            StrParam param = new StrParam();
            string curPath = System.Windows.Forms.Application.StartupPath;
            string tempPath = curPath + @"\GlobalParam\PreviewParam.ini";
            if (System.IO.File.Exists(tempPath))//从本地配置文件读取上一次设置的参数
            {
                StringBuilder picPath = new StringBuilder(255);
                NativeMethods.GetPrivateProfileString("snap", "save", "", picPath, 255, tempPath);
                param.m_snapPath = picPath.ToString();

                StringBuilder type = new StringBuilder(255);
                NativeMethods.GetPrivateProfileString("snap", "picture", "", type, 255, tempPath);
                param.m_snapType = type.ToString();

                StringBuilder VdPath = new StringBuilder(255);
                NativeMethods.GetPrivateProfileString("record", "save", "", VdPath, 255, tempPath);
                param.m_recordPath = VdPath.ToString();

                StringBuilder sizeIdx = new StringBuilder(255);
                NativeMethods.GetPrivateProfileString("record", "packSize", "", sizeIdx, 255, tempPath);
                param.m_recordSizeIdx = sizeIdx.ToString();

                StringBuilder decode = new StringBuilder(255);
                NativeMethods.GetPrivateProfileString("performance", "decode", "", decode, 255, tempPath);
                param.m_decode = decode.ToString();
            }
            else
            {
                param.m_snapPath = curPath + @"\snap";
                param.m_snapType = "jpeg";
                param.m_recordPath = curPath + @"\clip";
                param.m_recordSizeIdx = "256";
                param.m_decode = "5";
            }
            return param;
        }

        public static bool SetParam(AxPlatformPreviewLib.AxPlatformPreview axPlatformPreview1, StrParam param)
        {
            string curPath = System.Windows.Forms.Application.StartupPath;
            string sPath = curPath + @"\GlobalParam";
            if (!Directory.Exists(sPath))
            {
                Directory.CreateDirectory(sPath);
            }
            string tempPath = curPath + @"\GlobalParam\PreviewParam.ini";
            //组成xml报文
            XmlDocument xmldoc = new XmlDocument();
            XmlDeclaration dec = xmldoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmldoc.AppendChild(dec);
            XmlElement root = xmldoc.CreateElement("global");
            XmlElement child;
            child = xmldoc.CreateElement("snap");
            child.SetAttribute("picture", param.m_snapType);
            child.SetAttribute("save", param.m_snapPath);
            root.AppendChild(child);
            child = xmldoc.CreateElement("record");
            child.SetAttribute("packSize", param.m_recordSizeIdx);
            child.SetAttribute("save", param.m_recordPath);
            root.AppendChild(child);
            child = xmldoc.CreateElement("performance");
            child.SetAttribute("decode", param.m_decode);
            root.AppendChild(child);
            xmldoc.AppendChild(root);
            string strxml = xmldoc.InnerXml;

            //调用控件接口
            int ret = axPlatformPreview1.SetGlobalParam(strxml);

            if (ret == 0) //成功
            {
                Trace.TraceInformation("全局参数设置 PlatformPreview.ocx!SetGlobalParam 成功({0}):成功", ret);
                NativeMethods.WritePrivateProfileString("snap", "picture", param.m_snapType, tempPath);
                NativeMethods.WritePrivateProfileString("snap", "save", param.m_snapPath, tempPath);
                NativeMethods.WritePrivateProfileString("record", "packSize", param.m_recordSizeIdx, tempPath);
                NativeMethods.WritePrivateProfileString("record", "save", param.m_recordPath, tempPath);
                NativeMethods.WritePrivateProfileString("performance", "decode", param.m_decode, tempPath);
                return true;
            }
            else          //失败
            {
                Trace.TraceInformation("全局参数设置 PlatformPreview.ocx!SetGlobalParam 失败({0}):失败", ret);
                return false;
            }
        }
    }
}
