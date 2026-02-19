using UnityEngine;

public class KeyboardInputProvider : IInputProvider
{
    public Vector2 ReadMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        var v = new Vector2(x, y);

        return v.sqrMagnitude > 1f ? v.normalized : v;
    }
}