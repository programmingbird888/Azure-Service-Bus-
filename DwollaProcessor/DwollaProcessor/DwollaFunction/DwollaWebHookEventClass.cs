using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwollaProcessor.DwollaFunction
{
    public class DwollaWebHookEventClass
    {
        public string EventType { get; set; }
        public object Payload { get; set; }
    }
}
