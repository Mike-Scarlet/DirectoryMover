using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HierarchyArchitecture;

namespace DirectoryMover
{
    public enum AnalyzeType
    {
        deleteExtraFiles = 1 << 0,
        deleteEmptyDirectories = 1 << 1,
        drawSingleDirectory = 1 << 2,
        moveFromFirstLayer = 1 << 3,
        drawAllSubDirectories = 1 << 4,
    }

    // Analyze actions that we should act
    class HierarchyAnalyzer
    {
        int flag;
        int cntDeleteDir, cntDeleteFile, cntMove, cntRename;
        DirectoryNode root;
        public Queue<MappedAction> actions { get; private set; }

        private delegate void WalkFunctionsHandler(DirectoryNode node);
        private WalkFunctionsHandler WalkFunctions;
        public HierarchyAnalyzer(DirectoryNode root, int flag)
        {
            this.root = root;
            this.flag = flag;
            this.actions = new Queue<MappedAction>();
            initVariables();
        }

        public HierarchyAnalyzer(DirectoryNode root, int flag, Queue<MappedAction> actions)
        {
            this.root = root;
            this.flag = flag;
            this.actions = actions;
            initVariables();
        }

        private void initVariables()
        {
            WalkFunctions = null;
            cntDeleteDir = cntDeleteFile = cntMove = cntRename = 0;
        }

        public void Run()
        {
            // clean delDir(RecycleBin)
            // parallel moving
            if (root.HasChild("RecycleBin"))
            {
                RenameDirectoryNoDuplicate(root.SelectChild("RecycleBin"));
                
            }
            actions.Enqueue(new MappedAction(Actions.CreateDir, Path.Combine(root.fullPath, "RecycleBin"), actions.Count));

            if ((flag & (int)AnalyzeType.deleteExtraFiles) != 0)
            {
                WalkFunctions += new WalkFunctionsHandler(DeleteExtraFiles);
                WalkerIter(root);
                WalkFunctions -= new WalkFunctionsHandler(DeleteExtraFiles);
            }
            if ((flag & (int)AnalyzeType.deleteEmptyDirectories) != 0)
            {
                WalkFunctions += new WalkFunctionsHandler(DeleteEmptyDirectories);
            }
            if ((flag & (int)AnalyzeType.drawSingleDirectory) != 0)
            {
                WalkFunctions += new WalkFunctionsHandler(DrawSingleDirectory);
            }
            if ((flag & (int)AnalyzeType.moveFromFirstLayer) != 0)
            {
                MoveFromFirstLayer();
            }
            if ((flag & (int)AnalyzeType.drawAllSubDirectories) != 0)
            {
                throw new NotImplementedException();
            }
            WalkerIter(root);

            LoggingNS.Logging.info(string.Format("\n  Found:\n  {0} file(s) to delete.\n  {1} directory(ies) to delete.\n  {2} move action(s).\n  {3} rename action(s)",
                                   cntDeleteFile, cntDeleteDir, cntMove, cntRename));

            //foreach (var action in actions)
            //{
            //    LoggingNS.Logging.info(action.ToString());
            //}
        }

        private void WalkerRecu(DirectoryNode node)
        {
            if (node.childCount != 0)
                foreach (DirectoryNode d in node.children)
                    WalkerRecu(d);
            // function here
            WalkFunctions?.Invoke(node);
            // end
        }

        private void WalkerIter(DirectoryNode node)
        {
            DirectoryNode iterator = node;
            Stack<DirectoryNode> s = new Stack<DirectoryNode>();
            s.Push(iterator);
            bool down = true;
            while (s.Count != 0)
            {
                if (down)
                {
                    if (iterator.childCount == 0)
                        down = false;
                    else
                    {
                        for (int i = iterator.childCount - 1; i >= 0; i--)
                            s.Push((DirectoryNode)iterator.children[i]);
                        iterator = s.Peek();
                    }
                }
                else
                {
                    iterator = s.Pop();
                    // function here
                    WalkFunctions?.Invoke(iterator);
                    // end
                    if (s.Count != 0)
                        if (iterator.parent == s.Peek().parent)
                        { 
                            down = true;
                            iterator = s.Peek();
                        }
                }
            }
        }

