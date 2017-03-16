using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using VideoTest.WebServiceInterface;

namespace _9800VideoTest
{
    public enum SOURCETYPE
    {
        CAM = 0,//监控点
        ORG,//组织资源
        DEVICE,//设备
        MOVDEV,//移动设备
        ALARMEGN,//报警主机
        SYNPLAT,//视频综合平台
        IOALARM,//IO报警
        DOOR,//门禁
        USER,//用户
        ENV,//环境量
        TALK,//对讲通道
        DEFEN,//防区
        NOTYPE//不存在
    }
    class PlatformService
    {
        const string VERSION_NOTE = "提示：该版本过低";
        const int MAX_SIZE = 2147483647;
        public static Dictionary<string, string> _ComandValueMap = new Dictionary<string, string>();
        private static Dictionary<string, SOURCETYPE> _SourceTypeMap = new Dictionary<string, SOURCETYPE>();
        private VmsSdkWebServicePortTypeClient _ws;
        private const int _StartPage = 1;
        private const int _PageSize = 200;
        public VmsSdkWebServicePortTypeClient Interface { get { return _ws; } }

        static PlatformService()
        {
            string temp = System.Windows.Forms.Application.StartupPath;
            string ConfigFile = temp + @"\ServiceValueDef.xml";
            XElement xe = null;
            try
            {
                xe = XElement.Load(ConfigFile);
            }
            catch (Exception)
            {
                System.Diagnostics.Trace.TraceInformation("文件加载失败 ServiceValueDef.xml 失败(0):失败");
                Application.Exit();
            }
            try
            {
                Program.LoginInfo.HttpsPort = int.Parse(xe.Element("httpsport").Value);
            }
            catch (Exception)
            {
                MessageBox.Show("无法获取指定HttpsPort号，默认HttpsPort=443");
            }
            foreach (var res in xe.Element("resources").Elements("res"))
            {
                _ComandValueMap.Add(res.Attribute("des").Value, res.Attribute("value").Value);
            }

            foreach (var res in xe.Element("operations").Elements("op"))
            {
                _ComandValueMap.Add(res.Attribute("des").Value, res.Attribute("value").Value);
            }

            foreach (var res in xe.Element("ptz").Elements("cmd"))
            {
                _ComandValueMap.Add(res.Attribute("des").Value, res.Attribute("value").Value);
            }
            InitSourceTypeMap();
        }

        public static void InitSourceTypeMap()
        {
            _SourceTypeMap.Add("监控点", SOURCETYPE.CAM);
            _SourceTypeMap.Add("组织资源", SOURCETYPE.ORG);
            _SourceTypeMap.Add("设备", SOURCETYPE.DEVICE);
            _SourceTypeMap.Add("移动设备", SOURCETYPE.MOVDEV);
            _SourceTypeMap.Add("IO报警", SOURCETYPE.IOALARM);
            _SourceTypeMap.Add("报警器", SOURCETYPE.IOALARM);
            _SourceTypeMap.Add("门禁", SOURCETYPE.DOOR);
            _SourceTypeMap.Add("用户", SOURCETYPE.USER);
            _SourceTypeMap.Add("环境量", SOURCETYPE.ENV);
            _SourceTypeMap.Add("对讲通道", SOURCETYPE.TALK);
            _SourceTypeMap.Add("视频综合平台", SOURCETYPE.SYNPLAT);
            _SourceTypeMap.Add("报警主机", SOURCETYPE.ALARMEGN);
            _SourceTypeMap.Add("防区", SOURCETYPE.DEFEN);
        }

        public string Token
        {
            get
            {//获取token,接口调用失败返回空
                try
                {
                    string xmltoken = this.Interface.applyToken(Program.LoginInfo.TGT);
                    if (xmltoken == "")
                    {
                        return "";
                    }
                    ServiceResult sr = ServiceResult.Parse(xmltoken);
                    if (sr.ResultCode == 0)
                    {
                        System.Diagnostics.Trace.TraceInformation("获取Token VmsSdkService!applyToken 成功(0):成功");
                        return sr.Rows[0]["st"];
                    }
                    else
                    {
                        System.Diagnostics.Trace.TraceInformation("获取Token VmsSdkService!applyToken 失败({0}):{1}", sr.ResultCode, sr.ResultMsg);
                        return "";
                    }

                }
                catch
                {
                    System.Diagnostics.Trace.TraceInformation("WS接口调用失败 applyToken 失败(0):失败");
                }
                return "";
            }
        }

