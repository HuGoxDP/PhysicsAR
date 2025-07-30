using UnityEngine;

namespace HuGox.Utils
{
    public static class ScreenBounds
    {
        private static Camera _cam;

        public static Vector2 GetScreenBounds(Vector2 viewportPosition)
        {
            _cam ??= Camera.main;
            return _cam.ViewportToWorldPoint(new Vector3(viewportPosition.x, viewportPosition.y, _cam.nearClipPlane));
        }

        public static Vector2 GetRandomEdgePosition()
        {
            float rand = Random.value;
            if (rand < 0.25f) return GetScreenBounds(new Vector2(0, Random.value));
            if (rand < 0.5f) return GetScreenBounds(new Vector2(1, Random.value));
            if (rand < 0.75f) return GetScreenBounds(new Vector2(Random.value, 0));
            return GetScreenBounds(new Vector2(Random.value, 1));
        }

        public static Quaternion GetRotationTowardsCenter(Vector2 spawnPosition)
        {
            _cam ??= Camera.main;

            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Vector3 worldCenter = _cam.ScreenToWorldPoint(screenCenter);
            Vector2 direction = worldCenter - (Vector3)spawnPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, 0, angle - 90);
        }
    }
}