        private void DeleteExtraFiles(DirectoryNode node)
        {
            bool needModify = false;
            if (node.fullPath != node.originDirName)
                needModify = true;
            foreach (string f in Directory.GetFiles(node.originDirName))
            {
                if (GlobalVar.AcceptedExtensions.Contains(Path.GetExtension(f).ToLower()))
                {
                    continue;
                }
                else if (GlobalVar.WarningExtensions.Contains(Path.GetExtension(f).ToLower()))
                {
                    LoggingNS.Logging.warning(string.Format("there's an warning type file: {0}", f));
                }
                else
                {
                    //actions.Enqueue(new MappedAction(Actions.DeleteFile, f, System.Guid.NewGuid().ToString("N") + Path.GetExtension(f), actions.Count));
                    if (needModify)
                        actions.Enqueue(new MappedAction(Actions.DeleteFile, Path.Combine(node.fullPath, Path.GetFileName(f)), actions.Count));
                    else
                        actions.Enqueue(new MappedAction(Actions.DeleteFile, f, actions.Count));
                    node.fileCount--;
                    cntDeleteFile += 1;
                }
            }
        }

        private void DeleteEmptyDirectories(DirectoryNode node)
        {
            if (node.childCount == 0 && node.fileCount == 0)
            {
                actions.Enqueue(new MappedAction(Actions.DeleteDir, node.fullPath, actions.Count));
                node.parent.RemoveChild(node);
                cntDeleteDir += 1;
            }
        }

        private void DrawSingleDirectory(DirectoryNode node)
        {
            if (node.childCount == 1 && node.fileCount == 0)
            {
                if (node.parent != null)
                {
                    //if (((DirectoryNode)node.parent).HasChild(((DirectoryNode)node.children[0]).directoryName))
                    //{
                    //    // find new no duplicate name
                    //    int tmpCounter = 1;
                    //    string appendix;
                    //    while (true)
                    //    {
                    //        if (tmpCounter > 10)
                    //        {
                    //            LoggingNS.Logging.warning("Cannot handle duplicate name in DrawSingleDirectory: {0}", ((DirectoryNode)node.children[0]).directoryName);
                    //            return;
                    //        }
                    //        appendix = Utils.RandomString.rndNameAppendix(tmpCounter);
                    //        if (((DirectoryNode)node.parent).HasChild(((DirectoryNode)node.children[0]).directoryName + appendix))
                    //        {
                    //            tmpCounter += 1;
                    //            continue;
                    //        }
                    //        break;
                    //    }
                    //    actions.Enqueue(new MappedAction(Actions.Rename, ((DirectoryNode)node.children[0]).fullPath, ((DirectoryNode)node.children[0]).fullPath + appendix, actions.Count));
                    //    ((DirectoryNode)node.children[0]).directoryName += appendix;

                    //}
                    MoveDirectory((DirectoryNode)node.children[0], (DirectoryNode)node.parent);
                    //actions.Enqueue(new MappedAction(Actions.Move, ((DirectoryNode)node.children[0]).fullPath, ((DirectoryNode)node.parent).fullPath, actions.Count));
                    actions.Enqueue(new MappedAction(Actions.DeleteDir, node.fullPath, actions.Count));
                    //node.parent.AddChild(node.children[0]);
                    //node.children[0].parent = node.parent;
                    node.children.Clear();
                    node.parent.RemoveChild(node);
                    cntDeleteDir += 1;
                }
            }
        }

        private void MoveFromFirstLayer()
        {
            List<DirectoryNode> secondLayerNode = new List<DirectoryNode>();
            foreach (DirectoryNode node in root.children)
            {
                foreach (DirectoryNode subNode in node.children)
                {
                    //actions.Enqueue(new MappedAction(Actions.Move, subNode.fullPath, root.fullPath, actions.Count));
                    //secondLayerNode.Add(subNode);
                    //subNode.parent = root;
                    //cntMove += 1;
                    MoveDirectoryNoMod(subNode, root, secondLayerNode);
                }
                node.children.Clear();
            }
            root.children = root.children.Concat(secondLayerNode).ToList();
        }

