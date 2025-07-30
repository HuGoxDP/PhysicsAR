using UnityEngine;

namespace HuGox.Utils
{
    public static class Mouse3D
    {
        public static Vector3? GetMouseWorldPosition()
        {
            return GetMouseWorldPosition(Input.mousePosition, Camera.main);
        }

        public static Vector3? GetMouseWorldPosition(Vector3 screenPosition, Camera worldCamera)
        {
            Ray ray = worldCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out var hit))
            {
                Debug.DrawLine(hit.point, hit.point + new Vector3(0.1f, 0, 0.1f));
                return hit.point;
            }

            Debug.LogWarning("Ray doesnt make hit");
            return null;
        }
    }
}