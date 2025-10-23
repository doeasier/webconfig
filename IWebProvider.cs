using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Configuration.WebConfig
{
    public interface IWebProvider
    {
        //
        // 摘要:
        //     Locate a web config info.
        //
        // 返回结果:
        //     The web information. Caller must check Exists property.
        WebConfigInfo GetConfingInfo();
        //
        // 摘要:
        //     Creates a Microsoft.Extensions.Primitives.IChangeToken for the specified web.
        //
        //
        // 返回结果:
        //     An Microsoft.Extensions.Primitives.IChangeToken that is notified when a web config
        //     is added, modified or deleted.
        IChangeToken Watch(int reloadDelay, string? accessToken);
    }
}
