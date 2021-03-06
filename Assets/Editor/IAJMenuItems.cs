﻿using UnityEngine;
using UnityEditor;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.Utils;
using RAIN.Navigation.NavMesh;
using System.Collections.Generic;
using RAIN.Navigation.Graph;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using System;

public class IAJMenuItems  {

    [MenuItem("IAJ/Create Cluster Graph")]
    private static void CreateClusterGraph()
    {
        Cluster cluster;
        Gateway gateway;

        //get cluster game objects
        var clusters = GameObject.FindGameObjectsWithTag("Cluster");
        //sort by name to be able to manually create the master clusters
        Array.Sort(clusters, CompareObNames);


        //get gateway game objects
        var gateways = GameObject.FindGameObjectsWithTag("Gateway");
        //get the NavMeshGraph from the current scene
        NavMeshPathGraph navMesh = GameObject.Find("Navigation Mesh").GetComponent<NavMeshRig>().NavMesh.Graph;

        ClusterGraph clusterGraph = ScriptableObject.CreateInstance<ClusterGraph>();

        //create gateway instances for each gateway game object
        for (int i = 0; i < gateways.Length; i++)
        {
            var gatewayGO = gateways[i];
            gateway = ScriptableObject.CreateInstance<Gateway>();
            gateway.Initialize(i, gatewayGO);
            clusterGraph.gateways.Add(gateway);
        }

        //create cluster instances for each cluster game object and check for connections through gateways
        foreach (var clusterGO in clusters)
        {

            cluster = ScriptableObject.CreateInstance<Cluster>();
            cluster.Initialize(clusterGO);
            clusterGraph.clusters.Add(cluster);

            //determine intersection between cluster and gateways and add connections when they intersect
            foreach (var gate in clusterGraph.gateways)
            {
                if (MathHelper.BoundingBoxIntersection(cluster.min, cluster.max, gate.min, gate.max))
                {
                    cluster.gateways.Add(gate);
                    gate.clusters.Add(cluster);
                }
            }
        }
        //Creating master clusters
        clusterGraph.MasterClusters.Add(new MasterCluster(clusterGraph.clusters[1], clusterGraph.clusters[2]));
        clusterGraph.MasterClusters.Add(new MasterCluster(clusterGraph.clusters[3], clusterGraph.clusters[4], clusterGraph.clusters[5], clusterGraph.clusters[18]));
        clusterGraph.MasterClusters.Add(new MasterCluster(clusterGraph.clusters[19], clusterGraph.clusters[20], clusterGraph.clusters[21], clusterGraph.clusters[22], clusterGraph.clusters[23]));
        clusterGraph.MasterClusters.Add(new MasterCluster(clusterGraph.clusters[24], clusterGraph.clusters[25], clusterGraph.clusters[26], clusterGraph.clusters[27], clusterGraph.clusters[28], clusterGraph.clusters[29]));
        clusterGraph.MasterClusters.Add(new MasterCluster(clusterGraph.clusters[7], clusterGraph.clusters[8], clusterGraph.clusters[9], clusterGraph.clusters[10], clusterGraph.clusters[11], clusterGraph.clusters[12], clusterGraph.clusters[13], clusterGraph.clusters[14]));
        clusterGraph.MasterClusters.Add(new MasterCluster(clusterGraph.clusters[6], clusterGraph.clusters[16], clusterGraph.clusters[17]));
        clusterGraph.MasterClusters.Add(new MasterCluster(clusterGraph.clusters[0]));
        clusterGraph.MasterClusters.Add(new MasterCluster(clusterGraph.clusters[15]));

        // Second stage of the algorithm, calculation of the Gateway table

        GlobalPath solution = null;
        float cost;
        Gateway startGate;
        Gateway endGate;
        int gatewaysCount = gateways.Length;
        int k, j;
        var pathfindingAlgorithm = new NodeArrayAStarPathFinding(navMesh, new EuclidianHeuristic());
        
        GatewayDistanceTableRow[] distanceTable = new GatewayDistanceTableRow[gatewaysCount];

        for (k = 0; k < gatewaysCount; k++) {
            GatewayDistanceTableRow distanceTableRow = ScriptableObject.CreateInstance<GatewayDistanceTableRow>();
            distanceTableRow.Initialize(gatewaysCount);

            startGate = clusterGraph.gateways[k];

            for (j = 0; j < gatewaysCount; j++) {
          
                endGate = clusterGraph.gateways[j];

                if (startGate == endGate)
                    cost = 0;
                else {
                    pathfindingAlgorithm.InitializePathfindingSearch(startGate.Localize(), endGate.Localize());
                    pathfindingAlgorithm.Search(out solution);
                    solution.CalculateLength();
                    cost = solution.Length;
                }
                GatewayDistanceTableEntry entry = ScriptableObject.CreateInstance<GatewayDistanceTableEntry>();
                entry.Initialize(startGate.Localize(), endGate.Localize(), cost);

                distanceTableRow.AddEntry(entry, j);
            }
            distanceTable[k] = distanceTableRow;
        }

        clusterGraph.gatewayDistanceTable = distanceTable;

        //create a new asset that will contain the ClusterGraph and save it to disk (DO NOT REMOVE THIS LINE)
        clusterGraph.SaveToAssetDatabase();
    }


    private static List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
    {
        //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
        //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
        //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
        //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
        //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
        //that's why we're using the type of the base class in the reflection call
        return (List<NavigationGraphNode>)Assets.Scripts.IAJ.Unity.Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
    }

    static int CompareObNames(GameObject x, GameObject y)
    {
        return x.name.CompareTo(y.name);
    }
}
