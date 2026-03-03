using NUnit.Framework;
using Shababeek.ReactiveVars;
using Shababeek.Sequencing;
using Shababeek.Sequencing.Editors;
using UnityEngine;

namespace Shababeek.ReactiveVars.EditorTests
{
    [TestFixture]
    public class BranchConditionTests
    {
        private BranchCondition CreateCondition(ScriptableVariable variable, ComparisonType comparison)
        {
            var condition = new BranchCondition();
            BranchConditionHelper.SetVariable(condition, variable);
            BranchConditionHelper.SetComparison(condition, comparison);
            return condition;
        }

        [Test]
        public void NullVariableReturnsTrue()
        {
            var condition = new BranchCondition();
            Assert.IsTrue(condition.Evaluate());
        }

        // Bool tests

        [Test]
        public void BoolEqualsTrue()
        {
            var boolVar = ScriptableObject.CreateInstance<BoolVariable>();
            boolVar.SetValueWithoutNotify(true);

            var condition = CreateCondition(boolVar, ComparisonType.Equals);
            BranchConditionHelper.SetBool(condition, true);

            Assert.IsTrue(condition.Evaluate());
        }

        [Test]
        public void BoolEqualsFalseWhenMismatch()
        {
            var boolVar = ScriptableObject.CreateInstance<BoolVariable>();
            boolVar.SetValueWithoutNotify(false);

            var condition = CreateCondition(boolVar, ComparisonType.Equals);
            BranchConditionHelper.SetBool(condition, true);

            Assert.IsFalse(condition.Evaluate());
        }

        [Test]
        public void BoolNotEquals()
        {
            var boolVar = ScriptableObject.CreateInstance<BoolVariable>();
            boolVar.SetValueWithoutNotify(false);

            var condition = CreateCondition(boolVar, ComparisonType.NotEquals);
            BranchConditionHelper.SetBool(condition, true);

            Assert.IsTrue(condition.Evaluate());
        }

        [Test]
        public void BoolUnsupportedComparisonFallsBackToEquals()
        {
            var boolVar = ScriptableObject.CreateInstance<BoolVariable>();
            boolVar.SetValueWithoutNotify(true);

            var condition = CreateCondition(boolVar, ComparisonType.GreaterThan);
            BranchConditionHelper.SetBool(condition, true);

            Assert.IsTrue(condition.Evaluate());
        }

        // Int tests

        [Test]
        public void IntEquals()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.SetValueWithoutNotify(10);

            var condition = CreateCondition(intVar, ComparisonType.Equals);
            BranchConditionHelper.SetInt(condition, 10);

            Assert.IsTrue(condition.Evaluate());
        }

        [Test]
        public void IntNotEquals()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.SetValueWithoutNotify(10);

            var condition = CreateCondition(intVar, ComparisonType.NotEquals);
            BranchConditionHelper.SetInt(condition, 5);

