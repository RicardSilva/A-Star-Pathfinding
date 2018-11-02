using RAIN.Navigation.Graph;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class GatewayHeuristic : IHeuristic
    {
        private ClusterGraph ClusterGraph { get; set; }

        public GatewayHeuristic(ClusterGraph clusterGraph)
        {
            this.ClusterGraph = clusterGraph;
        }

        public float H(NavigationGraphNode node, NavigationGraphNode goalNode)
        {
            Cluster startCluster = ClusterGraph.Quantize2(node);
            Cluster goalCluster = ClusterGraph.Quantize2(goalNode);
            Vector3 startPosition = node.LocalPosition;
            Vector3 goalPosition = goalNode.LocalPosition;

            if (startCluster == goalCluster) return (goalPosition - startPosition).magnitude;

            Vector3 startGatewayPosition;
            Vector3 goalGatewayPosition;
            List<Gateway> startGateways = startCluster.gateways;
            List<Gateway> goalGateways = goalCluster.gateways;

            float h = int.MaxValue;
            float h1 = 0; float h2 = 0;
            float startGatewaysCount = startGateways.Count;
            float goalGatewaysCount = goalGateways.Count;
            int i, j;

            for (i = 0; i < startGatewaysCount; i++) {
                startGatewayPosition = startGateways[i].center;
                h1 = (startGatewayPosition - startPosition).magnitude;

                for (j = 0; j < goalGatewaysCount; j++) {
                    goalGatewayPosition = goalGateways[j].center;
                    h2  = h1 + ClusterGraph.DistanceBetweenGateways(startGateways[i].id, goalGateways[j].id)
                        + (goalPosition - goalGatewayPosition).magnitude;
                    if (h2 < h) h = h2;
                }
            }

            return h;
        }

        public float EuclideanDistance(Vector3 startPosition, Vector3 endPosition)
        {
            return (endPosition - startPosition).magnitude;
        }
    }
}
