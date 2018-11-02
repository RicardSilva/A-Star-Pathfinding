using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures
{
    public class GatewayDistanceTableRow : ScriptableObject
    {
        public GatewayDistanceTableEntry[] entries;

        public void Initialize(int collumns) {
            entries = new GatewayDistanceTableEntry[collumns];
        }

        public void AddEntry(GatewayDistanceTableEntry entry, int collumn) {
            entries[collumn] = entry;
        }
    }

    
}
