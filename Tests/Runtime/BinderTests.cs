using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Shababeek.ReactiveVars.Tests
{
    [TestFixture]
    public class GameObjectActiveBinderTests
    {
        private GameObject _go;
        private BoolVariable _boolVar;

        [SetUp]
        public void SetUp()
        {
            _boolVar = ScriptableObject.CreateInstance<BoolVariable>();
            _boolVar.SetValueWithoutNotify(true);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.Destroy(_go);
        }

        private GameObjectActiveBinder CreateBinder(bool invert = false, GameObject target = null)
        {
            _go = new GameObject("BinderTestGO");
            _go.SetActive(false);
            var binder = _go.AddComponent<GameObjectActiveBinder>();

            var variableField = typeof(GameObjectActiveBinder).GetField("variable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            variableField?.SetValue(binder, _boolVar);

            var invertField = typeof(GameObjectActiveBinder).GetField("invert",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            invertField?.SetValue(binder, invert);

            if (target != null)
            {
                var targetField = typeof(GameObjectActiveBinder).GetField("targetObject",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                targetField?.SetValue(binder, target);
            }

            _go.SetActive(true);
            return binder;
        }

        [UnityTest]
        public IEnumerator BinderSetsActiveStateOnEnable()
        {
            _boolVar.SetValueWithoutNotify(true);
            var binder = CreateBinder();
            yield return null;

            Assert.IsTrue(_go.activeSelf);
        }

        [UnityTest]
        public IEnumerator BinderReactsToVariableChange()
        {
            var target = new GameObject("Target");
            _boolVar.SetValueWithoutNotify(true);
            var binder = CreateBinder(false, target);
            yield return null;

            Assert.IsTrue(target.activeSelf);

            _boolVar.Value = false;
            yield return null;

            Assert.IsFalse(target.activeSelf);

            Object.Destroy(target);
        }

        [UnityTest]
        public IEnumerator BinderWithTargetObjectSetsTargetActive()
        {
            var target = new GameObject("Target");
            _boolVar.SetValueWithoutNotify(false);
            var binder = CreateBinder(false, target);
            yield return null;

            Assert.IsFalse(target.activeSelf);

            _boolVar.Value = true;
            yield return null;

            Assert.IsTrue(target.activeSelf);

            Object.Destroy(target);
        }

        [UnityTest]
        public IEnumerator BinderInvertFlagWorks()
        {
            var target = new GameObject("Target");
            _boolVar.SetValueWithoutNotify(true);
            var binder = CreateBinder(true, target);
            yield return null;

            // Invert: true becomes false
            Assert.IsFalse(target.activeSelf);

            _boolVar.Value = false;
            yield return null;

            // Invert: false becomes true
            Assert.IsTrue(target.activeSelf);

            Object.Destroy(target);
        }
    }

    [TestFixture]
    public class VariableBinderBaseTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.Destroy(_go);
        }

        [UnityTest]
        public IEnumerator NullVariableLogsWarning()
        {
            _go = new GameObject("BinderTestGO");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Variable is not assigned"));
            var binder = _go.AddComponent<GameObjectActiveBinder>();
            // Variable is null by default, OnEnable logs warning

            yield return null;
        }

        [UnityTest]
        public IEnumerator OnDisableDisposesSubscription()
        {
            var boolVar = ScriptableObject.CreateInstance<BoolVariable>();
            boolVar.SetValueWithoutNotify(true);

            var target = new GameObject("Target");
            _go = new GameObject("BinderTestGO");
            _go.SetActive(false);
            var binder = _go.AddComponent<GameObjectActiveBinder>();

            var variableField = typeof(GameObjectActiveBinder).GetField("variable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            variableField?.SetValue(binder, boolVar);

            var targetField = typeof(GameObjectActiveBinder).GetField("targetObject",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            targetField?.SetValue(binder, target);

            _go.SetActive(true);
            yield return null;
            Assert.IsTrue(target.activeSelf);

            // Disable the binder
            binder.enabled = false;
            yield return null;

            // Change the variable - binder should not react
            boolVar.Value = false;
            yield return null;

            // Target should still be true since binder is disabled
            Assert.IsTrue(target.activeSelf);

            // Re-enable should re-subscribe and apply current value
            binder.enabled = true;
            yield return null;

            Assert.IsFalse(target.activeSelf);

            Object.Destroy(target);
        }
    }
}
