using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LoggingNS;

namespace HierarchyArchitecture
{
    public enum ErgodicMethod
    {
        Deep, Wide
    }
    /// <summary>
    /// build hierarchy from a given root directory
    /// </summary>
    class DirectoryHierarchy
    {
        public DirectoryNode root;
        string rootDir;
        string rootParent;
        ErgodicMethod ergodicMethod;
        int maxLevel;
        int totalLevel;
        HashSet<string> fileExtensions;

        public DirectoryHierarchy(string rootDir, ErgodicMethod method = ErgodicMethod.Deep, int maxLevel = 10) { this.rootDir = rootDir; InitVariables(method, maxLevel); }

        public void InitVariables(ErgodicMethod method, int maxLevel)
        {
            ergodicMethod = method;
            this.maxLevel = maxLevel;
            totalLevel = 0;
            this.rootParent = Directory.GetParent(rootDir).ToString();
            fileExtensions = new HashSet<string>();
        }

        #region Building
        public void Build()
        {
            root = new DirectoryNode(rootDir);
            switch (ergodicMethod)
            {
                case ErgodicMethod.Deep:
                    BuildHierachyRecu(root);
                    break;
                case ErgodicMethod.Wide:
                    BuildHierachyIter(root);
                    break;
            }

            Logging.info("Finish building the hierarchy form directory: " + root.originDirName);
            string exts = "";
            foreach (string ext in fileExtensions)
            {
                exts += ext + ' ';
            }
            Logging.info("Extensions: ", exts);
            Logging.info("Total levels: ", totalLevel.ToString());
        }

        void BuildHierachyRecu(DirectoryNode node)
        {
            // load extensions
            foreach (var fileName in Directory.GetFiles(node.originDirName))
            {
                fileExtensions.Add(Path.GetExtension(fileName));
            }

            if (node.level > totalLevel)
                totalLevel = node.level;
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
                BuildHierachyRecu(tmp);
            }
        }

        void BuildHierachyIter(DirectoryNode node)
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
                    // load extensions
                    foreach (var fileName in Directory.GetFiles(n.originDirName))
                    {
                        fileExtensions.Add(Path.GetExtension(fileName));
                    }

                    foreach (string s in Directory.GetDirectories(n.originDirName))
                    {
                        DirectoryNode tmp = new DirectoryNode(s, n);
                        n.AddChild(tmp);
                        rtmp.Add(tmp);
                    }
                }
                if (rtmp.Count <= 0)
                { 
                    noneediter = true;
                    totalLevel = itercnt;
                }
                ltmp = rtmp;
                itercnt++;
            }
        }
        #endregion
    }
}
