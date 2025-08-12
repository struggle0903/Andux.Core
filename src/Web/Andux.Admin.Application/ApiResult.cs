using System.Net;

namespace Andux.Admin.Application
{
    /// <summary>
    /// 通用API返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T>
    {
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 状态码
        /// </summary>
        public int Code { get; set; } = (int)HttpStatusCode.InternalServerError;

        /// <summary>
        /// 数据集
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 接口执行成功对象
        /// </summary>
        /// <param name="resultData"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult<T> Success(T? resultData, string message = "操作成功~")
        {
            return new ApiResult<T>
            {
                Code = (int)HttpStatusCode.OK,
                Data = resultData,
                Message = message
            };
        }

        /// <summary>
        /// 接口执行成功对象
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult<bool> Success(bool isSuccess = true, string message = "操作成功~")
        {
            return new ApiResult<bool>
            {
                Code = (int)HttpStatusCode.OK,
                Data = isSuccess,
                Message = message
            };
        }

        /// <summary>
        /// 接口执行失败对象
        /// </summary>
        /// <param name="resultData"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult<T> Error(T? resultData, string message = "操作失败x")
        {
            return new ApiResult<T>
            {
                Code = (int)HttpStatusCode.InternalServerError,
                Data = resultData,
                Message = message
            };
        }

        /// <summary>
        /// 接口执行失败对象
        /// </summary>
        /// <param name="resultData"></param>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult<T> Error(T? resultData, HttpStatusCode statusCode, string message = "操作失败x")
        {
            return new ApiResult<T>
            {
                Code = (int)statusCode,
                Data = resultData,
                Message = message
            };
        }
    }

    /// <summary>
    /// api验证结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiValidationResult<T> : ApiResult<T>
    {
        /// <summary>
        /// 异常集合
        /// </summary>
        public IList<string> Errors { get; set; }
    }

}
