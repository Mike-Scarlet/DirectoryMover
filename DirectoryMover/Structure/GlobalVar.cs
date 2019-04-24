using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMover
{
    static class GlobalVar
    {
        static readonly string[] whitelist = { ".png", ".jpg", ".jpeg", ".gif" };
        static readonly string[] warnlist = { ".zip", ".rar" };
        public static HashSet<string> AcceptedExtensions = new HashSet<string>(whitelist);
        public static HashSet<string> WarningExtensions = new HashSet<string>(warnlist);
    }
}
