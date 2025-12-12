using System;
using UnityEngine;

namespace ImprovedTimers {
    /// <summary>
    /// Test MonoBehaviour that demonstrates and tests all timer types.
    /// Add this component to a GameObject in your scene to run timer tests.
    /// </summary>
    public class TimerTestRunner : MonoBehaviour {
        [Header("Test Configuration")]
        [Tooltip("Number of countdown timers to create for stress testing")]
        [SerializeField] int countdownTimerCount = 100;
        
        [Tooltip("Duration for countdown timers in seconds")]
        [SerializeField] float countdownDuration = 3f;
        
        [Tooltip("Ticks per second for frequency timer")]
        [SerializeField] int frequencyTicksPerSecond = 5;
        
        [Tooltip("Total duration for interval timer")]
        [SerializeField] float intervalTotalTime = 5f;
        
        [Tooltip("Interval between events in seconds")]
        [SerializeField] float intervalStep = 1f;
        
        [Header("Test Results")]
        [SerializeField] int countdownsCompleted;
        [SerializeField] int frequencyTicks;
        [SerializeField] int intervalTicks;
        [SerializeField] float stopwatchTime;
        [SerializeField] bool allTestsPassed;
        
        CountdownTimer[] countdownTimers;
        FrequencyTimer frequencyTimer;
        IntervalTimer intervalTimer;
        StopwatchTimer stopwatchTimer;
        
        bool testsStarted;
        bool countdownTestPassed;
        bool frequencyTestPassed;
        bool intervalTestPassed;
        bool stopwatchTestPassed;
        
        void Start() {
            StartTests();
        }
        
        void Update() {
            if (stopwatchTimer != null && stopwatchTimer.IsRunning) {
                stopwatchTime = stopwatchTimer.CurrentTime;
            }
            
            // Check if all tests completed
            if (testsStarted && !allTestsPassed) {
                CheckTestCompletion();
            }
        }
        
        void OnDestroy() {
            CleanupTimers();
        }
        
        [ContextMenu("Start Tests")]
        public void StartTests() {
            CleanupTimers();
            ResetTestResults();
            
            Debug.Log("[TimerTestRunner] Starting timer tests with Unity Job System...");
            testsStarted = true;
            
            // Test 1: Multiple Countdown Timers (stress test for parallel processing)
            StartCountdownTest();
            
            // Test 2: Frequency Timer
            StartFrequencyTest();
            
            // Test 3: Interval Timer
            StartIntervalTest();
            
            // Test 4: Stopwatch Timer
            StartStopwatchTest();
            
            Debug.Log($"[TimerTestRunner] Created {countdownTimerCount} countdown timers, 1 frequency timer, 1 interval timer, and 1 stopwatch timer.");
        }
        
        [ContextMenu("Stop Tests")]
        public void StopTests() {
            CleanupTimers();
            Debug.Log("[TimerTestRunner] Tests stopped and timers cleaned up.");
        }
        
        void StartCountdownTest() {
            countdownTimers = new CountdownTimer[countdownTimerCount];
            
            for (int i = 0; i < countdownTimerCount; i++) {
                // Vary the duration slightly to test different completion times
                float duration = countdownDuration + (i * 0.01f);
                countdownTimers[i] = new CountdownTimer(duration);
                
                int index = i; // Capture for closure
                countdownTimers[i].OnTimerStart += () => {
                    if (index == 0) {
                        Debug.Log($"[CountdownTest] First countdown timer started (duration: {duration}s)");
                    }
                };
                
                countdownTimers[i].OnTimerStop += () => {
                    countdownsCompleted++;
                    if (countdownsCompleted == 1) {
                        Debug.Log($"[CountdownTest] First countdown timer completed!");
                    }
                    if (countdownsCompleted == countdownTimerCount) {
                        countdownTestPassed = true;
                        Debug.Log($"[CountdownTest] All {countdownTimerCount} countdown timers completed successfully!");
                    }
                };
                
                countdownTimers[i].Start();
            }
        }
        
