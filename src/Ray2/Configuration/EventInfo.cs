using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    /// <summary>
    /// Ray event information
    /// </summary>
    public class EventInfo
    {
        /// <summary>
        /// The name of the event
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Type of event
        /// </summary>
        public Type Type { get; set; }
    }
}
