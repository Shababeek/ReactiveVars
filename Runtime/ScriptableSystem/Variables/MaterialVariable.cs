using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Scriptable variable that stores a Material reference.
    /// </summary>
    [CreateAssetMenu(menuName = "ReactiveVars/Variables/MaterialVariable")]
    public class MaterialVariable : ScriptableVariable<Material>
    {
        public static bool operator ==(MaterialVariable a, MaterialVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(MaterialVariable a, MaterialVariable b) => !(a == b);

        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// A reference that can point to either a MaterialVariable or use a constant Material value.
    /// </summary>
    [System.Serializable]
    public class MaterialReference : VariableReference<Material>
    {
    }
}
