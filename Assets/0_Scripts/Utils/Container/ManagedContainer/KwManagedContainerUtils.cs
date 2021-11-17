using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using System.Runtime.CompilerServices;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
namespace KaizerWaldCode.Utils
{
    public static class KwManagedContainerUtils
    {
        public static NativeArray<T> ToNativeArray<T>(this T[] array, in Allocator a = Allocator.TempJob , in NativeArrayOptions nao = NativeArrayOptions.UninitializedMemory) 
            where T : struct
        {
            NativeArray<T> nA = new NativeArray<T>(array.Length, a, nao);
            nA.CopyFrom(array);
            return nA;
        }
        
        public static unsafe NativeArray<T> toNativeArray<T>(T* ptr, int length) where T : unmanaged
        {
            NativeArray<T> arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, length, Allocator.Invalid);
            #if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
            #endif
            return arr;
        }
        
        /// <summary>
        /// Convert HashSet To Array
        /// </summary>
        /// <param name="hashSet"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] ToArray<T>(this HashSet<T> hashSet)
            where T : unmanaged
        {
            T[] arr = new T[hashSet.Count];
            hashSet.CopyTo(arr);
            return arr;
        }
        
        public static NativeArray<T> ToNativeArray<T>(this HashSet<T> hashSet)
            where T : unmanaged
        {
            T[] arr = new T[hashSet.Count];
            hashSet.CopyTo(arr);
            NativeArray<T> ntvAry = new NativeArray<T>(hashSet.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            ntvAry.CopyFrom(arr);
            return ntvAry;
        }
        
        public static T[] RemoveDuplicates<T>(this T[] s) 
            where T : struct
        {
            HashSet<T> set = new HashSet<T>(s);
            T[] result = new T[set.Count];
            set.CopyTo(result);
            return result;
        }
        
        public static U[] ReinterpretArray<T,U>(this T[] array) 
            where T : struct //from
            where U : struct //to
        {
            using NativeArray<T> temp = new NativeArray<T>(array.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            temp.CopyFrom(array);
            return temp.Reinterpret<U>().ToArray();
        }
        
        public static bool IsNullOrEmpty<T>(this T[] array)
            where T : struct
        {
            return array == null || array.Length == 0;
        }
        

 
        public static unsafe NativeArray<T> toNativeArray<T>(Span<T> span) where T : unmanaged
        {
            // assumes the GC is non-moving
            fixed(T* ptr = span)
                return toNativeArray(ptr, span.Length);
        }
 
        public static unsafe NativeArray<T> toNativeArray<T>(ReadOnlySpan<T> span) where T : unmanaged
        {
            // assumes the GC is non-moving
            fixed(T* ptr = span)
                return toNativeArray(ptr, span.Length);
        }
    }
}