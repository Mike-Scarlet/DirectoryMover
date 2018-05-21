#define LOAD_FILE_COUNT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if LOAD_FILE_COUNT
using System.IO;
#endif

namespace DirectoryHierachy
{
    class HierachyNode
    {
        public HierachyNode parent;
        public List<HierachyNode> children;
        public int childcount { get { return children.Count; } }
        // reference: root node
        public int level { get; private set; }

        public HierachyNode()
        {
            parent = null;
            level = 0;
            children = new List<HierachyNode>();
        }
        public HierachyNode(HierachyNode _parent)
        {
            parent = _parent;
            level = _parent.level + 1;
            children = new List<HierachyNode>();
        }
        public void AddChild(HierachyNode _child)
        {
            children.Add(_child);
        }
        public void RemoveChild(HierachyNode _child)
        {
            children.Remove(_child);
        }
    }

    class DirectoryNode : HierachyNode
    {
        public string dirname;
        public int filecount;

        public DirectoryNode(string _dirname) : base()
        {
            dirname = _dirname;
#if LOAD_FILE_COUNT
            filecount = Directory.GetFiles(_dirname).Length;
#endif
        }

        public DirectoryNode(string _dirname, DirectoryNode _parent) : base(_parent)
        {
            dirname = _dirname;
#if LOAD_FILE_COUNT
            filecount = Directory.GetFiles(_dirname).Length;
#endif
        }
    }
}
