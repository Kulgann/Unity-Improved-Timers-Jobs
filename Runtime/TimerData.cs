namespace ImprovedTimers {
    /// <summary>
    /// Enum representing the type of timer for job-based processing.
    /// </summary>
    public enum TimerType : byte {
        Countdown = 0,
        Stopwatch = 1,
        Frequency = 2,
        Interval = 3
    }

    /// <summary>
    /// Flags representing the timer state.
    /// </summary>
    public enum TimerFlags : byte {
        None = 0,
        Running = 1 << 0,
        Finished = 1 << 1,
        FrequencyTicked = 1 << 2,
        IntervalTicked = 1 << 3
    }
}
