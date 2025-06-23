using System.Net;

namespace Andux.Core.Extensions
{
    /// <summary>
    /// 业务抛出异常信息
    /// </summary>
    public class BusinessException : System.Exception
    {
        public readonly int Code;
        public readonly string Message;
        public readonly int StatusCode = (int)HttpStatusCode.OK;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public BusinessException(string message)
            : base(message)
        {
            Message = message;
        }

        public BusinessException(string message, int code)
            : base(message)
        {
            Message = message;
            Code = code;
        }

        public BusinessException(string message, int code, int statusCode)
            : base(message)
        {
            Message = message;
            Code = code;
            StatusCode = statusCode;
        }


        public BusinessException(string message, System.Exception innerException)
            : base(message, innerException)
        {
            Message = message;
        }

        public BusinessException(string message, int code, System.Exception innerException)
            : base(message, innerException)
        {
            Message = message;
            Code = code;
        }

        public BusinessException(string message, int code, int statusCode, System.Exception innerException)
            : base(message, innerException)
        {
            Message = message;
            Code = code;
            StatusCode = statusCode;
        }

        /// <summary>
        /// 获取当前异常消息
        /// </summary>
        /// <returns></returns>
        public string GetMessage()
        {
            return Message;
        }
    }
}
