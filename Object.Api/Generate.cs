/*
 * Author : DungNT – Navisoft.
 * Summary: Api call to Ollama to get the recommend code Generated from Ollama
 * Modification Logs:
 * DATE             AUTHOR      DESCRIPTION
 * --------------------------------------------------------
 * May 15, 2023	DungNT     Created
 */

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Object.Api
{
    public class Generate
    {
        public string Model { get; set; }
        public bool Raw { get; set; }
        [JsonPropertyName("keep_alive")]
        public int KeepAlive { get; set; }

        public Option Options { get; set; }

        public string Prompt { get; set; }

        public string Format { get; set; }
    }

    public sealed class Option
    {
        public double Temperature { get; set; } = 0.5;
        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; } = 1024;
    
        public string[] Stop { get; set; }
        [JsonPropertyName("num_ctx")]
        public int NumCtx { get; set; } = 4096;
    }

}
