using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ReturnModel
    {
        public bool Succeed { set; get; }
        public object SearchResult { get; set; }
        public object ReturnValue { get; set; }
        public string Message { set; get; }
    }
}
