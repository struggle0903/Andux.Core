using RabbitMQ.Client;

namespace Andux.Core.RabbitMQ.Interfaces
{
    /// <summary>
    /// RabbitMQ 消息消费接口
    /// </summary>
    public interface IRabbitMQConsumer
    {
        /// <summary>
        /// 开始消费队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="handler"></param>
        /// <param name="autoAck"></param>
        void StartConsuming<T>(string queueName, Func<T, Task> handler, bool autoAck = false) where T : class;

        /// <summary>
        /// 订阅 Topic Exchange 消息（随机队列名）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">Topic 交换机名称</param>
        /// <param name="routingKey">路由键，可使用 * 或 # 通配符</param>
        /// <param name="handler">消息处理方法</param>
        /// <param name="autoAck">是否自动确认</param>
        void StartConsumingTopic<T>(string exchangeName, string routingKey, Func<T, Task> handler, bool autoAck = false) where T : class;

        /// <summary>
        /// 订阅 Topic Exchange 消息（自定义队列名）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">Topic 交换机名称</param>
        /// <param name="routingKey">路由键，可使用 * 或 # 通配符</param>
        /// <param name="queueName">用户订阅队列名</param>
        /// <param name="handler">消息处理方法</param>
        /// <param name="autoAck">是否自动确认</param>
        void StartConsumingTopic<T>(string exchangeName, string routingKey, string queueName, Func<T, Task> handler, bool autoAck = false) where T : class;

        /// <summary>
        /// 开始消费 Exchange 消息（支持 Topic 或 Fanout）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">Exchange 名称</param>
        /// <param name="queueName">可选：固定队列名。如果为 null 或空，则创建临时随机队列，用于广播模式</param>
        /// <param name="routingKey">路由键，可使用 * 或 # 通配符，Fanout 可为空</param>
        /// <param name="handler">消息处理方法</param>
        /// <param name="autoAck">是否自动确认</param>
        void StartConsumingExchange<T>(
            string exchangeName,
            string? queueName,
            string routingKey,
            Func<T, Task> handler,
            bool autoAck = false) where T : class;

        /// <summary>
        /// 开始消费广播消息（Fanout 交换机专用）
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="exchangeName">Fanout 交换机名称</param>
        /// <param name="queueName">服务标识，用于生成队列名（如服务名）</param>
        /// <param name="handler">消息处理方法</param>
        /// <param name="autoAck">是否自动确认</param>
        /// <param name="isExclusive">是否排他队列（建议true，广播通常是临时消费）</param>
        /// <remarks>
        /// 此方法专为 Fanout 广播设计：
        /// 1. 每个消费者会创建唯一的临时队列
        /// 2. 自动绑定到指定 Fanout 交换机
        /// 3. routingKey 被忽略（传递空字符串）
        /// </remarks>
        void StartConsumingBroadcast<T>(string exchangeName, string queueName,
            Func<T, Task> handler, bool autoAck = false, bool isExclusive = true) where T : class;

        #region 根据IModel订阅消息

        /// <summary>
        /// 开始消费队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName">队列名</param>
        /// <param name="handler">处理函数</param>
        /// <param name="channel">通道</param>
        /// <param name="autoAck">是否自动确认</param>
        void StartConsuming<T>(IModel channel, string queueName, Func<T, Task> handler, bool autoAck = false)
            where T : class;

        /// <summary>
        /// 订阅 Topic Exchange 消息（随机队列名）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">Topic 交换机名称</param>
        /// <param name="routingKey">路由键，可使用 * 或 # 通配符</param>
        /// <param name="handler">消息处理方法</param>
        /// <param name="channel">消息通道</param>
        /// <param name="autoAck">是否自动确认</param>
        void StartConsumingTopic<T>(IModel channel, string exchangeName, string routingKey, Func<T, Task> handler,
            bool autoAck = false) where T : class;

        /// <summary>
        /// 订阅 Topic Exchange 消息（自定义队列名）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">Topic 交换机名称</param>
        /// <param name="routingKey">路由键，可使用 * 或 # 通配符</param>
        /// <param name="queueName">用户订阅队列名</param>
        /// <param name="handler">消息处理方法</param>
        /// <param name="channel">消息通道</param>
        /// <param name="autoAck">是否自动确认</param>
        void StartConsumingTopic<T>(IModel channel, string exchangeName, string routingKey, string queueName, Func<T, Task> handler,
            bool autoAck = false) where T : class;

        /// <summary>
        /// 开始消费 Exchange 消息（支持 Topic 或 Fanout）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">Exchange 名称</param>
        /// <param name="queueName">可选：固定队列名。如果为 null 或空，则创建临时随机队列，用于广播模式</param>
        /// <param name="routingKey">路由键，可使用 * 或 # 通配符，Fanout 可为空</param>
        /// <param name="handler">消息处理方法</param>
        /// <param name="channel">消息通道</param>
        /// <param name="autoAck">是否自动确认</param>
        void StartConsumingExchange<T>(IModel channel, string exchangeName, string? queueName, string routingKey,
            Func<T, Task> handler, bool autoAck = false) where T : class;

        /// <summary>
        /// 开始消费广播消息（Fanout 交换机专用）
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="channel">消息通道</param>
        /// <param name="exchangeName">Fanout 交换机名称</param>
        /// <param name="queueName">服务标识，用于生成队列名（如服务名）</param>
        /// <param name="handler">消息处理方法</param>
        /// <param name="autoAck">是否自动确认</param>
        /// <param name="isExclusive">是否排他队列（建议true，广播通常是临时消费）</param>
        /// <remarks>
        /// 此方法专为 Fanout 广播设计：
        /// 1. 每个消费者会创建唯一的临时队列
        /// 2. 自动绑定到指定 Fanout 交换机
        /// 3. routingKey 被忽略（传递空字符串）
        /// </remarks>
        void StartConsumingBroadcast<T>(IModel channel, string exchangeName, string queueName,
            Func<T, Task> handler, bool autoAck = false, bool isExclusive = true) where T : class;

        #endregion

        /// <summary>
        /// 停止所有消费者
        /// </summary>
        void StopConsuming();

        /// <summary>
        /// 停止指定队列的消费者
        /// </summary>
        /// <param name="queueName"></param>
        void StopConsuming(string queueName);
    }
}
