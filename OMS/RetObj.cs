using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OMS
{
    public class RetObj
    {
        public Dictionary<string, object> RetDics = new Dictionary<string, object>();
        public RetObj()
        {
            RetDics.Add("state",false);
            RetDics.Add("info", "");
        }
    }
}