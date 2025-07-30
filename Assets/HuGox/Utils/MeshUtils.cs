using System;
using UnityEngine;

namespace HuGox.Utils
{
    public static class MeshUtils
    {
        private static Quaternion[] _cachedQuaternionEulerXZArr;

        private static void CacheQuaternionEulerXZ()
        {
            if (_cachedQuaternionEulerXZArr != null) return;
            _cachedQuaternionEulerXZArr = new Quaternion[360];
            for (int i = 0; i < 360; i++)
            {
                _cachedQuaternionEulerXZArr[i] = Quaternion.Euler(0, i, 0);
            }
        }

        private static Quaternion GetQuaternionEulerXZ(float rotationValue)
        {
            int rotation = Mathf.RoundToInt(rotationValue);
            rotation = rotation % 360;
            if (rotation < 0) rotation += 360;

            if (_cachedQuaternionEulerXZArr == null) CacheQuaternionEulerXZ();
            return _cachedQuaternionEulerXZArr[rotation];
        }

        public static Mesh CreateEmptyMesh()
        {
            Mesh mesh = new Mesh
            {
                vertices = Array.Empty<Vector3>(),
                uv = Array.Empty<Vector2>(),
                triangles = Array.Empty<int>()
            };
            return mesh;
        }

        public static void CreateEmptyMeshArrays(int quadCounts, out Vector3[] vertices, out Vector2[] uv,
            out int[] triangles)
        {
            vertices = new Vector3[4 * quadCounts];
            uv = new Vector2[4 * quadCounts];
            triangles = new int[6 * quadCounts];
        }

        public static void AddToMeshArraysXZ(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 pos,
            float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11)
        {
            //Relocate vertices
            int vIndex = index * 4;
            int vIndex0 = vIndex;
            int vIndex1 = vIndex + 1;
            int vIndex2 = vIndex + 2;
            int vIndex3 = vIndex + 3;

            baseSize *= .5f;

            bool skewed = baseSize.x != baseSize.z;
            if (skewed)
            {
                vertices[vIndex0] = pos + GetQuaternionEulerXZ(rot) * new Vector3(-baseSize.x, 0, baseSize.z);
                vertices[vIndex1] = pos + GetQuaternionEulerXZ(rot) * new Vector3(-baseSize.x, 0, -baseSize.z);
                vertices[vIndex2] = pos + GetQuaternionEulerXZ(rot) * new Vector3(baseSize.x, 0, -baseSize.z);
                vertices[vIndex3] = pos + GetQuaternionEulerXZ(rot) * baseSize;
            }
            else
            {
                vertices[vIndex0] = pos + GetQuaternionEulerXZ(rot - 270) * baseSize;
                vertices[vIndex1] = pos + GetQuaternionEulerXZ(rot - 180) * baseSize;
                vertices[vIndex2] = pos + GetQuaternionEulerXZ(rot - 90) * baseSize;
                vertices[vIndex3] = pos + GetQuaternionEulerXZ(rot - 0) * baseSize;
            }

            //Relocate UVs
            uvs[vIndex0] = new Vector2(uv00.x, uv11.y);
            uvs[vIndex1] = new Vector2(uv00.x, uv00.y);
            uvs[vIndex2] = new Vector2(uv11.x, uv00.y);
            uvs[vIndex3] = new Vector2(uv11.x, uv11.y);

            //Create triangles
            int tIndex = index * 6;

            triangles[tIndex + 0] = vIndex0;
            triangles[tIndex + 1] = vIndex3;
            triangles[tIndex + 2] = vIndex1;

            triangles[tIndex + 3] = vIndex1;
            triangles[tIndex + 4] = vIndex3;
            triangles[tIndex + 5] = vIndex2;
        }
    }
}