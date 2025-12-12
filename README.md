# Improved Unity Timers
![PlayerLoop2](https://github.com/adammyhre/Unity-Improved-Timers/assets/38876398/faefe51b-3fef-42ea-8440-1a92cd5c5abc)

An extensible Timer solution for Unity Game Development.  Timers are self managing 
by injecting a Timer Manager class into Unity's Update loop.

**New in this version:** The timer system now uses Unity's Job System with a Structure of Arrays (SOA) 
data layout for high-performance parallel processing. Timers are processed in batches of 64 per thread 
using `IJobParallelFor`, enabling efficient handling of thousands of timers with minimal main thread impact.

Create your own Timers by extending the Timer abstract class!

## Example Usage

```csharp
CountdownTimer timer = new CountdownTimer(5f);

void Start() {
    timer.OnTimerStart += () => Debug.Log("Timer started");
    timer.OnTimerStop += () => Debug.Log("Timer stopped");
    timer.Start();
    
    timer.Pause();
    timer.Resume();
    
    timer.Reset();
    timer.Reset(10f);
    
    Debug.Log(timer.IsRunning ? "Timer is running" : "Timer is not running");
    Debug.Log(timer.IsFinished ? "Timer is finished" : "Timer is not finished");
    
    timer.Stop();
}

void Update() {
    Debug.Log(timer.CurrentTime);
    Debug.Log(timer.Progress);
}

void OnDestroy() {
    timer.Dispose();
}
```

## Notes

Several Timers are already included, but you can create any kind of Timer you need that can be 
adjusted every frame.  The included Timers are:

- CountdownTimer: Counts down from a specified time to zero.
- FrequencyTimer: Ticks N times per second.
- StopwatchTimer: Counts up from zero to infinity.
- IntervalTimer: Countdown timer that fires events at regular intervals.

Classes extending the Timer class must implement:
- The `Tick` method to increment or decrement the Timer
- The `IsFinished` property which is a convenience for consumers
- The `GetTimerType()` method to return the appropriate `TimerType` enum value
- Optionally override `WriteToArrays` and `ReadFromArrays` for custom job data

Call `Dispose` when you don't need a Timer anymore to ensure proper garbage collection.

## Architecture

The timer system uses a **Structure of Arrays (SOA)** layout for optimal cache performance:

- **NativeArrays** store timer data (currentTimes, initialTimes, types, flags, etc.)
- **IJobParallelFor** processes timers in parallel batches of 64
- **Burst compilation** provides additional performance optimizations
- Timer callbacks (OnTick, OnInterval, OnTimerStop) are invoked on the main thread after job completion

## How to Install

Simply download the library into your Unity project and access the utilities across your scripts or import it in Unity with 
the Unity Package Manager using this URL:

`https://github.com/adammyhre/Unity-Improved-Timers.git`

### Add to Manifest

Alternatively, you can add the following line to your project's `manifest.json` file.

```
"com.gitamend.improvedtimers": "https://github.com/adammyhre/Unity-Improved-Timers.git"
```

## YouTube

- [Improved Timers in Unity](https://youtu.be/ilvmOQtl57c)

You can also check out my [YouTube channel](https://www.youtube.com/@git-amend?sub_confirmation=1) for more Unity content.
