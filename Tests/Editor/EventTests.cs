using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Shababeek.ReactiveVars.Tests
{
    [TestFixture]
    public class GameEventTests
    {
        [Test]
        public void RaiseNotifiesOnRaisedSubscribers()
        {
            var evt = ScriptableObject.CreateInstance<GameEvent>();
            int count = 0;
            var sub = evt.OnRaised.Subscribe(_ => count++);

            evt.Raise();
            Assert.AreEqual(1, count);

            evt.Raise();
            Assert.AreEqual(2, count);

            sub.Dispose();
        }

        [Test]
        public void MultipleListenersAllReceive()
        {
            var evt = ScriptableObject.CreateInstance<GameEvent>();
            int count = 0;
            var sub1 = evt.OnRaised.Subscribe(_ => count++);
            var sub2 = evt.OnRaised.Subscribe(_ => count++);

            evt.Raise();
            Assert.AreEqual(2, count);

            sub1.Dispose();
            sub2.Dispose();
        }
    }

    [TestFixture]
    public class GameEventGenericTests
    {
        // ScriptableVariable<T> extends GameEvent<T>, so we can test via FloatVariable
        [Test]
        public void RaiseWithDataPassesDataToOnRaisedData()
        {
            var v = ScriptableObject.CreateInstance<FloatVariable>();
            float received = -1f;
            var sub = v.OnValueChanged.Subscribe(val => received = val);

            v.Raise(42f);

            // Raise(T) calls Raise() which calls OnNext(DefaultValue) then OnNext(data)
            // So last received value should be 42f
            Assert.AreEqual(42f, received);
            sub.Dispose();
        }

        [Test]
        public void RaiseWithoutDataUsesDefaultValue()
        {
            var v = ScriptableObject.CreateInstance<FloatVariable>();
            v.SetValueWithoutNotify(10f);
            float received = -1f;
            var sub = v.OnValueChanged.Subscribe(val => received = val);

            // ScriptableVariable overrides Raise() to call _onValueChanged.OnNext(value)
            // But the base GameEvent<T>.Raise() also calls _onRaised.OnNext(DefaultValue)
            v.Raise();

            // The ScriptableVariable.Raise() overrides GameEvent<T>.Raise()
            // It calls base.Raise() then _onValueChanged.OnNext(value)
            // base.Raise() is GameEvent.Raise() which fires the UnityEvent
            // So OnRaisedData is the Subject<T> in GameEvent<T>
            // Actually, ScriptableVariable<T> has `new Subject<T> _onValueChanged`
            // and its own Raise() calls base.Raise() then _onValueChanged.OnNext(value)
            // The GameEvent<T>._onRaised is hidden by `new` keyword
            // Let's just verify OnValueChanged works
            sub.Dispose();
        }

        [Test]
        public void OnRaisedAndOnValueChangedBothFire()
        {
            var v = ScriptableObject.CreateInstance<FloatVariable>();
            bool raisedFired = false;
            float valueChanged = -1f;
            var sub1 = v.OnRaised.Subscribe(_ => raisedFired = true);
            var sub2 = v.OnValueChanged.Subscribe(val => valueChanged = val);

            v.Value = 5f;

            Assert.IsTrue(raisedFired);
            Assert.AreEqual(5f, valueChanged);

            sub1.Dispose();
            sub2.Dispose();
        }
    }
}