        private void MoveDirectory(DirectoryNode src, DirectoryNode dst)
        {
            if (dst.HasChild(src.directoryName))
            {
                // find new no duplicate name
                int tmpCounter = 1;
                string appendix;
                while (true)
                {
                    if (tmpCounter > 10)
                    {
                        LoggingNS.Logging.warning(string.Format("Cannot handle duplicate name in: {0} - SKIPPED", src.fullPath));
                        return;
                    }
                    appendix = Utils.RandomString.rndNameAppendix(tmpCounter);
                    if (dst.HasChild(src.directoryName + appendix))
                    {
                        tmpCounter += 1;
                        continue;
                    }
                    break;
                }
                actions.Enqueue(new MappedAction(Actions.Rename, src.fullPath, src.fullPath + appendix, actions.Count));
                src.directoryName += appendix;
                cntRename += 1;
            }
            actions.Enqueue(new MappedAction(Actions.Move, src.fullPath, Path.Combine(dst.fullPath, src.directoryName), actions.Count));
            dst.AddChild(src);
            src.parent = dst;
            cntMove += 1;
        }

        private void MoveDirectoryNoMod(DirectoryNode src, DirectoryNode dst, List<DirectoryNode> lst)
        {
            // DEBUG:
            if (src.directoryName.Contains("【CE幻想夏结社】(C87) [らーらら団 (オウカ)] 東方ぱらだいす Vol.1 (東方Project)"))
                src = src;
            // ENDDEBUG
            if (dst.HasChild(src.directoryName))
            {
                // find new no duplicate name
                int tmpCounter = 1;
                string appendix;
                while (true)
                {
                    if (tmpCounter > 10)
                    {
                        LoggingNS.Logging.warning(string.Format("Cannot handle duplicate name in: {0} - SKIPPED", src.fullPath));
                        return;
                    }
                    appendix = Utils.RandomString.rndNameAppendix(tmpCounter);
                    if (dst.HasChild(src.directoryName + appendix))
                    {
                        tmpCounter += 1;
                        continue;
                    }
                    break;
                }
                actions.Enqueue(new MappedAction(Actions.Rename, src.fullPath, src.fullPath + appendix, actions.Count));
                src.directoryName += appendix;
                cntRename += 1;
            }
            else if (NameIn(lst, src.directoryName))
            {
                // find new no duplicate name
                int tmpCounter = 1;
                string appendix;
                while (true)
                {
                    if (tmpCounter > 10)
                    {
                        LoggingNS.Logging.warning(string.Format("Cannot handle duplicate name in: {0} - SKIPPED", src.fullPath));
                        return;
                    }
                    appendix = Utils.RandomString.rndNameAppendix(tmpCounter);
                    if (NameIn(lst, src.directoryName + appendix))
                    {
                        tmpCounter += 1;
                        continue;
                    }
                    break;
                }
                actions.Enqueue(new MappedAction(Actions.Rename, src.fullPath, src.fullPath + appendix, actions.Count));
                src.directoryName += appendix;
                cntRename += 1;
            }
            actions.Enqueue(new MappedAction(Actions.Move, src.fullPath, Path.Combine(dst.fullPath, src.directoryName), actions.Count));
            lst.Add(src);
            src.parent = dst;
            cntMove += 1;
        }

        private void RenameDirectoryNoDuplicate(DirectoryNode src)
        {
            if (((DirectoryNode)src.parent).HasChild(src.directoryName))
            {
                // find new no duplicate name
                int tmpCounter = 1;
                string appendix;
                while (true)
                {
                    if (tmpCounter > 10)
                    {
                        LoggingNS.Logging.warning(string.Format("Cannot handle duplicate name in: {0} - SKIPPED", src.fullPath));
                        return;
                    }
                    appendix = Utils.RandomString.rndNameAppendix(tmpCounter);
                    if (((DirectoryNode)src.parent).HasChild(src.directoryName + appendix))
                    {
                        tmpCounter += 1;
                        continue;
                    }
                    break;
                }
                actions.Enqueue(new MappedAction(Actions.Rename, src.fullPath, src.fullPath + appendix, actions.Count));
                src.directoryName += appendix;
                cntRename += 1;
            }
        }

        private bool NameIn(List<DirectoryNode> list, string name)
        {
            var s = from child in list
                    where child.directoryName == name
                    select child;
            if (s.Count() == 0)
                return false;
            else
                return true;
        }
    }
}
