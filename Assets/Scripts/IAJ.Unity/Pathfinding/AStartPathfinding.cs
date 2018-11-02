using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class AStarPathfinding
    {
        public NavMeshPathGraph NavMeshGraph { get; protected set; }
        //how many nodes do we process on each call to the search method
        public uint NodesPerSearch { get; set; }

        public uint TotalProcessedNodes { get; protected set; }
        public int MaxOpenNodes { get; protected set; }
        public float TotalProcessingTime { get; protected set; }
        public float StartTime { get; protected set; }
        public bool InProgress { get; protected set; }

        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }

        public NavigationGraphNode GoalNode { get; protected set; }
        public NavigationGraphNode StartNode { get; protected set; }
        public Vector3 StartPosition { get; protected set; }
        public Vector3 GoalPosition { get; protected set; }

        //heuristic function
        public IHeuristic Heuristic { get; protected set; }

        public AStarPathfinding(NavMeshPathGraph graph, IOpenSet open, IClosedSet closed, IHeuristic heuristic)
        {
            this.NavMeshGraph = graph;
            this.Open = open;
            this.Closed = closed;
            this.NodesPerSearch = uint.MaxValue; //by default we process all nodes in a single request
            this.InProgress = false;
            this.Heuristic = heuristic;
        }

        public void InitializePathfindingSearch(Vector3 startPosition, Vector3 goalPosition)
        {
            this.StartPosition = startPosition;
            this.GoalPosition = goalPosition;
            this.StartNode = this.Quantize(this.StartPosition);
            this.GoalNode = this.Quantize(this.GoalPosition);

            //if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
            if (this.StartNode == null || this.GoalNode == null) return;

            //I need to do this because in Recast NavMesh graph, the edges of polygons are considered to be nodes and not the connections.
            //Theoretically the Quantize method should then return the appropriate edge, but instead it returns a polygon
            //Therefore, we need to create one explicit connection between the polygon and each edge of the corresponding polygon for the search algorithm to work
            ((NavMeshPoly)this.StartNode).AddConnectedPoly(this.StartPosition);
            ((NavMeshPoly)this.GoalNode).AddConnectedPoly(this.GoalPosition);

            this.InProgress = true;
            this.TotalProcessedNodes = 0;
            this.TotalProcessingTime = 0.0f;
            this.MaxOpenNodes = 0;

            var initialNode = new NodeRecord
            {
                gValue = 0,
                hValue = this.Heuristic.H(this.StartNode, this.GoalNode),
                node = this.StartNode
            };

            initialNode.fValue = AStarPathfinding.F(initialNode);

            this.Open.Initialize();
            this.Open.AddToOpen(initialNode);
            this.Closed.Initialize();
            this.StartTime = Time.realtimeSinceStartup;
        }

        protected virtual void ProcessChildNode(NodeRecord bestNode, NavigationGraphEdge connectionEdge)
        {
            //this is where you process a child node 
            var childNode = GenerateChildNodeRecord(bestNode, connectionEdge);

            // CHECK IF CHILD IS IN OPEN
            NodeRecord existingNode;
            if ((existingNode = this.Open.SearchInOpen(childNode)) != null)
            {
                if (existingNode.fValue > childNode.fValue)
                    this.Open.Replace(existingNode, childNode);
            }
            // CHECK IF CHILD IS IN CLOSED
            else if ((existingNode = this.Closed.SearchInClosed(childNode)) != null)
            {
                if (existingNode.fValue > childNode.fValue)
                {
                    this.Closed.RemoveFromClosed(existingNode);
                    this.Open.AddToOpen(childNode);
                }
            }
            // NOT IN OPEN OR CLOSED
            else
            {
                this.Open.AddToOpen(childNode);
            }
        }

        //this method should return true if the Search process finished (i.e either because it found a solution or because there was no solution
        //it should return false if the search process didn't finish yet
        //when returning true, the solution parameter should be set to null if there is no solution. Otherwise it must contain the found solution
        //if the parameter returnPartialSolution is true, then the user wants to have a partial path to the best node so far even when the search has not finished searching
        public virtual bool Search(out GlobalPath solution, bool returnPartialSolution = false)
        {
            var startTime = Time.realtimeSinceStartup;
            NodeRecord bestNode;
            uint nodes = this.NodesPerSearch;
            for (; nodes > 0; nodes--)
            {
                if (this.Open.CountOpen() == 0)
                {
                    this.CleanUp();
                    solution = null;
                    this.InProgress = false;
                    if (returnPartialSolution)
                        this.TotalProcessingTime = Time.realtimeSinceStartup - this.StartTime;
                    else
                        this.TotalProcessingTime = Time.realtimeSinceStartup - startTime;

                    return true;
                }
                bestNode = this.Open.GetBestAndRemove();
                if (bestNode.node.Equals(this.GoalNode))
                {
                    this.CleanUp();
                    solution = this.CalculateSolution(bestNode, returnPartialSolution);
                    this.InProgress = false;
                    if (returnPartialSolution)
                        this.TotalProcessingTime = Time.realtimeSinceStartup - this.StartTime;
                    else
                        this.TotalProcessingTime = Time.realtimeSinceStartup - startTime;
                    return true;
                }
                this.Open.RemoveFromOpen(bestNode);
                this.Closed.AddToClosed(bestNode);
                this.TotalProcessedNodes++;

                var outConnections = bestNode.node.OutEdgeCount;
                for (int i = 0; i < outConnections; i++)
                {
                    this.ProcessChildNode(bestNode, bestNode.node.EdgeOut(i));
                }

                if (this.Open.CountOpen() > this.MaxOpenNodes)
                    this.MaxOpenNodes = this.Open.CountOpen();

            }

            if (returnPartialSolution == true)
            {
                solution = this.CalculateSolution(this.Open.PeekBest(), returnPartialSolution);
                return true;
            }

            solution = null;
            return false;
        }

        protected NavigationGraphNode Quantize(Vector3 position)
        {
            return this.NavMeshGraph.QuantizeToNode(position, 1.0f);
        }

        protected void CleanUp()
        {
            //I need to remove the connections created in the initialization process
            if (this.StartNode != null)
            {
                ((NavMeshPoly)this.StartNode).RemoveConnectedPoly();
            }

            if (this.GoalNode != null)
            {
                ((NavMeshPoly)this.GoalNode).RemoveConnectedPoly();
            }
        }

        protected virtual NodeRecord GenerateChildNodeRecord(NodeRecord parent, NavigationGraphEdge connectionEdge)
        {
            var childNode = connectionEdge.ToNode;
            var childNodeRecord = new NodeRecord
            {
                node = childNode,
                parent = parent,
                gValue = parent.gValue + connectionEdge.Cost,
                hValue = this.Heuristic.H(childNode, this.GoalNode)
            };

            childNodeRecord.fValue = F(childNodeRecord);

            return childNodeRecord;
        }

        protected GlobalPath CalculateSolution(NodeRecord node, bool partial)
        {
            var path = new GlobalPath
            {
                IsPartial = partial
            };
            var currentNode = node;

            path.PathPositions.Add(this.GoalPosition);

            //I need to remove the first Node and the last Node because they correspond to the dummy first and last Polygons that were created by the initialization.
            //And for instance I don't want to be forced to go to the center of the initial polygon before starting to move towards my destination.

            //skip the last node, but only if the solution is not partial (if the solution is partial, the last node does not correspond to the dummy goal polygon)
            if (!partial && currentNode.parent != null)
            {
                currentNode = currentNode.parent;
            }

            while (currentNode.parent != null)
            {
                path.PathNodes.Add(currentNode.node); //we need to reverse the list because this operator add elements to the end of the list
                path.PathPositions.Add(currentNode.node.LocalPosition);

                if (currentNode.parent.parent == null) break; //this skips the first node
                currentNode = currentNode.parent;
            }

            path.PathNodes.Reverse();
            path.PathPositions.Reverse();
            return path;
        }

        public static float F(NodeRecord node)
        {
            return F(node.gValue, node.hValue);
        }

        public static float F(float g, float h)
        {
            return g + 1.01f * h;
        }

    }
}
