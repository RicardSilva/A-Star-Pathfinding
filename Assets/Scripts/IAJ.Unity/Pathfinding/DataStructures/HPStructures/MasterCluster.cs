using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures
{
    public class MasterCluster : ScriptableObject
    {
        public Vector3 center;
        public Vector3 min;
        public Vector3 max;
        public List<Cluster> clusters;

        public MasterCluster()
        {
            this.clusters = new List<Cluster>();
        }
        public MasterCluster(params Cluster[] clusters)
        {
            this.clusters = new List<Cluster>();
            this.min = new Vector3(int.MaxValue, 0, int.MaxValue);
            this.max = new Vector3(-int.MaxValue, 0, -int.MaxValue);

            for (int i = 0; i < clusters.Length; i++)
            {
                this.clusters.Add(clusters[i]);
                if (clusters[i].min.x < this.min.x) this.min.x = clusters[i].min.x;
                if (clusters[i].min.z < this.min.z) this.min.z = clusters[i].min.z;
                if (clusters[i].max.x > this.max.x) this.max.x = clusters[i].max.x;
                if (clusters[i].max.z > this.max.z) this.max.z = clusters[i].max.z;
            }
            this.center = new Vector3(this.max.x - this.min.x, 0, this.max.z - this.min.z);
        }
        public void Initialize(List<Cluster> clusters)
        {
            this.min = new Vector3(int.MaxValue, 0, int.MaxValue);
            this.max = new Vector3(-int.MaxValue, 0, -int.MaxValue);

            foreach(Cluster c in clusters) {
                this.clusters.Add(c);
                if (c.min.x < this.min.x) this.min.x = c.min.x;
                if (c.min.z < this.min.z) this.min.z = c.min.z;
                if (c.max.x > this.max.x) this.max.x = c.max.x;
                if (c.max.z > this.max.z) this.max.z = c.max.z;
            }
            this.center = new Vector3(this.max.x - this.min.x, 0, this.max.z - this.min.z);
        }

        public Vector3 Localize()
        {
            return this.center;
        }
        
    }
}

