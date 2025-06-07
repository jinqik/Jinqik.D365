using System.Collections.Generic;
using System.Linq;

namespace Jinqik.D365.Services
{
    public class NullMessageService : IMessageService
    {
        public string GetMessage(string code)
        {
            return code;
        }

        public Dictionary<string, string> GetMessages(List<string> codes)
        {
            return codes.ToDictionary(x => x, GetMessage);
        }
    }
}