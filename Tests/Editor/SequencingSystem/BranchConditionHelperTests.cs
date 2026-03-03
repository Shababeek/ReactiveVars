using NUnit.Framework;
using Shababeek.ReactiveVars;
using Shababeek.Sequencing;
using Shababeek.Sequencing.Editors;
using UnityEngine;

namespace Shababeek.ReactiveVars.EditorTests
{
    [TestFixture]
    public class BranchConditionHelperTests
    {
        private BranchCondition _condition;

        [SetUp]
        public void SetUp()
        {
            _condition = new BranchCondition();
        }

        [Test]
        public void SetAndGetVariable()
        {
            var floatVar = ScriptableObject.CreateInstance<FloatVariable>();
            BranchConditionHelper.SetVariable(_condition, floatVar);
            Assert.AreEqual(floatVar, _condition.Variable);
        }

        [Test]
        public void SetVariableToNull()
        {
            var floatVar = ScriptableObject.CreateInstance<FloatVariable>();
            BranchConditionHelper.SetVariable(_condition, floatVar);
            BranchConditionHelper.SetVariable(_condition, null);
            Assert.IsNull(_condition.Variable);
        }

        [Test]
        public void SetAndGetComparison()
        {
            BranchConditionHelper.SetComparison(_condition, ComparisonType.GreaterThan);
            Assert.AreEqual(ComparisonType.GreaterThan, _condition.Comparison);
        }

        [Test]
        public void SetAndGetBool()
        {
            BranchConditionHelper.SetBool(_condition, true);
            Assert.IsTrue(BranchConditionHelper.GetBool(_condition));

            BranchConditionHelper.SetBool(_condition, false);
            Assert.IsFalse(BranchConditionHelper.GetBool(_condition));
        }

        [Test]
        public void SetAndGetInt()
        {
            BranchConditionHelper.SetInt(_condition, 42);
            Assert.AreEqual(42, BranchConditionHelper.GetInt(_condition));
        }

        [Test]
        public void SetAndGetIntNegative()
        {
            BranchConditionHelper.SetInt(_condition, -100);
            Assert.AreEqual(-100, BranchConditionHelper.GetInt(_condition));
        }

        [Test]
        public void SetAndGetFloat()
        {
            BranchConditionHelper.SetFloat(_condition, 3.14f);
            Assert.AreEqual(3.14f, BranchConditionHelper.GetFloat(_condition), 0.001f);
        }

        [Test]
        public void SetAndGetFloatZero()
        {
            BranchConditionHelper.SetFloat(_condition, 0f);
            Assert.AreEqual(0f, BranchConditionHelper.GetFloat(_condition));
        }

        [Test]
        public void SetAndGetString()
        {
            BranchConditionHelper.SetString(_condition, "hello world");
            Assert.AreEqual("hello world", BranchConditionHelper.GetString(_condition));
        }

        [Test]
        public void SetAndGetStringEmpty()
        {
            BranchConditionHelper.SetString(_condition, "");
            Assert.AreEqual("", BranchConditionHelper.GetString(_condition));
        }

        [Test]
        public void GetStringDefaultsToEmptyNotNull()
        {
            Assert.AreEqual("", BranchConditionHelper.GetString(_condition));
        }

        [Test]
        public void AllComparisonTypesCanBeSet()
        {
            foreach (ComparisonType ct in System.Enum.GetValues(typeof(ComparisonType)))
            {
                BranchConditionHelper.SetComparison(_condition, ct);
                Assert.AreEqual(ct, _condition.Comparison,
                    $"Failed to set ComparisonType.{ct}");
            }
        }
    }
}
