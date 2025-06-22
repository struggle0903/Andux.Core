using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace Andux.Core.Helper.Network
{
    /// <summary>
    /// 网络工具帮助类
    /// </summary>
    public static class NetworkHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// 获取主机名
        /// </summary>
        /// <returns></returns>
        public static string GetHostName()
        {
            return Dns.GetHostName();
        }

        /// <summary>
        /// 检查端口是否开放
        /// </summary>
        /// <returns></returns>
        public static string GetMacAddress()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(nic =>
                    nic.OperationalStatus == OperationalStatus.Up &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            return interfaces?.GetPhysicalAddress().ToString() ?? string.Empty;
        }

        /// <summary>
        /// 检查端口是否开放
        /// </summary>
        /// <param name="host">主机名</param>
        /// <param name="port">端口</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public static async Task<bool> IsPortOpenAsync(string host, int port, int timeout = 2000)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port);
                var result = await Task.WhenAny(connectTask, Task.Delay(timeout));
                return result == connectTask && client.Connected;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取本机 IPv4 地址
        /// </summary>
        public static string GetLocalIPv4()
        {
            foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取公网 IP（通过接口）
        /// </summary>
        public static async Task<string> GetPublicIpAsync()
        {
            try
            {
                // 你可以换成你信任的 API
                var response = await _httpClient.GetStringAsync("https://api.ipify.org");
                return response.Trim();
            }
            catch
            {
                return "未知";
            }
        }

        /// <summary>
        /// 判断是否连接到互联网（通过 ping 8.8.8.8）
        /// </summary>
        public static bool IsConnectedToInternet()
        {
            try
            {
                using var ping = new Ping();
                var reply = ping.Send("8.8.8.8", 2000);
                return reply?.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ping 一个地址
        /// </summary>
        /// <param name="host">host地址</param>
        /// <param name="timeout">等待时间</param>
        /// <returns></returns>
        public static async Task<bool> PingHostAsync(string host, int timeout = 2000)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host, timeout);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 下载网页源码
        /// </summary>
        /// <param name="url">网页url</param>
        /// <returns></returns>
        public static async Task<string> DownloadHtmlAsync(string url)
        {
            try
            {
                return await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                return $"下载失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 下载文件到本地
        /// </summary>
        /// <param name="url">文件url</param>
        /// <param name="localPath">本地路径</param>
        /// <returns></returns>
        public static async Task<bool> DownloadFileAsync(string url, string localPath)
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(localPath, bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static async Task<string> UploadFileAsync(string url, string filePath, string fieldName = "file")
        {
            using var content = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(filePath);
            content.Add(new StreamContent(fileStream), fieldName, Path.GetFileName(filePath));
            var response = await _httpClient.PostAsync(url, content);
            return await response.Content.ReadAsStringAsync();
        }

    }
}
