using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nanolod
{

    public static class CollectionExtensions
    {

        /// <summary>
        /// Adds an item at the end of a given array, resizing the array to fit it.
        /// </summary>
        /// <typeparam name="T">Array element type</typeparam>
        /// <param name="array">Array</param>
        /// <param name="item">Item to append</param>
        public static void Append<T>(ref T[] array, T item)
        {
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = item;
        }

        /// <summary>
        /// Adds items at the end of a given array, resizing the array to fit it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array1">Array</param>
        /// <param name="array2">Array to append</param>
        public static void Append<T>(ref T[] array1, T[] array2)
        {
            if (array2.Length == 0)
            {
                return;
            }

            int array1OriginalLength = array1.Length;
            Array.Resize<T>(ref array1, array1OriginalLength + array2.Length);
            Array.Copy(array2, 0, array1, array1OriginalLength, array2.Length);
        }

        /// <summary>
        /// Remove array element at given index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public static void RemoveAt<T>(ref T[] array, int index)
        {
            if (index < 0 || index > array.Length - 1)
            {
                return;
            }

            if (index > 0)
            {
                Array.Copy(array, 0, array, 0, index);
            }

            if (index < array.Length - 1)
            {
                Array.Copy(array, index + 1, array, index, array.Length - index - 1);
            }

            Array.Resize(ref array, array.Length - 1);
        }

        /// <summary>
        /// Insert element in array at given index. Previous element at index is moved to index + 1.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public static void Insert<T>(ref T[] array, T item, int index)
        {
            if (index < 0 || index > array.Length - 1)
            {
                return;
            }

            Array.Resize(ref array, array.Length + 1);
            Array.Copy(array, index, array, index + 1, array.Length - index - 1);
            array[index] = item;
        }

        /// <summary>
        /// Get the array slice between the two indexes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            // Handles negative ends.
            if (end < 0)
            {
                end = source.Length + end;
            }
            int len = end - start;

            // Return new array.
            T[] res = new T[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }

        /// <summary>
        /// Create an array by concatenating several arrays one after another.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrays">Source arrays</param>
        /// <returns></returns>
        public static T[] CreateArrayFromArrays<T>(params T[][] arrays)
        {
            T[] array = new T[arrays.Sum(x => x.Length)];
            int pos = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                Array.Copy(arrays[i], 0, array, pos, arrays[i].Length);
                pos += arrays[i].Length;
            }
            return array;
        }

        /// <summary>
        /// Resizes a base Array.
        /// Difference with the Array.Resize is that this method works also on Array base class without any casting.
        /// </summary>
        /// <param name="oldArray"></param>
        /// <param name="newSize"></param>
        /// <returns></returns>
        public static Array Resize(this Array oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            Type elementType = oldArray.GetType().GetElementType();
            Array newArray = Array.CreateInstance(elementType, newSize);
            int preserveLength = Math.Min(oldSize, newSize);
            if (preserveLength > 0)
            {
                Array.Copy(oldArray, newArray, preserveLength);
            }

            return newArray;
        }

        /// <summary>
        /// Transforms an IEnumerator to an IEnumerable to given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return (T)enumerator.Current;
            }
        }
    }
}