#define LOAD_FILE_COUNT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if LOAD_FILE_COUNT
using System.IO;
#endif

namespace HierarchyArchitecture
{
    class DirectoryNode : HierarchyNode
    {
        public string originDirName;
        public string directoryName;
        public int fileCount;

        public string fullPath
        {
            get { return GetFullPathRecu(this, ""); }
        }

        public string relativePath
        {
            get { return GetRelativePathRecu(this, ""); }
        }

        public int dirCount { get { return childCount; } }

        public DirectoryNode(string originDirName) : base()
        {
            this.originDirName = originDirName;
            this.directoryName = Path.GetFileName(originDirName);
#if LOAD_FILE_COUNT
            fileCount = Directory.GetFiles(originDirName).Length;
#endif
        }

        public DirectoryNode(string originDirName, DirectoryNode parent) : base(parent)
        {
            this.originDirName = originDirName;
            this.directoryName = Path.GetFileName(originDirName);
#if LOAD_FILE_COUNT
            fileCount = Directory.GetFiles(originDirName).Length;
#endif
        }

        private string GetRelativePathRecu(DirectoryNode node, string pathSoFar)
        {
            if (node.parent != null)
                return GetRelativePathRecu(node.parent as DirectoryNode, Path.Combine(node.directoryName, pathSoFar));
            else
                return Path.Combine(".", pathSoFar);
        }

        private string GetFullPathRecu(DirectoryNode node, string pathSoFar)
        {
            if (node.parent != null)
                return GetFullPathRecu(node.parent as DirectoryNode, Path.Combine(node.directoryName, pathSoFar));
            else
                return Path.Combine(node.originDirName, pathSoFar);
        }

        public bool HasChild(string dirName)
        {
            var s = from child in this.children
                    where ((DirectoryNode)child).directoryName == dirName
                    select child;
            if (s.Count() == 0)
                return false;
            else
                return true;
        }

        public DirectoryNode SelectChild(string dirName)
        {
            var s = from child in this.children
                    where ((DirectoryNode)child).directoryName == dirName
                    select child;
            return s.First() as DirectoryNode;
        }

        //public MoveTo(DirectoryNode newParent)
        //{
        //    // Move out from parent
        //    if(parent != null)
        //        if (!parent.children.Remove(this))
        //            LoggingNS.Logging.warning(string.Format("DirectoryNode \"{}\"'s parent don't have this child", this.dirName));

        //    // Move to new parent
        //    newParent.AddChild(this);
        //}
    }
}