            Assert.IsTrue(condition.Evaluate());
        }

        [Test]
        public void IntGreaterThan()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.SetValueWithoutNotify(10);

            var condition = CreateCondition(intVar, ComparisonType.GreaterThan);
            BranchConditionHelper.SetInt(condition, 5);

            Assert.IsTrue(condition.Evaluate());
        }

        [Test]
        public void IntGreaterThanFailsWhenEqual()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.SetValueWithoutNotify(5);

            var condition = CreateCondition(intVar, ComparisonType.GreaterThan);
            BranchConditionHelper.SetInt(condition, 5);

            Assert.IsFalse(condition.Evaluate());
        }

        [Test]
        public void IntLessThan()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.SetValueWithoutNotify(3);

            var condition = CreateCondition(intVar, ComparisonType.LessThan);
            BranchConditionHelper.SetInt(condition, 5);

            Assert.IsTrue(condition.Evaluate());
        }

        [Test]
        public void IntGreaterOrEqual()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.SetValueWithoutNotify(5);

            var condition = CreateCondition(intVar, ComparisonType.GreaterOrEqual);
            BranchConditionHelper.SetInt(condition, 5);

            Assert.IsTrue(condition.Evaluate());
        }

        [Test]
        public void IntLessOrEqual()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.SetValueWithoutNotify(5);

            var condition = CreateCondition(intVar, ComparisonType.LessOrEqual);
            BranchConditionHelper.SetInt(condition, 5);

            Assert.IsTrue(condition.Evaluate());
        }

        [Test]
        public void IntLessOrEqualFailsWhenGreater()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.SetValueWithoutNotify(10);

            var condition = CreateCondition(intVar, ComparisonType.LessOrEqual);
            BranchConditionHelper.SetInt(condition, 5);

            Assert.IsFalse(condition.Evaluate());
        }

        // Float tests

        [Test]
        public void FloatEquals()
        {
            var floatVar = ScriptableObject.CreateInstance<FloatVariable>();
            floatVar.SetValueWithoutNotify(3.14f);

            var condition = CreateCondition(floatVar, ComparisonType.Equals);
            BranchConditionHelper.SetFloat(condition, 3.14f);

            Assert.IsTrue(condition.Evaluate());
        }

        [Test]
        public void FloatGreaterThan()
        {
            var floatVar = ScriptableObject.CreateInstance<FloatVariable>();
            floatVar.SetValueWithoutNotify(10.5f);

            var condition = CreateCondition(floatVar, ComparisonType.GreaterThan);
            BranchConditionHelper.SetFloat(condition, 5.0f);

            Assert.IsTrue(condition.Evaluate());
        }

        [Test]
        public void FloatLessThan()
        {
            var floatVar = ScriptableObject.CreateInstance<FloatVariable>();
            floatVar.SetValueWithoutNotify(2.5f);

            var condition = CreateCondition(floatVar, ComparisonType.LessThan);
            BranchConditionHelper.SetFloat(condition, 5.0f);

            Assert.IsTrue(condition.Evaluate());
        }

        // String tests

        [Test]
        public void StringEquals()
        {
            var textVar = ScriptableObject.CreateInstance<TextVariable>();
            textVar.SetValueWithoutNotify("hello");

            var condition = CreateCondition(textVar, ComparisonType.Equals);
            BranchConditionHelper.SetString(condition, "hello");

            Assert.IsTrue(condition.Evaluate());
        }

        [Test]
        public void StringEqualsCaseSensitive()
        {
            var textVar = ScriptableObject.CreateInstance<TextVariable>();
            textVar.SetValueWithoutNotify("Hello");

            var condition = CreateCondition(textVar, ComparisonType.Equals);
            BranchConditionHelper.SetString(condition, "hello");

            Assert.IsFalse(condition.Evaluate());
        }

        [Test]
        public void StringNotEquals()
        {
            var textVar = ScriptableObject.CreateInstance<TextVariable>();
            textVar.SetValueWithoutNotify("hello");

            var condition = CreateCondition(textVar, ComparisonType.NotEquals);
            BranchConditionHelper.SetString(condition, "world");

            Assert.IsTrue(condition.Evaluate());
        }

        [Test]
        public void StringUnsupportedComparisonFallsBackToEquals()
        {
            var textVar = ScriptableObject.CreateInstance<TextVariable>();
            textVar.SetValueWithoutNotify("hello");

            var condition = CreateCondition(textVar, ComparisonType.GreaterThan);
            BranchConditionHelper.SetString(condition, "hello");

            Assert.IsTrue(condition.Evaluate());
        }

        // Reactivity test — condition re-evaluates with updated variable

        [Test]
        public void ConditionReactsToVariableChanges()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.SetValueWithoutNotify(3);

            var condition = CreateCondition(intVar, ComparisonType.GreaterThan);
            BranchConditionHelper.SetInt(condition, 5);

            Assert.IsFalse(condition.Evaluate());

            intVar.SetValueWithoutNotify(10);

            Assert.IsTrue(condition.Evaluate());
        }
    }
}
