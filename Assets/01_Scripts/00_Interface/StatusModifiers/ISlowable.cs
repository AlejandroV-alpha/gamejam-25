/// <summary>
/// Interfaz para efectos de ralentización.
/// </summary>
public interface ISlowable
{
    void Slow(float duration, float speedMultiplier);
}