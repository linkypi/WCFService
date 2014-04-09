using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ConsoleApplication1
{
    class Program
    {
        private static dynamic Getmodel()
        {
            dynamic model = new JObject();
            model.seller = "S001";
            model.service = "login";
            model.timestamp = 23423423;
            model.user = "user";
            model.pass = "pwd";
            model.sign = "";
            dynamic parent = new JObject();
            parent.data = model;
            return parent;
        }

        public static string GetTimeStamp(bool bflag = true)
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string ret = string.Empty;
            if (bflag)
                ret = Convert.ToInt64(ts.TotalSeconds).ToString();
            else
                ret = Convert.ToInt64(ts.TotalMilliseconds).ToString();

            return ret;
        }


        const string KEY = "K632F764133F45be";
        static void Main(string[] args)
        {
            NewMethod();
            //string filePath = @"C:\Users\Administrator\Desktop\Beyond_CY\ConsoleApplication1\bin\Debug\map.jpg";
            //FileStream fileByte = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            //BinaryReader bytes = new BinaryReader(fileByte);
            //byte[] photo = bytes.ReadBytes((int)fileByte.Length);
            //fileByte.Close();
            //bytes.Close();

            //GetMicroImage(filePath,"d:",64,64);
            //Console.ReadLine();

            //string connStr = "server=web.zsit.cc,6369;Database=cypf_web_test;Persist Security Info=True;User ID=webtest;Password=webtest_123;";
            //SqlConnection conn = new SqlConnection(connStr);

            //string cmdText = "update[商品档案] set [图片] = @Picture where id=187099";
            //SqlCommand cmd = new SqlCommand(cmdText, conn);
            //cmd.Parameters.Add("@Picture", SqlDbType.Image, photo.Length).Value = photo;

            //conn.Open();
            //cmd.ExecuteNonQuery();
            //conn.Close();
        }

        private static void NewMethod()
        {
            string url2 = "http://localhost:50923/OMSService.svc/exec";
            string url = "http://183.238.203.72/OMS/OMSService.svc/exec";
            var model = Getmodel();

            List<string> list = new List<string>() { "seller=S001", "service=login", "timestamp=23423423", "user=user", "pass=pwd" };
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

            string sign = GetMD5_32(code).ToUpper();
            //model.data.sign = sign;
            model.data.sign = sign;
            string json = model.ToString();

            // var json = "\r\n{\r\n \"data\":{\r\n\t \"seller\": \"S001\",\r\n\t \"service\": \"login\",\r\n\t \"timestamp\": 23423423,\r\n\t \"user\": \"user\",\r\n\t \"pass\": \"afsdf\",\r\n\t \"sign\": \"CE15FB9383D3A9AA4571801FBE599E7\",\r\n\t \"\u5176\u4ed6\u5b57\u6bb5\":\"value\"\r\n }\r\n}";
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url2);
            req.ContentType = "application/json";
            req.Method = "POST";
            //req.ContentType = "application/x-www-form-urlencoded";
            byte[] bs = Encoding.ASCII.GetBytes(json);
            req.ContentLength = bs.Length;

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }
            byte[] buf = new byte[1024];
            using (WebResponse wr = req.GetResponse())
            {
                //在这里对接收到的页面内容进行处理
                Stream s = wr.GetResponseStream();
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                //s.Read(buf,0,1024);
                string a = reader.ReadToEnd();//Encoding.Default.GetString(buf);
                Console.WriteLine(a.Trim());
                Console.ReadLine();
            }
        }

        #region

        private bool Validate(List<string> list, string sign)
        {
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

            return GetMD5_32(code).ToUpper().Equals(sign.ToUpper());
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

        #endregion 

        private static int OriginFileName=0;

        /// <summary>
        /// 高质量缩放图片
        /// </summary>
        /// <param name="OriginFilePath">源图的路径</param>
        /// <param name="TargetFilePath">存储缩略图的路径</param>
        /// <param name="DestWidth">缩放后图片宽度</param>
        /// <param name="DestHeight">缩放后图片高度</param>
        /// <returns>表明此次操作是否成功</returns>
        private static bool GetMicroImage(string OriginFilePath, string TargetFilePath, int DestWidth, int DestHeight)
        {
            Image OriginImage = Image.FromFile(OriginFilePath);
            System.Drawing.Imaging.ImageFormat thisFormat = OriginImage.RawFormat;
            //按比例缩放
            int sW = 0, sH = 0;
            int ImageWidth = OriginImage.Width;
            int ImageHeight = OriginImage.Height;

            if (ImageWidth > DestWidth || ImageHeight > DestHeight)
            {
                if ((ImageWidth * DestWidth) > (ImageHeight * DestHeight))
                {
                    sW = DestWidth;
                    sH = (DestHeight * ImageHeight) / ImageWidth;
                }
                else
                {
                    sH = DestHeight;
                    sW = (DestWidth * ImageWidth) / ImageHeight;
                }
            }
            else
            {
                sW = ImageWidth;
                sH = ImageHeight;
            }

            Bitmap bt = new Bitmap(DestWidth, DestHeight); //根据指定大小创建Bitmap实例
            using (Graphics g = Graphics.FromImage(bt))
            {
                g.Clear(Color.White);
                //设置画布的描绘质量
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(OriginImage, new Rectangle((DestWidth - sW) / 2, (DestHeight - sH) / 2, sW, sH));
                g.Dispose();

                //设置高质量插值法  
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                ////设置高质量,低速度呈现平滑程度  
                //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                ////清空画布并以透明背景色填充  
                //g.Clear(Color.Transparent); 
            }

            System.Drawing.Imaging.EncoderParameters EncoderParams = new System.Drawing.Imaging.EncoderParameters(); //取得内置的编码器
            long[] Quality = new long[1];
            Quality[0] = 100;
            System.Drawing.Imaging.EncoderParameter EncoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Quality);
            EncoderParams.Param[0] = EncoderParam;

            try
            {
                //获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象
                //System.Drawing.Imaging.ImageCodecInfo[] arrayICI = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
                //System.Drawing.Imaging.ImageCodecInfo jpegICI = null;
                //for (int i = 0; i < arrayICI.Length; i++)
                //{
                //    if (arrayICI[i].FormatDescription.Equals("JPEG"))
                //    {
                //        jpegICI = arrayICI[i]; //设置为JPEG编码方式
                //        break;
                //    }
                //}

                //if (jpegICI != null) //保存缩略图
                //{
                //    bt.Save(TargetFilePath + @"/" + OriginFileName.ToString() + ".jpg", jpegICI, EncoderParams);
                //}
                //else
                //{
                //    bt.Save(TargetFilePath + @"/" + OriginFileName.ToString() + ".jpg", thisFormat);
                //}
                OriginFileName++;
                bt.Save(TargetFilePath + @"/" + OriginFileName.ToString() + ".jpg",
                 System.Drawing.Imaging.ImageFormat.Jpeg   );//thisFormat
                OriginImage.Dispose();
                return true;
            }
            catch (System.Runtime.InteropServices.ExternalException e1)  //GDI+发生一般错误
            {
                //MessageBox.Show("错误信息:" + e1.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception e2)
            {
                //MessageBox.Show("错误信息:" + e2.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
