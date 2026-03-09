using System;
using UnityEngine;

namespace Fictology.Util
{
    public class Utils
    {
        public static float ClampPeriod(float value, float min, float max)
        {
            return value % max - min == 0 && value / max <= 1 ? value :
                (float) (value - max * Mathf.Floor(value / max));
        }

        public static float SinePeriod(float time, float period, float min, float max) {
            var amplitude = (max - min) / 2.0f;
            var midpoint = (min + max) / 2.0f;
            var phase = time / period * 2.0f * Mathf.PI;
            var sineValue = Mathf.Sin(phase);
            return midpoint + amplitude * sineValue;
        }
        
        public static float TriangularPeriod(float time, float min, float max) {
            var period = max - min;
            var mod = time % (period * 2);
            return mod <= period ? min + mod : max - (mod - period);
        }

        public static Vector3 EquiangularSpiral(float angleDeg)
        {
            var theta = Mathf.Deg2Rad * angleDeg;
            return new Vector3(
                Mathf.Pow((float)Math.E, theta) * Mathf.Cos(theta),
                Mathf.Pow((float)Math.E, theta) * Mathf.Sin(theta), 
                0);
        }
    }
}