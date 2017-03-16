using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9800Client
{
    public class Options
    {
        /// <summary>
        /// 摄像头Id，以','隔开，最多四个。
        /// </summary>
        [Option("cameraIds", Required = true)]
        public string CameraIds { get; set; }
        /// <summary>
        /// 测试
        /// </summary>
        [VerbOption("debug", DefaultValue = false)]
        public Boolean Debug { get; set; }
        /// <summary>
        /// 注册
        /// </summary>
        [VerbOption("register", DefaultValue = false, MutuallyExclusiveSet = "unregister")]
        public Boolean Register { get; set; }
        /// <summary>
        /// 卸载
        /// </summary>
        [VerbOption("unregister", DefaultValue = false, MutuallyExclusiveSet = "register")]
        public Boolean UnRegister { get; set; }
        /// <summary>
        /// 是否一直保持
        /// </summary>
        [VerbOption("keepalive", DefaultValue = false)]
        public Boolean keepalive { get; set; }

    }
}
