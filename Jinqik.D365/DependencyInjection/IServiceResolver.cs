using System;
using System.Collections.Generic;

namespace Jinqik.D365.DependencyInjection
{
    public interface IServiceResolver : IDisposable
    {
        object GetService(Type serviceType);
    }
}