using CommonLib;
using Object.Api;
using Object;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.IO;
namespace OllamaClient
{
    public class Client : IClient
    {
        private string _ip { get; set; }
        private string _port { get; set; }
        private string _url { get; set; }
        public Client(string ip, string port)
        {
            _ip = ip;
            _port = port;
            _url = "http://localhost:11434";
        }
        public string Url
        {
            get
            {
                return _url;
            }
        }
        private readonly JsonSerializerSettings _jsonCamelCaseSetting = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Include
        };
        public async Task<List<ResponseGenerate>> Generate(Generate request)
        {
            try
            {
                List<ResponseGenerate> responses = new List<ResponseGenerate>();
                HttpClient client = new HttpClient();
                string Content = JsonConvert.SerializeObject(request, _jsonCamelCaseSetting);
                StringContent packageContent = new StringContent(Content, Encoding.UTF8, "application/json");

                string ResponseGenerate = string.Empty;
                var httpWebRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate");
                httpWebRequest.Content = packageContent;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                HttpResponseMessage response = client.SendAsync(httpWebRequest).Result;
                using (var stream = response.Content.ReadAsStreamAsync().Result)
                {
                    if (response.IsSuccessStatusCode)
                    {

                        using (var reader = new StreamReader(stream))
                        {

                            while (!reader.EndOfStream)
                            {
                                var line = await reader.ReadLineAsync();
                                ResponseGenerate responseGen = JsonConvert.DeserializeObject<ResponseGenerate>(line);
                                if (string.IsNullOrEmpty(responseGen.Error))
                                {
                                    responses.Add(responseGen);
                                }
                                else
                                {
                                    throw new Exception(responseGen.Error);
                                }
                            }
                            return responses;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<List<ResponseChat>> Chat(Chat request, List<Message> messages, bool isDone)
        {
            List<ResponseChat> responseChats = new List<ResponseChat>();
            HttpClient client = new HttpClient();
            string Content = JsonConvert.SerializeObject(request);
            StringContent packageContent = new StringContent(JsonConvert.SerializeObject(request, _jsonCamelCaseSetting), Encoding.UTF8, "application/json");

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            HttpResponseMessage response = await client.PostAsync(Url, packageContent);
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        ResponseChat responseChat = JsonConvert.DeserializeObject<ResponseChat>(line);
                        responseChats.Add(responseChat);
                        if (string.IsNullOrEmpty(responseChat.Error))
                        {
                            isDone = responseChat.Done;
                            messages.Add(responseChat.Message);
                            if (responseChat.Done)
                            {
                                break;
                            }
                        }
                        else
                        {
                            throw new Exception(responseChat.Error);
                        }
                    }
                    return responseChats;
                }
            }
        }
    }
}
