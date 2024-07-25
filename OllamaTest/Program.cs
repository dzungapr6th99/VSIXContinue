using Object.Api;
using OllamaClient;
using System.Security.Permissions;
namespace OllamaTest
{
    public static class Program
    {
        public static void Main()
        {
            Client client = new Client("127.0.0.1", "11434");
            Generate gen = new Generate()
            {
                Model = "starcoder2:3b",
                Prompt = "for (int i =",
                Format = "json"
            };
            var result = client.Generate(gen).Result;
            for (int i = 0; i< result.Count; i++)
            {
                Console.WriteLine(result[i].Response);
            }
            // See https://aka.ms/new-console-template for more information
            Console.WriteLine("Done");
        }
    }
}
