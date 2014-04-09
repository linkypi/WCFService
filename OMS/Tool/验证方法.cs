using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using Model;
using JsonFx.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;

namespace OMS.Tool
{
    public class Common
    {                    
        const string KEY = "K632F764133F45be";

        public static bool Validate(ReqModel data)
        {

            List<string> list = new List<string>();
            Type t = typeof(ReqModel);
            List<string> args = new List<string>();
            foreach (PropertyInfo pi in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                object obj = pi.GetValue(data, null);
                if (obj == null) { continue; }
                if (pi.Name == "sign") { continue; }
                list.Add(pi.Name + "=" + obj.ToString());
               
            }
            list.Sort();

            string code = "";
            int index = 1;
            foreach (var item in list)
            {
                code += item;
                if (index < list.Count)
                {
                    code += "&";
                }
                index++;
            }
            code += KEY;

            return GetMD5_32(code).ToUpper().Equals(data.sign.ToUpper());
        }


        /// <summary>
        /// MD5　32位加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetMD5_32(string key)
        {
            string temp = "";
            MD5 md5 = MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，
                //如果使用大写（X）则格式后的字符是大写字符
                temp = temp + s[i].ToString("X2");
            }
            return temp;
        }

    }

        public class ReqModel
    {
        public string seller { get; set; }
        public string service { set; get; }
        public int? timestamp { get; set; }
        public string user { set; get; }
        public string pass { get; set; }
        public string sign { set; get; }
        public string callback { set; get; }
        public int?    filter_type{ set; get; }
        public string keyword{get;set;}
        public int? page_no{ set; get; }
        public int? page_size{ set; get; }
        public int? type_id{ set; get; }
        public int? sort_type{ set; get; }
        public int? in_stock { set; get; }
        public int? goods_id { set; get; }
        public string goods_ids { set; get; }
        public int? member_id { set; get; }
        public string stime { set; get; }
        public string etime { set; get; }
        public string order_note { get; set; }
        public string goods_data { set; get; }
        public int? order_id { set; get; }
    }
}