using Object.Api;
using OllamaClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OllamNetFrame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client("127.0.0.1", "11434");
            Generate gen = new Generate()
            {
                Model = "starcoder2:3b",
                Prompt = "for (int i =",
                Format = "json"
            };
            var result = client.Generate(gen).Result;
            string resultString = "";
            for (int i = 0; i < result.Count; i++)
            {
                resultString += result[i].Response.ToString();

            }
            Console.Write(resultString);
            // See https://aka.ms/new-console-template for more information
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
