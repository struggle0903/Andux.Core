using Andux.Core.Helper.Config;
using Andux.Core.Helper.Monitor;
using Andux.Core.Helper.Network;
using Andux.Core.Helper.Picture;
using Andux.Core.Helper.Reflection;
using Andux.Core.Helper.Tasks;
using Andux.Core.Helper.Validation;
using Andux.Core.Testing.Model;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelperController : ControllerBase
    {
        private readonly ConfigHelper _config;

        public HelperController(ConfigHelper config)
        {
            _config = config;
        }

        [HttpGet("get-config")]
        public IActionResult TestGet()
        {
            var settings = _config.Get<AppSettings>("AppSettings");
            return Ok(settings);
        }

        [HttpGet("validation-test")]
        public IActionResult TestValidation()
        {
            #region 验证方式1
            var user = new ValidationModel { UserName = "", Email = "invalid-email" };
            var validator = new ValidationHelper().ValidateDataAnnotations(user);
            if (!validator.IsValid)
            {
                foreach (var error in validator.ErrorMessages)
                {
                    Console.WriteLine(error);
                }
            }
            #endregion

            #region 验证方式2
            // 简单验证
            var validationResult = Validator.Validate(user);

            // 直接验证并抛出异常
            //Validator.ValidateAndThrow(user);

            // 创建验证器实例
            var validator2 = new ValidationHelper()
                .ValidateDataAnnotations(user)  // 验证数据注解
                .Required("", "密码")  // 必填验证
                .Length("123", "密码", 6, 20)  // 长度验证
                .Must(() => "123" == "345", "两次密码输入不一致");  // 自定义验证

            // 获取验证结果
            var result = validator2.ToResult();
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error);
                }
            }
            #endregion

            #region 验证方式3
            // 单个验证
            var result2 = new ValidationHelper()
                .ValidateDataAnnotations(user)
                .Must(() => "123" == "456", "两次密码输入不一致")
                .ToResult();
            if (!result2.IsValid)
            {
                return BadRequest(result2.Errors);
            }
            #endregion

            return NoContent();
        }

        [HttpGet("monitor")]
        public async Task<IActionResult> Monitor()
        {
            // 1. 简单计时
            var elapsedMs = PerformanceMonitor.MeasureExecutionTime(() =>
            {
                Thread.Sleep(100);
                _ = new byte[1024 * 1024]; // 分配1MB内存
            });
            Console.WriteLine($"执行时间: {elapsedMs}ms");

            // 2. 内存使用
            var memory = PerformanceMonitor.GetMemoryUsage();
            Console.WriteLine($"内存使用: {memory}");

            // 3. CPU使用率
            double cpuUsage = await PerformanceMonitor.GetCpuUsageAsync();
            Console.WriteLine($"CPU 使用率: {cpuUsage:0.00}%");

            // 4. 使用代码块计时（推荐）
            using (PerformanceMonitor.TimeBlock("数据库查询"))
            {
                // 业务代码
                Thread.Sleep(100);
            }

            return NoContent();
        }

        [HttpGet("task")]
        public async Task<IActionResult> TaskTest()
        {
            // 超时执行
            var result = await TaskHelper.RunWithTimeoutAsync(async () =>
            {
                await Task.Delay(3000);
                return "OK";
            }, TimeSpan.FromSeconds(2)); // 会抛 TimeoutException

            // 重试操作
            int count = 0;
            var value = await TaskHelper.RetryAsync(async () =>
            {
                count++;
                if (count < 3)
                    throw new Exception("模拟失败");
                return "Success";
            });

            return NoContent();
        }

        [HttpGet("network")]
        public async Task<IActionResult> NetworkTest()
        {
            Console.WriteLine("本地IP: " + NetworkHelper.GetLocalIPv4());

            string publicIp = await NetworkHelper.GetPublicIpAsync();
            Console.WriteLine("公网IP: " + publicIp);

            bool isOnline = NetworkHelper.IsConnectedToInternet();
            Console.WriteLine("联网状态: " + isOnline);

            bool pingBaidu = await NetworkHelper.PingHostAsync("baidu.com");
            Console.WriteLine("Ping 百度: " + pingBaidu);

            string html = await NetworkHelper.DownloadHtmlAsync("https://www.example.com");
            Console.WriteLine("网页源码前100字: " + html.Substring(0, 100));

            bool downloadSuccess = await NetworkHelper.DownloadFileAsync("https://img0.baidu.com/it/u=748313141,2176232325&fm=253&fmt=auto&app=120&f=JPEG?w=500&h=667", "D:\\素材\\images\\test\\123.jpeg");
            Console.WriteLine("文件下载成功: " + downloadSuccess);

            return NoContent();
        }

        [HttpGet("rflection")]
        public IActionResult ReflectionTest()
        {
            // 获取/设置属性
            var person = new ValidationModel { UserName = "Tom" };
            var name = ReflectionHelper.GetPropertyValue(person, "Email"); // "Tom"
            ReflectionHelper.SetPropertyValue(person, "Email", "2212907254@qq.com");

            // 获取所有方法和属性
            var props = ReflectionHelper.GetPropertyNames(typeof(ValidationModel), includePrivate: true);
            var methods = ReflectionHelper.GetMethodNames(typeof(ValidationModel));

            // 动态创建对象实例
            //var instance = (ValidationModel)ReflectionHelper.CreateInstance(typeof(ValidationModel), "UserName");

            return NoContent();
        }

        [HttpGet("image")]
        public IActionResult ImageTest()
        {
            string source = "D:\\素材\\images\\test\\001.jpg";
            string target = "D:\\素材\\images\\test\\001_resize.png";

            double scale = 0.5; // 缩小50%
            ImageHelper.ResizeImage(source, target, scale);
            Console.WriteLine("图片缩放完成");

            // 裁剪区域：从(50, 50)开始，宽100，高100像素
            ImageHelper.CropImage(source, "D:\\素材\\images\\test\\001_crop.png", 50, 50, 100, 100);
            Console.WriteLine("图片裁剪完成");

            string imagePath = "D:\\素材\\images\\test\\001.jpg";
            string base64String = ImageHelper.ImageToBase64(imagePath);
            Console.WriteLine("Base64字符串：");
            Console.WriteLine(base64String);

            string outputPath = "D:\\素材\\images\\test\\base_001.jpg";
            ImageHelper.Base64ToImage(base64String, outputPath);
            Console.WriteLine("Base64转图片完成");

            return NoContent();
        }
    }
}
