using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Model
{
    public class RetObj
    {
        public Dictionary<string, object> RetDics = new Dictionary<string, object>();
        public RetObj(string msg)
        {
            RetDics.Add("state",0);
            RetDics.Add("info", msg);
        }
        public RetObj(int state , string msg)
        {
            RetDics.Add("state", state);
            RetDics.Add("info", msg);
        }

        public RetObj()
        {
            RetDics.Add("state", 0);
            RetDics.Add("info", "");
        }
    }
}