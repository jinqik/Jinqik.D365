using System;

namespace Jinqik.D365.DependencyInjection
{
    public class ServiceDescriptor
    {
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public Func<IServiceResolver, object> Factory { get; set; }
        public object Instance { get; set; }
    }
}