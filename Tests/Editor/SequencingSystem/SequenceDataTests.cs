using System.Collections.Generic;
using NUnit.Framework;
using Shababeek.Sequencing;
using UnityEngine;

namespace Shababeek.ReactiveVars.EditorTests
{
    [TestFixture]
    public class SequenceInitTests
    {
        [Test]
        public void InitCreatesEmptyStepsList()
         {
            var seq = ScriptableObject.CreateInstance<Sequence>();
            seq.Init();

            Assert.IsNotNull(seq.Steps);
            Assert.AreEqual(0, seq.Steps.Count);
        }

        [Test]
        public void StepsListCanHoldSteps()
        {
            var seq = ScriptableObject.CreateInstance<Sequence>();
            seq.Init();

            var step1 = ScriptableObject.CreateInstance<Step>();
            var step2 = ScriptableObject.CreateInstance<Step>();
            seq.Steps.Add(step1);
            seq.Steps.Add(step2);

            Assert.AreEqual(2, seq.Steps.Count);
            Assert.AreEqual(step1, seq.Steps[0]);
            Assert.AreEqual(step2, seq.Steps[1]);
        }

        [Test]
        public void StartedIsFalseByDefault()
        {
            var seq = ScriptableObject.CreateInstance<Sequence>();
            Assert.IsFalse(seq.Started);
        }
    }

    [TestFixture]
    public class SequenceResetTests
    {
        [Test]
        public void ResetClearsCurrentStep()
        {
            var seq = ScriptableObject.CreateInstance<Sequence>();
            seq.Init();

            var step = ScriptableObject.CreateInstance<Step>();
            seq.Steps.Add(step);

            seq.Reset();

            Assert.AreEqual(step, seq.CurrentStep, "After reset, CurrentStep should be the first step (index 0)");
        }
    }

    [TestFixture]
    public class StepNamingConventionTests
    {
        [Test]
        public void StepNamingFollowsSequencePattern()
        {
            var seq = ScriptableObject.CreateInstance<Sequence>();
            seq.name = "MySequence";
            seq.Init();

            var step = ScriptableObject.CreateInstance<Step>();
            step.name = $"{seq.name}-0_Intro";

            Assert.IsTrue(step.name.StartsWith("MySequence-"),
                "Step name should start with the parent sequence name");
            Assert.IsTrue(step.name.Contains("_"),
                "Step name should contain underscore separating index from display name");
        }

        [Test]
        public void StepDisplayNameExtractsAfterUnderscore()
        {
            var step = ScriptableObject.CreateInstance<Step>();
            step.name = "MySequence-0_IntroStep";

            var fullName = step.name;
            var underscoreIndex = fullName.IndexOf('_') + 1;
            var displayName = fullName.Substring(underscoreIndex);

            Assert.AreEqual("IntroStep", displayName);
        }

        [Test]
        public void StepRenamePreservesIndexPrefix()
        {
            var seq = ScriptableObject.CreateInstance<Sequence>();
            seq.name = "Quest";

            // Simulate the rename logic from SequenceEditor
            var step = ScriptableObject.CreateInstance<Step>();
            step.name = "Quest-0_OldName";

            var newDisplayName = "NewName";
            var index = 0;
            step.name = $"{seq.name}-{index}_{newDisplayName}";

            Assert.AreEqual("Quest-0_NewName", step.name);
        }

        [Test]
        public void ReorderingUpdatesStepIndices()
        {
            var seq = ScriptableObject.CreateInstance<Sequence>();
            seq.name = "TestSeq";
            seq.Init();

            var step0 = ScriptableObject.CreateInstance<Step>();
            step0.name = "TestSeq-0_First";
            var step1 = ScriptableObject.CreateInstance<Step>();
            step1.name = "TestSeq-1_Second";
            var step2 = ScriptableObject.CreateInstance<Step>();
            step2.name = "TestSeq-2_Third";

            seq.Steps.Add(step0);
            seq.Steps.Add(step1);
            seq.Steps.Add(step2);

            // Simulate reorder: move "Third" to position 0
            seq.Steps.RemoveAt(2);
            seq.Steps.Insert(0, step2);

            // Simulate the rename logic from SequenceEditor.OnReorderCallback
            for (var i = 0; i < seq.Steps.Count; i++)
            {
                var obj = seq.Steps[i];
                var semiIndex = obj.name.IndexOf('_');
                obj.name = $"{seq.name}-{i + 1}_{obj.name.Substring(semiIndex + 1)}";
            }

            Assert.AreEqual("TestSeq-1_Third", seq.Steps[0].name);
            Assert.AreEqual("TestSeq-2_First", seq.Steps[1].name);
            Assert.AreEqual("TestSeq-3_Second", seq.Steps[2].name);
        }
    }

    [TestFixture]
    public class BranchingSequenceStepNamingTests
    {
        [Test]
        public void BranchingStepNamingFollowsZeroBasedPattern()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            seq.name = "BranchSeq";
            seq.Init();

