using UnityEngine;
using System.Collections;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicArrive : DynamicVelocityMatch
    {

        public override string Name
        {
            get { return "DynamicArrive"; }
        }

        public KinematicData ArriveTarget { get; set; }
        public float MaxSpeed { get; set; }
        public float StopRadius { get; set; }
        public float SlowRadius { get; set; }

        public DynamicArrive()
        {
            this.Target = new KinematicData();
        }

        public override MovementOutput GetMovement()
        {

            var direction = this.ArriveTarget.position - this.Character.position;
            var distance = direction.magnitude;
            float targetSpeed;

            if (distance < this.StopRadius) { 
                targetSpeed = 0;
            this.Character.velocity = Vector3.zero; }
            else if (distance < this.SlowRadius)
                targetSpeed = this.MaxSpeed * (distance / this.SlowRadius);
            else
                targetSpeed = this.MaxSpeed;

            this.Target.velocity = direction.normalized * targetSpeed;
            this.Target.position = this.ArriveTarget.position;
            return base.GetMovement();
        }
    }
}
