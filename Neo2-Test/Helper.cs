using Neo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Neo2_Test
{
    public static class Helper
    {
        public static string HttpGet(string url)
        {
            WebClient wc = new WebClient();
            return wc.DownloadString(url);
        }

        public static string HttpPost(string url, string data)
        {
            HttpWebRequest req = null;
            HttpWebResponse rsp = null;
            Stream reqStream = null;
            req = WebRequest.CreateHttp(new Uri(url));
            req.ContentType = "application/json;charset=utf-8";

            req.Method = "POST";
            //req.Accept = "text/xml,text/javascript";
            req.ContinueTimeout = 10000;

            byte[] postData = Encoding.UTF8.GetBytes(data);
            reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            //reqStream.Dispose();

            rsp = (HttpWebResponse)req.GetResponse();
            string result = GetResponseAsString(rsp);

            return result;
        }

        private static string GetResponseAsString(HttpWebResponse rsp)
        {
            Stream stream = null;
            StreamReader reader = null;

            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, Encoding.UTF8);

                return reader.ReadToEnd();
            }
            finally
            {
                // 释放资源
                if (reader != null)
                    reader.Close();
                if (stream != null)
                    stream.Close();

            }
        }
    }

    public class Utxo
    {
        public UInt256 txid;
        public int n;
        public decimal value;

        public Utxo(UInt256 _txid, decimal _value, int _n)
        {
            txid = _txid;
            value = _value;
            n = _n;
        }
    }

    public class TransactionInput
    {
        public UInt256 hash;
        public int index;
    }
}
