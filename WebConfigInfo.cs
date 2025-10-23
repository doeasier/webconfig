using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Configuration.WebConfig
{
    /// <summary>
    /// 记录web config的相关信息
    /// </summary>
    public class WebConfigInfo
    {
        public string Url { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsExists { get; set; } = false;

        public WebConfigInfo() 
        { 
            Url = string.Empty;
            LastModified = DateTime.MinValue;
        }

        public WebConfigInfo(string url)
        {
            Url = url;
            LastModified = DateTime.MinValue;
        }

        public WebConfigInfo(string url, DateTime lastModified)
        {
            Url = url;
            LastModified = lastModified;
        }
    }
}
