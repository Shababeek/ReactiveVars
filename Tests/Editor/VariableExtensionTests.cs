using System;
using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Shababeek.ReactiveVars.Tests
{
    [TestFixture]
    public class VariableExtensionTests
    {
        private BoolVariable CreateBool(bool initial = false)
        {
            var v = ScriptableObject.CreateInstance<BoolVariable>();
            v.SetValueWithoutNotify(initial);
            return v;
        }

        private FloatVariable CreateFloat(float initial = 0f)
        {
            var v = ScriptableObject.CreateInstance<FloatVariable>();
            v.SetValueWithoutNotify(initial);
            return v;
        }

        private IntVariable CreateInt(int initial = 0)
        {
            var v = ScriptableObject.CreateInstance<IntVariable>();
            v.SetValueWithoutNotify(initial);
            return v;
        }

        [Test]
        public void WhenTrueOnlyFiresOnTrueTransitions()
        {
            var v = CreateBool(false);
            int trueCount = 0;
            var sub = v.WhenTrue().Subscribe(_ => trueCount++);

            v.Value = true;
            Assert.AreEqual(1, trueCount);

            v.Value = false;
            Assert.AreEqual(1, trueCount);

            v.Value = true;
            Assert.AreEqual(2, trueCount);

            sub.Dispose();
        }

        [Test]
        public void WhenFalseOnlyFiresOnFalseTransitions()
        {
            var v = CreateBool(true);
            int falseCount = 0;
            var sub = v.WhenFalse().Subscribe(_ => falseCount++);

            v.Value = false;
            Assert.AreEqual(1, falseCount);

            v.Value = true;
            Assert.AreEqual(1, falseCount);

            v.Value = false;
            Assert.AreEqual(2, falseCount);

            sub.Dispose();
        }

        [Test]
        public void WhenAboveFiltersCorrectly()
        {
            var v = CreateFloat(0f);
            int count = 0;
            var sub = v.WhenAbove(10f).Subscribe(_ => count++);

            v.Value = 5f;
            Assert.AreEqual(0, count);

            v.Value = 15f;
            Assert.AreEqual(1, count);

            v.Value = 10f; // Not above, equal
            Assert.AreEqual(1, count);

            v.Value = 20f;
            Assert.AreEqual(2, count);

            sub.Dispose();
        }

        [Test]
        public void WhenBelowFiltersCorrectly()
        {
            var v = CreateFloat(50f);
            int count = 0;
            var sub = v.WhenBelow(10f).Subscribe(_ => count++);

            v.Value = 15f;
            Assert.AreEqual(0, count);

            v.Value = 5f;
            Assert.AreEqual(1, count);

            v.Value = 10f; // Not below, equal
            Assert.AreEqual(1, count);

            sub.Dispose();
        }

        [Test]
        public void WhenInRangeFiltersCorrectly()
        {
            var v = CreateFloat(0f);
            int count = 0;
            var sub = v.WhenInRange(10f, 20f).Subscribe(_ => count++);

            v.Value = 5f;
            Assert.AreEqual(0, count);

            v.Value = 15f;
            Assert.AreEqual(1, count);

            v.Value = 10f; // Inclusive lower bound
            Assert.AreEqual(2, count);

            v.Value = 20f; // Inclusive upper bound
            Assert.AreEqual(3, count);

            v.Value = 25f;
            Assert.AreEqual(3, count);

            sub.Dispose();
        }

        [Test]
        public void DistinctSkipsDuplicateValues()
        {
            var v = CreateFloat(0f);
            int count = 0;
            var sub = v.Distinct().Subscribe(_ => count++);

            v.Value = 10f;
            Assert.AreEqual(1, count);

            v.Value = 10f; // Same value, should skip
            Assert.AreEqual(1, count);

            v.Value = 20f;
            Assert.AreEqual(2, count);

            sub.Dispose();
        }

        [Test]
        public void OnFloatChangedEmitsFloatValues()
        {
            var v = CreateInt(0);
            float received = -1f;
            var sub = ((INumericalVariable)v).OnFloatChanged().Subscribe(val => received = val);

            v.Value = 42;

            Assert.AreEqual(42f, received);
            sub.Dispose();
        }

        [Test]
        public void WhenAboveWorksWithIntVariable()
        {
            var v = CreateInt(0);
            int count = 0;
            var sub = ((INumericalVariable)v).WhenAbove(5f).Subscribe(_ => count++);

            v.Value = 3;
            Assert.AreEqual(0, count);

            v.Value = 10;
            Assert.AreEqual(1, count);

            sub.Dispose();
        }
    }
}
