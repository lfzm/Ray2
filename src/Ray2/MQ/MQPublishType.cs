using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.MQ
{
    /// <summary>
    /// MQ publishing mode
    /// </summary>
    public enum MQPublishType
    {
        /// <summary>
        /// Can know if the publish to MQ is successful
        /// </summary>
        Synchronous,
        /// <summary>
        /// After the publish, I will hand it over to Ray. I don’t know if it is successful to publish to MQ.
        /// </summary>
        Asynchronous,
        /// <summary>
        /// not publish MQ
        /// </summary>
        NotPublish
    }
}
