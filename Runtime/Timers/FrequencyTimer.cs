using System;
using Unity.Collections;
using UnityEngine;

namespace ImprovedTimers {
    /// <summary>
    /// Timer that ticks at a specific frequency. (N times per second)
    /// </summary>
    public class FrequencyTimer : Timer {
        public int TicksPerSecond { get; private set; }
        
        public Action OnTick = delegate { };
        
        float timeThreshold;

        public FrequencyTimer(int ticksPerSecond) : base(0) {
            CalculateTimeThreshold(ticksPerSecond);
        }

        public override void Tick() {
            if (IsRunning && CurrentTime >= timeThreshold) {
                CurrentTime -= timeThreshold;
                OnTick.Invoke();
            }

            if (IsRunning && CurrentTime < timeThreshold) {
                CurrentTime += Time.deltaTime;
            }
        }

        public override bool IsFinished => !IsRunning;
        
        public override TimerType GetTimerType() => TimerType.Frequency;

        public override void Reset() {
            CurrentTime = 0;
        }
        
        public void Reset(int newTicksPerSecond) {
            CalculateTimeThreshold(newTicksPerSecond);
            Reset();
        }
        
        void CalculateTimeThreshold(int ticksPerSecond) {
            TicksPerSecond = ticksPerSecond;
            timeThreshold = 1f / TicksPerSecond;
        }
        
        public override void WriteToArrays(int index,
            NativeArray<float> currentTimes,
            NativeArray<float> initialTimes,
            NativeArray<float> thresholdValues,
            NativeArray<float> intervalSteps,
            NativeArray<TimerType> types,
            NativeArray<TimerFlags> flags) {
            
            base.WriteToArrays(index, currentTimes, initialTimes, thresholdValues, intervalSteps, types, flags);
            thresholdValues[index] = timeThreshold;
        }
        
        public override void ReadFromArrays(int index,
            NativeArray<float> currentTimes,
            NativeArray<float> thresholdValues,
            NativeArray<TimerFlags> flags) {
            
            base.ReadFromArrays(index, currentTimes, thresholdValues, flags);
            
            // Check if a tick occurred during job processing
            if ((flags[index] & TimerFlags.FrequencyTicked) != 0) {
                OnTick.Invoke();
            }
        }
    }
}