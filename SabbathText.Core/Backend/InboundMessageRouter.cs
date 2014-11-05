﻿using SabbathText.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend
{
    public class InboundMessageRouter
    {
        private Dictionary<string, Type> processors;
        private MessageQueue outboundQueue;

        public InboundMessageRouter()
        {
            this.processors = new Dictionary<string, Type>();
            this.outboundQueue = new MessageQueue(MessageQueue.OutboundMessageQueue);
        }

        public InboundMessageRouter AddProcessor<T>(string verb) where T : IProcessor
        {
            if (string.IsNullOrWhiteSpace(verb))
            {
                throw new ArgumentException("verb", "verb cannot be null or white space");
            }

            verb = verb.Trim().ToLowerInvariant();

            this.processors.Add(verb, typeof(T));

            return this;
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

            string body = message.Body.Trim();            
            string verb = string.Empty;
            
            int whiteSpaceIndex = body.IndexOf(' ');
            if (whiteSpaceIndex == -1)
            {
                verb = body;
            }
            else
            {
                verb = body.Substring(0, whiteSpaceIndex);
            }

            verb = verb.ToLowerInvariant();

            if (!this.processors.ContainsKey(verb))
            {
                return false;
            }

            Type processorType = this.processors[verb];
            IProcessor processor = Activator.CreateInstance(processorType) as IProcessor;

            if (processor == null)
            {
                return false;
            }

            Message response = await processor.ProcessMessage(message);

            if (response != null)
            {
                await this.outboundQueue.AddMessage(response);
            }

            return true;
        }
    }
}