using System.Collections.Generic;
using NUnit.Framework;
using Shababeek.Sequencing;
using UnityEngine;

namespace Shababeek.ReactiveVars.EditorTests
{
    [TestFixture]
    public class BranchingSequenceInitTests
    {
        [Test]
        public void InitCreatesEmptyLists()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            seq.Init();

            Assert.IsNotNull(seq.AllSteps);
            Assert.AreEqual(0, seq.AllSteps.Count);
            Assert.IsNotNull(seq.TransitionGroups);
            Assert.AreEqual(0, seq.TransitionGroups.Count);
        }

        [Test]
        public void AllStepsListCanHoldSteps()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            seq.Init();

            var step1 = ScriptableObject.CreateInstance<Step>();
            var step2 = ScriptableObject.CreateInstance<Step>();
            seq.AllSteps.Add(step1);
            seq.AllSteps.Add(step2);

            Assert.AreEqual(2, seq.AllSteps.Count);
            Assert.AreEqual(step1, seq.AllSteps[0]);
            Assert.AreEqual(step2, seq.AllSteps[1]);
        }

        [Test]
        public void TransitionGroupsListCanHoldGroups()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            seq.Init();

            var step = ScriptableObject.CreateInstance<Step>();
            var group = new StepTransitionGroup(step);
            seq.TransitionGroups.Add(group);

            Assert.AreEqual(1, seq.TransitionGroups.Count);
            Assert.AreEqual(step, seq.TransitionGroups[0].fromStep);
        }
    }

    [TestFixture]
    public class BranchingSequencePositionTests
    {
        [Test]
        public void SetAndGetStepPositionRoundTrips()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            var step = ScriptableObject.CreateInstance<Step>();
            var position = new Vector2(100, 200);

            seq.SetStepPosition(step, position);
            var result = seq.GetStepPosition(step);

            Assert.AreEqual(position, result);
        }

        [Test]
        public void GetStepPositionReturnsNaNForUnknownStep()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            var step = ScriptableObject.CreateInstance<Step>();

            var result = seq.GetStepPosition(step);

            Assert.IsTrue(float.IsNaN(result.x));
            Assert.IsTrue(float.IsNaN(result.y));
        }

        [Test]
        public void SetStepPositionOverwritesExisting()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            var step = ScriptableObject.CreateInstance<Step>();

            seq.SetStepPosition(step, new Vector2(10, 20));
            seq.SetStepPosition(step, new Vector2(300, 400));

            var result = seq.GetStepPosition(step);
            Assert.AreEqual(new Vector2(300, 400), result);
        }

        [Test]
        public void MultipleStepsHaveIndependentPositions()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            var step1 = ScriptableObject.CreateInstance<Step>();
            var step2 = ScriptableObject.CreateInstance<Step>();

            seq.SetStepPosition(step1, new Vector2(10, 20));
            seq.SetStepPosition(step2, new Vector2(300, 400));

            Assert.AreEqual(new Vector2(10, 20), seq.GetStepPosition(step1));
            Assert.AreEqual(new Vector2(300, 400), seq.GetStepPosition(step2));
        }
    }

    [TestFixture]
    public class BranchingSequenceResetTests
    {
        [Test]
        public void ResetClearsCurrentStep()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            seq.Init();

            seq.Reset();

            Assert.IsNull(seq.CurrentStep);
        }

        [Test]
        public void EntryStepIsNullByDefault()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            Assert.IsNull(seq.EntryStep);
        }

        [Test]
        public void StartedIsFalseByDefault()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            Assert.IsFalse(seq.Started);
        }
    }

    [TestFixture]
    public class BranchingSequenceTransitionGroupManagementTests
    {
        [Test]
        public void TransitionGroupWithTransitionsHoldsData()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            seq.Init();

            var stepA = ScriptableObject.CreateInstance<Step>();
            var stepB = ScriptableObject.CreateInstance<Step>();
            seq.AllSteps.Add(stepA);
            seq.AllSteps.Add(stepB);

            var group = new StepTransitionGroup(stepA);
            var transition = new StepTransition();

            // Set target step via reflection since it's internal
            var targetField = typeof(StepTransition).GetField("targetStep",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            targetField.SetValue(transition, stepB);

            group.transitions.Add(transition);
            seq.TransitionGroups.Add(group);

            Assert.AreEqual(1, seq.TransitionGroups.Count);
            Assert.AreEqual(stepA, seq.TransitionGroups[0].fromStep);
            Assert.AreEqual(1, seq.TransitionGroups[0].transitions.Count);
            Assert.AreEqual(stepB, seq.TransitionGroups[0].transitions[0].TargetStep);
        }

        [Test]
        public void MultipleTransitionGroupsForDifferentSteps()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            seq.Init();

            var stepA = ScriptableObject.CreateInstance<Step>();
            var stepB = ScriptableObject.CreateInstance<Step>();

            var groupA = new StepTransitionGroup(stepA);
            var groupB = new StepTransitionGroup(stepB);

            groupA.transitions.Add(new StepTransition());
            groupB.transitions.Add(new StepTransition());
            groupB.transitions.Add(new StepTransition());

            seq.TransitionGroups.Add(groupA);
            seq.TransitionGroups.Add(groupB);

            Assert.AreEqual(2, seq.TransitionGroups.Count);
            Assert.AreEqual(1, seq.TransitionGroups[0].transitions.Count);
            Assert.AreEqual(2, seq.TransitionGroups[1].transitions.Count);
        }
    }
}
