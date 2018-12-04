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
            this.Model.Dispose();
        }
        public bool IsOpen()
        {
            if (this.Model.IsClosed && this.Model.IsOpen)
            {
                return true;
            }
            else
            {
                this.Model.Dispose();
                return false;
            }
        }
        public uint MessageCount(string quene)
        {
            return this.Model.MessageCount(quene);
        }
    }
}