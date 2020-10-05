using UnityEngine;


public static class HelpClass
{
    public static Vector2 NormalizeForce2D(Vector2 speed, Vector2 normal)
    {
        Vector2 projection;
        if (Vector2.Dot(speed, normal) > 0)
        {
            return projection = 0 * normal;
        }
        projection = Vector2.Dot(speed, normal) * normal;
        return -projection;
    }

    /// <summary>
    /// Calculates the normal force of a velocity and the normal of the collision.
    /// </summary>
    /// <param name="speed"> A velocity Vector3</param>
    /// <param name="normal"> The normal of the surface the Velocity will collide with</param>
    /// <returns></returns>
    public static Vector3 NormalizeForce(Vector3 speed, Vector3 normal)
    {
        Vector3 projection;
        if (Vector3.Dot(speed, normal) > 0)
        {
            return projection = 0 * normal;
        }
        projection = Vector3.Dot(speed, normal) * normal;
        return -projection;
    }


}