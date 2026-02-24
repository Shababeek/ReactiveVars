namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Interface for objects that can be tweened over time by a VariableTweener.
    /// </summary>
    public interface ITweenable
    {
        /// <summary>Advances the tween by one step. Returns true when complete.</summary>
        bool Tween(float scaledDeltaTime);
    }
}
