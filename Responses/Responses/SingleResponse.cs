using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Responses
{
    public class SingleResponse<T> : Response
    {
        public SingleResponse(string message, bool hasSuccess, Exception exception, T item) : base(message, hasSuccess, exception) => Item = item;
        public SingleResponse()
        {

        }
        public T Item { get; set; }
    }
}
