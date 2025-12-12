using System;
using UnityEngine;

namespace ImprovedTimers {
    /// <summary>
    /// Timer that counts down from a specific value to zero.
    /// </summary>
    public class CountdownTimer : Timer {
        public CountdownTimer(float value) : base(value) { }

        #pragma warning disable CS0672 // Member overrides obsolete member
        public override void Tick() {
            if (IsRunning && CurrentTime > 0) {
                CurrentTime -= Time.deltaTime;
            }

            if (IsRunning && CurrentTime <= 0) {
                Stop();
            }
        }
        #pragma warning restore CS0672

        public override bool IsFinished => CurrentTime <= 0;
        
        public override TimerType GetTimerType() => TimerType.Countdown;
    }
}