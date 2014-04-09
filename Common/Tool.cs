using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;


namespace Common
{
    public class Tool
    {  
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

        /// <summary>
        /// 过滤SQL语句,防止注入
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns>0 - 没有注入, 1 - 有注入 </returns>
        public static int filterSql(string sSql)
        {
            int srcLen, decLen = 0;
            sSql = sSql.ToLower().Trim();
            srcLen = sSql.Length;
            sSql = sSql.Replace("exec", "");
            sSql = sSql.Replace("delete", "");
            sSql = sSql.Replace("master", "");
            sSql = sSql.Replace("truncate", "");
            sSql = sSql.Replace("declare", "");
            sSql = sSql.Replace("create", "");
            sSql = sSql.Replace("xp_", "no");
            decLen = sSql.Length;
            // or ' - _ " :
            if (srcLen == decLen) return 0; else return 1;
        }

        //图片 转为    base64编码的文本  
        public static string ImgToBase64String(string Imagefilename)
        {
            try
            {
                Bitmap bmp = new Bitmap(Imagefilename);
                
                FileStream fs = new FileStream(Imagefilename + ".txt", FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);

                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                String strbaser64 = Convert.ToBase64String(arr);
                sw.Write(strbaser64);

                sw.Close();
                fs.Close();
                return strbaser64;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        /// <summary>
        /// 高质量缩放图片
        /// </summary>
        /// <param name="OriginFilePath">源图的路径</param>
        /// <param name="DestWidth">缩放后图片宽度</param>
        /// <param name="DestHeight">缩放后图片高度</param>
        public static byte[] GetMicroImage(byte[] arr, int DestWidth, int DestHeight)
        {
            if (arr == null) return new byte[] { };
            MemoryStream stream = new MemoryStream(arr);
            //    returnnew Bitmap((Image)new Bitmap(stream)); 
            Image OriginImage = Image.FromStream(stream);
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

            Bitmap bmp = new Bitmap(DestWidth, DestHeight); //根据指定大小创建Bitmap实例
            using (Graphics g = Graphics.FromImage(bmp))
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

            OriginImage.Dispose();

            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            //byte[] byteImage = new Byte[ms.Length]; 
            //byteImage = ms.ToArray(); 
            return ms.ToArray();

        }


        /// <summary>
        /// 高质量缩放图片
        /// </summary>
        /// <param name="OriginFilePath">源图的路径</param>
        /// <param name="DestWidth">缩放后图片宽度</param>
        /// <param name="DestHeight">缩放后图片高度</param>
        private static byte[] GetMicroImage(string OriginFilePath, int DestWidth, int DestHeight)
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

            Bitmap bmp = new Bitmap(DestWidth, DestHeight); //根据指定大小创建Bitmap实例
            using (Graphics g = Graphics.FromImage(bmp))
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

            OriginImage.Dispose();

            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            //byte[] byteImage = new Byte[ms.Length]; 
            //byteImage = ms.ToArray(); 
            return ms.ToArray();

        }

    }
}