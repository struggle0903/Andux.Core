using Andux.Core.Helper.Http;
using Andux.Core.Testing.Controllers.Base;
using Andux.Core.Testing.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers
{
    [AllowAnonymous]
    public class HttpHelperController : ApiBaseController
    {
        private readonly IHttpHelper _httpHelper;
        private readonly ILogger<HttpHelperController> _logger;

        public HttpHelperController(
            IHttpHelper httpHelper,
            ILogger<HttpHelperController> logger)
        {
            _httpHelper = httpHelper;
            _logger = logger;
        }

        [HttpGet("test-get")]
        public async Task<IActionResult> TestGet()
        {
            try
            {
                // 测试普通GET请求
                var result = await _httpHelper.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestGet failed");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("test-get-typed")]
        public async Task<IActionResult> TestGetTyped()
        {
            try
            {
                // 测试带类型的GET请求
                var post = await _httpHelper.GetAsync<PostModel>("https://jsonplaceholder.typicode.com/posts/1");
                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestGetTyped failed");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("test-post")]
        public async Task<IActionResult> TestPost([FromBody] PostModel data)
        {
            try
            {
                // 测试POST请求
                var result = await _httpHelper.PostAsync("https://jsonplaceholder.typicode.com/posts", data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestPost failed");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("test-post-typed")]
        public async Task<IActionResult> TestPostTyped([FromBody] PostModel data)
        {
            try
            {
                // 测试带类型的POST请求
                var createdPost = await _httpHelper.PostAsync<PostModel>("https://jsonplaceholder.typicode.com/posts", data);
                return Ok(createdPost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestPostTyped failed");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("test-upload")]
        public async Task<IActionResult> TestUpload(IFormFile file)
        {
            try
            {
                // 测试文件上传
                var tempPath = Path.GetTempFileName();
                await using (var stream = System.IO.File.Create(tempPath))
                {
                    await file.CopyToAsync(stream);
                }

                var result = await _httpHelper.UploadFileAsync(
                    "https://example.com/upload",
                    tempPath,
                    formData: new Dictionary<string, string> { { "description", "Test file" } });

                System.IO.File.Delete(tempPath);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestUpload failed");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("test-download")]
        public async Task<IActionResult> TestDownload()
        {
            try
            {
                // 测试文件下载
                var savePath = Path.Combine(Path.GetTempPath(), "downloaded-file.txt");
                var success = await _httpHelper.DownloadFileAsync(
                    "https://example.com/sample-file.txt",
                    savePath);

                if (!success)
                    return BadRequest("Download failed");

                var fileBytes = await System.IO.File.ReadAllBytesAsync(savePath);
                System.IO.File.Delete(savePath);
                return File(fileBytes, "application/octet-stream", "downloaded-file.txt");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TestDownload failed");
                return StatusCode(500, ex.Message);
            }
        }

    }
}
