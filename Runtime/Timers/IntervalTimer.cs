using System;
using Unity.Collections;
using UnityEngine;

namespace ImprovedTimers {
    /// <summary>
    /// Countdown timer that fires an event every interval until completion.
    /// </summary>
    public class IntervalTimer : Timer {
        readonly float interval;
        float nextInterval;

        public Action OnInterval = delegate { };

        public IntervalTimer(float totalTime, float intervalSeconds) : base(totalTime) {
            interval = intervalSeconds;
            nextInterval = totalTime - interval;
        }

        public override void Tick() {
            if (IsRunning && CurrentTime > 0) {
                CurrentTime -= Time.deltaTime;

                // Fire interval events as long as thresholds are crossed
                while (CurrentTime <= nextInterval && nextInterval >= 0) {
                    OnInterval.Invoke();
                    nextInterval -= interval;
                }
            }

            if (IsRunning && CurrentTime <= 0) {
                CurrentTime = 0;
                Stop();
            }
        }

        public override bool IsFinished => CurrentTime <= 0;
        
        public override TimerType GetTimerType() => TimerType.Interval;

        public override void Reset() {
            base.Reset();
            nextInterval = initialTime - interval;
        }

        public override void Reset(float newTime) {
            base.Reset(newTime);
            nextInterval = initialTime - interval;
        }
        
        public override void WriteToArrays(int index,
            NativeArray<float> currentTimes,
            NativeArray<float> initialTimes,
            NativeArray<float> thresholdValues,
            NativeArray<float> intervalSteps,
            NativeArray<TimerType> types,
            NativeArray<TimerFlags> flags) {
            
            base.WriteToArrays(index, currentTimes, initialTimes, thresholdValues, intervalSteps, types, flags);
            thresholdValues[index] = nextInterval;
            intervalSteps[index] = interval;
        }
        
        public override void ReadFromArrays(int index,
            NativeArray<float> currentTimes,
            NativeArray<float> thresholdValues,
            NativeArray<TimerFlags> flags) {
            
            base.ReadFromArrays(index, currentTimes, thresholdValues, flags);
            
            // Update the next interval threshold from the job
            nextInterval = thresholdValues[index];
            
            // Check if an interval tick occurred during job processing
            if ((flags[index] & TimerFlags.IntervalTicked) != 0) {
                OnInterval.Invoke();
            }
        }
    }
}