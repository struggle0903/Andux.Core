namespace Andux.Core.RabbitMQ.Exceptions
{
    /// <summary>
    /// RabbitMQ 自定义异常
    /// </summary>
    public class RabbitMQException : Exception
    {
        public RabbitMQException() { }
        public RabbitMQException(string message) : base(message) { }
        public RabbitMQException(string message, Exception inner) : base(message, inner) { }
    }
}
