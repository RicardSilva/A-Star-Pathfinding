using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures
{
    public class Cluster : ScriptableObject
    {
        public Vector3 center;
        public Vector3 min;
        public Vector3 max;
        public List<Gateway> gateways;

        public Cluster()
        {
            this.gateways = new List<Gateway>();
        }

        public void Initialize(GameObject clusterObject)
        {
            this.center = clusterObject.transform.position;
            //clusters have a size of 10 multipled by the scale
            this.min = new Vector3(this.center.x - clusterObject.transform.localScale.x * 10/2 - 1, 0, this.center.z - clusterObject.transform.localScale.z * 10/2 - 1);
            this.max = new Vector3(this.center.x + clusterObject.transform.localScale.x * 10/2 + 1, 0, this.center.z + clusterObject.transform.localScale.z * 10/2 + 1);
        }

        public Vector3 Localize()
        {
            return this.center;
        }

        public static bool operator ==(Cluster c1, Cluster c2)
        {
            if (object.ReferenceEquals(c1, null))
            {
                return object.ReferenceEquals(c2, null);
            }

            if (object.ReferenceEquals(c2, null))
            {
                return object.ReferenceEquals(c1, null);
            }

            return (c1.center == c2.center && c1.min == c2.min && c1.max == c2.max);
        }

        public static bool operator !=(Cluster c1, Cluster c2)
        {
            return !(c1 == c2);
        }
    }
}