        void StartFrequencyTest() {
            frequencyTimer = new FrequencyTimer(frequencyTicksPerSecond);
            
            frequencyTimer.OnTimerStart += () => {
                Debug.Log($"[FrequencyTest] Frequency timer started ({frequencyTicksPerSecond} ticks/second)");
            };
            
            frequencyTimer.OnTick += () => {
                frequencyTicks++;
                if (frequencyTicks <= 3) {
                    Debug.Log($"[FrequencyTest] Tick #{frequencyTicks}");
                }
                
                // Stop after expected number of ticks based on countdown duration
                if (frequencyTicks >= frequencyTicksPerSecond * countdownDuration) {
                    frequencyTestPassed = true;
                    Debug.Log($"[FrequencyTest] Completed with {frequencyTicks} ticks!");
                    frequencyTimer.Stop();
                }
            };
            
            frequencyTimer.Start();
        }
        
        void StartIntervalTest() {
            intervalTimer = new IntervalTimer(intervalTotalTime, intervalStep);
            
            intervalTimer.OnTimerStart += () => {
                Debug.Log($"[IntervalTest] Interval timer started (total: {intervalTotalTime}s, interval: {intervalStep}s)");
            };
            
            intervalTimer.OnInterval += () => {
                intervalTicks++;
                Debug.Log($"[IntervalTest] Interval #{intervalTicks} at time {intervalTimer.CurrentTime:F2}s");
            };
            
            intervalTimer.OnTimerStop += () => {
                int expectedTicks = Mathf.FloorToInt(intervalTotalTime / intervalStep);
                intervalTestPassed = intervalTicks >= expectedTicks - 1; // Allow some tolerance
                Debug.Log($"[IntervalTest] Completed with {intervalTicks} intervals (expected ~{expectedTicks})!");
            };
            
            intervalTimer.Start();
        }
        
        void StartStopwatchTest() {
            stopwatchTimer = new StopwatchTimer();
            
            stopwatchTimer.OnTimerStart += () => {
                Debug.Log("[StopwatchTest] Stopwatch timer started");
            };
            
            stopwatchTimer.Start();
            
            // We'll check the stopwatch result when other tests complete
        }
        
        void CheckTestCompletion() {
            // Check if countdown and interval tests are done
            bool countdownDone = countdownTestPassed;
            bool intervalDone = intervalTimer == null || !intervalTimer.IsRunning;
            bool frequencyDone = frequencyTestPassed;
            
            if (countdownDone && intervalDone && frequencyDone) {
                // Stop and validate stopwatch
                if (stopwatchTimer != null && stopwatchTimer.IsRunning) {
                    stopwatchTimer.Stop();
                    stopwatchTestPassed = stopwatchTimer.CurrentTime > 0;
                    Debug.Log($"[StopwatchTest] Completed with time: {stopwatchTimer.CurrentTime:F2}s");
                }
                
                // Final results
                allTestsPassed = countdownTestPassed && frequencyTestPassed && intervalTestPassed && stopwatchTestPassed;
                
                Debug.Log("=== Timer Test Results ===");
                Debug.Log($"Countdown Test: {(countdownTestPassed ? "PASSED" : "FAILED")} ({countdownsCompleted}/{countdownTimerCount} completed)");
                Debug.Log($"Frequency Test: {(frequencyTestPassed ? "PASSED" : "FAILED")} ({frequencyTicks} ticks)");
                Debug.Log($"Interval Test: {(intervalTestPassed ? "PASSED" : "FAILED")} ({intervalTicks} intervals)");
                Debug.Log($"Stopwatch Test: {(stopwatchTestPassed ? "PASSED" : "FAILED")} ({stopwatchTime:F2}s elapsed)");
                Debug.Log($"Overall: {(allTestsPassed ? "ALL TESTS PASSED" : "SOME TESTS FAILED")}");
                Debug.Log("==========================");
            }
        }
        
        void ResetTestResults() {
            countdownsCompleted = 0;
            frequencyTicks = 0;
            intervalTicks = 0;
            stopwatchTime = 0;
            allTestsPassed = false;
            countdownTestPassed = false;
            frequencyTestPassed = false;
            intervalTestPassed = false;
            stopwatchTestPassed = false;
            testsStarted = false;
        }
        
        void CleanupTimers() {
            if (countdownTimers != null) {
                foreach (var timer in countdownTimers) {
                    timer?.Dispose();
                }
                countdownTimers = null;
            }
            
            frequencyTimer?.Dispose();
            frequencyTimer = null;
            
            intervalTimer?.Dispose();
            intervalTimer = null;
            
            stopwatchTimer?.Dispose();
            stopwatchTimer = null;
        }
    }
}
