using UnityEngine;

public class KeyboardInputProvider : IInputProvider
{
    public Vector2 ReadMove()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        var direction = new Vector2(horizontal, vertical);

        return direction;
    }
}
