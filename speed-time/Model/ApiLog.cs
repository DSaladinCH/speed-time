using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Model
{
    public class ApiLogEntry
    {
        public string Context { get; set; }
        public string ResponseMessage { get; set; }
        public int ResponseStatus { get; set; }
        public bool IsSuccess { get => ResponseStatus >= 200 && ResponseStatus <= 299; }

        public ApiLogEntry(string context, string responseMessage, int responseStatus)
        {
            Context = context;
            ResponseMessage = responseMessage;
            ResponseStatus = responseStatus;
        }
    }
}
