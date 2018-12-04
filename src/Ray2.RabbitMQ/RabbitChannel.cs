using RabbitMQ.Client;

namespace Ray2.RabbitMQ
{
    public class RabbitChannel : IRabbitChannel
    {
        private IRabbitConnection Connection { get; set; }
        public RabbitChannel(IModel model, IRabbitConnection connection)
        {
            this.Model = model;
            this.Connection = connection;
        }
        public IModel Model { get; }

        public void Close()
        {
            this.Model.Close();
        }
        public bool IsOpen()
        {
            return this.Model.IsOpen;
        }
        public uint MessageCount(string quene)
        {
            return this.Model.MessageCount(quene);
        }
    }
}