using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace _9800VideoTest
{
    public class ServiceResult
    {
        public int ResultCode { get; set; }
        public string ResultMsg { get; set; }
        public int Total { get; set; }
        public int PageSize { get; set; }
        public int PageNo { get; set; }

        public ServiceResult()
        {
            Rows = new List<Dictionary<string, string>>();
        }
        public List<Dictionary<string, string>> Rows { get; set; }
        static public ServiceResult Parse(string result)
        {
            ServiceResult lr = new ServiceResult();
            using (MemoryStream mem = new MemoryStream(UnicodeEncoding.UTF8.GetBytes(result)))
            {
                XmlTextReader xml = new XmlTextReader(mem);
                while (xml.Read())
                {
                    if (xml.NodeType == XmlNodeType.Element)
                    {
                        if (xml.LocalName == "result")
                        {
                            lr.ResultCode = int.Parse(xml.GetAttribute("result_code"));
                            lr.ResultMsg = xml.GetAttribute("message");
                            string tmp = xml.GetAttribute("total");
                            if (tmp != null)
                            {
                                lr.Total = int.Parse(tmp);
                            }
                            tmp = xml.GetAttribute("page_size");
                            if (tmp != null)
                            {
                                lr.PageSize = int.Parse(tmp);
                            }
                            tmp = xml.GetAttribute("page_no");
                            if (tmp != null)
                            {
                                lr.PageNo = int.Parse(tmp);
                            }
                        }
                        else if (xml.LocalName == "row")
                        {
                            if (xml.HasAttributes)
                            {
                                Dictionary<string, string> rowItem = new Dictionary<string, string>();
                                for (int i = 0; i < xml.AttributeCount; i++)
                                {
                                    xml.MoveToAttribute(i);
                                    rowItem[xml.Name] = xml.Value;
                                }
                                xml.MoveToElement();
                                lr.Rows.Add(rowItem);
                            }
                        }
                    }
                }
            }
            return lr;
        }

        static public ServiceResult Add(ServiceResult lr, string addstr)
        {
            using (MemoryStream mem = new MemoryStream(UnicodeEncoding.UTF8.GetBytes(addstr)))
            {
                XmlTextReader xml = new XmlTextReader(mem);
                while (xml.Read())
                {
                    if (xml.NodeType == XmlNodeType.Element)
                    {
                        if (xml.LocalName == "row")
                        {
                            if (xml.HasAttributes)
                            {
                                Dictionary<string, string> rowItem = new Dictionary<string, string>();
                                for (int i = 0; i < xml.AttributeCount; i++)
                                {
                                    xml.MoveToAttribute(i);
                                    rowItem[xml.Name] = xml.Value;
                                }
                                xml.MoveToElement();
                                lr.Rows.Add(rowItem);
                            }
                        }
                    }
                }
            }
            return lr;
        }
        static public ServiceResult Print(string result)
        {
            ServiceResult lr = new ServiceResult();
            using (MemoryStream mem = new MemoryStream(UnicodeEncoding.UTF8.GetBytes(result)))
            {
                XmlTextReader xml = new XmlTextReader(mem);
                while (xml.Read())
                {
                    if (xml.NodeType == XmlNodeType.Element)
                    {
                        if (xml.LocalName == "result")
                        {
                            lr.ResultCode = int.Parse(xml.GetAttribute("result_code"));
                            lr.ResultMsg = xml.GetAttribute("message");
                            break;
                            //string tmp = xml.GetAttribute("total");
                            //if (tmp != null)
                            //{
                            //    lr.Total = int.Parse(tmp);
                            //}
                            //tmp = xml.GetAttribute("page_size");
                            //if (tmp != null)
                            //{
                            //    lr.PageSize = int.Parse(tmp);
                            //}
                            //tmp = xml.GetAttribute("page_no");
                            //if (tmp != null)
                            //{
                            //    lr.PageNo = int.Parse(tmp);
                            //}
                        }
                    }
                }
            }
            return lr;
        }
    }
}
