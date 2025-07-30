using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// ReSharper disable RedundantAssignment

namespace HuGox.Utils
{
    public static class Extensions
    {
        public static T GetComponentOrAdd<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }

        public static T GetComponentOrAdd<T>(this MonoBehaviour monoBehaviour) where T : Component
        {
            var gameObject = monoBehaviour.gameObject;
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(list[i], item))
                    return i;
            }

            return -1;
        }

        public static Color HexadecimalToRGB(this Color color, string hex)
        {
            color = Color.white;

            if (hex.Length != 6)
            {
                return color;
            }

            var hexRed = int.Parse(
                hex[0].ToString() + hex[1].ToString(),
                NumberStyles.HexNumber
            );

            var hexGreen = int.Parse(
                hex[2].ToString() + hex[3].ToString(),
                NumberStyles.HexNumber
            );

            var hexBlue = int.Parse(
                hex[4].ToString() + hex[5].ToString(),
                NumberStyles.HexNumber
            );

            color = new Color(hexRed / 255f, hexGreen / 255f, hexBlue / 255f);
            return color;
        }


        public static T Next<T>(this T src) where T : Enum
        {
            // Получаем массив всех значений данного enum
            T[] values = (T[])Enum.GetValues(src.GetType());

            // Находим текущий индекс элемента
            int index = Array.IndexOf(values, src) + 1;

            // Если это последний элемент, возвращаем первый, иначе следующий
            return (index == values.Length) ? values[0] : values[index];
        }

        public static T Previous<T>(this T src) where T : Enum
        {
            // Получаем массив всех значений данного enum
            T[] values = (T[])Enum.GetValues(src.GetType());

            // Находим текущий индекс элемента
            int index = Array.IndexOf(values, src) - 1;

            // Если это последний элемент, возвращаем первый, иначе следующий
            return (index < 0) ? values[values.Length - 1] : values[index];
        }
    }
}