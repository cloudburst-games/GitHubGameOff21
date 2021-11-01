// Math utils: useful mathematical methods
using Godot;
using System;

namespace BaseProject.Utils
{
    public static class Math
    {
        
        // TODO - figure out HOW THIS WORKS!!! taken from godot Qs
        // https://godotengine.org/qa/5770/how-to-lerp-between-two-angles
        public static float LerpAngle(float a, float b, float t)
        {
            if ((float)System.Math.Abs (a - b) >= (float)System.Math.PI) {
                if (a > b)
                    a = (float)NormaliseAngle (a) - 2.0f * (float)System.Math.PI;
                else
                    b = (float)NormaliseAngle (b) - 2.0f * (float)System.Math.PI;
            }
            return (Lerp (a, b, t));


        }

        // how does this work? what is posmod?
        public static float NormaliseAngle(float x)
        {
            return (float)Mathf.PosMod (x + (float)System.Math.PI, 2.0f * (float)System.Math.PI) - (float)System.Math.PI;
        }


        public static float Lerp(float p1, float p2, float fraction)
        {
            return p1 + (p2 - p1) * fraction;
        }

        public static Vector2 Lerp(Vector2 p1, Vector2 p2, float fraction)
        {
            return p1 + (p2 - p1) * fraction;
        }

        public static Vector2 PerpendicularClockwise(Vector2 vec)
        {
            return new Vector2(-vec.y, vec.x);
        }

        public static Vector2 PerpendicularCounterClockwise(Vector2 vec)
        {
            return new Vector2(vec.y, -vec.x);
        }

    }
}