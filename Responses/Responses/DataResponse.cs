using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Responses
{
    public class DataResponse<T> : Response
    {
        public DataResponse(string message, bool hasSuccess, Exception exception, List<T> itens) : base(message, hasSuccess, exception) => Itens = itens;
        public DataResponse()
        {

        }

        public List<T> Itens { get; set; }
    }
}
