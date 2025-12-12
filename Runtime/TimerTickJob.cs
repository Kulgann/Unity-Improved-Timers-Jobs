using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ImprovedTimers {
    /// <summary>
    /// Job that processes timer ticks in parallel batches of 64 timers per thread.
    /// Uses SOA (Structure of Arrays) layout for cache-efficient processing.
    /// </summary>
    [BurstCompile]
    public struct TimerTickJob : IJobParallelFor {
        public float DeltaTime;

        // SOA data layout for cache efficiency
        public NativeArray<float> CurrentTimes;
        public NativeArray<float> InitialTimes;
        public NativeArray<float> ThresholdValues; // timeThreshold for Frequency, nextInterval for Interval
        public NativeArray<float> IntervalSteps;   // interval step for Interval timers
        public NativeArray<TimerType> Types;
        public NativeArray<TimerFlags> Flags;

        public void Execute(int index) {
            TimerFlags flags = Flags[index];
            
            // Skip if not running
            if ((flags & TimerFlags.Running) == 0) return;

            float currentTime = CurrentTimes[index];
            TimerType type = Types[index];

            // Clear tick flags from previous frame
            flags &= ~(TimerFlags.FrequencyTicked | TimerFlags.IntervalTicked | TimerFlags.Finished);

            switch (type) {
                case TimerType.Countdown:
                    currentTime = TickCountdown(currentTime, ref flags);
                    break;
                case TimerType.Stopwatch:
                    currentTime = TickStopwatch(currentTime);
                    break;
                case TimerType.Frequency:
                    currentTime = TickFrequency(index, currentTime, ref flags);
                    break;
                case TimerType.Interval:
                    currentTime = TickInterval(index, currentTime, ref flags);
                    break;
            }

            CurrentTimes[index] = currentTime;
            Flags[index] = flags;
        }

        float TickCountdown(float currentTime, ref TimerFlags flags) {
            if (currentTime > 0) {
                currentTime -= DeltaTime;
            }
            if (currentTime <= 0) {
                currentTime = 0;
                flags |= TimerFlags.Finished;
            }
            return currentTime;
        }

        float TickStopwatch(float currentTime) {
            return currentTime + DeltaTime;
        }

        float TickFrequency(int index, float currentTime, ref TimerFlags flags) {
            float threshold = ThresholdValues[index];
            
            // Always add deltaTime first
            currentTime += DeltaTime;
            
            // Then check if threshold is exceeded
            if (currentTime >= threshold) {
                currentTime -= threshold;
                flags |= TimerFlags.FrequencyTicked;
            }
            
            return currentTime;
        }

        float TickInterval(int index, float currentTime, ref TimerFlags flags) {
            float nextInterval = ThresholdValues[index];
            float interval = IntervalSteps[index];
            
            if (currentTime > 0) {
                currentTime -= DeltaTime;
                
                // Check if we crossed an interval threshold
                while (currentTime <= nextInterval && nextInterval >= 0) {
                    flags |= TimerFlags.IntervalTicked;
                    nextInterval -= interval;
                }
                
                // Store updated next interval
                ThresholdValues[index] = nextInterval;
            }
            
            if (currentTime <= 0) {
                currentTime = 0;
                flags |= TimerFlags.Finished;
            }
            
            return currentTime;
        }
    }
}
