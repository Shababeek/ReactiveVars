using System;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

namespace Shababeek.ReactiveVars.Tests
{
    [TestFixture]
    public class ScriptableVariableTests
    {
        private FloatVariable CreateFloat(float initial = 0f)
        {
            var v = ScriptableObject.CreateInstance<FloatVariable>();
            v.SetValueWithoutNotify(initial);
            return v;
        }

        [TearDown]
        public void TearDown()
        {
            // ScriptableObject instances created during tests are cleaned up by Unity
        }

        [Test]
        public void ValueSetterTriggersOnValueChanged()
        {
            var v = CreateFloat();
            float received = -1f;
            var sub = v.OnValueChanged.Subscribe(val => received = val);

            v.Value = 42f;

            Assert.AreEqual(42f, received);
            sub.Dispose();
        }

        [Test]
        public void ValueSetterTriggersOnRaised()
        {
            var v = CreateFloat();
            bool raised = false;
            var sub = v.OnRaised.Subscribe(_ => raised = true);

            v.Value = 10f;

            Assert.IsTrue(raised);
            sub.Dispose();
        }

        [Test]
        public void SetValueWithoutNotifyDoesNotTriggerEvents()
        {
            var v = CreateFloat();
            bool changed = false;
            var sub = v.OnValueChanged.Subscribe(_ => changed = true);

            v.SetValueWithoutNotify(99f);

            Assert.IsFalse(changed);
            Assert.AreEqual(99f, v.Value);
            sub.Dispose();
        }

        [Test]
        public void RaiseManuallyTriggersSubscribers()
        {
            var v = CreateFloat(5f);
            float received = -1f;
            var sub = v.OnValueChanged.Subscribe(val => received = val);

            v.Raise();

            Assert.AreEqual(5f, received);
            sub.Dispose();
        }

        [Test]
        public void MultipleSubscribersAllReceiveNotifications()
        {
            var v = CreateFloat();
            int count = 0;
            var sub1 = v.OnValueChanged.Subscribe(_ => count++);
            var sub2 = v.OnValueChanged.Subscribe(_ => count++);
            var sub3 = v.OnValueChanged.Subscribe(_ => count++);

            v.Value = 1f;

            Assert.AreEqual(3, count);
            sub1.Dispose();
            sub2.Dispose();
            sub3.Dispose();
        }

        [Test]
        public void DisposedSubscriptionStopsReceiving()
        {
            var v = CreateFloat();
            int count = 0;
            var sub = v.OnValueChanged.Subscribe(_ => count++);

            v.Value = 1f;
            Assert.AreEqual(1, count);

            sub.Dispose();
            v.Value = 2f;
            Assert.AreEqual(1, count);
        }

        [Test]
        public void SetValueWithObjectSetsTypedValue()
        {
            var v = CreateFloat();
            v.SetValue(42f);
            Assert.AreEqual(42f, v.Value);
        }

        [Test]
        public void SetValueWithWrongTypeLogsError()
        {
            var v = CreateFloat();
            v.SetValueWithoutNotify(5f);
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Cannot set value"));
            v.SetValue("not a float");
            Assert.AreEqual(5f, v.Value);
        }

        [Test]
        public void GetValueReturnsBoxedValue()
        {
            var v = CreateFloat(7f);
            object boxed = v.GetValue();
            Assert.AreEqual(7f, (float)boxed);
        }

        [Test]
        public void ToStringReturnsValueString()
        {
            var v = CreateFloat(3.14f);
            Assert.AreEqual(3.14f.ToString(), v.ToString());
        }

        [Test]
        public void ResetSetsToDefault()
        {
            var v = CreateFloat(100f);
            v.Reset();
            Assert.AreEqual(0f, v.Value);
        }

        [Test]
        public void ResetWithValueSetsSpecificValue()
        {
            var v = CreateFloat(100f);
            v.Reset(50f);
            Assert.AreEqual(50f, v.Value);
        }

        [Test]
        public void InitSetsValueWithoutEvents()
        {
            var v = CreateFloat();
            bool changed = false;
            var sub = v.OnValueChanged.Subscribe(_ => changed = true);

            v.Init(42f);

            Assert.IsFalse(changed);
            Assert.AreEqual(42f, v.Value);
            sub.Dispose();
        }

        [Test]
        public void IObservableSubscribeWorks()
        {
            var v = CreateFloat();
            float received = -1f;
            IObservable<float> observable = v;
            var sub = observable.Subscribe(val => received = val);

            v.Value = 10f;

            Assert.AreEqual(10f, received);
            sub.Dispose();
        }
    }

