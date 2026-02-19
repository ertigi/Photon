using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;

    public override void FixedUpdateNetwork()
    {
        // Двигаем объект только на стороне StateAuthority (обычно это сервер/хост)
        if (!HasStateAuthority)
            return;

        if (GetInput(out InputData input))
        {
            Vector2 move = input.Move;
            Vector3 dir = new Vector3(move.x, 0f, move.y);

            // Runner.DeltaTime — сетевой дельта-тайм
            transform.position += dir * (_speed * Runner.DeltaTime);
        }
    }
}
