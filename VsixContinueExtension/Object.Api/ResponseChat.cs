using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Object.Api
{
    public class ResponseChat
    {
        public string Error { get; set; }   
        public Message Message { get; set; }
        public bool Done {  get; set; } = false;    
    }
}
