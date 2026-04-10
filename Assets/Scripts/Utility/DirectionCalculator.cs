using UnityEngine;
public enum Direction
{
    Up,
    Left,
    Right,
    Down,
    Forward
}

public static class DirectionCalculator
{
    /// <summary>
    /// Convert a Vector3 direction into a simplified enum direction.
    /// </summary>
    public static Direction GetDirection(Vector3 dir)
    {
        // Prevent zero vector issues.
        if (dir == Vector3.zero)
            return Direction.Forward;

        dir.Normalize();

        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);
        float absZ = Mathf.Abs(dir.z);

        // Determine dominant axis.
        if (absY > absX && absY > absZ)
        {
            return dir.y > 0 ? Direction.Up : Direction.Down;
        }

        if (absX > absZ)
        {
            return dir.x > 0 ? Direction.Right : Direction.Left;
        }

        // Default to forward/backward mapped as Forward.
        return Direction.Forward;
    }
}