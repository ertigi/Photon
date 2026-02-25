using UnityEngine;

public class KeyboardInputProvider : IInputProvider
{
    public Vector2 ReadMove()
    {
        float x = Input.GetAxisRaw("Vertical");
        float y = Input.GetAxisRaw("Horizontal");
        var direction = new Vector2(x, y);

        return direction;
    }
}