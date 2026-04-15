using DotNet.Util;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;

namespace DotNet.WebService
{
    /// <summary>
    /// Summary description for InforBOD
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class InforBOD : System.Web.Services.WebService
    {

        [WebMethod]
        //允许 POST 请求接收原始 XML
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Xml)]
        public string Save()
        {
            try
            {
                // 获取整个请求体的 XML 内容
                using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                {
                    // 重置流的位置到开头（重要！）
                    HttpContext.Current.Request.InputStream.Position = 0;

                    // 读取完整的 XML 内容
                    string xmlContent = reader.ReadToEnd();

                    // 记录或验证 XML
                    //System.Diagnostics.Debug.WriteLine($"Received XML: {xmlContent}");
                    LogUtil.WriteLog(xmlContent);

                    // 处理 XML
                    return ProcessXmlContent(xmlContent);
                }
            }
            catch (Exception ex)
            {
                return $"Error processing XML: {ex.Message}";
            }
        }

        private string ProcessXmlContent(string xmlContent)
        {
            // 在这里处理 XML
            if (string.IsNullOrEmpty(xmlContent))
                return "No XML content received";

            // 例如：解析 XML
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(xmlContent);
            }
            catch
            {
                return "Invalid XML content";
            }

            // 做一些处理...
            return $"Successfully processed XML with root: {xmlDoc.DocumentElement.Name}";
        }
    }
}
