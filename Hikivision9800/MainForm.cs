using _9800Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace _9800VideoTest
{
    public partial class MainForm : Form
    {
        const int EVENT_SNAPSHOT = 0x02000001;
        const int EVENT_RECORD = 0x02000002;
        const int EVENT_STARTPREVIEW = 0x02000005;
        const int EVENT_STOPPREVIEW = 0x02000006;
        const int EVENT_GETTOKEN = 0x0200000E;
        const int EVENT_WNDCHOSECHANGE = 0x02000011;//窗口选中改变
        const int EVENT_INITFINISHED = 0x02000007;//控件初始化完毕事件
        const int EVENT_WNDEXCHANDE = 0x02000012;//窗口交换改变
        public int m_CurrentWnd;
        //窗口是否最小化和线程保持
        private bool IsAlive = false;
        private bool IsDebug = false;
        //视频参数列表
        private string[] CamaraIds = null;
        public MainForm(Options options)
        {
            IsDebug = options.Debug;
            IsAlive = options.keepalive;
            try
            {
                if (options.CameraIds != null)
                {
                    string[] temp = options.CameraIds.Split(',');
                    CamaraIds = new string[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        CamaraIds[i] =temp[i];
                        Trace.TraceInformation("参数" + (i + 1)+"【" + temp[i]+"】");
                    }
                }
            }
            catch (Exception ex)
            {
               
                if (!IsDebug)
                {
                    Trace.TraceInformation("参数转换错误，程序退出" + ex.Message);
                    System.Environment.Exit(0);
                }
                else
                {
                    Trace.TraceInformation("参数转换错误:" + ex.Message+"进入调试模式");
                }

            }
            InitializeComponent();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            InitForm();
        }
        private System.Timers.Timer timer = new System.Timers.Timer(30000);
        public AxPlatformPreviewLib.AxPlatformPreview axPlatformPreview1;
        public static Dictionary<int, string> m_CameraIndexMap = new Dictionary<int, string>();//key窗口索引号，value监控点编号
        public Dictionary<int, string> m_dicWndCode = new Dictionary<int, string>();//窗口索引与该窗口中预览的监控点IndexCode
        //public int clickCount = 1;
        private void button1_Click(object sender, EventArgs e)
        {
            InitialData();
        }
        public void InitialData()
        {
            if (IsDebug)
            {
                if (CamaraIds != null)
                {
                    this.Text = "监控编码: ";
                    foreach (string temp in CamaraIds)
                    {
                        this.Text += temp.ToString() + " ";
                    }
                    this.Text += " 视频测试";
                }
                else
                {
                    this.Text += "无参数(播放测试)";
                }
                //播放测试视频
                PlayTestVideo();
            }
            else
            {
                
                if (CamaraIds.Length != 0)
                {
                    for (int i = 0; i < CamaraIds.Length; i++)
                    {
                        Trace.TraceInformation("获取资源:" + CamaraIds[i]);
                        LoadSrc(CamaraIds[i]);
                    }
                }
                else
                {
                    MessageBox.Show("没有任何参数！");
                    Trace.TraceInformation("没有任何参数,程序退出");
                    System.Environment.Exit(0);
                }
            }
                ////Trace.TraceInformation("获取曹坪6KV配电室（code:001151）资源");
                ////  001151 曹坪6KV配电室
                ////LoadSrc("001151");
                //Trace.TraceInformation("获取藤桥原水口（code:001459）资源");
                ////藤桥原水口
                //LoadSrc("001459");
                //Trace.TraceInformation("获取藤桥原水口上游（code:001453）资源");
                ////藤桥原水口上游
                //LoadSrc("001453");
                //Trace.TraceInformation("获取藤桥后大门（code:001461）资源");
                /////藤桥后大门
                //LoadSrc("001461");
                //Trace.TraceInformation("获取西山MCC1（code:001547）资源");
                ////西山MCC1
                //LoadSrc("001547");
                //// timer.Start();
        }
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string st = Program.Service.Token;
            //Trace.TraceInformation("获取TOKEN："+st);
        }
        public void PlayTestVideo()
        {
            MessageBox.Show("PLAYING TEST VIDEO IS ON BUILDING!");
            Trace.TraceInformation("调试完毕，程序退出！");
            System.Environment.Exit(0);
        }
        private void InitForm()
        {
            Trace.TraceInformation(" 动态加载预览控件，axPlatformPreview1");
            try
            {
                // 动态加载预览控件，axPlatformPreview1
                this.axPlatformPreview1 = new AxPlatformPreviewLib.AxPlatformPreview() { Dock = DockStyle.Fill };
                this.axPlatformPreview1.Enabled = true;
                this.axPlatformPreview1.Location = new System.Drawing.Point(0, 0);
                this.axPlatformPreview1.Name = "axPlatformPreview1";
                this.axPlatformPreview1.Size = new System.Drawing.Size(917, 529);
                //this.axPlatformPreview1.TabIndex = 4;
                //this.Load += PreviewForm_Shown;
                //CrusisNumber.Text = "1";
                //Program.SetRegisCamState(0, UpdateCamState);//订阅监控点实时在线状态
                //SetCamStateInfoCallback();
                axPlatformPreview1.EventNotify += axPlatformPreview1_EventNotify;
                this.panel1.Controls.Add(axPlatformPreview1);
                //StrParam param = PreviewGlobalParam.GetParam();
                //PreviewGlobalParam.SetParam(axPlatformPreview1, param);
                //axPlatformPreview1.SetLayoutType(2);
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(" 动态加载预览控件失败："+ex.Message+ex.ToString());
            }
        }
        void axPlatformPreview1_EventNotify(object sender, AxPlatformPreviewLib._DPlatformPreviewEvents_EventNotifyEvent e)
        {
            int type = e.eventType;
            string strXml = e.eventXml;
            int wndIdx = -1;
            switch (type)
            {
                case EVENT_SNAPSHOT:    //抓图回调            
                    if (strXml == "0")
                    {
                        Trace.TraceInformation("抓图事件通知 PlatformPreview.ocx!EventNotify 窗口{0}:抓图完成", e.wndIndex);
                    }
                    else
                    {
                        Trace.TraceInformation("抓图事件通知 PlatformPreview.ocx!EventNotify 窗口{0}:抓图失败", e.wndIndex);
                    }
                    break;
                case EVENT_RECORD:    //录像回调                   
                    if (strXml == "0")
                    {
                        Trace.TraceInformation("录像事件通知 PlatformPreview.ocx!EventNotify 窗口{0}:录像完成", e.wndIndex);
                    }
                    else
                    {
                        Trace.TraceInformation("录像事件通知 PlatformPreview.ocx!EventNotify 窗口{0}:录像失败", e.wndIndex);
                    }
                    break;
                case EVENT_STARTPREVIEW:    //开始预览
                    if (strXml == "0")
                    {
                        //setOcxOprPrvg();
                        Trace.TraceInformation("开始预览事件通知 PlatformPreview.ocx!EventNotify 窗口{0}:开始预览", e.wndIndex);
                        WndSelectChange();
                    }
                    else
                    {
                        Trace.TraceInformation("开始预览事件通知 PlatformPreview.ocx!EventNotify 窗口{0}:预览失败", e.wndIndex);
                    }
                    break;
                case EVENT_STOPPREVIEW:    //停止预览     
                    if (strXml == "0")
                    {
                        Trace.TraceInformation("停止预览事件通知 PlatformPreview.ocx!EventNotify 窗口{0}:停止预览", e.wndIndex);
                        wndIdx = axPlatformPreview1.GetCurrentSelectWindow();
                        m_dicWndCode.Remove(wndIdx);
                        WndSelectChange();
                        if (m_CameraIndexMap.ContainsKey(wndIdx))
                        {
                            m_CameraIndexMap.Remove(wndIdx);
                        }
                    }
                    else
                    {
                        Trace.TraceInformation("停止预览事件通知 PlatformPreview.ocx!EventNotify 窗口{0}:停止预览失败", e.wndIndex);
                    }
                    break;
                case EVENT_GETTOKEN:    //token请求
                    int reqId;
                    Int32.TryParse(e.eventXml, out reqId);
                    //获取Token
                    string strToken = Program.Service.Token;
                    //设置Token
                    int ret = axPlatformPreview1.SetToken(reqId, strToken);
                    if (ret == 0)
                    {
                        Trace.TraceInformation("设置Token PlatformPreview.ocx!SetToken 成功(0):成功");
                    }
                    else
                    {
                        Trace.TraceInformation("设置Token PlatformPreview.ocx!SetToken 失败({0}):失败", ret);
                    }
                    break;
                case EVENT_WNDCHOSECHANGE:    //窗口选中改变
                    m_CurrentWnd = axPlatformPreview1.GetCurrentSelectWindow();
                    Trace.TraceInformation("窗口选中改变事件通知 PlatformPreview.ocx!EventNotify 当前窗口：{0}", e.wndIndex);
                    WndSelectChange();
                    break;
                case EVENT_INITFINISHED://初始化结束
                    //setOcxOprPrvg();                      
                    Trace.TraceInformation("控件初始化结束事件通知 PlatformPreview.ocx!EventNotify 初始化完成");
                    break;
                case EVENT_WNDEXCHANDE://窗口交换改变
                    wndIdx = axPlatformPreview1.GetCurrentSelectWindow();
                    if (axPlatformPreview1.IsPreview(wndIdx) != 0)//正在预览
                    {
                        string camindexfrom = "";
                        string camindexto = "";
                        wndIdx = axPlatformPreview1.GetCurrentSelectWindow();
                        if (m_CameraIndexMap.ContainsKey(m_CurrentWnd))//窗口交换后更新巡航路径对应预置点列表
                        {
                            camindexfrom = m_CameraIndexMap[m_CurrentWnd];
                            if (m_CameraIndexMap.ContainsKey(wndIdx))//交换窗口都在预览
                            {
                                camindexto = m_CameraIndexMap[wndIdx];
                                m_CameraIndexMap[m_CurrentWnd] = camindexto;
                                m_CameraIndexMap[wndIdx] = camindexfrom;
                            }
                            else
                            {
                                m_CameraIndexMap[wndIdx] = camindexfrom;
                                m_CameraIndexMap.Remove(m_CurrentWnd);
                            }
                        }
                    }
                    Trace.TraceInformation("窗口交换改变事件通知 PlatformPreview.ocx!EventNotify 成功(0):成功");
                    break;
                default:
                    break;
            }
        }
        private void WndSelectChange()
        {
         //   MessageBox.Show("Function Damanded");
        }

        private void LoadSrc(string CameraCode)
        {
            try
            {
                StrParam param = PreviewGlobalParam.GetParam();
                PreviewGlobalParam.SetParam(axPlatformPreview1, param);
                axPlatformPreview1.SetLayoutType(2);
                //TreeNode node = treeViewDevice.SelectedNode;
                //if (node.Nodes.Count > 0)//表明是父节点
                //{
                //    return;
                //}
                //treeViewDevice.BackColor = Color.White;
                //node.BackColor = Color.Blue;
                ////buttonStopPlay.Enabled = false;

                //SourceInfo nodeinfo = (SourceInfo)(node.Tag);
                string indexCode = CameraCode;

                //获取网域id
                int netID = Program.LoginInfo.NetZoneID;

                //获取预览XML
                //string strXml = Program.Service.Interface.getStreamInfo(strToken,indexCode,netID);
                string vmsIp = Program.LoginInfo.ServiceIP;
                Trace.TraceInformation("获取平台服务资源Code::" + indexCode + vmsIp);
                string strXml = PlatformService.getPreviewOcxOptions(indexCode, vmsIp);
                XmlDocument xmldoc = new XmlDocument();
                try
                {
                    Trace.TraceInformation("从指定字符串" + strXml + "加载xml");
                    xmldoc.LoadXml(strXml);//从指定字符串加载xml
                }
                catch (Exception ex)
                {
                    MessageBox.Show("获取预览xml失败" + ex.Message);
                    return;
                }
                Trace.TraceInformation("获取当前空闲窗口");
                int wndIdx = axPlatformPreview1.GetCurrentSelectWindow();
                Trace.TraceInformation("开始预览");
                int ret = axPlatformPreview1.StartPreview(strXml, -1);

                if (ret == 0) //成功
                {
                    if (m_CameraIndexMap.ContainsKey(wndIdx))
                    {
                        m_CameraIndexMap[wndIdx] = indexCode;
                    }
                    else
                    {
                        m_CameraIndexMap.Add(wndIdx, indexCode);
                    }
                    Trace.TraceInformation("开始预览 PlatformPreview.ocx!StartPreview 成功({0}):成功", ret);
                }
                else          //失败
                {
                    Trace.TraceInformation("开始预览 PlatformPreview.ocx!StartPreview 失败({0}):失败", ret);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("资源加载失败:" + ex.Message);
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            InitialData();
        }

    }
}
