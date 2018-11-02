using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicVelocityMatch : DynamicMovement
    {
        public override string Name
        {
            get { return "VelocityMatch"; }
        }

        public override KinematicData Target { get; set; }

        public float TimeToTargetSpeed { get; set; }
        

        public DynamicVelocityMatch()
        {
           
        }
        public override MovementOutput GetMovement()
        {
            var output = new MovementOutput();
            output.linear = (this.Target.velocity - this.Character.velocity)/this.TimeToTargetSpeed;

            if (output.linear.sqrMagnitude > this.MaxAcceleration * this.MaxAcceleration)
                output.linear = output.linear.normalized * this.MaxAcceleration;
             
            output.angular = 0;
            return output;
        }
    }
}
