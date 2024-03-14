using Microsoft.DotNet.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OraculumInteractive
{
    public static class OraculumKernelExtension
    {
        public static void LoadOraculum(Microsoft.DotNet.Interactive.Kernel kernel)
        {
            if (kernel is CompositeKernel compositeKernel)
            {
                compositeKernel.AddKernelConnector(new ConnectOraculumCommand());
                KernelInvocationContext.Current?.Display("Oraculum kernel loaded", new[]{ "text/plain" });
            }
        }
    }
}
