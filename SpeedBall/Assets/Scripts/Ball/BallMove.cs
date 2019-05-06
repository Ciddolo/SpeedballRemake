using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BallBehaviour))]
public class BallMove : MonoBehaviour
{
    private const float DEFAULT_FRICTION = 250.0f;

    private Rigidbody2D rigidBody2D;

    public Vector2 Velocity { get { return rigidBody2D.velocity; } set { rigidBody2D.velocity = value; } }
    public Vector2 Direction { get; set; }
    public float Force { get; set; }
    public float Friction { get; set; }

    private Vector2 velocity;

    void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        Friction = DEFAULT_FRICTION;
    }

    void FixedUpdate()
    {
        Velocity = velocity;
    }

    void Update()
    {
        velocity = Direction * Force * Time.deltaTime;

        if (Force > 0.0f)
            Force -= Friction * Time.deltaTime;
    }
}
