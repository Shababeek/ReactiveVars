using System.IO;
using System.Linq;
using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Shababeek.ReactiveVars.Tests
{
    [TestFixture]
    public class VariableContainerTests
    {
        private VariableContainer _container;
        private FloatVariable _floatVar;
        private IntVariable _intVar;
        private BoolVariable _boolVar;
        private string _testSavePath;

        [SetUp]
        public void SetUp()
        {
            _container = ScriptableObject.CreateInstance<VariableContainer>();
            _container.name = "TestContainer";

            _floatVar = ScriptableObject.CreateInstance<FloatVariable>();
            _floatVar.name = "Health";
            _floatVar.SetValueWithoutNotify(100f);

            _intVar = ScriptableObject.CreateInstance<IntVariable>();
            _intVar.name = "Score";
            _intVar.SetValueWithoutNotify(0);

            _boolVar = ScriptableObject.CreateInstance<BoolVariable>();
            _boolVar.name = "IsAlive";
            _boolVar.SetValueWithoutNotify(true);

            // Use EditorAddVariable to populate the container
#if UNITY_EDITOR
            _container.EditorAddVariable(_floatVar);
            _container.EditorAddVariable(_intVar);
            _container.EditorAddVariable(_boolVar);
#endif

            _testSavePath = Path.Combine(Application.temporaryCachePath, "test_container.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testSavePath))
                File.Delete(_testSavePath);
        }

        [Test]
        public void GetVariableByNameReturnsCorrectVariable()
        {
            var result = _container.GetVariable("Health");
            Assert.IsNotNull(result);
            Assert.AreEqual("Health", result.name);
        }

        [Test]
        public void GetVariableGenericByNameReturnsTyped()
        {
            var result = _container.GetVariable<FloatVariable>("Health");
            Assert.IsNotNull(result);
            Assert.AreEqual(100f, result.Value);
        }

        [Test]
        public void TryGetVariableReturnsFalseForMissing()
        {
            bool found = _container.TryGetVariable<FloatVariable>("NonExistent", out var variable);
            Assert.IsFalse(found);
            Assert.IsNull(variable);
        }

        [Test]
        public void TryGetVariableReturnsTrueForExisting()
        {
            bool found = _container.TryGetVariable<FloatVariable>("Health", out var variable);
            Assert.IsTrue(found);
            Assert.IsNotNull(variable);
        }

        [Test]
        public void HasVariableReturnsTrueForExisting()
        {
            Assert.IsTrue(_container.HasVariable("Health"));
            Assert.IsFalse(_container.HasVariable("NonExistent"));
        }

        [Test]
        public void GetAllVariablesOfTypeFiltersCorrectly()
        {
            var floats = _container.GetAllVariables<FloatVariable>().ToList();
            Assert.AreEqual(1, floats.Count);
            Assert.AreEqual("Health", floats[0].name);
        }

        [Test]
        public void GetAllNumericalReturnsNumericalVariables()
        {
            var numericals = _container.GetAllNumerical().ToList();
            Assert.AreEqual(2, numericals.Count); // FloatVariable + IntVariable
        }

        [Test]
        public void GetVariableNamesReturnsAllNames()
        {
            var names = _container.GetVariableNames().ToList();
            Assert.AreEqual(3, names.Count);
            Assert.Contains("Health", names);
            Assert.Contains("Score", names);
            Assert.Contains("IsAlive", names);
        }

        [Test]
        public void VariableCountReturnsCorrectCount()
        {
            Assert.AreEqual(3, _container.VariableCount);
        }

        [Test]
        public void SaveToFileCreatesValidJSON()
        {
            bool success = _container.SaveToFile(_testSavePath);
            Assert.IsTrue(success);
            Assert.IsTrue(File.Exists(_testSavePath));

            string json = File.ReadAllText(_testSavePath);
            Assert.IsTrue(json.Contains("Health"));
            Assert.IsTrue(json.Contains("Score"));
            Assert.IsTrue(json.Contains("IsAlive"));
        }

        [Test]
        public void LoadFromFileRestoresValues()
        {
            // Set and save
            _floatVar.Value = 50f;
            _intVar.Value = 999;
            _boolVar.Value = false;
            _container.SaveToFile(_testSavePath);

            // Reset values
            _floatVar.SetValueWithoutNotify(0f);
            _intVar.SetValueWithoutNotify(0);
            _boolVar.SetValueWithoutNotify(true);

            // Load
            bool success = _container.LoadFromFile(_testSavePath);
            Assert.IsTrue(success);
            Assert.AreEqual(50f, _floatVar.Value);
            Assert.AreEqual(999, _intVar.Value);
            Assert.IsFalse(_boolVar.Value);
        }

        [Test]
        public void LoadFromMissingFileReturnsFalse()
        {
            bool success = _container.LoadFromFile("/nonexistent/path/file.json");
            Assert.IsFalse(success);
        }

        [Test]
        public void ResetAllVariablesResetsToDefaults()
        {
            _floatVar.Value = 999f;
            _intVar.Value = 999;
            _boolVar.Value = true;

            _container.ResetAllVariables();

            Assert.AreEqual(0f, _floatVar.Value);
            Assert.AreEqual(0, _intVar.Value);
            Assert.IsFalse(_boolVar.Value);
        }

        [Test]
        public void RaiseAllVariablesRaisesEach()
        {
            int raiseCount = 0;
            var sub1 = _floatVar.OnRaised.Do(_ => raiseCount++).Subscribe();
            var sub2 = _intVar.OnRaised.Do(_ => raiseCount++).Subscribe();
            var sub3 = _boolVar.OnRaised.Do(_ => raiseCount++).Subscribe();

            _container.RaiseAllVariables();

            Assert.AreEqual(3, raiseCount);
            sub1.Dispose();
            sub2.Dispose();
            sub3.Dispose();
        }

        [Test]
        public void GetVariableByIndexReturnsCorrect()
        {
            var first = _container.GetVariable(0);
            Assert.IsNotNull(first);
        }
    }
}
