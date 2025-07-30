using UnityEngine;

namespace HuGox.Utils
{
    public static class UtilsClass
    {
        public const int SortingOrderDefault = 5000;

        public static Vector3 GetMouseWorldPosition()
        {
            Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
            vec.z = 0f;
            return vec;
        }

        public static Vector3 GetMouseWorldPositionWithZ()
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        }

        public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
        }

        public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
        {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }


        public static Vector3 DirectionToMouse(Vector3 from)
        {
            return DirectionTo(from, GetMouseWorldPositionWithZ());
        }

        public static Vector3 DirectionToMouse(Vector3 from, Camera camera)
        {
            return DirectionTo(from, GetMouseWorldPositionWithZ(camera));
        }

        public static Vector3 DirectionTo(Vector3 from, Vector3 target)
        {
            var heading = target - from;
            var distance = heading.magnitude;
            var direction = heading / distance;
            return direction;
        }

        public static TextMesh CreateWorldText(string text, Transform parent = null,
            Vector3 localPosition = default(Vector3), Quaternion localRotation = default(Quaternion), int fontSize = 40,
            Color? color = null, TextAnchor textAnchor = TextAnchor.MiddleCenter,
            TextAlignment textAlignment = TextAlignment.Center, int sortingOrder = SortingOrderDefault)
        {
            if (color == null) color = Color.white;
            return CreateWorldText(
                parent,
                text,
                localPosition,
                localRotation,
                fontSize,
                (Color)color,
                textAnchor,
                textAlignment,
                sortingOrder
            );
        }

        public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition,
            Quaternion localRotation, int fontSize,
            Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("WorldText", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent);
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            return textMesh;
        }

        public static Collider CreateWorldBoxCollision(Vector3 localPosition, Transform parent = null,
            Vector3 size = default(Vector3),
            bool isTrigger = false, bool isProvidesContacts = false)
        {
            return CreateWorldBoxCollision(parent, localPosition, size, isTrigger, isProvidesContacts);
        }

        public static BoxCollider CreateWorldBoxCollision(Transform parent, Vector3 localPosition, Vector3 size,
            bool isTrigger, bool isProvidesContacts)
        {
            GameObject gameObject = new GameObject("WorldCollision", typeof(BoxCollider));
            Transform transform = gameObject.transform;
            transform.SetParent(parent);
            transform.localPosition = localPosition;
            BoxCollider collider = gameObject.GetComponent<BoxCollider>();
            collider.size = size;
            collider.center = size * 0.5f;
            collider.isTrigger = isTrigger;
            collider.providesContacts = isProvidesContacts;
            return collider;
        }
    }
}