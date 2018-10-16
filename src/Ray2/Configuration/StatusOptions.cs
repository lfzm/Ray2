using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration
{
    public class StatusOptions : StorageOptions
    {

        public StatusMode StatusMode { get; set; } = StatusMode.Asynchronous;
    }

    public enum StatusMode
    {
        Synchronous,
        Asynchronous
    }
}
