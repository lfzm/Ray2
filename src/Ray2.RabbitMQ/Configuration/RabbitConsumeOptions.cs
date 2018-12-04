namespace Ray2.RabbitMQ.Configuration
{
    public class RabbitConsumeOptions
    {
        public string Group { get; set; }
        public string Topic { get; set; }
        public bool AutoAck { get; set; }
        public int NoticeRetriesCount { get; set; }
        public int MaxQueueCount { get; set; } = 2;
        public ushort OneFetchCount { get; set; } = 20;
    }
}
