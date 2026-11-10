using System;
using System.Collections.Generic;
using System.Text;

namespace MiniConsole.Configuration
{
    public class AppSettings
    {
        public bool RemoveLogTags { get; set; } = true;
        public int PollIntervalMs { get; set; } = 5;
        public int ReadBufferSize { get; set; } = 65536;
        public string[] TargetDataFolders { get; set; } =
        {
            "miniworddata410",
            "miniworddata402",
            "miniworddata110"
        };
    }
}
