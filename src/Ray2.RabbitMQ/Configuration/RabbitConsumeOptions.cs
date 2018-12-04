namespace Ray2.RabbitMQ.Configuration
{
    public class RabbitConsumeOptions
    {
        public string Group { get; set; }
        public string Topic { get; set; }
        public bool AutoAck { get; set; }
        public int NoticeRetriesCount { get; set; }
        public ushort OneFetchCount { get; set; } = 20;
    }
}
