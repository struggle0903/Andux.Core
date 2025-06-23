using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Andux.Core.Helper.Http
{
    /// <summary>
    /// HTTP 请求帮助类，封装了常用的 HTTP 请求操作
    /// 使用 HttpClientFactory 管理 HttpClient 生命周期
    /// 支持 GET、POST、PUT、DELETE 等 HTTP 方法
    /// 支持文件上传下载
    /// 支持 JSON 序列化/反序列化
    /// </summary>
    public class HttpHelper : IHttpHelper, IDisposable
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        /// <summary>
        /// 私有构造函数，防止外部直接实例化
        /// </summary>
        /// <param name="httpClient">HttpClient 实例</param>
        /// <param name="baseAddress">API 基础地址</param>
        /// <param name="timeout">请求超时时间（秒）</param>
        private HttpHelper(HttpClient httpClient, string? baseAddress = null, int timeout = 30)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            if (!string.IsNullOrEmpty(baseAddress))
            {
                _httpClient.BaseAddress = new Uri(baseAddress);
            }

            _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpHelper");
        }

        /// <summary>
        /// 创建 HttpHelper 实例（推荐使用依赖注入）
        /// </summary>
        /// <param name="httpClientFactory">IHttpClientFactory 实例</param>
        /// <param name="baseAddress">API 基础地址</param>
        /// <param name="timeout">请求超时时间（秒）</param>
        /// <returns>HttpHelper 实例</returns>
        public static HttpHelper Create(IHttpClientFactory httpClientFactory, string? baseAddress = null, int timeout = 30)
        {
            var httpClient = httpClientFactory.CreateClient();
            return new HttpHelper(httpClient, baseAddress, timeout);
        }

        /// <summary>
        /// 设置请求头
        /// </summary>
        /// <param name="name">请求头名称</param>
        /// <param name="value">请求头值</param>
        public void SetHeader(string name, string value)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(name))
            {
                _httpClient.DefaultRequestHeaders.Remove(name);
            }
            _httpClient.DefaultRequestHeaders.Add(name, value);
        }

        /// <summary>
        /// 设置授权头
        /// </summary>
        /// <param name="scheme">授权方案（如 Bearer）</param>
        /// <param name="parameter">授权参数</param>
        public void SetAuthorization(string scheme, string parameter)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, parameter);
        }

        /// <summary>
        /// 发送 GET 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>响应字符串</returns>
        public async Task<string> GetAsync(string url, Dictionary<string, string>? headers = null)
        {
            return await SendRequestAsync(HttpMethod.Get, url, null, headers);
        }

        /// <summary>
        /// 发送 GET 请求并返回指定类型
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>反序列化后的对象</returns>
        public async Task<T> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
        {
            var response = await GetAsync(url, headers);
            return JsonSerializer.Deserialize<T>(response, _jsonOptions);
        }

        /// <summary>
        /// 发送 POST 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求数据</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>响应字符串</returns>
        public async Task<string> PostAsync(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            return await SendRequestAsync(HttpMethod.Post, url, data, headers);
        }

        /// <summary>
        /// 发送 POST 请求并返回指定类型
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求数据</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>反序列化后的对象</returns>
        public async Task<T> PostAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            var response = await PostAsync(url, data, headers);
            return JsonSerializer.Deserialize<T>(response, _jsonOptions);
        }

        /// <summary>
        /// 发送 PUT 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求数据</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>响应字符串</returns>
        public async Task<string> PutAsync(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            return await SendRequestAsync(HttpMethod.Put, url, data, headers);
        }

        /// <summary>
        /// 发送 PUT 请求并返回指定类型
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求数据</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>反序列化后的对象</returns>
        public async Task<T> PutAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            var response = await PutAsync(url, data, headers);
            return JsonSerializer.Deserialize<T>(response, _jsonOptions);
        }

        /// <summary>
        /// 发送 DELETE 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>响应字符串</returns>
        public async Task<string> DeleteAsync(string url, Dictionary<string, string>? headers = null)
        {
            return await SendRequestAsync(HttpMethod.Delete, url, null, headers);
        }

        /// <summary>
        /// 发送 DELETE 请求并返回指定类型
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>反序列化后的对象</returns>
        public async Task<T> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null)
        {
            var response = await DeleteAsync(url, headers);
            return JsonSerializer.Deserialize<T>(response, _jsonOptions);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="filePath">本地文件路径</param>
        /// <param name="fileParamName">文件参数名（默认为"file"）</param>
        /// <param name="formData">附加表单数据</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>响应字符串</returns>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出</exception>
        public async Task<string> UploadFileAsync(string url, string filePath, string fileParamName = "file", Dictionary<string, string>? formData = null, Dictionary<string, string>? headers = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("文件不存在", filePath);
            }

            using var content = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(filePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            content.Add(fileContent, fileParamName, Path.GetFileName(filePath));

            if (formData != null)
            {
                foreach (var item in formData)
                {
                    content.Add(new StringContent(item.Value), item.Key);
                }
            }

            return await SendRequestAsync(HttpMethod.Post, url, content, headers);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">文件地址</param>
        /// <param name="savePath">本地保存路径</param>
        /// <param name="headers">自定义请求头</param>
        /// <returns>是否下载成功</returns>
        public async Task<bool> DownloadFileAsync(string url, string savePath, Dictionary<string, string>? headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var directory = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var fileStream = File.Create(savePath);
            await response.Content.CopyToAsync(fileStream);
            return true;
        }

        /// <summary>
        /// 发送 HTTP 请求（内部方法）
        /// </summary>
        /// <param name="method">HTTP 方法</param>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求数据</param>
        /// <param name="headers">自定义请求头</param>
        /// <param name="readAsString">是否读取为字符串</param>
        /// <returns>响应字符串或空字符串</returns>
        private async Task<string> SendRequestAsync(HttpMethod method, string url, object data, Dictionary<string, string> headers, bool readAsString = true)
        {
            HttpContent content = null;
            if (data != null)
            {
                if (data is HttpContent httpContent)
                {
                    content = httpContent;
                }
                else
                {
                    var json = JsonSerializer.Serialize(data, _jsonOptions);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }
            }

            return await SendRequestAsync(method, url, content, headers, readAsString);
        }

        /// <summary>
        /// 发送 HTTP 请求（内部方法）
        /// </summary>
        /// <param name="method">HTTP 方法</param>
        /// <param name="url">请求地址</param>
        /// <param name="content">HTTP 内容</param>
        /// <param name="headers">自定义请求头</param>
        /// <param name="readAsString">是否读取为字符串</param>
        /// <returns>响应字符串或空字符串</returns>
        private async Task<string> SendRequestAsync(HttpMethod method, string url, HttpContent content, Dictionary<string, string> headers, bool readAsString = true)
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = content
            };

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            if (!readAsString)
            {
                return string.Empty;
            }

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }

    }
}
