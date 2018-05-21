using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LoggingNS;

namespace DirectoryHierachy
{
    static class GlobalVar
    {
        static readonly string[] whitelist = { ".png", ".jpg", ".jpeg", ".gif", ".zip" };
        public static HashSet<string> AcceptedExtensions = new HashSet<string>(whitelist);
    }

    public class DirectoryHierachyManager
    {
        public enum ErgodicMethod
        {
            Deep, Wide
        }
        public enum Actions
        {
            DeleteDir, DeleteFile, Move
        }
        public struct MappedAction
        {
            public int level;
            public Actions act;
            public string src;
            // note: dst is relative path
            public string dst;

            public MappedAction(int _level, Actions _act, string _src, string _dst) { level = _level; act = _act; src = _src;dst = _dst; }
            public MappedAction(int _level, Actions _act, string _src) { level = _level; act = _act; src = _src; dst = ""; }
        }
        public class MappedActionComparer : IComparer<MappedAction>
        {
            public int Compare(MappedAction x, MappedAction y)
            {
                if (x.level > y.level)
                    return 1;
                else if (x.level == y.level)
                    return 0;
                else
                    return -1;
            }
        }
        int maxLevel;
        string rootDir;
        string delDir;
        DirectoryNode root;
        ErgodicMethod method;
        PriorityQueue<MappedAction> actions;

        public DirectoryHierachyManager(string _rootDir)
        {
            rootDir = _rootDir;
            initVariables(10, ErgodicMethod.Deep);
        }
        public DirectoryHierachyManager(string _rootDir, int _maxLevel, ErgodicMethod _method)
        {
            rootDir = _rootDir;
            initVariables(_maxLevel, _method);
        }
        public void build()
        {
            root = new DirectoryNode(rootDir);
            switch (method)
            {
                case ErgodicMethod.Deep:
                    buildhierachyrecu(root);
                    break;
                case ErgodicMethod.Wide:
                    buildhierachyiter(root);
                    break;
            }
        }
        public void analyze()
        {
            analyzeDelete();
        }

        void initVariables(int _maxLevel, ErgodicMethod _method)
        {
            delDir = rootDir + "RecycleBin";
            maxLevel = _maxLevel;
            method = _method;
            actions = new PriorityQueue<MappedAction>(new MappedActionComparer());
        }
        #region BuildHierarchy
        void buildhierachyrecu(DirectoryNode node)
        {
            if (node.level >= maxLevel)
            {
                Logging.warning("Exceed max levels");
                Logging.info(node.dirname);
                return;
            }
            foreach (string s in Directory.GetDirectories(node.dirname))
            {
                DirectoryNode tmp = new DirectoryNode(s, node);
                node.AddChild(tmp);
                buildhierachyrecu(tmp);
            }
        }
        void buildhierachyiter(DirectoryNode node)
        {
            int itercnt = 0;
            bool noneediter = false;
            List<DirectoryNode> ltmp = new List<DirectoryNode>();
            ltmp.Add(node);
            while (!noneediter)
            {
                List<DirectoryNode> rtmp = new List<DirectoryNode>();
                if (itercnt >= maxLevel)
                {
                    Logging.warning("Exceed max levels");
                    foreach (DirectoryNode n in ltmp)
                        Logging.info(n.dirname);
                    return;
                }
                foreach (DirectoryNode n in ltmp)
                {
                    foreach (string s in Directory.GetDirectories(n.dirname))
                    {
                        DirectoryNode tmp = new DirectoryNode(s, n);
                        n.AddChild(tmp);
                        rtmp.Add(tmp);
                    }
                }
                if (rtmp.Count <= 0)
                    noneediter = true;
                ltmp = rtmp;
                itercnt++;
            }
        }
        #endregion

        #region AnalyzeStructure

        [Obsolete("this is a Preorder Traverse method which couldn't match our satisfaction.")]
        void analyzeDeletePre()
        {
            DirectoryNode iterater = root;
            Stack<DirectoryNode> s = new Stack<DirectoryNode>();
            while (true)
            {
                /* do something */
                int v = iterater.filecount;
                foreach (string f in Directory.GetFiles(iterater.dirname))
                {
                    if (!GlobalVar.AcceptedExtensions.Contains(Path.GetExtension(f)))
                    {
                        actions.Push(new MappedAction(maxLevel + 2, Actions.DeleteFile, f));
                        iterater.filecount--;
                    }
                }
                if (iterater.childcount == 0 && iterater.filecount == 0)
                    actions.Push(new MappedAction(maxLevel + 1, Actions.DeleteDir, iterater.dirname));
                iterater.filecount = v;
                /* end */
                if (iterater.childcount != 0)
                {
                    for (int i = 1; i < iterater.childcount; i++)
                    {
                        s.Push((DirectoryNode)iterater.children[i]);
                    }
                    iterater = (DirectoryNode)iterater.children[0];
                }
                else
                {
                    if (s.Count != 0)
                        iterater = s.Pop();
                    else
                        break;
                }
            }
        }

        void analyzeDelete()
        {
            DirectoryNode iterater = root;
            Stack<DirectoryNode> s = new Stack<DirectoryNode>();
            int current_level = 0;
            bool moving_down = true;
            s.Push(root);
            while (true)
            {
                if (moving_down)
                {
                    if (iterater.childcount != 0)
                    {
                        for (int i = 1; i < iterater.childcount; i++)
                        {
                            s.Push((DirectoryNode)iterater.children[i]);
                        }
                        iterater = (DirectoryNode)iterater.children[0];
                        s.Push(iterater);
                    }
                    else
                    {
                        moving_down = false;
                    }
                    current_level = iterater.level + 1; // note: caution here
                }
                else
                {
                    if (s.Count != 0)
                        iterater = s.Peek();
                    else
                        break;
                    if (iterater.level < current_level)
                    {
                        /* do something */
                        foreach (string f in Directory.GetFiles(iterater.dirname))
                        {
                            if (!GlobalVar.AcceptedExtensions.Contains(Path.GetExtension(f)))
                            {
                                actions.Push(new MappedAction(maxLevel + 2, Actions.DeleteFile, f, System.Guid.NewGuid().ToString("N")));
                                iterater.filecount--;
                            }
                        }
                        if (iterater.childcount == 0 && iterater.filecount == 0)
                        { 
                            actions.Push(new MappedAction(maxLevel + 1, Actions.DeleteDir, iterater.dirname));
                            iterater.parent.RemoveChild(iterater);
                        }
                        /* end */
                        current_level = iterater.level;
                        s.Pop();
                    }
                    else if (iterater.level == current_level)
                    {
                        moving_down = true;
                    }
                    else
                        throw new NotImplementedException("shouldn't reach here");
                }
            }
        }

        void analyzeMove()
        {

        }
        #endregion
    }
}
