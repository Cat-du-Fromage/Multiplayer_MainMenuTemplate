using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using System.Runtime.CompilerServices;

using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static KaizerWaldCode.Utils.KwManagedContainerUtils;

namespace KaizerWaldCode.Utils
{
    public static class NativeCollectionUtils
    {
        public static NativeArray<T> AllocNtvAry<T>(in int size, in Allocator a = Allocator.TempJob) 
            where T : struct
        {
            return new NativeArray<T>(size, a, NativeArrayOptions.UninitializedMemory);
        }
        
        public static NativeArray<T> AllocNtvAryOpt<T>(in int size, in NativeArrayOptions nao = NativeArrayOptions.UninitializedMemory) 
            where T : struct
        {
            return new NativeArray<T>(size, Allocator.TempJob, nao);
        }

        public static NativeArray<T> AllocFillNtvAry<T>(in int size, in T val) 
            where T : struct
        {
            NativeArray<T> a = new NativeArray<T>(size, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < size; i++) { a[i] = val; }
            return a;
        }
        
        public static void Fill<T>(this NativeArray<T> array, T val)
            where T : struct
        {
            for (int i = 0; i < array.Length; i++) { array[i] = val; }
        }
        
        public static int NumValueNotEqualTo<T>(this NativeArray<T> array, T val) 
            where T : struct
        {
            int n = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(val)) continue;
                n++;
            }
            return n;
        }
        
        public static NativeArray<T> RemoveDuplicates<T>(this NativeArray<T> s, Allocator a = Allocator.TempJob, NativeArrayOptions nao = NativeArrayOptions.UninitializedMemory) 
            where T : unmanaged
        {
            HashSet<T> set = new HashSet<T>(s.ToArray());
            NativeArray<T> result = new NativeArray<T>(set.Count, a, nao);
            result.CopyFrom(set.ToArray());
            return result;
        }

        /// <summary>
        /// Conditional Add used in parallel Job system
        /// </summary>
        /// <param name="list"></param>
        /// <param name="flag"></param>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddNoResizeIf<T>(this NativeList<T>.ParallelWriter list, bool flag, T obj)
            where T : unmanaged
        {
            if (flag) { list.AddNoResize(obj); }
        }

    }
}
