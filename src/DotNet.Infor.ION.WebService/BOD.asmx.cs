using DotNet.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace DotNet.WebService
{
    /// <summary>
    /// Summary description for BOD
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class BOD : System.Web.Services.WebService
    {

        [WebMethod]
        public string Save(string bod)
        {
            try
            {
                // Log the incoming BOD request
                LogUtil.WriteLog("Received BOD request: " + (bod ?? string.Empty));
                // TODO: add any further processing of the BOD string here
                return "OK";
            }
            catch (Exception ex)
            {
                try
                {
                    LogUtil.WriteException(ex);
                }
                catch
                {
                    // Swallow any logging errors to avoid throwing from the web method
                }

                return "ERROR: " + ex.Message;
            }
        }

        [WebMethod]
        public string SaveXML(System.Xml.XmlDocument xml)
        {
            try
            {
                // Log the incoming BOD request
                LogUtil.WriteLog("Received BOD request: " + (xml.ToString()));
                // TODO: add any further processing of the BOD string here
                return "OK";
            }
            catch (Exception ex)
            {
                try
                {
                    LogUtil.WriteException(ex);
                }
                catch
                {
                    // Swallow any logging errors to avoid throwing from the web method
                }

                return "ERROR: " + ex.Message;
            }
        }
        [WebMethod]
        public void JsonResponse(string bod)
        {
            Context.Response.Clear();
            //指定返回结果也是application/json
            Context.Response.ContentType = "application/json";
            //自己拼凑个json格式字符串
            string s = "{\"data\":\"" + bod + "\"}";
            Context.Response.Write(s);
            //加上下面两句是防止接收到的返回字符串后面有{"d":null}
            Context.Response.Flush();
            Context.Response.End();
        }
    }
}
