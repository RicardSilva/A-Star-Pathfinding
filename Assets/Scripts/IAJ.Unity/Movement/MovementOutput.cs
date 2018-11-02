using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement
{
    public class MovementOutput
    {
        public Vector3 linear;
        public float angular;

        public MovementOutput()
        {
            this.Clear();
        }

        public void Clear()
        {
            this.linear = Vector3.zero;
            this.angular = 0;
        }

        public float SquareMagnitude()
        {
            return this.linear.sqrMagnitude + this.angular * this.angular;
        }


        public float Magnitude()
        {
            return (float)Math.Sqrt(this.SquareMagnitude());
        }

        public static bool operator ==(MovementOutput s1, MovementOutput s2)
        {
            if (object.ReferenceEquals(s1, null))
            {
                return object.ReferenceEquals(s2, null);
            }

            if (object.ReferenceEquals(s2, null))
            {
                return object.ReferenceEquals(s1, null);
            }

            return s1.linear == s2.linear && s1.angular == s2.angular;
        }

        public static bool operator !=(MovementOutput s1, MovementOutput s2)
        {
            if (object.ReferenceEquals(s1, null))
            {
                return !object.ReferenceEquals(s2, null);
            }

            if (object.ReferenceEquals(s2, null))
            {
                return !object.ReferenceEquals(s1, null);
            }

            return s1.linear != s2.linear || s1.angular != s2.angular;
        }

    }
}

