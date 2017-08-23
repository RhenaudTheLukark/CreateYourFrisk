using UnityEngine;

/// <summary>
/// RectTransform utility class for functions Unitale uses a lot, but aren't in natively as they're specific.
/// </summary>
public static class RTUtil {
    /// <summary>
    /// Get the center of a RectTransform in absolute screen coordinates (in pixels).
    /// </summary>
    /// <param name="rt">RectTransform you want the center of</param>
    /// <returns>Screen coordinates of the RectTransform's center</returns>
    public static Vector2 AbsCenterOf(RectTransform rt) { return new Vector2(rt.position.x + rt.rect.width * (0.5f - rt.pivot.x), rt.position.y + rt.rect.height * (0.5f - rt.pivot.y)); }
}
