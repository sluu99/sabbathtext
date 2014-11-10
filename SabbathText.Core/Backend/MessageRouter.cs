using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend
{
    public class MessageRouter
    {
        private Dictionary<Regex, Type> processors;
        private MessageQueue outboundQueue;

        public MessageRouter()
        {
            this.processors = new Dictionary<Regex, Type>();
            this.outboundQueue = new MessageQueue(MessageQueue.OutboundMessageQueue);
        }
        
        public MessageRouter AddProcessor<T>(Regex regex) where T : IProcessor
        {
            if (regex == null)
            {
                throw new ArgumentNullException("regex");
            }

            this.processors.Add(regex, typeof(T));

            return this;
        }

        public MessageRouter AddProcessor<T>(string verb) where T : IProcessor
        {
            if (string.IsNullOrWhiteSpace(verb))
            {
                throw new ArgumentException("verb", "verb cannot be null or white space");
            }

            return this.AddProcessor<T>(new Regex(string.Format("^{0}$", verb.Trim().ToLowerInvariant()), RegexOptions.IgnoreCase));
        }

        public async Task<bool> Route(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (string.IsNullOrWhiteSpace(message.Body))
            {
                throw new ApplicationException("Cannot route messages with empty body");
            }

            string body = message.Body.ExtractAlphaNumericSpace().Trim();

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ApplicationException("The message body does not contain any alpha numeric characters");
            }
            
            IProcessor processor = null;

            foreach (KeyValuePair<Regex, Type> kv in this.processors)
            {
                if (kv.Key.IsMatch(body))
                {
                    processor = Activator.CreateInstance(kv.Value) as IProcessor;
                    break;
                }
            }

            if (processor == null)
            {
                Trace.TraceWarning("Cannot find processor for message {0}", message.MessageId);
                return false;
            }

            Trace.TraceInformation("Processor: {0}", processor.GetType().Name);

            Message response = await processor.ProcessMessage(message);

            if (response != null)
            {
                await this.outboundQueue.AddMessage(response);
            }

            return true;
        }
    }
}
