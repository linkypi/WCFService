using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class HttpReturn
    {
        public HttpReturn()
        { }

        public HttpReturn(bool st,string msg)
        {
            this.info = msg;
            this.state = st;
        }

        public bool state { set; get; }
        public int m_id { get; set; }
        public string info { set; get; }
        public string m_name { set; get; }
        public string m_num { set; get; }
    }
}
