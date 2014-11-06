using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabbathText.Core.Entities
{
    public class EventMessage : TableEntity
    {
        public EventType Event { get; set; }
    }
}
