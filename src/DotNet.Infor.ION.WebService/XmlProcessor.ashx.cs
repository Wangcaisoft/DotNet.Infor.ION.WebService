using DotNet.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace DotNet.WebService
{
    /// <summary>
    /// Summary description for XmlProcessor
    /// </summary>
    public class XmlProcessor : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            // 确保是 POST 请求
            if (!string.Equals(context.Request.HttpMethod, "POST",
                StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 405; // Method Not Allowed
                context.Response.Write("<Error><Message>只支持 POST 请求</Message></Error>");
                return;
            }

            // 检查内容类型
            if (!context.Request.ContentType.StartsWith("text/xml") &&
                !context.Request.ContentType.StartsWith("application/xml"))
            {
                context.Response.StatusCode = 415; // Unsupported Media Type
                context.Response.Write("<Error><Message>只支持 XML 内容</Message></Error>");
                return;
            }

            context.Response.ContentType = "text/xml; charset=utf-8";

            try
            {
                // 读取原始 XML
                string rawXml;
                using (StreamReader reader = new StreamReader(
                    context.Request.InputStream,
                    Encoding.UTF8))
                {
                    rawXml = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(rawXml))
                {
                    context.Response.Write("<Error><Message>XML 内容为空</Message></Error>");
                    return;
                }

                // 处理 XML
                string result = ProcessXml(rawXml);
                context.Response.Write(result);
            }
            catch (XmlException xmlEx)
            {
                context.Response.StatusCode = 400; // Bad Request
                context.Response.Write($"<Error><Message>XML 格式错误: {xmlEx.Message}</Message></Error>");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500; // Internal Server Error
                context.Response.Write($"<Error><Message>服务器错误: {ex.Message}</Message></Error>");
            }
        }

        private string ProcessXml(string xmlContent)
        {
            // 解析 XML
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            LogUtil.WriteLog("Received XML: " + xmlContent, "XmlProcessor");

            // 这里可以添加你的业务逻辑
            XmlNode root = doc.DocumentElement;

            // 示例：创建一个响应
            XmlDocument response = new XmlDocument();
            XmlElement responseRoot = response.CreateElement("Response");
            response.AppendChild(responseRoot);

            XmlElement status = response.CreateElement("Status");
            status.InnerText = "Success";
            responseRoot.AppendChild(status);

            XmlElement timestamp = response.CreateElement("Timestamp");
            timestamp.InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            responseRoot.AppendChild(timestamp);

            return response.OuterXml;
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}