        public PlatformService(string ip, int port)//根据服务器ip、port动态加载平台服务
        {
            Trace.TraceInformation("加载平台服务");
            if (Program.LoginInfo.IsHttps == 1)
            {
                ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidate;
                BasicHttpBinding bhb = new BasicHttpBinding()
                {
                    MaxBufferSize = MAX_SIZE,
                    MaxReceivedMessageSize = MAX_SIZE,
                    Security = new BasicHttpSecurity() { Mode = BasicHttpSecurityMode.Transport },
                    ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas() { MaxStringContentLength = MAX_SIZE }
                };
                string url = string.Format("https://{0}:{1}/vms/services/VmsSdkWebService.VmsSdkWebServiceHttpSoap11Endpoint/", ip, port);
                _ws = new VmsSdkWebServicePortTypeClient(bhb, new EndpointAddress(url));
            }
            else
            {
                BasicHttpBinding bhb = new BasicHttpBinding()
                {
                    MaxBufferSize = MAX_SIZE,
                    MaxReceivedMessageSize = MAX_SIZE,
                    ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas() { MaxStringContentLength = MAX_SIZE }
                };
                string url = string.Format("http://{0}:{1}/vms/services/VmsSdkWebService.VmsSdkWebServiceHttpSoap11Endpoint/", ip, port);
                _ws = new VmsSdkWebServicePortTypeClient(bhb, new EndpointAddress(url));
                Trace.TraceInformation("平台服务加载完毕");
            }

        }

        private static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            // trust any certificate!!!  
            //System.Console.WriteLine("Warning, trust any certificate");
            return true;
        }

        public static void TracePrint(ServiceResult sr, string info, string Interface)//打印接口调用结果
        {
            string msg = info + ' ';
            msg += Interface + ' ';
            msg += sr.ResultMsg + '(' + sr.ResultCode + ')' + ':';
            msg += sr.ResultMsg;
            System.Diagnostics.Trace.TraceInformation(msg);
        }

        public static TreeNode FindTreeNode(string name, TreeNode tnParent)//根据parent_id查找父节点
        {
            if (tnParent == null)
                return null;
            SourceInfo sourceinfo = (SourceInfo)(tnParent.Tag);
            if (sourceinfo.sourceid == name)
                return tnParent;
            TreeNode tnRet = null;
            foreach (TreeNode tn in tnParent.Nodes)
            {
                tnRet = FindTreeNode(name, tn);
                if (tnRet != null)
                    break;
            }
            return tnRet;
        }

        public static TreeNode CallFindNodeByName(string name, TreeView treeView)
        {
            TreeNodeCollection nodes = treeView.Nodes;
            foreach (TreeNode n in nodes)
            {
                TreeNode temp = FindTreeNode(name, n);
                if (temp != null)
                    return temp;
            }
            return null;
        }

        public static string SHA256Encrypt(string str)//256加密
        {
            byte[] SHA256Data = Encoding.UTF8.GetBytes(str);
            System.Security.Cryptography.SHA256 Sha256 = new System.Security.Cryptography.SHA256Managed();
            byte[] by = Sha256.ComputeHash(SHA256Data);
            return BitConverter.ToString(by).Replace("-", "").ToLower();
        }

