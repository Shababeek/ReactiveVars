using NUnit.Framework;
using Shababeek.ReactiveVars;
using Shababeek.Sequencing;
using Shababeek.Sequencing.Editors;
using UnityEngine;

namespace Shababeek.ReactiveVars.EditorTests
{
    [TestFixture]
    public class StepTransitionTests
    {
        [Test]
        public void EvaluateReturnsTrueWhenNoCondition()
        {
            var transition = new StepTransition();
            Assert.IsTrue(transition.Evaluate());
        }

        [Test]
        public void EvaluateReturnsTrueWhenConditionMet()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.SetValueWithoutNotify(10);

            var transition = new StepTransition();
            BranchConditionHelper.SetVariable(transition.Condition, intVar);
            BranchConditionHelper.SetComparison(transition.Condition, ComparisonType.GreaterThan);
            BranchConditionHelper.SetInt(transition.Condition, 5);

            Assert.IsTrue(transition.Evaluate());
        }

        [Test]
        public void EvaluateReturnsFalseWhenConditionNotMet()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.SetValueWithoutNotify(3);

            var transition = new StepTransition();
            BranchConditionHelper.SetVariable(transition.Condition, intVar);
            BranchConditionHelper.SetComparison(transition.Condition, ComparisonType.GreaterThan);
            BranchConditionHelper.SetInt(transition.Condition, 5);

            Assert.IsFalse(transition.Evaluate());
        }

        [Test]
        public void GetDisplayLabelReturnsLabelWhenSet()
        {
            var transition = new StepTransition();

            var labelField = typeof(StepTransition).GetField("label",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            labelField.SetValue(transition, "Go to next");

            Assert.AreEqual("Go to next", transition.GetDisplayLabel());
        }

        [Test]
        public void GetDisplayLabelReturnsDefaultWhenNoVariableAndNoLabel()
        {
            var transition = new StepTransition();
            Assert.AreEqual("Default", transition.GetDisplayLabel());
        }

        [Test]
        public void GetDisplayLabelReturnsVariableInfoWhenNoLabel()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.name = "Score";

            var transition = new StepTransition();
            BranchConditionHelper.SetVariable(transition.Condition, intVar);
            BranchConditionHelper.SetComparison(transition.Condition, ComparisonType.GreaterThan);

            string label = transition.GetDisplayLabel();
            Assert.IsTrue(label.Contains("Score"), $"Expected label to contain 'Score', got '{label}'");
            Assert.IsTrue(label.Contains("GreaterThan"), $"Expected label to contain 'GreaterThan', got '{label}'");
        }
    }

    [TestFixture]
    public class StepTransitionGroupTests
    {
        [Test]
        public void DefaultConstructorCreatesEmptyTransitions()
        {
            var group = new StepTransitionGroup();
            Assert.IsNotNull(group.transitions);
            Assert.AreEqual(0, group.transitions.Count);
        }

        [Test]
        public void ConstructorWithStepSetsFromStep()
        {
            var step = ScriptableObject.CreateInstance<Step>();
            var group = new StepTransitionGroup(step);

            Assert.AreEqual(step, group.fromStep);
            Assert.IsNotNull(group.transitions);
            Assert.AreEqual(0, group.transitions.Count);
        }

        [Test]
        public void CanAddTransitionsToGroup()
        {
            var step = ScriptableObject.CreateInstance<Step>();
            var group = new StepTransitionGroup(step);

            group.transitions.Add(new StepTransition());
            group.transitions.Add(new StepTransition());

            Assert.AreEqual(2, group.transitions.Count);
        }
    }
}
