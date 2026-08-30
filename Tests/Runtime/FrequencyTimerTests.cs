using GameInit.Timers;
using NUnit.Framework;
using UnityEngine;

namespace GameInit.Tests.Timers {
    public sealed class FrequencyTimerTests {
        sealed class TestFrequencyTimer : FrequencyTimer {
            public TestFrequencyTimer(int ticksPerSecond) : base(ticksPerSecond) { }
            public void SetCurrentTime(float value) => CurrentTime = value;
        }

        [Test]
        public void Tick_DoesNotFireAtZeroTimeScale() {
            float originalTimeScale = Time.timeScale;
            var timer = new TestFrequencyTimer(10);
            int ticks = 0;

            try {
                timer.OnTick += () => ticks++;
                timer.Resume();
                timer.SetCurrentTime(0.1f);
                Time.timeScale = 0f;

                timer.Tick();

                Assert.That(ticks, Is.Zero);
                Assert.That(timer.CurrentTime, Is.EqualTo(0.1f));
            }
            finally {
                Time.timeScale = originalTimeScale;
                timer.Dispose();
            }
        }
    }
}
