using Microsoft.WindowsAzure.Storage.Queue;
using SabbathText.Core.Entities;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend
{
    public class Supervisor
    {
        private int delayIndex;
        private volatile bool stopRequested;        
        private int poisonMessageThreshold;
        private string queueName;
        private int lastLogMinute;

        public Supervisor(string queueName)
        {
            this.DelayIntervals = new int[]{ 500, 1000, 1000, 3000 };
            this.delayIndex = 0;
            this.MessageQueue = new MessageQueue(queueName);
            this.PoisonMessageThreshold = 10;
            this.DataProvider = new AzureDataProvider();
            this.queueName = queueName;
            this.lastLogMinute = -1;
        }

        public int[] DelayIntervals { get; set; }
        public MessageQueue MessageQueue { get; set; }
        public IDataProvider DataProvider { get; set; }

        public int PoisonMessageThreshold
        {
            get { return this.poisonMessageThreshold; }
            set
            {
                if (value <= 1)
                {
                    throw new ArgumentException("PoisonMessageThreshold must be > 1"); ;
                }

                this.poisonMessageThreshold = value;
            }
        }
        
        public async Task Start(Func<Message, Task<bool>> process)
        {
            while (!this.stopRequested)
            {
                DateTime now = Clock.UtcNow;

                if (now.Minute % 5 == 0)
                {
                    if (this.lastLogMinute == -1 || now.Minute > this.lastLogMinute || (now.Minute == 0 && this.lastLogMinute == 55))
                    {
                        this.lastLogMinute = now.Minute;
                        Trace.TraceInformation("I'm alive! It is now {0}", now);
                    }                    
                }

                Tuple<CloudQueueMessage, Message> message = await this.MessageQueue.GetMessage();
                
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
                    if (message.Item1.DequeueCount > this.PoisonMessageThreshold)
                    {
                        Trace.TraceWarning("Poison message detected, ID = {0}", message.Item1.Id);
                        await this.DataProvider.RecordPoisonMessage(this.queueName, message.Item1.AsString);
                        await this.MessageQueue.DeleteMessage(message.Item1);
                    }
                    else
                    {
                        bool success = await process(message.Item2);

                        if (success)
                        {
                            Trace.TraceInformation("Message {0} processed", message.Item1.Id);
                            await this.MessageQueue.DeleteMessage(message.Item1);
                        }
                        else
                        {
                            Trace.TraceWarning("Failed to process message {0}", message.Item1.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    Trace.TraceError(ex.StackTrace);
                }
            }

            Trace.TraceInformation("Finished gracefully");
        }

        public void RequestStop()
        {
            this.stopRequested = true;
            Trace.TraceInformation("Stop requested");
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
                this.delayIndex++;
            }

            Thread.Sleep(delay);
        }
    }
}
