using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
   
    public class NodeHashTable : IClosedSet { 
        private Dictionary<int, NodeRecord> NodeRecords { get; set; }

        public NodeHashTable()
        {
            this.NodeRecords = new Dictionary<int, NodeRecord>();
        }

        public void Initialize()
        {
            this.NodeRecords.Clear();
        }


        public void AddToClosed(NodeRecord nodeRecord)
        {
            this.NodeRecords.Add(nodeRecord.GetHashCode(), nodeRecord);
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            this.NodeRecords.Remove(nodeRecord.GetHashCode());
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            NodeRecord ret;
            this.NodeRecords.TryGetValue(nodeRecord.GetHashCode(), out ret);
            return ret;
        }

        public ICollection<NodeRecord> All()
        {
            return this.NodeRecords.Values;
        }





    }
}
