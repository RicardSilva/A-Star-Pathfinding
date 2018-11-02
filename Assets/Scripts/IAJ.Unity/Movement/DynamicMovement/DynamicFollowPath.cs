using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicFollowPath : DynamicArrive
    {
        public GlobalPath Path { get; set; }
        public float PathOffset { get; set; }

        public float CurrentParam { get; set; }

        private MovementOutput EmptyMovementOutput { get; set; }


        public DynamicFollowPath(KinematicData character, GlobalPath path) 
        {
            this.Target = new KinematicData();
            this.ArriveTarget = new KinematicData();
            this.Character = character;
            this.Path = path;
            this.EmptyMovementOutput = new MovementOutput();
            this.MaxAcceleration = 150.0f;
            this.MaxSpeed = 55.0f;
            this.StopRadius = 0f;
            this.SlowRadius = 1.0f;
            this.TimeToTargetSpeed = 0.01f;
            this.PathOffset = 0.7f;
            this.CurrentParam = 0.0f;
        }

        public override MovementOutput GetMovement()
        {        
            this.CurrentParam = this.Path.GetParam(this.Character.position, this.CurrentParam);
            float targetParam = this.CurrentParam + this.PathOffset;
           
            Vector3 position = this.Path.GetPosition(targetParam);
            this.ArriveTarget.position = this.Path.GetPosition(targetParam);
            if (this.Path.PathEnd(targetParam)) this.StopRadius = 1.0f; //hack to get a good stop in the end without getting stuck in realy small local paths

            return base.GetMovement();

        }
    }
}
