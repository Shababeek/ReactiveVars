using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Scriptable variable that stores a Sprite reference.
    /// </summary>
    [CreateAssetMenu(menuName = "ReactiveVars/Variables/SpriteVariable")]
    public class SpriteVariable : ScriptableVariable<Sprite>
    {
        public static bool operator ==(SpriteVariable a, SpriteVariable b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(SpriteVariable a, SpriteVariable b) => !(a == b);

        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// A reference that can point to either a SpriteVariable or use a constant Sprite value.
    /// </summary>
    [System.Serializable]
    public class SpriteReference : VariableReference<Sprite>
    {
    }
}
