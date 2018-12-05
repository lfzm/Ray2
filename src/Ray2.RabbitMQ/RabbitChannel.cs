using RabbitMQ.Client;

namespace Ray2.RabbitMQ
{
    public class RabbitChannel : IRabbitChannel
    {
        public IRabbitConnection Connection { get; }
        public IModel Model { get; }

        public RabbitChannel(IRabbitConnection conn)
        {
            this.Connection = conn;
            this.Model = conn.Connection.CreateModel();
        }

        public void Close()
        {
            this.Model.Close();
            this.Model.Dispose();
        }
        public bool IsOpen()
        {
            if (!this.Model.IsClosed && this.Model.IsOpen)
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
            if (this.IsOpen())
            {
                return this.Model.MessageCount(quene);
            }
            else
            {
                return 0;
            }
        }
    }
}