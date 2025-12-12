using System;
using UnityEngine;

namespace ImprovedTimers {
    /// <summary>
    /// Timer that counts up from zero to infinity.  Great for measuring durations.
    /// </summary>
    public class StopwatchTimer : Timer {
        public StopwatchTimer() : base(0) { }

        #pragma warning disable CS0672 // Member overrides obsolete member
        public override void Tick() {
            if (IsRunning) {
                CurrentTime += Time.deltaTime;
            }
        }
        #pragma warning restore CS0672

        public override bool IsFinished => false;
        
        public override TimerType GetTimerType() => TimerType.Stopwatch;
    }
}