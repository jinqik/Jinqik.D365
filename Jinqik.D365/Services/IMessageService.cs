using System.Collections.Generic;

namespace Jinqik.D365.Services
{
    public interface IMessageService
    {
        string GetMessage(string code);
        Dictionary<string, string> GetMessages(List<string> codes);
    }
}