using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LoggingNS;
using static DirectoryMover.SystemFunction;
using System.Xml;
using System.Collections;
using HierarchyArchitecture;

namespace DirectoryMover
{
    public class DirectoryHierachyManager
    {
        public enum ErgodicMethod
        {
            Deep, Wide
        }
        public enum Actions
        {
            DeleteDir, DeleteFile, Move, Rename
        }
        class MappedAction
        {
            public int order;
            public Actions act;
            public string src;
            // note: dst is relative path
            public string dst;

            public MappedAction(Actions _act, string _src, string _dst, int _order) { act = _act; src = _src;dst = _dst; order = _order; }
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

        int maxLevel;
        string rootDir;
        string delDir;
        DirectoryNode root;
        ErgodicMethod method;
        Queue<MappedAction> actions;

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
            analyzeMove();
        }

        void initVariables(int _maxLevel, ErgodicMethod _method)
        {
            delDir = Path.Combine(rootDir, "RecycleBin");
            maxLevel = _maxLevel;
            method = _method;
            actions = new Queue<MappedAction>();
        }
        #region BuildHierarchy
        void buildhierachyrecu(DirectoryNode node)
        {
            if (node.level >= maxLevel)
            {
                Logging.warning("Exceed max levels");
                Logging.info(node.originDirName);
                return;
            }
            foreach (string s in Directory.GetDirectories(node.originDirName))
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
                        Logging.info(n.originDirName);
                    return;
                }
                foreach (DirectoryNode n in ltmp)
                {
                    foreach (string s in Directory.GetDirectories(n.originDirName))
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
            //DirectoryNode iterater = root;
            //Stack<DirectoryNode> s = new Stack<DirectoryNode>();
            //while (true)
            //{
            //    /* do something */
            //    int v = iterater.filecount;
            //    foreach (string f in Directory.GetFiles(iterater.dirname))
            //    {
            //        if (!GlobalVar.AcceptedExtensions.Contains(Path.GetExtension(f)))
            //        {
            //            actions.Push(new MappedAction(maxLevel + 2, Actions.DeleteFile, f));
            //            iterater.filecount--;
            //        }
            //    }
            //    if (iterater.childcount == 0 && iterater.filecount == 0)
            //        actions.Push(new MappedAction(maxLevel + 1, Actions.DeleteDir, iterater.dirname));
            //    iterater.filecount = v;
            //    /* end */
            //    if (iterater.childcount != 0)
            //    {
            //        for (int i = 1; i < iterater.childcount; i++)
            //        {
            //            s.Push((DirectoryNode)iterater.children[i]);
            //        }
            //        iterater = (DirectoryNode)iterater.children[0];
            //    }
            //    else
            //    {
            //        if (s.Count != 0)
            //            iterater = s.Pop();
            //        else
            //            break;
            //    }
            //}
        }

