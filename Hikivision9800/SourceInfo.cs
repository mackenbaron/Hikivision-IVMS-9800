using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _9800VideoTest
{
    public struct SourceInfo
    {
        public string IndexCode { get; set; }//资源索引号 
        public string sourceid { get; set; }
        public SOURCETYPE Type { get; set; }//资源类型
        public string UserLevel { get; set; }//用户等级
        public string ParentIndexCode { get; set; }//对应组织节点索引号
        public int AlarmType { get; set; }//报警类型
        //public int SourceLevel { get; set; }//资源级别,跟节点为1，监控点为0
        public int SignalState { get; set; }//资源在线状态 0在线 1不在线
        public string DeviceIndexcode { get; set; }//所属设备编号
        public string SourceName { get; set; }//资源名称

        public string parntid { get; set; }
        public int chosestate { get; set; }
        public string ENCODETYPE { get; set; }//编码设备类型 B-B接口；EHOME;GB28181;ONVIF
    }
    public enum EventState
    {
        State_Instant = 0,      //瞬时
        State_Start = 1,        //开始
        State_Stop = 2,         //停止
        State_Impluse = 3,      //脉冲
    };

    //事件等级
    public enum EventLevel
    {
        Level_High = 1,     //高
        Level_Medium = 2,   //中
        Level_Low = 3,      //低
    };

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct AlarmInfo
    {
        //事件发生的日志ID
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string eventLogId;
        //告警事件类型，见说明文档
        public int eventType;
        //事件状态
        public EventState status;
        //报警开始时间
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string startTime;
        //报警结束时间
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string stopTime;
        //事件配置单号
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string eventConfigId;
        //事件名称
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string eventName;
        //事件等级
        public EventLevel eventLevel;
        //产生告警的对象类型，见说明文档
        public int objectType;
        //事件源对象编号
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string objectIndexcode;
        //事件源对象名称
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string objectName;
        //告警源所在组织编号
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string orgIndexCode;
        //告警源所在组织名称
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string orgName;
        //描述信息
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string describe;
        //图片信息
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string picUrls;
        //抓拍机图片信息
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string carPlate;
        //扩展信息
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string extInfo;

        //触发信息
        //public TriggerInfo Triggerinfo;

    }

    //[StructLayoutAttribute(LayoutKind.Sequential)]
    //public struct TriggerInfo
    //    {
    //        public string triggertype;
    //        public int triggerret;
    //        public char triggertime;
    //        public string triggerobject;
    //        public string triggerinfo;//含抓图信息
    //        public TriggerInfo next;
    //    };
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct EnvInfo
    {
        //事件源对象ID，即设备ID
        public int objectId;
        //事件源对象类型，即设备类型，见说明文档
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string objectType;
        //事件源对象名称，即环境名称
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string objectName;
        //事件源对象编号，即设备编码
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string objectCode;
        //事件源对象类型
        public int subType;
        //实时值
        public double analogVal;
        //通道号
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string channelNo;
        //触发时间
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string alarmTime;
    };

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct GPSInfo
    {
        //设备编号 
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string deviceIndexcode;
        //元素id
        public int elementId;
        //经度
        public double longitude;
        //纬度
        public double latitude;
        //速度
        public double speed;
        //方向
        public double direction;
        //数据上报时间，格式如：2015-07-21 18:14:11
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dataTime;
        //东西半球
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string devisionEw;
        //南北半球
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string devisionNs;
        //GPS卫星数量
        public int sateNum;
    };

    //监控点状态信息
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct NmsMsgInfo
    {
        //事件发生的日志ID

        public int resType;//告警源类型

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string resindexcode;//告警源编号

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string alarmtype;//告警类型
        public EventState alarmstate;//事件状态

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string alarmtime;//产生时间

        public int alarmLevel;//事件等级

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string resName;//告警源名称

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string alarmcontext;//告警事件描述
    };

    //实时报警回调定义
    public delegate int AlarmInfoCallback(ref AlarmInfo alarminfo, IntPtr userdata);

    //环境量实时数据回调定义
    public delegate int EnvInfoCallback(ref EnvInfo envinfo, IntPtr userdata);


    //GPS实时数据回调定义
    public delegate int GPSInfoCallback(ref GPSInfo gpsInfo, IntPtr userdata);

    //实时获取监控点状态
    public delegate int NmsMsgCallback(ref NmsMsgInfo camstatinfo, IntPtr userdata);
    internal static class NativeMethods
    {
        //初始化接口
        [DllImport("MsgReceiverdll\\PlatformMsgReceiver.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Init();

        //反初始化接口
        [DllImport("MsgReceiverdll\\PlatformMsgReceiver.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void UnInit();

        // 声明INI文件的写操作函数 WritePrivateProfileString()  
        [System.Runtime.InteropServices.DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern uint WritePrivateProfileString(string section, string key, string val, string filePath);
        // 声明INI文件的读操作函数 GetPrivateProfileString()  
        [System.Runtime.InteropServices.DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern uint GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);

        //获取句柄(0表示接口调用失败，非0表示所返回的句柄), type为平台类型标识字符串
        [DllImport("MsgReceiverdll\\PlatformMsgReceiver.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern int GetHandle(string type, string platAddr);

        //释放句柄
        [DllImport("MsgReceiverdll\\PlatformMsgReceiver.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int FreeHandle(int handle);

        //注册报警信息回调接口(9800平台userId请填0)
        [DllImport("MsgReceiverdll\\PlatformMsgReceiver.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int SetAlarmInfoCallback(int handle, AlarmInfoCallback cbf, IntPtr userdata, int userId);

        //注册环境量实时信息回调接口
        [DllImport("MsgReceiverdll\\PlatformMsgReceiver.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int SetEnvInfoCallback(int handle, EnvInfoCallback cbf, IntPtr userdata);

        //注册GPS实时数据回调接口
        [DllImport("MsgReceiverdll\\PlatformMsgReceiver.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int SetGPSInfoCallback(int handle, GPSInfoCallback cbf, IntPtr userdata);

        //注册GPS实时数据回调接口
        [DllImport("MsgReceiverdll\\PlatformMsgReceiver.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int SetNmsMsgCallback(int handle, NmsMsgCallback cbf, IntPtr userdata);

        //下载智能设备告警抓图图片
        [DllImport("MsgReceiverdll\\PlatformMsgReceiver.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int DownloadPic(string url, string filePath, string token);
    }
}
