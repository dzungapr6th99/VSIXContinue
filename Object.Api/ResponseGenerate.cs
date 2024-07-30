/*
 * Author : DungNT – Navisoft.
 * Summary: Api Response get from Ollama
 * Modification Logs:
 * DATE             AUTHOR      DESCRIPTION
 * --------------------------------------------------------
 * May 15, 2023	DungNT     Created
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Object.Api
{
    public class ResponseGenerate
    {
        public string Model { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreateAt { get; set; }
        public string Response { get; set; }
        public bool Done { get; set; }

        public string Error { get; set; }   
    }
}
