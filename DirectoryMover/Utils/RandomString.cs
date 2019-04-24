using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMover.Utils
{
    static class RandomString
    {
        static private Random rnd = new Random();

        static public string rndNameAppendix(int count)
        {
            StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < count; i++)
            //{
            //    int rndResult = rnd.Next() % 62;
            //    if (rndResult <= 25) // 0~25 -> a~z
            //        sb.Append((char)('a' + rndResult));
            //    else if (rndResult <= 51) // 26~51 -> A~Z
            //        sb.Append((char)('A' + rndResult - 26));
            //    else // 52~61 -> 0~9
            //        sb.Append((char)('0' + rndResult - 52));
            //}
            for (int i = 0; i < count; i++)
            {
                int rndResult = rnd.Next() % 36;
                if (rndResult <= 25) // 0~25 -> a~z
                    sb.Append((char)('a' + rndResult));
                else // 26~35 -> 0~9
                    sb.Append((char)('0' + rndResult - 26));
            }
            return sb.ToString();
        }
    }
}
