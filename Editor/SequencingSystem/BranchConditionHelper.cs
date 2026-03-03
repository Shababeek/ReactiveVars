using System.Runtime.CompilerServices;
using Shababeek.ReactiveVars;

[assembly: InternalsVisibleTo("Shababeek.ReactiveVars.EditorTests")]

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// Provides typed access to BranchCondition's internal fields for editor and test use.
    /// </summary>
    internal static class BranchConditionHelper
    {
        /// <summary>
        /// Sets the ScriptableVariable on a BranchCondition.
        /// </summary>
        public static void SetVariable(BranchCondition c, ScriptableVariable v) => c.SetVariable(v);

        /// <summary>
        /// Sets the ComparisonType on a BranchCondition.
        /// </summary>
        public static void SetComparison(BranchCondition c, ComparisonType v) => c.SetComparison(v);

        /// <summary>
        /// Gets the bool target value from a BranchCondition.
        /// </summary>
        public static bool GetBool(BranchCondition c) => c.BoolValue;

        /// <summary>
        /// Sets the bool target value on a BranchCondition.
        /// </summary>
        public static void SetBool(BranchCondition c, bool v) => c.BoolValue = v;

        /// <summary>
        /// Gets the int target value from a BranchCondition.
        /// </summary>
        public static int GetInt(BranchCondition c) => c.IntValue;

        /// <summary>
        /// Sets the int target value on a BranchCondition.
        /// </summary>
        public static void SetInt(BranchCondition c, int v) => c.IntValue = v;

        /// <summary>
        /// Gets the float target value from a BranchCondition.
        /// </summary>
        public static float GetFloat(BranchCondition c) => c.FloatValue;

        /// <summary>
        /// Sets the float target value on a BranchCondition.
        /// </summary>
        public static void SetFloat(BranchCondition c, float v) => c.FloatValue = v;

        /// <summary>
        /// Gets the string target value from a BranchCondition.
        /// </summary>
        public static string GetString(BranchCondition c) => c.StringValue;

        /// <summary>
        /// Sets the string target value on a BranchCondition.
        /// </summary>
        public static void SetString(BranchCondition c, string v) => c.StringValue = v;
    }
}
