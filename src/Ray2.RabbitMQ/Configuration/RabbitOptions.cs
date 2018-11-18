using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.RabbitMQ.Configuration
{
    public class RabbitOptions
    {
        /// <summary>
        /// This is the UserName of Rebbit
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// This is the Password of Rebbit
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        ///   /// <summary>
        /// This is the VirtualHost of Rebbit
        /// </summary>
        /// </summary>
        public string VirtualHost { get; set; }
        /// <summary>
        /// Data serialization type
        /// </summary>
        public string SerializationType { get; set; } = Ray2.SerializationType.JsonUTF8;
        /// <summary>
        /// This is the HostName of Rebbit
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// This is the HostName of multiple Rebbits.
        /// </summary>
        public string[] HostNames { get; set; } = new string[0];
        /// <summary>
        ///This is the Consumer Options
        /// </summary>
        public List<RabbitConsumeOptions> ConsumeOptions { get; set; } = new List<RabbitConsumeOptions>();
        public List<AmqpTcpEndpoint> EndPoints
        {
            get
            {
              
               var list = new List<AmqpTcpEndpoint>();
                foreach (var host in HostNames)
                {
                    list.Add(AmqpTcpEndpoint.Parse(host));
                }
                return list;
            }
        }
    }
}