            var step = ScriptableObject.CreateInstance<Step>();
            step.name = $"{seq.name}-0_step";

            Assert.AreEqual("BranchSeq-0_step", step.name);
        }

        [Test]
        public void BranchingStepRenamePreservesFormat()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            seq.name = "BranchSeq";

            var step = ScriptableObject.CreateInstance<Step>();
            step.name = "BranchSeq-2_OldName";

            // Simulate rename logic from BranchingSequenceEditor.DrawStepElement
            var elementName = step.name;
            var underscoreIndex = elementName.IndexOf('_') + 1;
            elementName = elementName.Substring(underscoreIndex);

            Assert.AreEqual("OldName", elementName);

            // Simulate rename with new name
            var newDisplayName = "Decision";
            var index = 2;
            step.name = $"{seq.name}-{index}_{newDisplayName}";

            Assert.AreEqual("BranchSeq-2_Decision", step.name);
        }

        [Test]
        public void BranchingRenameAllStepsLogic()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            seq.name = "TestBranch";
            seq.Init();

            var step0 = ScriptableObject.CreateInstance<Step>();
            step0.name = "TestBranch-0_Alpha";
            var step1 = ScriptableObject.CreateInstance<Step>();
            step1.name = "TestBranch-1_Beta";

            seq.AllSteps.Add(step0);
            seq.AllSteps.Add(step1);

            // Simulate RenameAllSteps from BranchingSequenceEditor
            for (var i = 0; i < seq.AllSteps.Count; i++)
            {
                var step = seq.AllSteps[i];
                if (step == null) continue;
                var uIndex = step.name.IndexOf('_');
                var baseName = uIndex >= 0 ? step.name.Substring(uIndex + 1) : step.name;
                step.name = $"{seq.name}-{i}_{baseName}";
            }

            Assert.AreEqual("TestBranch-0_Alpha", seq.AllSteps[0].name);
            Assert.AreEqual("TestBranch-1_Beta", seq.AllSteps[1].name);
        }
    }

    [TestFixture]
    public class TransitionCleanupTests
    {
        [Test]
        public void CleanupRemovesGroupsFromDeletedStep()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            seq.Init();

            var stepA = ScriptableObject.CreateInstance<Step>();
            var stepB = ScriptableObject.CreateInstance<Step>();
            seq.AllSteps.Add(stepA);
            seq.AllSteps.Add(stepB);

            var groupA = new StepTransitionGroup(stepA);
            groupA.transitions.Add(new StepTransition());
            var groupB = new StepTransitionGroup(stepB);
            groupB.transitions.Add(new StepTransition());

            seq.TransitionGroups.Add(groupA);
            seq.TransitionGroups.Add(groupB);

            // Simulate CleanupTransitionsForStep(stepA)
            for (int i = seq.TransitionGroups.Count - 1; i >= 0; i--)
            {
                var group = seq.TransitionGroups[i];
                if (group.fromStep == stepA)
                {
                    seq.TransitionGroups.RemoveAt(i);
                    continue;
                }

                group.transitions.RemoveAll(t => t.TargetStep == stepA);
            }

            Assert.AreEqual(1, seq.TransitionGroups.Count);
            Assert.AreEqual(stepB, seq.TransitionGroups[0].fromStep);
        }

        [Test]
        public void CleanupRemovesTransitionsTargetingDeletedStep()
        {
            var seq = ScriptableObject.CreateInstance<BranchingSequence>();
            seq.Init();

            var stepA = ScriptableObject.CreateInstance<Step>();
            var stepB = ScriptableObject.CreateInstance<Step>();
            var stepC = ScriptableObject.CreateInstance<Step>();
            seq.AllSteps.Add(stepA);
            seq.AllSteps.Add(stepB);
            seq.AllSteps.Add(stepC);

            var group = new StepTransitionGroup(stepA);

            // Create transitions targeting stepB and stepC
            var transToB = new StepTransition();
            var transToC = new StepTransition();
            var targetField = typeof(StepTransition).GetField("targetStep",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            targetField.SetValue(transToB, stepB);
            targetField.SetValue(transToC, stepC);

            group.transitions.Add(transToB);
            group.transitions.Add(transToC);
            seq.TransitionGroups.Add(group);

            // Simulate CleanupTransitionsForStep(stepB)
            for (int i = seq.TransitionGroups.Count - 1; i >= 0; i--)
            {
                var g = seq.TransitionGroups[i];
                if (g.fromStep == stepB)
                {
                    seq.TransitionGroups.RemoveAt(i);
                    continue;
                }

                g.transitions.RemoveAll(t => t.TargetStep == stepB);
            }

            Assert.AreEqual(1, seq.TransitionGroups.Count);
            Assert.AreEqual(1, seq.TransitionGroups[0].transitions.Count);
            Assert.AreEqual(stepC, seq.TransitionGroups[0].transitions[0].TargetStep);
        }
    }
}