    [TestFixture]
    public class FloatVariableTests
    {
        private FloatVariable CreateFloat(float initial = 0f)
        {
            var v = ScriptableObject.CreateInstance<FloatVariable>();
            v.SetValueWithoutNotify(initial);
            return v;
        }

        [Test]
        public void AddUpdatesValue()
        {
            var v = CreateFloat(10f);
            v.Add(5f);
            Assert.AreEqual(15f, v.Value);
        }

        [Test]
        public void SubtractUpdatesValue()
        {
            var v = CreateFloat(10f);
            v.Subtract(3f);
            Assert.AreEqual(7f, v.Value);
        }

        [Test]
        public void MultiplyUpdatesValue()
        {
            var v = CreateFloat(5f);
            v.Multiply(3f);
            Assert.AreEqual(15f, v.Value);
        }

        [Test]
        public void DivideByZeroLogsWarning()
        {
            var v = CreateFloat(10f);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Cannot divide by zero"));
            v.Divide(0f);
            Assert.AreEqual(10f, v.Value);
        }

        [Test]
        public void DivideUpdatesValue()
        {
            var v = CreateFloat(10f);
            v.Divide(2f);
            Assert.AreEqual(5f, v.Value);
        }

        [Test]
        public void ClampConstrainsValue()
        {
            var v = CreateFloat(100f);
            v.Clamp(0f, 50f);
            Assert.AreEqual(50f, v.Value);

            v.Value = -10f;
            v.Clamp(0f, 50f);
            Assert.AreEqual(0f, v.Value);
        }

        [Test]
        public void GetNormalizedReturnsCorrectRange()
        {
            var v = CreateFloat(50f);
            float norm = v.GetNormalized(0f, 100f);
            Assert.AreEqual(0.5f, norm, 0.001f);
        }

        [Test]
        public void GetNormalizedClampsToZeroOne()
        {
            var v = CreateFloat(200f);
            float norm = v.GetNormalized(0f, 100f);
            Assert.AreEqual(1f, norm);
        }

        [Test]
        public void GetNormalizedEqualMinMaxReturnsZero()
        {
            var v = CreateFloat(50f);
            float norm = v.GetNormalized(50f, 50f);
            Assert.AreEqual(0f, norm);
        }

        [Test]
        public void SetFromNormalizedMapsCorrectly()
        {
            var v = CreateFloat();
            v.SetFromNormalized(0.5f, 0f, 100f);
            Assert.AreEqual(50f, v.Value, 0.001f);
        }

        [Test]
        public void LerpToInterpolatesCorrectly()
        {
            var v = CreateFloat(0f);
            v.LerpTo(100f, 0.5f);
            Assert.AreEqual(50f, v.Value, 0.001f);
        }

        [Test]
        public void MoveTowardsRespectsMaxDelta()
        {
            var v = CreateFloat(0f);
            v.MoveTowards(100f, 10f);
            Assert.AreEqual(10f, v.Value, 0.001f);
        }

        [Test]
        public void AsFloatReturnsValue()
        {
            var v = CreateFloat(42f);
            Assert.AreEqual(42f, v.AsFloat);
        }

        [Test]
        public void AsIntRoundsCorrectly()
        {
            var v = CreateFloat(3.7f);
            Assert.AreEqual(4, v.AsInt);

            v.Value = 3.2f;
            Assert.AreEqual(3, v.AsInt);
        }

        [Test]
        public void SetFromFloatSetsValue()
        {
            var v = CreateFloat();
            v.SetFromFloat(99f);
            Assert.AreEqual(99f, v.Value);
        }

        [Test]
        public void SetFromFloatWithoutNotifyDoesNotFireEvents()
        {
            var v = CreateFloat();
            bool changed = false;
            var sub = v.OnValueChanged.Subscribe(_ => changed = true);

            v.SetFromFloatWithoutNotify(50f);

            Assert.IsFalse(changed);
            Assert.AreEqual(50f, v.Value);
            sub.Dispose();
        }

        [Test]
        public void OperatorAddReturnsCorrectValue()
        {
            var a = CreateFloat(10f);
            var b = CreateFloat(20f);
            Assert.AreEqual(30f, a + b);
            Assert.AreEqual(15f, a + 5f);
            Assert.AreEqual(15f, 5f + a);
        }

        [Test]
        public void OperatorSubtractReturnsCorrectValue()
        {
            var a = CreateFloat(20f);
            var b = CreateFloat(5f);
            Assert.AreEqual(15f, a - b);
            Assert.AreEqual(15f, a - 5f);
        }

