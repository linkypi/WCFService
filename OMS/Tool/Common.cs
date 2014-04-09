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
using System.Drawing;
using Newtonsoft.Json.Linq;

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
                //args.Add(pi.Name);
            }

           // args.Sort();
            //foreach (PropertyInfo pi in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            //{
            //    object obj = pi.GetValue(data, null);
            //    if (obj == null) { continue; }
            //    if (pi.Name == "sign") { continue; }
            //    //list.Add(pi.Name + "=" + obj.ToString());
            //    args.Add(pi.Name);
            //}
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

        public static Stream ReturnValue(RetObj ret,DateTime recdate)
        {
            try
            {
                //formater.WriteObject(ms, new Person() { Age = 23, Name = "Bob", Gender = "male" });
                //ms.Position = 0;
                //StreamReader sr = new StreamReader(ms);
                //string objContent = sr.ReadToEnd();
                //// string returnStr = callback + "(" + objContent + ")";
                //sr.Close();
                //ms = new MemoryStream();
                //StreamWriter sw = new StreamWriter(ms);
                //sw.AutoFlush = true;
                //sw.Write(objContent);
                //ms.Position = 0;
                //WebOperationContext.Current.OutgoingResponse.ContentType = "application/json"; // "text/plain";
                //WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                DateTime end = DateTime.Now;
                TimeSpan hao = end - recdate;
                ret.RetDics.Add("服务器处理时长", hao.TotalMilliseconds);
                ret.RetDics.Add("服务器接收时间", recdate.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                ret.RetDics.Add("服务器返回时间", end.ToString("yyyy-MM-dd HH:mm:ss:fff"));
                JsonFx.Json.JsonWriter writer = new JsonFx.Json.JsonWriter();
                var json = writer.Write(ret.RetDics);
                //if (!string.IsNullOrEmpty(callback))
                //{
                //    json = string.Format("{0}({1})", callback, json);
                //}
                //else if (!string.IsNullOrEmpty("&"+callback))
                //{
                //    json = string.Format("{0}({1})", callback, json);
                //}
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.AutoFlush = true;
                sw.Write(json);
                ms.Position = 0;
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/json"; // "text/plain";
                //WebOperationContext.Current.OutgoingResponse.Headers.Add("charset","utf-8");
                //WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                return ms;
            }
            catch(Exception ex)
            {
                RetObj ret2 = new RetObj(ex.Message);
                JsonFx.Json.JsonWriter writer = new JsonFx.Json.JsonWriter();
                var json = writer.Write(ret2.RetDics);
                //if (!string.IsNullOrEmpty(callback))
                //{
                //    json = string.Format("{0}({1})", callback, json);
                //}
                //else if (!string.IsNullOrEmpty("&"+callback))
                //{
                //    json = string.Format("{0}({1})", callback, json);
                //}
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.AutoFlush = true;
                sw.Write(json);
                ms.Position = 0;
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/json"; // "text/plain";
                //WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                return ms;
            }
        }

        public static Stream ReturnValue(RetObj ret)
        {
            try
            {
                //formater.WriteObject(ms, new Person() { Age = 23, Name = "Bob", Gender = "male" });
                //ms.Position = 0;
                //StreamReader sr = new StreamReader(ms);
                //string objContent = sr.ReadToEnd();
                //// string returnStr = callback + "(" + objContent + ")";
                //sr.Close();
                //ms = new MemoryStream();
                //StreamWriter sw = new StreamWriter(ms);
                //sw.AutoFlush = true;
                //sw.Write(objContent);
                //ms.Position = 0;
                //WebOperationContext.Current.OutgoingResponse.ContentType = "application/json"; // "text/plain";
                //WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                 
                JsonFx.Json.JsonWriter writer = new JsonFx.Json.JsonWriter();
                var json = writer.Write(ret.RetDics);
                //if (!string.IsNullOrEmpty(callback))
                //{
                //    json = string.Format("{0}({1})", callback, json);
                //}
                //else if (!string.IsNullOrEmpty("&"+callback))
                //{
                //    json = string.Format("{0}({1})", callback, json);
                //}
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.AutoFlush = true;
                sw.Write(json);
                ms.Position = 0;
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/json"; // "text/plain";
                //WebOperationContext.Current.OutgoingResponse.Headers.Add("charset","utf-8");
                //WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                return ms;
            }
            catch(Exception ex)
            {
                RetObj ret2 = new RetObj(ex.Message);
                JsonFx.Json.JsonWriter writer = new JsonFx.Json.JsonWriter();
                var json = writer.Write(ret2.RetDics);
                //if (!string.IsNullOrEmpty(callback))
                //{
                //    json = string.Format("{0}({1})", callback, json);
                //}
                //else if (!string.IsNullOrEmpty("&"+callback))
                //{
                //    json = string.Format("{0}({1})", callback, json);
                //}
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.AutoFlush = true;
                sw.Write(json);
                ms.Position = 0;
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/json"; // "text/plain";
                //WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                return ms;
            }
        }

        public static Stream ReturnValue(HttpReturn ret)
        {
            //formater.WriteObject(ms, new Person() { Age = 23, Name = "Bob", Gender = "male" });
            //ms.Position = 0;
            //StreamReader sr = new StreamReader(ms);
            //string objContent = sr.ReadToEnd();
            //// string returnStr = callback + "(" + objContent + ")";
            //sr.Close();
            //ms = new MemoryStream();
            //StreamWriter sw = new StreamWriter(ms);
            //sw.AutoFlush = true;
            //sw.Write(objContent);
            //ms.Position = 0;
            //WebOperationContext.Current.OutgoingResponse.ContentType = "application/json"; // "text/plain";
            //WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            JsonFx.Json.JsonWriter writer = new JsonFx.Json.JsonWriter();
            var json = writer.Write(ret);
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.AutoFlush = true;
            sw.Write(json);
            ms.Position = 0;
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/json";
            return ms;
        }

        public static string ReturnValue(JObject ret)
        {
           // MemoryStream ms = new MemoryStream();
           // System.Runtime.Serialization.Json.DataContractJsonSerializer
           //formater = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(JObject));

           // formater.WriteObject(ms, ret);
           // ms.Position = 0;
           // StreamReader sr = new StreamReader(ms);
           // string objContent = sr.ReadToEnd();
           // // string returnStr = callback + "(" + objContent + ")";
           // sr.Close();
           // ms = new MemoryStream();
           // StreamWriter sw = new StreamWriter(ms);
           // sw.AutoFlush = true;
           // sw.Write(objContent);
           // ms.Position = 0;
           // WebOperationContext.Current.OutgoingResponse.ContentType = "application/json"; // "text/plain";
            //WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            JsonFx.Json.JsonWriter writer = new JsonFx.Json.JsonWriter();
            var json = writer.Write(ret.ToString());
            //MemoryStream ms = new MemoryStream();
            //StreamWriter sw = new StreamWriter(ms);
            //sw.AutoFlush = true;
            //sw.Write(json);
            //ms.Position = 0;
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/json";
            //StringWriter sw = new StringWriter();
           // JsonWriter writer = new JsonWriter();
            //JavaScriptConvert.SerializeObject(目标对象);
            //writer.WriteStartArray();
            //writer.WriteValue("JSON!");
            //writer.WriteValue(1);
            //writer.WriteValue(true);
            //writer.WriteStartObject();
            //writer.WritePropertyName("property");
            //writer.WriteValue("value");
            //writer.WriteEndObject();
            //writer.WriteEndArray();

            //writer.Flush();
            return ret.ToString();
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
}