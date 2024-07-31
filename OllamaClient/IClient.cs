using Object.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OllamaClient
{
    public interface IClient
    {
        string Url { get; }
        Task<List<ResponseGenerate>> Generate(Generate request);
        Task<List<ResponseChat>> Chat(Chat request, List<Message> messages, bool isDone);
    }
}
