using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HierarchyArchitecture
{
    class HierarchyNode
    {
        public HierarchyNode parent
        {
            set { _parent = value; this.level = value.level + 1; }
            get { return _parent; }
        }
        private HierarchyNode _parent;
        public List<HierarchyNode> children;
        public int childCount { get { return children.Count; } }
        // reference: root node
        public int level { get; private set; }

        public HierarchyNode()
        {
            this._parent = null;
            this.level = 0;
            this.children = new List<HierarchyNode>();
        }
        public HierarchyNode(HierarchyNode parent)
        {
            this.parent = parent;
            this.children = new List<HierarchyNode>();
        }
        public void AddChild(HierarchyNode child)
        {
            children.Add(child);
        }
        public void RemoveChild(HierarchyNode child)
        {
            children.Remove(child);
        }
    }
}
