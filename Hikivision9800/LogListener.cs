using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _9800VideoTest
{
    public class LogListener:System.Diagnostics.TraceListener 
    {
        string FileName = null;
        public LogListener()
        {
            try
            {
                string FileFolder = AppDomain.CurrentDomain.BaseDirectory;
                FileName = FileFolder + "Log.txt";
                StreamWriter ws= File.CreateText(FileFolder + "Log.txt");
                ws.Close();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public override void Write(string message)
        {
           
        }
        public override void Write(object o)
        {
            base.Write(o);
        }
        public override void WriteLine(string message)
        {
            string Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (FileName!=null)
            {
               FileStream fs=new FileStream(FileName,FileMode.Append,FileAccess.Write);
               StreamWriter ws=new StreamWriter(fs);
               ws.WriteLine("【"+Time+"】"+message);
               ws.Close();
               fs.Close();
            }
           
        }
    }
}
