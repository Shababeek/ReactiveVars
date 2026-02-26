using System.Collections;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

namespace Shababeek.ReactiveVars.Tests
{
    [TestFixture]
    public class VariableResetterTests
    {
        private GameObject _go;
        private FloatVariable _floatVar;
        private IntVariable _intVar;
        private BoolVariable _boolVar;

        [SetUp]
        public void SetUp()
        {
            _floatVar = ScriptableObject.CreateInstance<FloatVariable>();
            _floatVar.SetValueWithoutNotify(100f);

            _intVar = ScriptableObject.CreateInstance<IntVariable>();
            _intVar.SetValueWithoutNotify(42);

            _boolVar = ScriptableObject.CreateInstance<BoolVariable>();
            _boolVar.SetValueWithoutNotify(true);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.Destroy(_go);
        }

        private VariableResetter CreateResetter()
        {
            _go = new GameObject("ResetterTestGO");
            _go.SetActive(false);
            var resetter = _go.AddComponent<VariableResetter>();

            var variablesField = typeof(VariableResetter).GetField("variables",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var list = new System.Collections.Generic.List<ScriptableVariable>
            {
                _floatVar,
                _intVar,
                _boolVar
            };
            variablesField?.SetValue(resetter, list);

            _go.SetActive(true);
            return resetter;
        }

        [UnityTest]
        public IEnumerator SnapshotsValuesOnAwake()
        {
            var resetter = CreateResetter();
            yield return null;

            // Change values at runtime
            _floatVar.Value = 0f;
            _intVar.Value = 0;
            _boolVar.Value = false;

            // Verify values changed
            Assert.AreEqual(0f, _floatVar.Value);
            Assert.AreEqual(0, _intVar.Value);
            Assert.IsFalse(_boolVar.Value);

            yield return null;
        }

        [UnityTest]
        public IEnumerator RestoresValuesOnDestroy()
        {
            var resetter = CreateResetter();
            yield return null;

            // Modify values
            _floatVar.Value = 0f;
            _intVar.Value = 999;
            _boolVar.Value = false;

            // Destroy the resetter (triggers OnDestroy -> RestoreSnapshot)
            Object.Destroy(_go);
            _go = null;
            yield return null;

            // Values should be restored to snapshot values
            Assert.AreEqual(100f, _floatVar.Value);
            Assert.AreEqual(42, _intVar.Value);
            Assert.IsTrue(_boolVar.Value);
        }

        [UnityTest]
        public IEnumerator NullVariablesAreSkipped()
        {
            _go = new GameObject("ResetterTestGO");
            _go.SetActive(false);
            var resetter = _go.AddComponent<VariableResetter>();

            var variablesField = typeof(VariableResetter).GetField("variables",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var list = new System.Collections.Generic.List<ScriptableVariable>
            {
                _floatVar,
                null,
                _intVar
            };
            variablesField?.SetValue(resetter, list);

            _go.SetActive(true);
            yield return null;

            _floatVar.Value = 0f;
            _intVar.Value = 0;

            Object.Destroy(_go);
            _go = null;
            yield return null;

            Assert.AreEqual(100f, _floatVar.Value);
            Assert.AreEqual(42, _intVar.Value);
        }
    }

    [TestFixture]
    public class VariableLoggerTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.Destroy(_go);
        }

        [UnityTest]
        public IEnumerator LogsValueChangesToConsole()
        {
            var floatVar = ScriptableObject.CreateInstance<FloatVariable>();
            floatVar.name = "Health";
            floatVar.SetValueWithoutNotify(100f);

            _go = new GameObject("LoggerTestGO");
            _go.SetActive(false);
            var logger = _go.AddComponent<VariableLogger>();

            var variablesField = typeof(VariableLogger).GetField("variables",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            variablesField?.SetValue(logger, new System.Collections.Generic.List<ScriptableVariable> { floatVar });

            var prefixField = typeof(VariableLogger).GetField("logPrefix",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prefixField?.SetValue(logger, "Test");

            _go.SetActive(true);
            yield return null;

            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(@"\[Test_Health\] = 50"));
            floatVar.Value = 50f;

            yield return null;
        }

        [UnityTest]
        public IEnumerator LogsWithoutPrefixOmitsUnderscore()
        {
            var intVar = ScriptableObject.CreateInstance<IntVariable>();
            intVar.name = "Score";
            intVar.SetValueWithoutNotify(0);

            _go = new GameObject("LoggerTestGO");
            _go.SetActive(false);
            var logger = _go.AddComponent<VariableLogger>();

            var variablesField = typeof(VariableLogger).GetField("variables",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            variablesField?.SetValue(logger, new System.Collections.Generic.List<ScriptableVariable> { intVar });

            var prefixField = typeof(VariableLogger).GetField("logPrefix",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prefixField?.SetValue(logger, "");

            _go.SetActive(true);
            yield return null;

            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(@"\[Score\] = 10"));
            intVar.Value = 10;

            yield return null;
        }

        [UnityTest]
        public IEnumerator UsesCorrectLogLevel()
        {
            var boolVar = ScriptableObject.CreateInstance<BoolVariable>();
            boolVar.name = "Flag";
            boolVar.SetValueWithoutNotify(false);

            _go = new GameObject("LoggerTestGO");
            _go.SetActive(false);
            var logger = _go.AddComponent<VariableLogger>();

            var variablesField = typeof(VariableLogger).GetField("variables",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            variablesField?.SetValue(logger, new System.Collections.Generic.List<ScriptableVariable> { boolVar });

            var logLevelField = typeof(VariableLogger).GetField("logLevel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            logLevelField?.SetValue(logger, VariableLogger.LogLevel.Warning);

            _go.SetActive(true);
            yield return null;

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"\[Flag\] = True"));
            boolVar.Value = true;

            yield return null;
        }
    }
}