        public static string GetEnvParamSpot(string sourcetype, TreeNode treenode, ref List<TreeNode> SourceMonitor)//按组织获取环境量资源
        {
            //string[] code = {"sensor_158813956587359","sensor_158842351613543"};
            //string Inf = Program.Service.Interface.getResourceByCodes(Program.Service.Token, int.Parse(ComandValueMap[sourcetype]), code);//按资源编号获取资源信息
            TreeNode node_child;
            SourceInfo parntinfo = (SourceInfo)treenode.Tag;
            string indexcode = parntinfo.IndexCode;
            string pInBuf = "\r\n" + treenode.Text + sourcetype + "\r\n";
            if (treenode.Text == "杭州市")
            {
                string aa = "";
                aa += "23fearf";
            }
            int command = int.Parse(_ComandValueMap[sourcetype]);
            string Info = PlatformService.getResourceByOrgCode(command, indexcode, "");
            if (Info == "")
            {
                return "";
            }
            ServiceResult sr = ServiceResult.Parse(Info);
            SourceInfo sourceinfo = new SourceInfo();
            int count = sr.Rows.Count;
            for (int i = 0; i < count; i++)
            {
                node_child = new TreeNode();
                sourceinfo.SourceName = node_child.Text = sr.Rows[i]["extend_resource_name"];
                sourceinfo.Type = GetSourceType(sourcetype);//环境量
                sourceinfo.IndexCode = sr.Rows[i]["extend_resource_code"];
                SourceInfo panrtinfo = (SourceInfo)(treenode.Tag);
                sourceinfo.ParentIndexCode = panrtinfo.IndexCode;//用户对应组织节点索引号
                treenode.Expand();
                node_child.Tag = sourceinfo;
                treenode.Nodes.Add(node_child);
                if (SourceMonitor != null)
                {
                    SourceMonitor.Add(node_child);
                }
            }
            TracePrint(sr, "获取" + sourcetype, "getResourceByOrgCode");
            return pInBuf + Info;
        }

        public static string GetComnSourceSpot(string sourcetype, TreeNode treenode, ref List<TreeNode> SourceMonitor, bool hisalarm)//获取监控点资源，并将监控点信息保存
        {
            TreeNode node_child;
            SourceInfo sourinfo = (SourceInfo)(treenode.Tag);
            string indexcode = sourinfo.IndexCode;
            string command = _ComandValueMap[sourcetype];
            string Info = PlatformService.getResourceByOrgCode(int.Parse(command), indexcode, "");
            if (Info == "")
            {
                return "";
            }
            if (treenode.Text == "主控中心")
            {
                string aa = "";
                aa = "测试代码";
            }
            ServiceResult sr = ServiceResult.Parse(Info);
            SourceInfo sourceinfo = new SourceInfo();
            int count = sr.Rows.Count;
            for (int i = 0; i < count; i++)
            {
                SOURCETYPE type = GetSourceType(sourcetype);
                if (type == SOURCETYPE.IOALARM)//报警器，且为报警输出，历史告警资源查询,或为防区
                {
                    if ((hisalarm == true && type == SOURCETYPE.IOALARM && sr.Rows[i]["i_type"] == "1") || sr.Rows[i]["i_res_type"] != "40000")
                    {
                        continue;
                    }
                }
                node_child = new TreeNode();
                sourceinfo.SourceName = node_child.Text = sr.Rows[i]["c_name"];
                sourceinfo.Type = GetSourceType(sourcetype);
                sourceinfo.IndexCode = sr.Rows[i]["c_index_code"];
                SourceInfo parntinf = (SourceInfo)(treenode.Tag);
                sourceinfo.ParentIndexCode = parntinf.IndexCode;
                sourceinfo.DeviceIndexcode = sr.Rows[i]["c_device_index_code"];
                if (sourceinfo.Type == SOURCETYPE.IOALARM)//IO资源
                {
                    sourceinfo.AlarmType = int.Parse(sr.Rows[i]["i_res_type"]);//报警器类型
                }
                if (sourceinfo.Type == SOURCETYPE.CAM)
                {
                    sourceinfo.SignalState = int.Parse(sr.Rows[i]["i_is_online"]);//在线状态
                }
                node_child.Tag = sourceinfo;
                treenode.Expand();
                treenode.Nodes.Add(node_child);
                if (SourceMonitor != null)
                {
                    SourceMonitor.Add(node_child);
                }
            }
            TracePrint(sr, "获取" + sourcetype, "getResourceByOrgCode");
            return Info;
        }

