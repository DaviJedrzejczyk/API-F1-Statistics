using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Responses
{
    public class Response
    {
        public Response(string message, bool hasSuccess, Exception exception)
        {
            Message = message;
            HasSuccess = hasSuccess;
            Exception = exception;
        }
        public Response()
        {

        }

        public string Message { get; set; }
        public bool HasSuccess { get; set; }
        public Exception Exception { get; set; }
    }
}
