using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System
{
    //internal static class Test
    //{
        

    //    internal static extern int CurrentProcessorNumber
    //    {
    //        [MethodImplAttribute(MethodImplOptions.InternalCall)]
    //        get;
    //    }

    //    // The upper bits of t_executionIdCache are the executionId. The lower bits of
    //    // the t_executionIdCache are counting down to get it periodically refreshed.
    //    // TODO: Consider flushing the executionIdCache on Wait operations or similar 
    //    // actions that are likely to result in changing the executing core
    //    [ThreadStatic]
    //    private static int t_executionIdCache;

    //    private const int ExecutionIdCacheShift = 16;
    //    private const int ExecutionIdCacheCountDownMask = (1 << ExecutionIdCacheShift) - 1;
    //    private const int ExecutionIdRefreshRate = 5000;

        
    //    private static int RefreshExecutionId()
    //    {
    //        int executionId = CurrentProcessorNumber;

    //        Debug.Assert(ExecutionIdRefreshRate <= ExecutionIdCacheCountDownMask);

    //        // Mask with Int32.MaxValue to ensure the execution Id is not negative
    //        t_executionIdCache = ((executionId << ExecutionIdCacheShift) & Int32.MaxValue) | ExecutionIdRefreshRate;

    //        return executionId;
    //    }

    //    // Cached processor number used as a hint for which per-core stack to access. It is periodically
    //    // refreshed to trail the actual thread core affinity.
    //    internal static int CurrentExecutionId
    //    {
    //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //        get
    //        {
    //            int executionIdCache = t_executionIdCache--;
    //            if ((executionIdCache & ExecutionIdCacheCountDownMask) == 0)
    //                return RefreshExecutionId();
    //            return (executionIdCache >> ExecutionIdCacheShift);
    //        }
    //    }
    //}
}
