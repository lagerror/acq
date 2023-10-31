using System.Security.Cryptography;
using System.Text;

namespace acq.Tools
{
    public class Tools
    {
        /// <summary>
        /// MD5加密后返回16进制格式大写的字符串
        /// </summary>
        /// <param name="input">需要加密的字符串</param>
        /// <returns></returns>
        public static string md5(string input)
        {
            string sign = "";
            MD5CryptoServiceProvider md=new MD5CryptoServiceProvider();
            byte[] bValue, bHash;
            bValue=Encoding.UTF8.GetBytes(input);
            bHash = md.ComputeHash(bValue);
            md.Clear();
            md.Dispose();
            for(int i = 0;i< bHash.Length; i++)
            {
                sign += bHash[i].ToString("X").PadLeft(2, '0');
            }
            sign = sign.ToUpper();
            return sign;
        }
    }
}
