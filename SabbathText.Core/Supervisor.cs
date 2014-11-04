using Microsoft.WindowsAzure.Storage.Queue;
using SabbathText.Core.Entities;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SabbathText.Core
{
    public class Supervisor
    {
        private int delayIndex;
        private volatile bool stopRequested;

        public Supervisor()
        {
            this.DelayIntervals = new int[]{ 500, 1000, 1000, 3000, 5000 };
            this.delayIndex = 0;
            this.MessageQueue = new MessageQueue();
        }

        public int[] DelayIntervals { get; set; }
        public MessageQueue MessageQueue { get; set; }
        
        public async Task Start(Func<Message, Task<bool>> process)
        {
            while (!this.stopRequested)
            {
                Tuple<CloudQueueMessage, Message> message = await this.MessageQueue.GetInboundMessage();
                
                if (message == null)
                {
                    this.Chillax();
                    continue;
                }
                else
                {
                    this.delayIndex = 0;
                }

                try
                {
                    bool success = await process(message.Item2);

                    if (success)
                    {
                        await this.MessageQueue.DeleteInboundMessage(message.Item1);
                    }
                    else
                    {
                        Trace.TraceWarning("Failed to process message {0}", message.Item1.Id);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);         
                }
            }
        }

        public void RequestStop()
        {
            this.stopRequested = true;
        }

        private void Chillax()
        {
            int delay = 0;
            if (this.delayIndex >= this.DelayIntervals.Length)
            {
                delay = this.DelayIntervals[this.DelayIntervals.Length - 1];
            }
            else
            {
                delay = this.DelayIntervals[this.delayIndex];
                delayIndex++;
            }

            Trace.TraceInformation("Chilling for {0}ms", delay);
            Thread.Sleep(delay);
        }
    }
}
