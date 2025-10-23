using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Configuration
{
    public class WebLoadExceptionContext
    {
        //
        // 摘要:
        //     Gets or sets the Microsoft.Extensions.Configuration.WebConfigurationProvider
        //     that caused the exception.
        public WebConfigurationProvider? Provider { get; set; }

        //
        // 摘要:
        //     Gets or sets the exception that occurred in Load.
        public Exception Exception { get; set; } = default!;

        //
        // 摘要:
        //     Gets or sets a value that indicates whether the exception is rethrown.
        //
        // 值:
        //     true if the exception isn't rethrown; otherwise, false.
        public bool Ignore { get; set; }
    }
}
