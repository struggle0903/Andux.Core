namespace Andux.Core.EventBus.Attributes
{
    /// <summary>
    /// 事件队列属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class EventQueueAttribute : Attribute
    {
        public string Name { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">队列名</param>
        public EventQueueAttribute(string name)
        {
            Name = name;
        }

    }
}
