using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ImprovedTimers {
    /// <summary>
    /// Manages timers using Unity Job System with SOA (Structure of Arrays) layout.
    /// Processes timers in parallel batches of 64 timers per thread for optimal performance.
    /// </summary>
    public static class TimerManager {
        const int BatchSize = 64;
        const int InitialCapacity = 256;
        
        static readonly List<Timer> timers = new();
        static readonly List<Timer> sweep = new();
        static readonly List<Timer> finishedTimers = new();
        
        // SOA native arrays for job system
        static NativeArray<float> currentTimes;
        static NativeArray<float> initialTimes;
        static NativeArray<float> thresholdValues;
        static NativeArray<float> intervalSteps;
        static NativeArray<TimerType> types;
        static NativeArray<TimerFlags> flags;
        
        static bool isInitialized;
        static int capacity;
        
        public static void RegisterTimer(Timer timer) {
            timers.Add(timer);
            EnsureCapacity(timers.Count);
        }
        
        public static void DeregisterTimer(Timer timer) {
            timers.Remove(timer);
        }

        public static void UpdateTimers() {
            if (timers.Count == 0) return;
            
            EnsureInitialized();
            
            int count = timers.Count;
            
            // Copy timer data to native arrays (write phase)
            for (int i = 0; i < count; i++) {
                var timer = timers[i];
                timer.WriteToArrays(i, currentTimes, initialTimes, thresholdValues, intervalSteps, types, flags);
            }
            
            // Schedule and run the parallel job
            var job = new TimerTickJob {
                DeltaTime = Time.deltaTime,
                CurrentTimes = currentTimes,
                InitialTimes = initialTimes,
                ThresholdValues = thresholdValues,
                IntervalSteps = intervalSteps,
                Types = types,
                Flags = flags
            };
            
            JobHandle handle = job.Schedule(count, BatchSize);
            handle.Complete();
            
            // Read back timer data and handle callbacks (read phase)
            finishedTimers.Clear();
            sweep.RefreshWith(timers);
            
            for (int i = 0; i < count; i++) {
                var timer = sweep[i];
                timer.ReadFromArrays(i, currentTimes, thresholdValues, flags);
                
                // Track finished timers for callback processing
                if ((flags[i] & TimerFlags.Finished) != 0) {
                    finishedTimers.Add(timer);
                }
            }
            
            // Process finished timers after all data is read back
            foreach (var timer in finishedTimers) {
                timer.Stop();
            }
        }
        
        static void EnsureInitialized() {
            if (isInitialized) return;
            
            capacity = InitialCapacity;
            AllocateArrays(capacity);
            isInitialized = true;
        }
        
        static void EnsureCapacity(int requiredCapacity) {
            if (!isInitialized) return;
            
            if (requiredCapacity <= capacity) return;
            
            // Double capacity until sufficient
            int newCapacity = capacity;
            while (newCapacity < requiredCapacity) {
                newCapacity *= 2;
            }
            
            // Dispose old arrays and allocate new ones
            DisposeArrays();
            AllocateArrays(newCapacity);
            capacity = newCapacity;
        }
        
        static void AllocateArrays(int size) {
            currentTimes = new NativeArray<float>(size, Allocator.Persistent);
            initialTimes = new NativeArray<float>(size, Allocator.Persistent);
            thresholdValues = new NativeArray<float>(size, Allocator.Persistent);
            intervalSteps = new NativeArray<float>(size, Allocator.Persistent);
            types = new NativeArray<TimerType>(size, Allocator.Persistent);
            flags = new NativeArray<TimerFlags>(size, Allocator.Persistent);
        }
        
        static void DisposeArrays() {
            if (currentTimes.IsCreated) currentTimes.Dispose();
            if (initialTimes.IsCreated) initialTimes.Dispose();
            if (thresholdValues.IsCreated) thresholdValues.Dispose();
            if (intervalSteps.IsCreated) intervalSteps.Dispose();
            if (types.IsCreated) types.Dispose();
            if (flags.IsCreated) flags.Dispose();
        }
        
        public static void Clear() {
            sweep.RefreshWith(timers);
            foreach (var timer in sweep) {
                timer.Dispose();
            }
            
            timers.Clear();
            sweep.Clear();
            finishedTimers.Clear();
            
            DisposeArrays();
            isInitialized = false;
        }
    }
}