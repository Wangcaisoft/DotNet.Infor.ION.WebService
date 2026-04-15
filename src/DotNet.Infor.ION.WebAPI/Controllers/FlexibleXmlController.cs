using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;

namespace DotNet.Infor.ION.WebAPI.Controllers
{
    public class FlexibleXmlController : ApiController
    {
        [HttpPost]
        [Route("api/xml/flexible")]
        public async Task<HttpResponseMessage> ProcessFlexible()
        {
            try
            {
                // 检查内容类型
                string contentType = Request.Content.Headers.ContentType?.MediaType ?? "";

                // 支持多种 XML 类型
                if (!contentType.Contains("xml") &&
                    !contentType.Contains("text") &&
                    !contentType.Contains("application/x-www-form-urlencoded"))
                {
                    return Request.CreateErrorResponse(
                        HttpStatusCode.UnsupportedMediaType,
                        "不支持的内容类型"
                    );
                }

                // 读取内容
                string content = await Request.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    return Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        "请求体为空"
                    );
                }

                // 尝试检测和清理 XML
                string cleanXml = CleanAndExtractXml(content, contentType);

                // 处理 XML
                var result = await ProcessXmlAsync(cleanXml);

                // 返回响应
                return CreateResponse(result);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex);
            }
        }

        [HttpPost]
        [Route("api/xml/upload")]
        public async Task<HttpResponseMessage> UploadXmlFile()
        {
            // 检查是否是 multipart/form-data
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateErrorResponse(
                    HttpStatusCode.UnsupportedMediaType,
                    "只支持 multipart/form-data"
                );
            }

            try
            {
                // 创建临时目录
                var tempPath = Path.Combine(Path.GetTempPath(), "XmlUploads");
                Directory.CreateDirectory(tempPath);

                var streamProvider = new MultipartFormDataStreamProvider(tempPath);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                // 处理上传的文件
                foreach (var file in streamProvider.FileData)
                {
                    if (Path.GetExtension(file.LocalFileName).ToLower() == ".xml")
                    {
                        string xmlContent = File.ReadAllText(file.LocalFileName, Encoding.UTF8);
                        var result = await ProcessXmlAsync(xmlContent);

                        // 清理临时文件
                        File.Delete(file.LocalFileName);

                        return CreateResponse(result);
                    }
                }

                return Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    "未找到 XML 文件"
                );
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex);
            }
        }

        private string CleanAndExtractXml(string content, string contentType)
        {
            // 如果是表单数据，尝试提取 XML
            if (contentType.Contains("x-www-form-urlencoded"))
            {
                // 假设 XML 在名为 "xml" 的表单字段中
                if (content.Contains("xml="))
                {
                    int start = content.IndexOf("xml=") + 4;
                    int end = content.IndexOf('&', start);
                    if (end == -1) end = content.Length;
                    content = content.Substring(start, end - start);
                    content = Uri.UnescapeDataString(content);
                }
            }

            // 清理可能的空格和换行
            content = content.Trim();

            // 检查是否是有效的 XML
            try
            {
                XDocument.Parse(content);
                return content;
            }
            catch
            {
                // 如果不是有效的 XML，尝试修复
                return content;
            }
        }

        private async Task<object> ProcessXmlAsync(string xmlContent)
        {
            // 异步处理 XML
            return await Task.Run(() =>
            {
                var xdoc = XDocument.Parse(xmlContent);

                return new
                {
                    Status = "Success",
                    Timestamp = DateTime.Now,
                    RootElement = xdoc.Root.Name.LocalName,
                    ElementCount = xdoc.Descendants().Count(),
                    FileSize = Encoding.UTF8.GetByteCount(xmlContent)
                };
            });
        }

        private HttpResponseMessage CreateResponse(object data)
        {
            // 根据 Accept 头返回不同格式
            var accept = Request.Headers.Accept.ToString();

            if (accept.Contains("application/json") || accept.Contains("*/*"))
            {
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            else if (accept.Contains("text/xml") || accept.Contains("application/xml"))
            {
                return CreateXmlResponse(data);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
        }

        private HttpResponseMessage CreateXmlResponse(object data)
        {
            // 将对象转换为 XML
            var xmlResponse = new XElement("Response",
                new XElement("Status", "Success"),
                new XElement("Data",
                    new XElement("Timestamp", DateTime.Now),
                    new XElement("Message", "XML 处理完成")
                )
            );

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(
                xmlResponse.ToString(),
                Encoding.UTF8,
                "text/xml"
            );
            return response;
        }

        private HttpResponseMessage CreateErrorResponse(Exception ex)
        {
            var errorResponse = new
            {
                Error = new
                {
                    Message = ex.Message,
                    Type = ex.GetType().Name,
                    StackTrace = ex.StackTrace
                }
            };

            return Request.CreateResponse(HttpStatusCode.InternalServerError, errorResponse);
        }
    }
}
