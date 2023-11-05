
using UnityEngine;

namespace Assets
{
    static class Utility
    {

        public static Vector3 Horizontal(Vector3 vector)
        {
            return new Vector3(vector.x, 0, vector.z);
        }

        public static Vector3 GetDirection(Vector3 point, Vector3 destination)
        {
            return (destination - point).normalized;
        }

    }
}
