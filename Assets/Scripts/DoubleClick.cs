using UnityEngine;

public static class DoubleClick
{
    static float lastClickTime;
    const float doubleClickDelay = 0.2f;

    public static bool IsDoubleClick()
    {
        float time = Time.time;

        if (time - lastClickTime <= doubleClickDelay)
        {
            lastClickTime = -1f;
            return true;
        }

        lastClickTime = time;
        return false;
    }
}