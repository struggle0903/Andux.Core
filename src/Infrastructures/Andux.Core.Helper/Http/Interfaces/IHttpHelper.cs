namespace Andux.Core.Helper.Http
{
    /// <summary>
    /// HTTP 请求帮助接口
    /// </summary>
    public interface IHttpHelper
    {
        /// <summary>
        /// 发送 GET 请求
        /// </summary>
        Task<string> GetAsync(string url, Dictionary<string, string>? headers = null);

        /// <summary>
        /// 发送 GET 请求并返回指定类型
        /// </summary>
        Task<T> GetAsync<T>(string url, Dictionary<string, string>? headers = null);

        /// <summary>
        /// 发送 POST 请求
        /// </summary>
        Task<string> PostAsync(string url, object data = null, Dictionary<string, string>? headers = null);

        /// <summary>
        /// 发送 POST 请求并返回指定类型
        /// </summary>
        Task<T> PostAsync<T>(string url, object data = null, Dictionary<string, string>? headers = null);

        /// <summary>
        /// 发送 PUT 请求
        /// </summary>
        Task<string> PutAsync(string url, object data = null, Dictionary<string, string>? headers = null);

        /// <summary>
        /// 发送 PUT 请求并返回指定类型
        /// </summary>
        Task<T> PutAsync<T>(string url, object data = null, Dictionary<string, string>? headers = null);

        /// <summary>
        /// 发送 DELETE 请求
        /// </summary>
        Task<string> DeleteAsync(string url, Dictionary<string, string>? headers = null);

        /// <summary>
        /// 发送 DELETE 请求并返回指定类型
        /// </summary>
        Task<T> DeleteAsync<T>(string url, Dictionary<string, string>? headers = null);

        /// <summary>
        /// 上传文件
        /// </summary>
        Task<string> UploadFileAsync(string url, string filePath, string fileParamName = "file", Dictionary<string, string>? formData = null, Dictionary<string, string>? headers = null);

        /// <summary>
        /// 下载文件
        /// </summary>
        Task<bool> DownloadFileAsync(string url, string savePath, Dictionary<string, string>? headers = null);

        /// <summary>
        /// 设置请求头
        /// </summary>
        void SetHeader(string name, string value);

        /// <summary>
        /// 设置授权头
        /// </summary>
        void SetAuthorization(string scheme, string parameter);
    }
}
