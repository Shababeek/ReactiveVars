using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

namespace Shababeek.ReactiveVars.Tests
{
    [TestFixture]
    public class TimerDriverTests
    {
        private GameObject _go;
        private FloatVariable _timerVar;
        private BoolVariable _runningVar;
        private GameEvent _completedEvent;

        [SetUp]
        public void SetUp()
        {
            _timerVar = ScriptableObject.CreateInstance<FloatVariable>();
            _runningVar = ScriptableObject.CreateInstance<BoolVariable>();
            _completedEvent = ScriptableObject.CreateInstance<GameEvent>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.Destroy(_go);
        }

        private TimerDriver CreateTimer(TimerDriver.TimerMode mode = TimerDriver.TimerMode.CountDown,
            float duration = 0.5f, bool loop = false, bool autoStart = true)
        {
            _go = new GameObject("TimerTestGO");
            _go.SetActive(false);
            var driver = _go.AddComponent<TimerDriver>();

            SetField(driver, "variable", _timerVar);
            SetField(driver, "mode", mode);
            SetField(driver, "duration", duration);
            SetField(driver, "loop", loop);
            SetField(driver, "autoStart", autoStart);
            SetField(driver, "onCompleted", _completedEvent);
            SetField(driver, "isRunningVariable", _runningVar);

            _go.SetActive(true);
            return driver;
        }

        private void SetField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        [UnityTest]
        public IEnumerator CountDownReachesZero()
        {
            var driver = CreateTimer(TimerDriver.TimerMode.CountDown, 0.1f);
            yield return null;

            Assert.IsTrue(_runningVar.Value);

            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(0f, _timerVar.Value, 0.01f);
            Assert.IsFalse(_runningVar.Value);
        }

        [UnityTest]
        public IEnumerator CountDownFiresCompletedEvent()
        {
            bool completed = false;
            var sub = _completedEvent.OnRaised.Subscribe(_ => completed = true);

            var driver = CreateTimer(TimerDriver.TimerMode.CountDown, 0.1f);
            yield return null;

            yield return new WaitForSeconds(0.2f);

            Assert.IsTrue(completed);
            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator CountUpReachesDuration()
        {
            var driver = CreateTimer(TimerDriver.TimerMode.CountUp, 0.1f);
            yield return null;

            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(0.1f, _timerVar.Value, 0.02f);
            Assert.IsFalse(_runningVar.Value);
        }

        [UnityTest]
        public IEnumerator StopAndResume()
        {
            var driver = CreateTimer(TimerDriver.TimerMode.CountDown, 1f);
            yield return null;

            yield return new WaitForSeconds(0.1f);

            float valueBeforeStop = _timerVar.Value;
            driver.StopTimer();
            Assert.IsFalse(_runningVar.Value);

            yield return new WaitForSeconds(0.1f);

            Assert.AreEqual(valueBeforeStop, _timerVar.Value, 0.02f);

            driver.ResumeTimer();
            Assert.IsTrue(_runningVar.Value);

            yield return new WaitForSeconds(0.1f);

            Assert.Less(_timerVar.Value, valueBeforeStop);
        }

        [UnityTest]
        public IEnumerator ResetTimerResetsToInitial()
        {
            var driver = CreateTimer(TimerDriver.TimerMode.CountDown, 1f);
            yield return null;

            yield return new WaitForSeconds(0.2f);

            driver.ResetTimer();

            Assert.AreEqual(1f, _timerVar.Value, 0.01f);
            Assert.IsFalse(_runningVar.Value);
        }

        [UnityTest]
        public IEnumerator LoopRestartsAfterCompletion()
        {
            int completedCount = 0;
            var sub = _completedEvent.OnRaised.Subscribe(_ => completedCount++);

            var driver = CreateTimer(TimerDriver.TimerMode.CountDown, 0.1f, loop: true);
            yield return null;

            yield return new WaitForSeconds(0.35f);

            Assert.GreaterOrEqual(completedCount, 2);
            Assert.IsTrue(_runningVar.Value);

            sub.Dispose();
        }

        [UnityTest]
        public IEnumerator AutoStartFalseDoesNotStart()
        {
            var driver = CreateTimer(autoStart: false);
            yield return null;

            Assert.IsFalse(_runningVar.Value);
        }
    }

    [TestFixture]
    public class CooldownDriverTests
    {
        private GameObject _go;
        private BoolVariable _cooldownVar;
        private FloatVariable _remainingVar;

        [SetUp]
        public void SetUp()
        {
            _cooldownVar = ScriptableObject.CreateInstance<BoolVariable>();
            _remainingVar = ScriptableObject.CreateInstance<FloatVariable>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.Destroy(_go);
        }

        private CooldownDriver CreateCooldown(float duration = 0.2f)
        {
            _go = new GameObject("CooldownTestGO");
            _go.SetActive(false);
            var driver = _go.AddComponent<CooldownDriver>();

            SetField(driver, "variable", _cooldownVar);
            SetField(driver, "duration", duration);
            SetField(driver, "remainingTimeVariable", _remainingVar);

            _go.SetActive(true);
            return driver;
        }

        private void SetField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        [UnityTest]
        public IEnumerator TriggerSetsTrueThenResetsToFalse()
        {
            var driver = CreateCooldown(0.15f);
            yield return null;

            driver.Trigger();
            yield return null;

            Assert.IsTrue(_cooldownVar.Value);

            yield return new WaitForSeconds(0.25f);

            Assert.IsFalse(_cooldownVar.Value);
        }

        [UnityTest]
        public IEnumerator CancelImmediatelyResets()
        {
            var driver = CreateCooldown(1f);
            yield return null;

            driver.Trigger();
            yield return null;
            Assert.IsTrue(_cooldownVar.Value);

            driver.Cancel();
            yield return null;

            Assert.IsFalse(_cooldownVar.Value);
            Assert.AreEqual(0f, _remainingVar.Value, 0.01f);
        }

        [UnityTest]
        public IEnumerator RemainingTimeVariableUpdatesEachFrame()
        {
            var driver = CreateCooldown(0.5f);
            yield return null;

            driver.Trigger();
            yield return null;

            Assert.Greater(_remainingVar.Value, 0f);

            yield return new WaitForSeconds(0.6f);

            Assert.AreEqual(0f, _remainingVar.Value, 0.01f);
        }

        [UnityTest]
        public IEnumerator RetriggerRestartsTheCooldown()
        {
            var driver = CreateCooldown(0.2f);
            yield return null;

            driver.Trigger();
            yield return new WaitForSeconds(0.1f);

            driver.Trigger();
            Assert.IsTrue(_cooldownVar.Value);

            yield return new WaitForSeconds(0.15f);

            Assert.IsTrue(_cooldownVar.Value);

            yield return new WaitForSeconds(0.1f);

            Assert.IsFalse(_cooldownVar.Value);
        }
    }
}
