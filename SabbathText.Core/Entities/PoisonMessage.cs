using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core.Entities
{
    public class PoisonMessage : TableEntity
    {
        public string QueueName { get; set; }
        public string Body { get; set; }
    }
}
