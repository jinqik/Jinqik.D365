using System;
using Microsoft.Xrm.Sdk;

namespace Jinqik.D365
{
    public class BasePlugin : IPlugin
    {
        private string _secureSetting;
        private string _unsecureSetting;


        protected BasePlugin(string secureSetting, string unsecureSetting)
        {
            _secureSetting = secureSetting;
            _unsecureSetting = unsecureSetting;
        }


        public void Execute(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}