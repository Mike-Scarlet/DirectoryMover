using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMover
{
    public enum Actions
    {
        DeleteDir, DeleteFile, Move, Rename, CreateDir
    }

    public class MappedAction
    {
        public int order;
        public Actions act;
        public string src;
        public string dst;

        public MappedAction(Actions _act, string _src, string _dst, int _order) { act = _act; src = _src; dst = _dst; order = _order; }
        public MappedAction(Actions _act, string _src, int _order) { act = _act; src = _src; dst = ""; order = _order; }

        public override string ToString()
        {
            string res = "";
            switch (act)
            {
                case Actions.DeleteDir:
                    res = "delete dir:\"" + src + '\"';
                    break;
                case Actions.DeleteFile:
                    res = "delete file:\"" + src + "\" to \"" + dst + '\"';
                    break;
                case Actions.Move:
                    res = "move \"" + src + "\" to \"" + dst + '\"';
                    break;
                case Actions.Rename:
                    res = "rename \"" + src + "\" to \"" + dst + '\"';
                    break;
                case Actions.CreateDir:
                    res = "create dir:\"" + src + '\"';
                    break;
            }
            return res;
        }

        public string GetActionStr()
        {
            string res = "";
            switch (act)
            {
                case Actions.DeleteDir:
                    res = "DeleteDir";
                    break;
                case Actions.DeleteFile:
                    res = "DeleteFile";
                    break;
                case Actions.Move:
                    res = "Move";
                    break;
                case Actions.Rename:
                    res = "Rename";
                    break;
                case Actions.CreateDir:
                    res = "CreateDir";
                    break;
            }
            return res;
        }
    }

    class MappedActionComparer : IComparer<MappedAction>
    {
        int IComparer<MappedAction>.Compare(MappedAction x, MappedAction y)
        {
            if (x.order < y.order)
                return 1;
            else if (x.order > y.order)
                return -1;
            else
                return 0;
        }
    }
}
