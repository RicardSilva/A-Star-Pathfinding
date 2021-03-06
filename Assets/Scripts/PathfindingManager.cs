﻿using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using UnityEngine;
using RAIN.Navigation;
using RAIN.Navigation.NavMesh;
using RAIN.Navigation.Graph;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures;

public class PathfindingManager : MonoBehaviour {

	//public fields to be set in Unity Editor
	public GameObject endDebugSphere;
	public Camera camera;
    public GameObject characterAvatar;
    public GameObject DebugDestination;

	//private fields for internal use only
	private Vector3 startPosition;
	private Vector3 endPosition;
	private NavMeshPathGraph navMesh;
    
    private GlobalPath currentSolution;
    private GlobalPath currentSmoothedSolution;

    private DynamicCharacter character;
    private bool draw;

    //properties
    private AStarPathfinding AStarPathFinding { get;  set; }

	// Use this for initialization
	void Awake ()
	{
        
        this.character = new DynamicCharacter(characterAvatar);
        var clusterGraph =  Resources.Load<ClusterGraph>("ClusterGraph");
        this.draw = false;
        this.navMesh = NavigationManager.Instance.NavMeshGraphs[0];
        //this.AStarPathFinding = new AStarPathfinding(this.navMesh, new NodePriorityHeap(), new NodeHashTable(), new EuclidianHeuristic());
        //this.AStarPathFinding = new NodeArrayAStarPathFinding(this.navMesh, new EuclidianHeuristic());
        this.AStarPathFinding = new NodeArrayAStarPathFinding(this.navMesh, new GatewayHeuristic(clusterGraph));
        
        this.AStarPathFinding.NodesPerSearch = 100;
        
    }
	
	// Update is called once per frame
	void Update () 
    {
		Vector3 position;

		if (Input.GetMouseButtonDown(0)) 
		{
			//if there is a valid position
			if(this.MouseClickPosition(out position))
			{
				//we're setting the end point
				//this is just a small adjustment to better see the debug sphere
				this.endDebugSphere.transform.position = position + Vector3.up;
				this.endDebugSphere.SetActive(true);
				//this.currentClickNumber = 1;
				this.endPosition = position;
                //this.endPosition = this.DebugDestination.transform.position;
                this.draw = true;
                //initialize the search algorithm
                this.AStarPathFinding.InitializePathfindingSearch(this.character.KinematicData.position,this.endPosition);
			}
		}

        //call the pathfinding method if the user specified a new goal
	    if (this.AStarPathFinding.InProgress)
	    {
	        var finished = this.AStarPathFinding.Search(out this.currentSolution);
	        if (finished && this.currentSolution != null)
	        {
                //lets smooth out the Path
                this.startPosition = this.character.KinematicData.position;
                this.currentSmoothedSolution = StringPullingPathSmoothing.SmoothPath(this.character.KinematicData, this.currentSolution);
                this.currentSmoothedSolution.CalculateLocalPathsFromPathPositions(this.character.KinematicData.position);
                this.character.Movement = new DynamicFollowPath(this.character.KinematicData, this.currentSmoothedSolution);

            }
	    }

        this.character.Update();
	}

    public void OnGUI()
    {
        if (this.currentSolution != null)
        {
            var time = this.AStarPathFinding.TotalProcessingTime*1000;
            float timePerNode;
            if (this.AStarPathFinding.TotalProcessedNodes > 0)
            {
                timePerNode = time/this.AStarPathFinding.TotalProcessedNodes;
            }
            else
            {
                timePerNode = 0;
            }
            var text = "Nodes Visited: " + this.AStarPathFinding.TotalProcessedNodes
                       + "\nMaximum Open Size: " + this.AStarPathFinding.MaxOpenNodes
                       + "\nProcessing time (ms): " + time
                       + "\nTime per Node (ms):" + timePerNode;
            GUI.contentColor = Color.yellow;
            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            GUI.Label(new Rect(10, 10, 200, 100), text, style);
        }

        int w = Screen.width, h = Screen.height;

        GUIStyle style2 = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style2.alignment = TextAnchor.UpperRight;
        style2.fontSize = h * 2 / 100;
        style2.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        float msec = Time.deltaTime * 1000.0f;
        float fps = 1.0f / Time.deltaTime;
        string text2 = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text2, style2);
    }

    public void OnDrawGizmos()
    {
        if (this.draw)
        {
            //draw the current Solution Path if any (for debug purposes)
            if (this.currentSolution != null)
            {
                var previousPosition = this.startPosition;
                foreach (var pathPosition in this.currentSolution.PathPositions)
                {
                    Debug.DrawLine(previousPosition, pathPosition, Color.red);
                    previousPosition = pathPosition;
                }

                previousPosition = this.startPosition;
                foreach (var pathPosition in this.currentSmoothedSolution.PathPositions)
                {
                    Debug.DrawLine(previousPosition, pathPosition, Color.green);
                    previousPosition = pathPosition;
                }
            }

            //draw the nodes in Open and Closed Sets
            if (this.AStarPathFinding != null)
            {
                Gizmos.color = Color.cyan;

                if (this.AStarPathFinding.Open != null)
                {
                    foreach (var nodeRecord in this.AStarPathFinding.Open.All())
                    {
                        Gizmos.DrawSphere(nodeRecord.node.LocalPosition, 1.0f);
                    }
                }

                Gizmos.color = Color.blue;

                if (this.AStarPathFinding.Closed != null)
                {
                    foreach (var nodeRecord in this.AStarPathFinding.Closed.All())
                    {
                        Gizmos.DrawSphere(nodeRecord.node.LocalPosition, 1.0f);
                    }
                }
            }

            Gizmos.color = Color.black;
            //draw the target for the follow path movement
            if (this.character.Movement != null)
            {
                Gizmos.DrawSphere(this.character.Movement.Target.position, 5.0f);
            }
        }
    }

	private bool MouseClickPosition(out Vector3 position)
	{
		RaycastHit hit;

		var ray = this.camera.ScreenPointToRay (Input.mousePosition);
		//test intersection with objects in the scene
		if (Physics.Raycast (ray, out hit)) 
		{
			//if there is a collision, we will get the collision point
			position = hit.point;
			return true;
		}

		position = Vector3.zero;
		//if not the point is not valid
		return false;
	}
}
