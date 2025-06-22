using Microsoft.AspNetCore.Mvc;
using Andux.Core.Extensions;
using System.Text;
using Andux.Core.Helper.Validation;

namespace Andux.Core.Testing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtensionsController : ControllerBase
    {
        /// <summary>
        /// string 扩展示例
        /// </summary>
        [HttpPost("strExample")]
        public IActionResult StringExExample()
        {
            var str = "HelloWorld";
            bool empty = str.IsNullOrEmptyEx();          // false
            string md5 = str.ToMD5();                    // MD5 哈希
            string shortStr = str.SafeSubstring(5);      // "Hello"
            string snake = str.ToSnakeCase();            // "hello_world"
            string camel = "hello_world".ToCamelCase();  // "HelloWorld"

            var s = "helloWorld";
            var sha = s.ToSHA256();
            var reversed = s.Reverse();
            var capitalized = s.Capitalize();
            var intVal = "123".ToInt();
            var safeInt = "abc".ToInt(999);       // 999
            var encoded = "你好".UrlEncode();
            var decoded = encoded.UrlDecode();

            var masked = "13812345678".Mask(3, 4);      // 138****5678
            var safeFile = "abc:123?.txt".ToSafeFileName();  // abc_123_.txt
            var clean = "<p>Hello</p>".RemoveHtmlTags();    // Hello
            var repeated = "Hi".Repeat(3);                 // HiHiHi
            var sqlEscaped = "100%_test".EscapeForSqlLike(); // 100[%][_]test
            var hex = "Hi".HexEncode();                    // 4869
            var isChs = "你好".IsChinese();                 // true


            return Ok(new { message = "登录成功" });
        }

        private static readonly Dictionary<string, string> _appSettings = new()
        {
            ["Theme"] = "Dark",
            ["Language"] = "zh-CN"
        };

        /// <summary>
        /// 更新配置项
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="value">新值</param>
        [HttpPut("{key}")]
        public IActionResult UpdateConfig(string key, [FromBody] string value)
        {
            // 使用 AddOrUpdate 扩展方法
            _appSettings.AddOrUpdate(key, value);
            return NoContent();
        }

        /// <summary>
        /// 文件操作
        /// </summary>
        [HttpPut("file")]
        public IActionResult FileTest()
        {
            var file = new FileInfo("example.txt");

            // 文件属性与元数据
            var friendlyTime = file.GetFriendlyLastWriteTime();
            bool isReadOnly = file.IsReadOnly();

            // 文件路径处理
            var pureName = file.GetFileNameWithoutExtension();
            var tempPath = file.GetTempCopyPath();

            // 文件内容分析
            int lines = file.GetLineCount();
            Encoding encoding = file.DetectEncoding();
            bool contains = file.ContainsText("important");

            // 文件操作
            file.SafeMoveTo("newLocation/example.txt");
            file.CopyWithTimestamps("backup/example.txt");
            string relativePath = file.GetRelativePath(new DirectoryInfo("parentDir"));
            return NoContent();
        }

        /// <summary>
        /// 异常操作
        /// </summary>
        [HttpPut("exception")]
        public IActionResult ExceptionTest()
        {
            try
            {
                // 模拟抛出异常
                throw new InvalidOperationException("外层异常",
                    new ArgumentNullException("参数为空",
                        new DivideByZeroException("不能除以零")));
            }
            catch (Exception ex)
            {
                // 获取完整异常消息
                string fullMessage = ex.GetFullMessage();
                Console.WriteLine("完整消息:");
                Console.WriteLine(fullMessage);

                // 获取完整堆栈跟踪
                string fullStackTrace = ex.GetFullStackTrace();
                Console.WriteLine("\n完整堆栈跟踪:");
                Console.WriteLine(fullStackTrace);

                // 检查是否包含特定类型异常
                bool containsArgumentNull = ex.ContainsInnerException<ArgumentNullException>();
                Console.WriteLine($"\n是否包含ArgumentNullException: {containsArgumentNull}");

                // 获取第一个特定类型异常
                var divideByZeroEx = ex.GetFirstInnerException<DivideByZeroException>();
                Console.WriteLine($"\n第一个DivideByZeroException消息: {divideByZeroEx?.Message}");

                // 获取异常数据
                var exceptionData = ex.GetExceptionData();
                Console.WriteLine("\n异常数据:");
                foreach (var kvp in exceptionData)
                {
                    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                }

                // 获取格式化字符串
                string formattedString = ex.ToFormattedString();
                Console.WriteLine("\n格式化异常信息:");
                Console.WriteLine(formattedString);

                // 获取所有异常详细信息
                var allDetails = ex.GetAllExceptionDetails();
                Console.WriteLine("\n所有异常详细信息:");
                foreach (var detail in allDetails)
                {
                    Console.WriteLine($"Level {detail.Level}: {detail.TypeName} - {detail.Message}");
                }

                // 转换为简单JSON
                string json = ex.ToSimpleJson();
                Console.WriteLine("\nJSON格式:");
                Console.WriteLine(json);
            }

            return NoContent();
        }

       
    }
}
