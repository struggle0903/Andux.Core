using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Andux.Core.Extensions
{
    /// <summary>
    /// 文件扩展类
    /// </summary>
    public static class FileExtensions
    {
        #region 文件信息

        /// <summary>
        /// 获取文件最后修改时间的友好显示
        /// </summary>
        public static string GetFriendlyLastWriteTime(this FileInfo file)
        {
            return file.Exists ? file.LastWriteTime.ToFriendlyRelativeTime() : "文件不存在";
        }

        /// <summary>
        /// 获取友好格式的文件大小（如 1.2 MB）
        /// </summary>
        public static string GetFriendlyFileSize(this FileInfo file)
        {
            if (!file.Exists)
                return "0 Bytes";

            long bytes = file.Length;
            string[] sizes = { "Bytes", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }
            return $"{bytes:0.##} {sizes[order]}";
        }

        /// <summary>
        /// 检查文件是否设置为只读
        /// </summary>
        public static bool IsReadOnly(this FileInfo file)
        {
            return file.Exists && file.IsReadOnly;
        }

        /// <summary>
        /// 获取文件MIME类型（基于扩展名）
        /// </summary>
        public static string GetMimeType(this FileInfo file)
        {
            string extension = file.Extension.ToLowerInvariant();
            return extension switch
            {
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// 获取不带扩展名的纯文件名
        /// </summary>
        public static string GetFileNameWithoutExtension(this FileInfo file)
        {
            return Path.GetFileNameWithoutExtension(file.Name);
        }

        /// <summary>
        /// 获取文件的临时副本路径（自动处理重名）
        /// </summary>
        public static string GetTempCopyPath(this FileInfo file)
        {
            var tempDir = Path.GetTempPath();
            var tempFileName = $"{Path.GetFileNameWithoutExtension(file.Name)}_{Guid.NewGuid()}{file.Extension}";
            return Path.Combine(tempDir, tempFileName);
        }

        /// <summary>
        /// 获取文件所在目录的父目录名称
        /// </summary>
        public static string GetParentDirectoryName(this FileInfo file)
        {
            return file.Directory?.Parent?.Name ?? string.Empty;
        }

        /// <summary>
        /// 获取文本文件的行数
        /// </summary>
        public static int GetLineCount(this FileInfo file)
        {
            if (!file.Exists) return 0;
            return File.ReadLines(file.FullName).Count();
        }

        /// <summary>
        /// 检测文本文件的编码类型
        /// </summary>
        public static Encoding DetectEncoding(this FileInfo file)
        {
            if (!file.Exists) return Encoding.Default;

            using var reader = new StreamReader(file.FullName, Encoding.Default, true);
            reader.Peek(); // 必须读取才能检测编码
            return reader.CurrentEncoding;
        }

        /// <summary>
        /// 检查文件是否包含指定文本（区分大小写）
        /// </summary>
        public static bool ContainsText(this FileInfo file, string searchText, bool ignoreCase = false)
        {
            if (!file.Exists) return false;

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return File.ReadLines(file.FullName)
                       .Any(line => line.Contains(searchText, comparison));
        }

        /// <summary>
        /// 比较两个文件内容是否完全相同
        /// </summary>
        public static bool ContentEquals(this FileInfo file, FileInfo otherFile)
        {
            if (!file.Exists || !otherFile.Exists) return false;
            if (file.Length != otherFile.Length) return false;

            return file.GetMd5Hash() == otherFile.GetMd5Hash();
        }

        /// <summary>
        /// 检查当前文件是否比另一个文件更新（基于修改时间）
        /// </summary>
        public static bool IsNewerThan(this FileInfo file, FileInfo otherFile)
        {
            if (!file.Exists || !otherFile.Exists) return false;
            return file.LastWriteTime > otherFile.LastWriteTime;
        }

        /// <summary>
        /// 安全移动文件（自动创建目标目录）
        /// </summary>
        public static void SafeMoveTo(this FileInfo file, string destPath)
        {
            var destFile = new FileInfo(destPath);
            destFile.Directory?.Create();
            file.MoveTo(destPath);
        }

        /// <summary>
        /// 复制文件并保留原始文件的创建/修改时间
        /// </summary>
        public static void CopyWithTimestamps(this FileInfo file, string destPath)
        {
            var destFile = new FileInfo(destPath);
            file.CopyTo(destPath, true);
            destFile.CreationTime = file.CreationTime;
            destFile.LastWriteTime = file.LastWriteTime;
            destFile.LastAccessTime = file.LastAccessTime;
        }

        /// <summary>
        /// 获取相对于指定基目录的文件路径
        /// </summary>
        public static string GetRelativePath(this FileInfo file, DirectoryInfo baseDir)
        {
            var fileUri = new Uri(file.FullName);
            var baseUri = new Uri(baseDir.FullName + Path.DirectorySeparatorChar);
            return Uri.UnescapeDataString(baseUri.MakeRelativeUri(fileUri).ToString());
        }

        #endregion

        #region 基础文件操作扩展

        /// <summary>
        /// 安全读取文件所有文本内容（自动处理文件不存在情况）
        /// </summary>
        public static string SafeReadAllText(this FileInfo file, string defaultValue = "")
        {
            return file.Exists ? File.ReadAllText(file.FullName) : defaultValue;
        }

        /// <summary>
        /// 异步安全读取文件所有文本内容
        /// </summary>
        public static async Task<string> SafeReadAllTextAsync(this FileInfo file, string defaultValue = "")
        {
            return file.Exists ? await File.ReadAllTextAsync(file.FullName) : defaultValue;
        }

        /// <summary>
        /// 安全写入文件内容（自动创建目录）
        /// </summary>
        public static void SafeWriteAllText(this FileInfo file, string content)
        {
            file.Directory?.Create();
            File.WriteAllText(file.FullName, content);
        }

        /// <summary>
        /// 异步安全写入文件内容
        /// </summary>
        public static async Task SafeWriteAllTextAsync(this FileInfo file, string content)
        {
            file.Directory?.Create();
            await File.WriteAllTextAsync(file.FullName, content);
        }

        /// <summary>
        /// 安全删除文件（如果存在）
        /// </summary>
        public static void SafeDelete(this FileInfo file)
        {
            if (file.Exists)
            {
                file.Delete();
            }
        }

        #endregion

        #region 文件校验与哈希

        /// <summary>
        /// 计算文件的MD5哈希值
        /// </summary>
        public static string GetMd5Hash(this FileInfo file)
        {
            using var md5 = MD5.Create();
            using var stream = file.OpenRead();
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 计算文件的SHA256哈希值
        /// </summary>
        public static string GetSha256Hash(this FileInfo file)
        {
            using var sha256 = SHA256.Create();
            using var stream = file.OpenRead();
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 验证文件是否与给定哈希匹配
        /// </summary>
        public static bool VerifyHash(this FileInfo file, string expectedHash, HashAlgorithmType algorithm = HashAlgorithmType.MD5)
        {
            var actualHash = algorithm == HashAlgorithmType.MD5 ? file.GetMd5Hash() : file.GetSha256Hash();
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        public enum HashAlgorithmType
        {
            MD5,
            SHA256
        }

        #endregion

        #region 文件类型判断

        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private static readonly string[] VideoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".flv", ".wmv" };
        private static readonly string[] DocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt" };

        /// <summary>
        /// 检查文件是否为图片（根据扩展名判断）
        /// </summary>
        public static bool IsImageFile(this FileInfo file)
        {
            return ImageExtensions.Contains(file.Extension.ToLowerInvariant());
        }

        /// <summary>
        /// 检查文件是否为视频（根据扩展名判断）
        /// </summary>
        public static bool IsVideoFile(this FileInfo file)
        {
            return VideoExtensions.Contains(file.Extension.ToLowerInvariant());
        }

        /// <summary>
        /// 检查文件是否为文档（根据扩展名判断）
        /// </summary>
        public static bool IsDocumentFile(this FileInfo file)
        {
            return DocumentExtensions.Contains(file.Extension.ToLowerInvariant());
        }

        #endregion

        #region 文件转换

        /// <summary>
        /// 将文件内容转换为Base64字符串
        /// </summary>
        public static string ToBase64String(this FileInfo file)
        {
            var bytes = File.ReadAllBytes(file.FullName);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 将文件内容转换为字节数组
        /// </summary>
        public static byte[] ToByteArray(this FileInfo file)
        {
            return File.ReadAllBytes(file.FullName);
        }

        /// <summary>
        /// 生成图片缩略图
        /// </summary>
        public static void CreateThumbnail(this FileInfo imageFile, FileInfo outputFile, int maxWidth, int maxHeight)
        {
            using var image = SixLabors.ImageSharp.Image.Load(imageFile.FullName);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(maxWidth, maxHeight),
                Mode = ResizeMode.Max
            }));
            image.Save(outputFile.FullName);
        }

        /// <summary>
        /// 文件格式转base64
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<string> ConvertIFormFileToBase64(this IFormFile file)
        {
            // 检查文件是否为空
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("文件为空");
            }

            // 创建一个内存流来保存文件内容
            using var memoryStream = new MemoryStream();

            // 将文件内容复制到内存流中
            await file.CopyToAsync(memoryStream);

            // 获取字节数组
            byte[] fileBytes = memoryStream.ToArray();

            // 将字节数组转换为 Base64 字符串
            return Convert.ToBase64String(fileBytes);
        }

        #endregion

        #region 字符串路径扩展

        /// <summary>
        /// 将字符串路径转换为FileInfo对象
        /// </summary>
        public static FileInfo ToFileInfo(this string filePath)
        {
            return new FileInfo(filePath);
        }

        /// <summary>
        /// 安全读取文件所有文本内容（字符串路径版本）
        /// </summary>
        public static string SafeReadFile(this string filePath, string defaultValue = "")
        {
            return filePath.ToFileInfo().SafeReadAllText(defaultValue);
        }

        /// <summary>
        /// 安全写入文件内容（字符串路径版本）
        /// </summary>
        public static void SafeWriteFile(this string filePath, string content)
        {
            filePath.ToFileInfo().SafeWriteAllText(content);
        }

        #endregion

    }
}
