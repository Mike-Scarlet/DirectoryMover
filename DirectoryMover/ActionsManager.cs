using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace DirectoryMover
{
    /// <summary>
    /// Execute actions and record them
    /// </summary>
    class ActionsManager
    {
        List<MappedAction> actionInput;
        public List<MappedAction> actionExecuted { get; private set; }
        string rootDir, delDir;

        public ActionsManager(string rootDir, List<MappedAction> input)
        {
            actionInput = input;
            actionExecuted = new List<MappedAction>();
            this.rootDir = rootDir;
            initVariables();
        }

        public ActionsManager(string rootDir)
        {
            actionInput = new List<MappedAction>();
            actionExecuted = new List<MappedAction>();
            this.rootDir = rootDir;
            initVariables();
        }

        void initVariables()
        {
            delDir = Path.Combine(rootDir, "RecycleBin");
        }

        public void Act()
        {
            MappedAction pointer = null;
            try
            {
                foreach (var action in actionInput)
                {
                    pointer = action;
                    switch (action.act)
                    {
                        case Actions.DeleteDir:
                            DeleteDirectory(action);
                            break;
                        case Actions.DeleteFile:
                            DeleteFile(action);
                            break;
                        case Actions.Move:
                        case Actions.Rename:
                            MoveDirectory(action);
                            break;
                        case Actions.CreateDir:
                            Directory.CreateDirectory(action.src);
                            //actionExecuted.Add(new MappedAction(Actions.CreateDir, action.src, actionExecuted.Count));
                            // TODO: add To Executed Action (or ignore)
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            catch (Exception e)
            {
                LoggingNS.Logging.error("We come across an error when acting: ", pointer.ToString());
                LoggingNS.Logging.error(e.Message);
                LoggingNS.Logging.error(e.StackTrace);
                LoggingNS.Logging.error("End action");
            }
            SystemFunction.ChangeFolderIcon(delDir);
            Save();
            LoggingNS.Logging.info("Finish saving the recovery file");
        }

        public void Recover()
        {
            int i;
            var tmplist = actionInput;
            bool isSafe = true;
            bool needClearEnv = true;
            for (i = actionInput.Count - 1; i >= 0; i--)
            {
                var action = tmplist[i];
                try
                {
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
                        case Actions.CreateDir:
                            break;
                    }
                }
                catch (Exception e)
                {
                    if (rootDir + action.src == delDir)
                    {
                        // means packing iteration
                        if (Directory.GetDirectories(delDir).Count() == 0 && Directory.GetFiles(delDir).Count() == 2 && actionExecuted.Count == 0)
                        {
                            // disable read-only attribute
                            if ((File.GetAttributes(delDir) & FileAttributes.ReadOnly) != 0)
                            {
                                File.SetAttributes(delDir, File.GetAttributes(delDir) ^ FileAttributes.ReadOnly);
                            }
                            Directory.Delete(delDir, true);
                            System.Threading.Thread.Sleep(1);
                            Directory.Move(rootDir + action.dst, delDir);
                            needClearEnv = false;
                            SystemFunction.ChangeFolderIcon(delDir);
                        }
                        else
                            actionExecuted.Add(action);
                    }
                    else
                    {
                        LoggingNS.Logging.warning("failed recover: ", action.ToString(), " ... pass");
                        action.src = rootDir + action.src;
                        if (action.dst != "")
                            action.dst = rootDir + action.dst;
                        actionExecuted.Add(action);
                        isSafe = false;
                    }
                }
            }
            if (isSafe)
            {
                if (needClearEnv)
                {
                    File.Delete(delDir + "\\backup.db");
                    try
                    {
                        if (File.Exists(Path.Combine(delDir, "desktop.ini")))
                        {
                            LoggingNS.Logging.info("deleting desktop.ini");
                            File.SetAttributes(Path.Combine(delDir, "desktop.ini"), FileAttributes.Archive);
                            File.Delete(Path.Combine(delDir, "desktop.ini"));
                        }
                        File.SetAttributes(delDir, FileAttributes.Directory);
                        Directory.Delete(delDir);
                    }
                    catch (IOException)
                    {
                        LoggingNS.Logging.warning("Fail to delete \"" + delDir + "\" please delete on your own");
                    }
                }
            }
            else
            {
                Save();
                LoggingNS.Logging.info("We left the recover file so you can try recover again with correct file placement");
            }
        }

        public void Save()
        {
            ActionsIO.DumpActions(actionExecuted, rootDir, delDir);
        }

        public void Load()
        {
            //Directory.Move(delDir + "\\backup.db", rootDir + "\\backup.db");
            actionInput = ActionsIO.EvalActions(delDir);

        }

        private void MoveDirectory(MappedAction action)
        // can also be used as renaming a directory
        {
            Directory.Move(action.src, action.dst);
            actionExecuted.Add(new MappedAction(Actions.Move, action.src, action.dst, actionExecuted.Count));
        }

        private void DeleteFile(MappedAction action)
        {
            // del system hidden files
            if (File.GetAttributes(action.src) == (FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden))
            {
                LoggingNS.Logging.warning(string.Format("Processing system file \"{0}\"", action.src));
                File.SetAttributes(action.src, FileAttributes.Archive);
            }
            string tempFileName = System.Guid.NewGuid().ToString("N") + Path.GetExtension(action.src);
            File.Move(action.src, Path.Combine(delDir, tempFileName));
            actionExecuted.Add(new MappedAction(Actions.Move, action.src, Path.Combine(delDir, tempFileName), actionExecuted.Count));
        }

        private void DeleteDirectory(MappedAction action)
        {
            // disable read-only attribute
            if ((File.GetAttributes(action.src) & FileAttributes.ReadOnly) != 0)
            {
                LoggingNS.Logging.warning(string.Format("Find read-only directory \"{0}\" removing...", action.src));
                File.SetAttributes(action.src, File.GetAttributes(action.src) ^ FileAttributes.ReadOnly);
            }
            Directory.Delete(action.src);
            actionExecuted.Add(new MappedAction(Actions.DeleteDir, action.src, actionExecuted.Count));
        }
    }

    static class ActionsIO
    {
        public static void DumpActions(List<MappedAction> actions, string rootDir, string delDir)
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
                        //Eledst.InnerText = "\\RecycleBin\\" + action.dst;
                        Eledst.InnerText = "";
                        break;
                    case Actions.DeleteDir:
                        Eledst.InnerText = "";
                        break;
                    default:
                        throw new NotImplementedException();
                }
                xe.AppendChild(Eleact);
                xe.AppendChild(Elesrc);
                xe.AppendChild(Eledst);
                EleActions.AppendChild(xe);
            }
            xd.AppendChild(EleActions);
            xd.Save(delDir + "\\backup.db"); ;
        }

        public static List<MappedAction> EvalActions(string rootDir)
        {
            List<MappedAction> actions = new List<MappedAction>();
            XmlDocument xd = new XmlDocument();
            xd.Load(rootDir + "\\backup.db");
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
                actions.Add(pq.Pop());
            }
            return actions;
        }
    }
}