        [Test]
        public void OperatorMultiplyReturnsCorrectValue()
        {
            var a = CreateFloat(5f);
            var b = CreateFloat(3f);
            Assert.AreEqual(15f, a * b);
            Assert.AreEqual(25f, a * 5f);
        }

        [Test]
        public void OperatorDivideReturnsCorrectValue()
        {
            var a = CreateFloat(20f);
            var b = CreateFloat(4f);
            Assert.AreEqual(5f, a / b);
            Assert.AreEqual(10f, a / 2f);
        }

        [Test]
        public void OperatorDivideByZeroReturnsZero()
        {
            var a = CreateFloat(10f);
            Assert.AreEqual(0f, a / 0f);
        }

        [Test]
        public void OperatorEqualityUsesApproximately()
        {
            var a = CreateFloat(1.0f);
            var b = CreateFloat(1.0f);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.IsTrue(a == 1.0f);
            Assert.IsTrue(1.0f == a);
        }
    }

    [TestFixture]
    public class IntVariableTests
    {
        private IntVariable CreateInt(int initial = 0)
        {
            var v = ScriptableObject.CreateInstance<IntVariable>();
            v.SetValueWithoutNotify(initial);
            return v;
        }

        [Test]
        public void IncrementAddsOne()
        {
            var v = CreateInt(5);
            v.Increment();
            Assert.AreEqual(6, v.Value);
        }

        [Test]
        public void DecrementSubtractsOne()
        {
            var v = CreateInt(5);
            v.Decrement();
            Assert.AreEqual(4, v.Value);
        }

        [Test]
        public void AddIntAmount()
        {
            var v = CreateInt(10);
            v.Add(5);
            Assert.AreEqual(15, v.Value);
        }

        [Test]
        public void AddFloatRoundsToInt()
        {
            var v = CreateInt(10);
            v.Add(2.7f);
            Assert.AreEqual(13, v.Value);
        }

        [Test]
        public void MultiplyRoundsResult()
        {
            var v = CreateInt(3);
            v.Multiply(2.5f);
            Assert.AreEqual(8, v.Value); // RoundToInt(3 * 2.5) = RoundToInt(7.5) = 8
        }

        [Test]
        public void ClampIntBounds()
        {
            var v = CreateInt(100);
            v.Clamp(0, 50);
            Assert.AreEqual(50, v.Value);

            v.Value = -10;
            v.Clamp(0, 50);
            Assert.AreEqual(0, v.Value);
        }

        [Test]
        public void SetFromFloatRoundsCorrectly()
        {
            var v = CreateInt();
            v.SetFromFloat(3.7f);
            Assert.AreEqual(4, v.Value);

            v.SetFromFloat(3.2f);
            Assert.AreEqual(3, v.Value);
        }

        [Test]
        public void SetFromFloatWithoutNotifyDoesNotFireEvents()
        {
            var v = CreateInt();
            bool changed = false;
            var sub = v.OnValueChanged.Subscribe(_ => changed = true);

            v.SetFromFloatWithoutNotify(42.6f);

            Assert.IsFalse(changed);
            Assert.AreEqual(43, v.Value);
            sub.Dispose();
        }

        [Test]
        public void AsFloatConvertsCorrectly()
        {
            var v = CreateInt(42);
            Assert.AreEqual(42f, v.AsFloat);
        }

        [Test]
        public void AsIntReturnsValue()
        {
            var v = CreateInt(7);
            Assert.AreEqual(7, v.AsInt);
        }

        [Test]
        public void OperatorAddReturnsCorrectValue()
        {
            var a = CreateInt(10);
            var b = CreateInt(20);
            Assert.AreEqual(30, a + b);
            Assert.AreEqual(15, a + 5);
        }

        [Test]
        public void OperatorSubtractReturnsCorrectValue()
        {
            var a = CreateInt(20);
            var b = CreateInt(5);
            Assert.AreEqual(15, a - b);
        }

        [Test]
        public void OperatorMultiplyReturnsCorrectValue()
        {
            var a = CreateInt(5);
            var b = CreateInt(3);
            Assert.AreEqual(15, a * b);
        }

        [Test]
        public void OperatorDivideReturnsCorrectValue()
        {
            var a = CreateInt(20);
            var b = CreateInt(4);
            Assert.AreEqual(5, a / b);
        }

        [Test]
        public void OperatorDivideByZeroReturnsZero()
        {
            var a = CreateInt(10);
            Assert.AreEqual(0, a / 0);
        }