        void analyzeDelete()
        {
            DirectoryNode iterater = root;
            Stack<DirectoryNode> s = new Stack<DirectoryNode>();
            int current_level = 0;
            bool moving_down = true;
            //s.Push(root);
            while (true)
            {
                if (moving_down)
                {
                    if (iterater.childCount != 0)
                    {
                        for (int i = 1; i < iterater.childCount; i++)
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
                        if (iterater.originDirName != delDir)
                        {
                            foreach (string f in Directory.GetFiles(iterater.originDirName))
                            {
                                if (!GlobalVar.AcceptedExtensions.Contains(Path.GetExtension(f).ToLower()))
                                {
                                    actions.Enqueue(new MappedAction(Actions.DeleteFile, f, System.Guid.NewGuid().ToString("N") + Path.GetExtension(f), actions.Count));
                                    iterater.fileCount--;
                                }
                            }
                            if (iterater.childCount == 0 && iterater.fileCount == 0)
                            {
                                actions.Enqueue(new MappedAction(Actions.DeleteDir, iterater.originDirName, actions.Count));
                                iterater.parent.RemoveChild(iterater);
                            }
                            else if (iterater.childCount == 1 && iterater.fileCount == 0)
                            {
                                var processstr = ((DirectoryNode)iterater.children[0]).originDirName;
                                var dirlastpath = processstr.Split('\\').Last();
                                var parentpath = (Directory.GetParent(iterater.originDirName)).ToString();
                                var targetpath = parentpath + "\\" + dirlastpath;
                                if (iterater.originDirName != targetpath)
                                {
                                    actions.Enqueue(new MappedAction(Actions.Move, processstr, targetpath, actions.Count));
                                    actions.Enqueue(new MappedAction(Actions.DeleteDir, iterater.originDirName, actions.Count));
                                }
                                else
                                {
                                    actions.Enqueue(new MappedAction(Actions.Move, processstr, targetpath + "_tmp", actions.Count));
                                    actions.Enqueue(new MappedAction(Actions.DeleteDir, iterater.originDirName, actions.Count));
                                    actions.Enqueue(new MappedAction(Actions.Rename, targetpath + "_tmp", targetpath, actions.Count));
                                }
                                iterater.children[0].parent = iterater.parent;
                                ((DirectoryNode)iterater.children[0]).originDirName = targetpath;
                                parentpath = parentpath + "\\" + dirlastpath;
                                foreach (DirectoryNode c_child in iterater.children[0].children)
                                {
                                    processstr = c_child.originDirName;
                                    c_child.originDirName = parentpath + "\\" + processstr.Split('\\').Last();
                                }
                                iterater.parent.AddChild(iterater.children[0]);
                                iterater.parent.RemoveChild(iterater);
                            }
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
            foreach (DirectoryNode dn in root.children)
            {
                List<DirectoryNode> a = new List<DirectoryNode>();
                foreach (DirectoryNode n in dn.children)
                {
                    var lastpath = n.originDirName.Split('\\').Last();
                    var targetpath = root.originDirName + "\\" + lastpath;
                    if (dn.originDirName != targetpath)
                        actions.Enqueue(new MappedAction(Actions.Move, n.originDirName, root.originDirName + '\\' + lastpath, actions.Count));
                    else
                        actions.Enqueue(new MappedAction(Actions.Move, n.originDirName, root.originDirName + '\\' + lastpath + "_GUID-" + System.Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper(), actions.Count));
                    a.Add(n);
                }
                foreach (DirectoryNode b in a)
                    dn.RemoveChild(b);
                if (dn.childCount == 0 && dn.fileCount == 0)
                {
                    actions.Enqueue(new MappedAction(Actions.DeleteDir, dn.originDirName, actions.Count));
                }
            }
        }
        #endregion

        #region Actor
        void display_actions()
        {
            foreach (var action in actions)
            {
                Logging.info(action.ToString());
            }
        }

        void process()
        {
            foreach (var action in actions)
            {
                switch (action.act)
                {
                    case Actions.DeleteDir:
                        // disable read-only attribute
                        if ((File.GetAttributes(action.src) & FileAttributes.ReadOnly) != 0)
                        {
                            Logging.warning(string.Format("Find read-only directory \"{0}\" removing...", action.src));
                            File.SetAttributes(action.src, File.GetAttributes(action.src) ^ FileAttributes.ReadOnly);
                        }
                        Directory.Delete(action.src);
                        break;
                    case Actions.DeleteFile:
                        // del system hidden files
                        if (File.GetAttributes(action.src) == (FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden))
                        {
                            Logging.warning(string.Format("Processing system file \"{0}\"", action.src));
                            File.SetAttributes(action.src, FileAttributes.Archive);
                        }
                        File.Move(action.src, Path.Combine(delDir, action.dst));
                        break;
                    case Actions.Move:
                    case Actions.Rename:
                        Directory.Move(action.src, action.dst);
                        break;
                }
            }
            ChangeFolderIcon(delDir);
        }

        void recover()
        {
            int i;
            var tmplist = actions.ToList();
            //foreach (var action in actions)
            for (i = actions.Count - 1; i >= 0; i--)
            {
                var action = tmplist[i];
                switch (action.act)
                {
                    case Actions.DeleteDir:
                        Directory.CreateDirectory(rootDir + action.src);
                        break;
                    case Actions.DeleteFile:
                        File.Move(rootDir + action.dst, rootDir + action.src);
                        break;
                    case Actions.Move:
                    case Actions.Rename:
                        Directory.Move(rootDir + action.dst, rootDir + action.src);
                        break;
                }
            }
            File.Delete(delDir + "\\backup.db");
            try
            {
                if (File.Exists(Path.Combine(delDir, "desktop.ini")))
                {
                    Logging.info("deleting desktop.ini");
                    File.SetAttributes(Path.Combine(delDir, "desktop.ini"), FileAttributes.Archive);
                    File.Delete(Path.Combine(delDir, "desktop.ini"));
                }
                File.SetAttributes(delDir, FileAttributes.Directory);
                Directory.Delete(delDir);
            }
            catch (IOException)
            {
                Logging.warning("Fail to delete \"" + delDir + "\" please delete on your own");
                ChangeFolderIcon(delDir);
            }
        }

        public void run()
        {
            if (File.Exists(delDir + "\\backup.db"))
            {
                Logging.info("Find recovery file in \"" + delDir + "\"");
                Logging.info("do you want to run recover procedure?(y/n)");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    Console.Write('\b');
                    Logging.info("Starting recovery");
                    loadXml();
                    Logging.info(string.Format("Found {0} actions to do", actions.Count));
                    recover();
                    Logging.info("Finish recovery");
                }
                else
                {
                    Console.Write('\b');
                    Logging.info("Shutting Down");
                    System.Threading.Thread.Sleep(1000);
                }
            }
            else
            {
                Logging.info("building structure for \"" + rootDir + "\"");
                build();
                analyze();
                Logging.info(string.Format("Found {0} actions to do", actions.Count));
                Logging.info("do you want to run packing procedure?(y/n)");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    Console.Write('\b');
                    Directory.CreateDirectory(delDir);
                    Logging.info("Start packing");
                    writeXml();
                    process();
                    Logging.info("Finish packing");
                }
                else
                {
                    Console.Write('\b');
                    Logging.info("Shutting Down");
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
        #endregion

        #region BacklogIO
        XmlDocument getXml()
        {
            /* prepare for relative path */
            int startindex = rootDir.Length;
            /* end */
            XmlDocument xd = new XmlDocument();
            XmlNode header = xd.CreateXmlDeclaration("1.0", "utf-8", null);
            xd.AppendChild(header);
            XmlElement EleActions = xd.CreateElement("Actions");
            foreach (MappedAction action in actions)
            {
                XmlElement xe = xd.CreateElement("Action");
                xe.SetAttribute("order", action.order.ToString());
                XmlElement Eleact = xd.CreateElement("Act");
                Eleact.InnerText = action.GetActionStr();
                XmlElement Elesrc = xd.CreateElement("Source");
                Elesrc.InnerText = action.src.Substring(startindex);
                XmlElement Eledst = xd.CreateElement("Destination");
                switch (action.act)
                {
                    case Actions.Rename:
                    case Actions.Move:
                        Eledst.InnerText = action.dst.Substring(startindex);
                        break;
                    case Actions.DeleteFile:
                        Eledst.InnerText = "\\RecycleBin\\" + action.dst;
                        break;
                    case Actions.DeleteDir:
                        Eledst.InnerText = "";
                        break;
                }
                xe.AppendChild(Eleact);
                xe.AppendChild(Elesrc);
                xe.AppendChild(Eledst);
                EleActions.AppendChild(xe);
            }
            xd.AppendChild(EleActions);
            return xd;
        }

        void writeXml()
        {
            getXml().Save(delDir + "\\backup.db");
        }

        void loadXml()
        {
            XmlDocument xd = new XmlDocument();
            xd.Load(delDir + "\\backup.db");
            int i;
            PriorityQueue<MappedAction> pq = new PriorityQueue<MappedAction>(new MappedActionComparer());
            var ActionCollection = xd.GetElementsByTagName("Action");
            foreach (XmlNode action in ActionCollection)
            {
                Actions act;
                switch (action.SelectSingleNode("Act").InnerText)
                {
                    case "DeleteDir":
                        act = Actions.DeleteDir;
                        break;
                    case "DeleteFile":
                        act = Actions.DeleteFile;
                        break;
                    case "Move":
                        act = Actions.Move;
                        break;
                    case "Rename":
                        act = Actions.Rename;
                        break;
                    default:
                        throw new NotImplementedException("shouldn't reach here");
                }

                pq.Push(new MappedAction(act, action.SelectSingleNode("Source").InnerText, action.SelectSingleNode("Destination").InnerText, int.Parse(action.Attributes[0].Value)));
            }
            for (i = 0; i < ActionCollection.Count; i++)
            {
                actions.Enqueue(pq.Pop());
            }
        }
        #endregion
    }
}