        public static SOURCETYPE GetSourceType(string sourceType)
        {
            if (_SourceTypeMap.ContainsKey(sourceType))
            {
                return _SourceTypeMap[sourceType];
            }
            return SOURCETYPE.NOTYPE;
        }

        public static string GetUsersInfo(string sourcetype, TreeNode treenode, ref List<TreeNode> SourceMonitor)//获取用户信息
        {
            TreeNode node_child;
            SourceInfo parntinfo = (SourceInfo)treenode.Tag;
            string indexcode = parntinfo.IndexCode;
            string pInBuf = "\r\n" + treenode.Text + sourcetype + "\r\n";
            string Info = PlatformService.getResourceByOrgCode(int.Parse(_ComandValueMap[sourcetype]), indexcode, "");
            if (Info == "")
            {
                return "";
            }
            ServiceResult sr = ServiceResult.Parse(Info);
            SourceInfo sourceinfo = new SourceInfo();
            int count = sr.Rows.Count;
            for (int i = 0; i < count; i++)
            {
                node_child = new TreeNode();
                node_child.Text = sourcetype + ":";
                node_child.Text += sr.Rows[i]["c_user_name"];
                node_child.Text += " ";
                node_child.Text += sr.Rows[i]["c_person_desc"];
                sourceinfo.Type = GetSourceType(sourcetype);

                sourceinfo.UserLevel = sr.Rows[i]["i_level"];//用户等级
                sourceinfo.SourceName = sr.Rows[i]["c_user_name"];
                SourceInfo panrtinfo = (SourceInfo)(treenode.Tag);
                sourceinfo.ParentIndexCode = panrtinfo.IndexCode;//用户对应组织节点索引号
                treenode.Expand();
                node_child.Tag = sourceinfo;
                treenode.Nodes.Add(node_child);
                if (SourceMonitor != null)
                {
                    SourceMonitor.Add(node_child);
                }
            }
            TracePrint(sr, "获取" + sourcetype, "getResourceByOrgCode");
            return pInBuf + Info;
        }

        public static string GetSource(TreeView treeview)//为回放、预览提供监控点资源树
        {
            string pinbuf;
            if (treeview.Nodes.Count != 0)
            {
                return "";
            }
            treeview.Nodes.Clear();
            ServiceResult SourceSr = null;
            string sourcetype = "组织资源";
            int command = int.Parse(PlatformService._ComandValueMap[sourcetype]);
            pinbuf = PlatformService.GetResourceByPage(command, ref SourceSr);
            int count = SourceSr.Rows.Count;
            TreeNode node_alarmtype, node_parent;
            SourceInfo sourceinfo = new SourceInfo();
            for (int i = 0; i < count; i++)//构建监控点资源树的组织资源
            {
                node_alarmtype = new TreeNode();
                sourceinfo.SourceName = node_alarmtype.Text = SourceSr.Rows[i]["c_org_name"];
                sourceinfo.sourceid = SourceSr.Rows[i]["i_id"];
                sourceinfo.IndexCode = SourceSr.Rows[i]["c_index_code"];
                sourceinfo.Type = GetSourceType(sourcetype);
                if (SourceSr.Rows[i]["i_parent_id"] == "null" || SourceSr.Rows[i]["i_parent_id"] == "c_0")
                {
                    sourceinfo.parntid = "";
                    node_alarmtype.Tag = sourceinfo;
                    treeview.Nodes.Add(node_alarmtype);
                }
                else
                {
                    node_parent = PlatformService.CallFindNodeByName(SourceSr.Rows[i]["i_parent_id"], treeview);
                    node_parent.Expand();
                    SourceInfo parntinfo = (SourceInfo)(node_parent.Tag);
                    node_alarmtype.Tag = sourceinfo;
                    node_parent.Nodes.Add(node_alarmtype);
                }
            }
            return pinbuf;
        }