        [Test]
        public void OperatorEqualityWorks()
        {
            var a = CreateInt(5);
            var b = CreateInt(5);
            Assert.IsTrue(a == b);
            Assert.IsTrue(a == 5);
            Assert.IsTrue(5 == a);
        }
    }

    [TestFixture]
    public class BoolVariableTests
    {
        private BoolVariable CreateBool(bool initial)
        {
            var v = ScriptableObject.CreateInstance<BoolVariable>();
            v.SetValueWithoutNotify(initial);
            return v;
        }

        [Test]
        public void ToggleFlipsValue()
        {
            var v = CreateBool(false);
            v.Toggle();
            Assert.IsTrue(v.Value);
            v.Toggle();
            Assert.IsFalse(v.Value);
        }

        [Test]
        public void LogicalAndOperator()
        {
            var a = CreateBool(true);
            var b = CreateBool(false);
            Assert.IsFalse(a & b);

            b.Value = true;
            Assert.IsTrue(a & b);
            Assert.IsTrue(a & true);
            Assert.IsFalse(a & false);
            Assert.IsTrue(true & a);
        }

        [Test]
        public void LogicalOrOperator()
        {
            var a = CreateBool(true);
            var b = CreateBool(false);
            Assert.IsTrue(a | b);

            a.SetValueWithoutNotify(false);
            b.SetValueWithoutNotify(false);
            Assert.IsFalse(a | b);
            Assert.IsTrue(a | true);
            Assert.IsTrue(true | a);
        }

        [Test]
        public void LogicalNotOperator()
        {
            var v = CreateBool(true);
            Assert.IsFalse(!v);

            v.SetValueWithoutNotify(false);
            Assert.IsTrue(!v);
        }

        [Test]
        public void EqualityOperators()
        {
            var a = CreateBool(true);
            var b = CreateBool(true);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.IsTrue(a == true);
            Assert.IsTrue(true == a);
            Assert.IsFalse(a == false);
        }
    }

    [TestFixture]
    public class NumericalVariableBaseTests
    {
        private FloatVariable CreateFloat(float initial = 0f)
        {
            var v = ScriptableObject.CreateInstance<FloatVariable>();
            v.SetValueWithoutNotify(initial);
            return v;
        }

        [Test]
        public void SubtractCallsAddWithNegative()
        {
            var v = CreateFloat(10f);
            v.Subtract(3f);
            Assert.AreEqual(7f, v.Value, 0.001f);
        }

        [Test]
        public void DivideByZeroLogsWarningAndDoesNotChange()
        {
            var v = CreateFloat(10f);
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Cannot divide by zero"));
            v.Divide(0f);
            Assert.AreEqual(10f, v.Value);
        }

        [Test]
        public void GetNormalizedMidpoint()
        {
            var v = CreateFloat(50f);
            Assert.AreEqual(0.5f, v.GetNormalized(0f, 100f), 0.001f);
        }

        [Test]
        public void GetNormalizedAtMin()
        {
            var v = CreateFloat(0f);
            Assert.AreEqual(0f, v.GetNormalized(0f, 100f), 0.001f);
        }

        [Test]
        public void GetNormalizedAtMax()
        {
            var v = CreateFloat(100f);
            Assert.AreEqual(1f, v.GetNormalized(0f, 100f), 0.001f);
        }

        [Test]
        public void SetFromNormalizedZero()
        {
            var v = CreateFloat();
            v.SetFromNormalized(0f, 10f, 20f);
            Assert.AreEqual(10f, v.Value, 0.001f);
        }

        [Test]
        public void SetFromNormalizedOne()
        {
            var v = CreateFloat();
            v.SetFromNormalized(1f, 10f, 20f);
            Assert.AreEqual(20f, v.Value, 0.001f);
        }

        [Test]
        public void LerpToZeroStays()
        {
            var v = CreateFloat(0f);
            v.LerpTo(100f, 0f);
            Assert.AreEqual(0f, v.Value, 0.001f);
        }

        [Test]
        public void LerpToOneReachesTarget()
        {
            var v = CreateFloat(0f);
            v.LerpTo(100f, 1f);
            Assert.AreEqual(100f, v.Value, 0.001f);
        }

        [Test]
        public void MoveTowardsStopsAtTarget()
        {
            var v = CreateFloat(95f);
            v.MoveTowards(100f, 100f);
            Assert.AreEqual(100f, v.Value, 0.001f);
        }
    }
}
