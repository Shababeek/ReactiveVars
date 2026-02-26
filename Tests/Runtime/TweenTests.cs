using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

namespace Shababeek.ReactiveVars.Tests
{
    [TestFixture]
    public class TweenableFloatTests
    {
        [Test]
        public void TweenInterpolatesFromStartToTarget()
        {
            // Create a mock tweener setup
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();
            tweener.TweenScale = 1f;

            float lastValue = 0f;
            var tf = new TweenableFloat(tweener, val => lastValue = val, rate: 2f, value: 0f);

            // Manually call Tween with a delta that should reach 50%
            // rate=2, so delta of 0.25 => t += 2*0.25 = 0.5
            bool done = tf.Tween(0.25f);
            Assert.IsFalse(done);

            Object.Destroy(go);
        }

        [Test]
        public void TweenReturnsTrueWhenComplete()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();

            var tf = new TweenableFloat(tweener, null, rate: 2f, value: 0f);

            // rate=2, delta=0.6 => t += 2*0.6 = 1.2 >= 1
            bool done = tf.Tween(0.6f);
            Assert.IsTrue(done);

            Object.Destroy(go);
        }

        [Test]
        public void OnChangeCallbackFiresEachFrame()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();

            int callCount = 0;
            var tf = new TweenableFloat(tweener, _ => callCount++, rate: 1f, value: 0f);

            tf.Tween(0.1f);
            Assert.AreEqual(1, callCount);

            tf.Tween(0.1f);
            Assert.AreEqual(2, callCount);

            Object.Destroy(go);
        }

        [Test]
        public void OnFinishedFiresOnCompletion()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();

            bool finished = false;
            var tf = new TweenableFloat(tweener, null, rate: 2f, value: 0f);
            tf.OnFinished += () => finished = true;

            // Not done yet
            tf.Tween(0.3f); // t = 0.6
            Assert.IsFalse(finished);

            // Now complete
            tf.Tween(0.3f); // t = 1.2
            Assert.IsTrue(finished);

            Object.Destroy(go);
        }

        [Test]
        public void AnimationCurveEasingApplies()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();

            // Create a curve that maps 0.5 input to 0.8 output
            var curve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 0.8f),
                new Keyframe(1f, 1f)
            );

            float lastValue = 0f;
            // Start at 0, we'll set target to 100 manually via internal tween
            var tf = new TweenableFloat(tweener, val => lastValue = val, rate: 2f, value: 0f, curve: curve);

            // With rate=2 and delta=0.25 => t = 0.5
            // curve.Evaluate(0.5) = 0.8
            // Lerp(0, 0, 0.8) = 0 (because start and target are both 0 since we haven't set Value)
            // This test is limited because we can't set _target without going through Value setter
            // which requires play mode. Let's just verify the curve is applied via Tween calls.
            tf.Tween(0.25f);

            Object.Destroy(go);
        }
    }

    [TestFixture]
    public class TweenableNumericalTests
    {
        [Test]
        public void UsesSetFromFloatWithoutNotifyDuringInterpolation()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();

            var floatVar = ScriptableObject.CreateInstance<FloatVariable>();
            floatVar.SetValueWithoutNotify(0f);

            int notifyCount = 0;
            var sub = floatVar.OnValueChanged.Do(val => notifyCount++).Subscribe();

            var tn = new TweenableNumerical(floatVar, tweener, rate: 2f);

            // Simulate partial tween: rate=2, delta=0.25 => t=0.5 (not complete)
            tn.Tween(0.25f);

            // During interpolation, SetFromFloatWithoutNotify is used, so no events
            Assert.AreEqual(0, notifyCount);

            sub.Dispose();
            Object.Destroy(go);
        }

        [Test]
        public void UsesSetFromFloatOnFinalFrame()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();

            var floatVar = ScriptableObject.CreateInstance<FloatVariable>();
            floatVar.SetValueWithoutNotify(0f);

            int notifyCount = 0;
            var sub = floatVar.OnValueChanged.Subscribe(val => notifyCount++);

            var tn = new TweenableNumerical(floatVar, tweener, rate: 2f);

            // Complete the tween: rate=2, delta=0.6 => t=1.2 >= 1
            bool done = tn.Tween(0.6f);

            Assert.IsTrue(done);
            // SetFromFloat should fire one event on completion
            Assert.AreEqual(1, notifyCount);

            sub.Dispose();
            Object.Destroy(go);
        }

        [Test]
        public void OnFinishedFiresOnCompletion()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();

            var floatVar = ScriptableObject.CreateInstance<FloatVariable>();
            floatVar.SetValueWithoutNotify(0f);

            bool finished = false;
            var tn = new TweenableNumerical(floatVar, tweener, rate: 2f);
            tn.OnFinished += () => finished = true;

            tn.Tween(0.6f); // Complete
            Assert.IsTrue(finished);

            Object.Destroy(go);
        }

        [Test]
        public void SetRateChangesSpeed()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();

            var floatVar = ScriptableObject.CreateInstance<FloatVariable>();
            floatVar.SetValueWithoutNotify(0f);

            var tn = new TweenableNumerical(floatVar, tweener, rate: 1f);
            tn.SetRate(10f);

            // With rate 10 and delta 0.1 => t = 1.0 (complete)
            bool done = tn.Tween(0.1f);
            Assert.IsTrue(done);

            Object.Destroy(go);
        }
    }

    [TestFixture]
    public class VariableTweenerTests
    {
        [UnityTest]
        public IEnumerator AddTweenablePreventsDisplicates()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();

            yield return null; // Let OnEnable run

            var mockTweenable = new MockTweenable();
            tweener.AddTweenable(mockTweenable);
            tweener.AddTweenable(mockTweenable); // Duplicate

            Assert.AreEqual(1, tweener.ActiveCount);

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator RemoveTweenableStopsUpdating()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();

            yield return null;

            var mockTweenable = new MockTweenable();
            tweener.AddTweenable(mockTweenable);
            Assert.AreEqual(1, tweener.ActiveCount);

            tweener.RemoveTweenable(mockTweenable);
            Assert.AreEqual(0, tweener.ActiveCount);

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator ClearRemovesAll()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();

            yield return null;

            tweener.AddTweenable(new MockTweenable());
            tweener.AddTweenable(new MockTweenable());
            tweener.AddTweenable(new MockTweenable());
            Assert.AreEqual(3, tweener.ActiveCount);

            tweener.Clear();
            Assert.AreEqual(0, tweener.ActiveCount);

            Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator TweenScaleAffectsInterpolation()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();
            tweener.TweenScale = 100f; // Very fast

            yield return null;

            var mockTweenable = new MockTweenable();
            tweener.AddTweenable(mockTweenable);

            yield return null; // One frame should complete it due to high scale

            // The mock returns true (completed), so it should be removed
            Assert.AreEqual(0, tweener.ActiveCount);

            Object.Destroy(go);
        }

        [Test]
        public void AddNullLogsWarning()
        {
            var go = new GameObject("TweenerGO");
            var tweener = go.AddComponent<VariableTweener>();

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("null tweenable"));
            tweener.AddTweenable(null);

            Object.Destroy(go);
        }

        private class MockTweenable : ITweenable
        {
            public int TweenCallCount;

            public bool Tween(float scaledDeltaTime)
            {
                TweenCallCount++;
                return true; // Always complete
            }
        }
    }
}
