using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures
{
    public class GatewayDistanceTableEntry : ScriptableObject
    {
        public Vector3 startGatewayPosition;
        public Vector3 endGatewayPosition;
        public float shortestDistance;

        public void Initialize(Vector3 startPosition, Vector3 endPosition, float cost) {
            this.startGatewayPosition = startPosition;
            this.endGatewayPosition = endPosition;
            this.shortestDistance = cost;
        }
    }
}
