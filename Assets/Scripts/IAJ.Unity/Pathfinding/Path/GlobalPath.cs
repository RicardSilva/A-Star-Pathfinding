using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Utils;
using RAIN.Navigation.Graph;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Path
{
    public class GlobalPath : Path
    {
        public List<NavigationGraphNode> PathNodes { get; protected set; }
        public List<Vector3> PathPositions { get; protected set; } 
        public bool IsPartial { get; set; }
        public float Length { get; set; }
        public List<LocalPath> LocalPaths { get; protected set; } 


        public GlobalPath()
        {
            this.PathNodes = new List<NavigationGraphNode>();
            this.PathPositions = new List<Vector3>();
            this.LocalPaths = new List<LocalPath>();
        }

        public void CalculateLocalPathsFromPathPositions(Vector3 initialPosition)
        {
            Vector3 previousPosition = initialPosition;
            for (int i = 0; i < this.PathPositions.Count; i++)
            {

                if (!previousPosition.Equals(this.PathPositions[i]))
                {
                    this.LocalPaths.Add(new LineSegmentPath(previousPosition, this.PathPositions[i]));
                    previousPosition = this.PathPositions[i];
                }
            }
        }

        public void CalculateLength()
        {
            this.Length = 0;
            for (int i = 0; i < this.PathPositions.Count - 1; i++)
            {
                this.Length += Vector3.Distance(this.PathPositions[i], this.PathPositions[i+1]); 
            }
        }

        //PARAM IS A FLOAT NUMBER WHERE THE INTEGER PART REPRESENTS THE LOCAL PATH AND THE FLOAT PART REPRESENTS THE PARAM IN THAT LINE
        public override float GetParam(Vector3 position, float previousParam)
        {
            if (previousParam > this.LocalPaths.Count - 1) previousParam = this.LocalPaths.Count - 0.1f;
            int path = (int)previousParam;
            return this.LocalPaths[path].GetParam(position, previousParam - path) + path;
        }

        public override Vector3 GetPosition(float param)
        {
            if (param > this.LocalPaths.Count - 1) param = this.LocalPaths.Count - 0.1f;
            int path = (int)param;
            return this.LocalPaths[path].GetPosition(param - path);
        }

        public override bool PathEnd(float param)
        {
            int path = (int)param;
            if (path > (this.LocalPaths.Count - 1)) return true;
            if (path == (this.LocalPaths.Count - 1) && this.LocalPaths[this.LocalPaths.Count - 1].PathEnd(param - path)) return true;
            return false;
        }

        
    }
}
