using UnityEngine;

public static class CursorManager
{
    public static void TravarCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void LiberarCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void AlternarCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            LiberarCursor();
        }
        else
        {
            TravarCursor();
        }
    }

    public static void LiberarCursorLimitado()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
}
