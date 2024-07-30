/*
 * Author : DungNT – Navisoft.
 * Summary: Contain the api Simple chat call to Ollama
 * Modification Logs:
 * DATE             AUTHOR      DESCRIPTION
 * --------------------------------------------------------
 * May 15, 2023	DungNT     Created
 */


namespace Object.Api
{
    public class Chat
    {
        /// <summary>
        /// Name of the model
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// List of the message, maybe a paragraph
        /// </summary>
        public Message[] Messages { get; set; }

        public bool Stream { get; set; } = true;
    }

    public class Message
    {
        /// <summary>
        /// Discriminate between the chat of user or the machine
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// Content of the message in Chat
        /// </summary>
        public string Content { get; set; }
    }

 
}