        public static string GetResourceByPage(int command, ref ServiceResult sr)//分页获取组织资源，并将组织资源信息保存在sr中
        {
            int startpage = _StartPage;
            string result = PlatformService.getResourceByPage(command, startpage, _PageSize, "", "", 1);
            if (result == "")
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("第{0}页", startpage));
            sb.Append("\r\n");
            sb.Append(result);
            sb.Append("\r\n");

            sr = ServiceResult.Parse(result);
            while (sr.ResultCode == 0 && sr.Total > sr.Rows.Count)
            {
                result = PlatformService.getResourceByPage(command, ++startpage, _PageSize, "", "", 1);
                if (result == "")
                {
                    return "";
                }
                ServiceResult tmp = ServiceResult.Parse(result);
                sr.Rows.AddRange(tmp.Rows);

                sb.Append(string.Format("第{0}页", startpage));
                sb.Append("\r\n");
                sb.Append(result);
                sb.Append("\r\n");
            }
            TracePrint(sr, "获取组织资源", "VmsSdkService!getResourceByPage");
            return sb.ToString();
        }

        public static string GetComnSource1(string sourcetype, TreeNodeCollection treenodes, ref List<TreeNode> sourcemonitor, bool hisalarm)//对组织树添加指定类型资源
        {
            string pInBuf = "";
            if (treenodes == null)
            {
                return "";
            }
            foreach (TreeNode child in treenodes)
            {
                if (((SourceInfo)child.Tag).Type == SOURCETYPE.NOTYPE)
                {
                    continue;
                }
                if (child.Nodes.Count > 0)
                {
                    pInBuf += GetComnSource1(sourcetype, child.Nodes, ref sourcemonitor, hisalarm);
                }
                if (sourcetype == "用户信息")
                {
                    pInBuf += "\r\n";
                    pInBuf += PlatformService.GetUsersInfo(sourcetype, child, ref sourcemonitor);
                }
                else if (sourcetype == "环境量")
                {
                    pInBuf += "\r\n";
                    pInBuf += PlatformService.GetEnvParamSpot(sourcetype, child, ref sourcemonitor);
                }
                else //监控点、io报警器、门禁、防区
                {
                    pInBuf += "\r\n";
                    pInBuf += PlatformService.GetComnSourceSpot(sourcetype, child, ref sourcemonitor, hisalarm);
                }
            }
            return pInBuf;
        }

        public static string UpdateEncodDeviceTree(string sourcetype, TreeView treeview, ref Dictionary<SourceInfo, string> sourcemap)//获取编码设备、报警主机、视频综合资源
        {
            string pInbuf;
            string token = Program.Service.Token;
            int command = int.Parse(PlatformService._ComandValueMap[sourcetype]);
            ServiceResult sr = null;
            pInbuf = PlatformService.GetResourceByPage(command, ref sr);
            int count = sr.Rows.Count;
            SourceInfo sourceinfo = new SourceInfo();
            for (int i = 0; i < count; i++)
            {
                sourceinfo.Type = GetSourceType(sourcetype);
                sourceinfo.IndexCode = sr.Rows[i]["c_index_code"];
                sourceinfo.sourceid = sr.Rows[i]["i_id"];
                sourceinfo.SourceName = sr.Rows[i]["c_name"];
                sourceinfo.ENCODETYPE = sr.Rows[i]["c_treaty_type"];
                if (treeview != null)
                {
                    TreeNode node_AlarmType;
                    node_AlarmType = new TreeNode();
                    node_AlarmType.Text = sr.Rows[i]["c_name"];
                    node_AlarmType.Tag = sourceinfo;
                    treeview.Nodes.Add(node_AlarmType);
                }
                if (sourcemap != null)
                {
                    sourcemap[sourceinfo] = sourceinfo.IndexCode;
                }
            }
            return pInbuf;
        }

        public static string UpdateCamSourceByPage(ref Dictionary<SourceInfo, string> sourcemap)//获取监控点资源
        {
            string pInbuf;
            string token = Program.Service.Token;
            string sourcetype = "监控点";
            int command = int.Parse(PlatformService._ComandValueMap[sourcetype]);
            ServiceResult sr = null;
            pInbuf = PlatformService.GetResourceByPage(command, ref sr);
            int count = sr.Rows.Count;
            SourceInfo sourceinfo = new SourceInfo();
            for (int i = 0; i < count; i++)
            {
                sourceinfo.SourceName = sr.Rows[i]["c_name"];
                sourceinfo.Type = GetSourceType(sourcetype);
                sourceinfo.IndexCode = sr.Rows[i]["c_index_code"];
                sourceinfo.SignalState = int.Parse(sr.Rows[i]["i_is_online"]);//在线状态
                sourceinfo.sourceid = sr.Rows[i]["i_id"];
                if (sourcemap.ContainsKey(sourceinfo))
                {
                    string ss = "";
                }
                sourcemap[sourceinfo] = sourceinfo.IndexCode;
            }
            return pInbuf;
        }

        //查询历史告警信息
        public static string GetAlarmHistory(string alarmName, int confirmStatus, string sourceName, string alarmTypes, string alarmLevels,
            string startTime, string endTime, string sourceIndexCodes, int sourceType, ref ServiceResult sr)
        {
            string orderFields = "";
            int sort = 0;
            int pageNo = _StartPage;
            string orgIndexCodes = "";
            string srcIndexCode = "";
            string result = "";
            result = PlatformService.getAlarmHistory(alarmName, confirmStatus, sourceName, alarmTypes, alarmLevels, startTime,
                    endTime, orderFields, sort, _PageSize, pageNo, sourceIndexCodes, sourceType, orgIndexCodes, srcIndexCode);
            if (result == "")
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(result);
            sr = ServiceResult.Parse(result);
            while (sr.ResultCode == 0 && sr.Total > sr.Rows.Count)
            {
                result = PlatformService.getAlarmHistory(alarmName, confirmStatus, sourceName, alarmTypes, alarmLevels, startTime,
                endTime, orderFields, sort, _PageSize, ++pageNo, sourceIndexCodes, sourceType, orgIndexCodes, srcIndexCode);
                if (result == "")
                {
                    continue;
                }
                ServiceResult tmp = ServiceResult.Parse(result);
                sr.Rows.AddRange(tmp.Rows);
                sb.Append("\r\n");
                sb.Append(result);
            }
            return sb.ToString();
        }

        public static string getAlarmHistory(string alarmName, int confirmStatus, string sourceName, string alarmTypes, string alarmLevels, string startTime, string endTime,
            string orderFields, int sort, int pageSize, int pageNo, string sourceIndexCodes, int sourceType, string orgIndexCodes, string srcIndexCode)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.getAlarmHistory(Program.Service.Token, alarmName, confirmStatus, sourceName, alarmTypes, alarmLevels, startTime,
                    endTime, orderFields, sort, _PageSize, pageNo, sourceIndexCodes, sourceType, orgIndexCodes, srcIndexCode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 getAlarmHistory 失败(0):失败");
            }
            return result;
        }

        public static string GetPicUrls(string guID, int protocol)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.getPictureUrl(Program.Service.Token, guID, protocol);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 getPictureUrl 失败(0):失败");
            }
            return result;
        }
        public static string getPlaybackOcxOptions(string cameraIndexCode, string clientIp, string beginTime, string endTime, string storeDeviceType)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.getPlaybackOcxOptions(Program.Service.Token, cameraIndexCode, clientIp, beginTime, endTime, storeDeviceType);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 getPlaybackOcxOptions 失败(0):失败");
            }
            return result;
        }

        public static string getCruisePath(string deviceIndexCode, string cameraIndexCode, int cruiseRoute)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.getCruisePath(Program.Service.Token, deviceIndexCode, cameraIndexCode, cruiseRoute);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 getCruisePath 失败(0):失败");
            }
            return result;
        }

        public static string checkResourcePrivilege(string resIndexCode, int resType, string operCode)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.checkResourcePrivilege(Program.Service.Token, resIndexCode, resType, operCode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 checkResourcePrivilege 失败(0):失败");
            }
            return result;
        }
        public static string getPreviewOcxOptions(string cameraIndexCode, string clientIp)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.getPreviewOcxOptions(Program.Service.Token, cameraIndexCode, clientIp);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 getPreviewOcxOptions 失败(0):失败");
            }
            return result;
        }
        public static string gotoCruisePath(string deviceIndexCode, string cameraIndexCode, int cruiseRoute, string command)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.gotoCruisePath(Program.Service.Token, deviceIndexCode, cameraIndexCode, cruiseRoute, command);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 gotoCruisePath 失败(0):失败");
            }
            return result;
        }
        public static string addPresetToCruisePath(string deviceIndexCode, string cameraIndexCode, int cruiseRoute, int presetIndex, int dwellTime, int speed)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.addPresetToCruisePath(Program.Service.Token, deviceIndexCode, cameraIndexCode, cruiseRoute, presetIndex, dwellTime, speed);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 addPresetToCruisePath 失败(0):失败");
            }
            return result;
        }
        public static string delPresetToCruisePathByNumber(string deviceIndexCode, string cameraIndexCode, int cruiseRoute, int index)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.delPresetToCruisePathByNumber(Program.Service.Token, deviceIndexCode, cameraIndexCode, cruiseRoute, index);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 delPresetToCruisePathByNumber 失败(0):失败");
            }
            return result;
        }
        public static string setPresetListToCruisePath(string deviceIndexCode, string cameraIndexCode, int cruiseRoute, string list, int dwellTime, int speed)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.setPresetListToCruisePath(Program.Service.Token, deviceIndexCode, cameraIndexCode, cruiseRoute, list, dwellTime, speed);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 setPresetListToCruisePath 失败(0):失败");
            }
            return result;
        }
        public static string getResourceByOrgCode(int resType, string orgCode, string operCode)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.getResourceByOrgCode(Program.Service.Token, resType, orgCode, operCode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 getResourceByOrgCode 失败(0):失败");
            }
            return result;
        }
        public static string getResourceByPage(int resType, int pageNo, int pageSize, string operCode, string orderBy, int sort)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.getResourceByPage(Program.Service.Token, resType, pageNo, pageSize, operCode, orderBy, sort);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 getResourceByPage 失败(0):失败");
            }
            return result;
        }
        public static string getAlarmIoStatus(int resType, string alarmIoIndexCode)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.getAlarmIoStatus(Program.Service.Token, resType, alarmIoIndexCode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 getAlarmIoStatus 失败(0):失败");
            }
            return result;
        }
        public static string getPresetInfo(string cameraIndexCode)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.getPresetInfo(Program.Service.Token, cameraIndexCode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 getPresetInfo 失败(0):失败");
            }
            return result;
        }
        public static string changePresetInfo(string deviceIndexCode, string cameraIndexCode, int presetNo, string presetName)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.changePresetInfo(Program.Service.Token, deviceIndexCode, cameraIndexCode, presetNo, presetName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 changePresetInfo 失败(0):失败");
            }
            return result;
        }
        public static string alarmIoControl(int resType, string alarmIoIndexCode, int controlValue)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.alarmIoControl(Program.Service.Token, resType, alarmIoIndexCode, controlValue);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 alarmIoControl 失败(0):失败");
            }
            return result;
        }
        public static string ptzControl(string cameraIndexCode, string command, string proprity, string action, string speed)
        {
            string result = "";
            try
            {
                result = Program.Service.Interface.ptzControl(Program.Service.Token, cameraIndexCode, command, proprity, action, speed);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 ptzControl 失败(0):失败");
            }
            return result;
        }
        public static string sdkLogin(string loginAccount, string password, string serviceIp, string clientIp, string clientMac)
        {
            Trace.TraceInformation("开始SDKLOGIN");
            string result = "";
            try
            {
                result = Program.Service.Interface.sdkLogin(loginAccount, password, serviceIp, clientIp, clientMac);
                Trace.TraceInformation("SDKLOGIN成功");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation(ex.ToString());

                Trace.TraceInformation("SDKLOGIN失败");
                System.Diagnostics.Trace.TraceInformation("WS接口调用失败 sdkLogin 失败(0):失败");
            }
            return result;
        }
    }
}
