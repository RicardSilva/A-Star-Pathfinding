using RAIN.Navigation.Graph;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Utils;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures
{
    public class ClusterGraph : ScriptableObject
    {
        public List<Cluster> clusters;
        public List<MasterCluster> MasterClusters;
        public List<Gateway> gateways;
        public GatewayDistanceTableRow[] gatewayDistanceTable;

        public ClusterGraph()
        {
            this.clusters = new List<Cluster>();
            this.gateways = new List<Gateway>();

            this.MasterClusters = new List<MasterCluster>();
        }
        

        // Uses only clusters
        public Cluster Quantize(NavigationGraphNode node)
        {
            Vector3 position = node.LocalPosition;
            //TODO: MAYBE IMPROVE BY DIVIDING CLUSTERS INTO ZONES
            foreach (var cluster in this.clusters)
            {
                if (MathHelper.PointInsideBoundingBox(position, cluster.min, cluster.max))
                {
                    return cluster;
                }
            }
            return null;
        }
        // Uses clusters of clusters
        public Cluster Quantize2(NavigationGraphNode node)
        {
            Vector3 position = node.LocalPosition;
            MasterCluster masterCluster = null;
            foreach (MasterCluster c in this.MasterClusters)
            {
                if (MathHelper.PointInsideBoundingBox(position, c.min, c.max))
                {
                    masterCluster = c;
                    foreach (Cluster cluster in masterCluster.clusters)
                    {
                        if (MathHelper.PointInsideBoundingBox(position, cluster.min, cluster.max))
                        {
                            return cluster;
                        }
                    }
                }
            }
             
            

            return null;
        }

        public float DistanceBetweenGateways(int l, int c) {
            return gatewayDistanceTable[l].entries[c].shortestDistance;
        }

        public void SaveToAssetDatabase()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (System.IO.Path.GetExtension(path) != "")
            {
                path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + typeof(ClusterGraph).Name.ToString() + ".asset");

            AssetDatabase.CreateAsset(this, assetPathAndName);
            EditorUtility.SetDirty(this);
            
            //save the clusters
            foreach(var cluster in this.clusters)
            {
                AssetDatabase.AddObjectToAsset(cluster, assetPathAndName);
            }

            //save the gateways
            foreach (var gateway in this.gateways)
            {
                AssetDatabase.AddObjectToAsset(gateway, assetPathAndName);
            }

            //save the MasterClusters
            foreach (var masterCluster in this.MasterClusters)
            {
                AssetDatabase.AddObjectToAsset(masterCluster, assetPathAndName);
            }

            //save the gatewayTableRows and tableEntries
            foreach (var tableRow in this.gatewayDistanceTable)
            {
                AssetDatabase.AddObjectToAsset(tableRow, assetPathAndName);
                foreach(var tableEntry in tableRow.entries)
                {
                    AssetDatabase.AddObjectToAsset(tableEntry, assetPathAndName);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = this;
        }
    }
}
