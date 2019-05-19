using UnityEngine;

public class PlayerTackle : MonoBehaviour
{
    public bool InputKeyDown { get; set; }
    public Vector2 AimDirection { get; set; }

    private bool withRaycast = false;

    //void Update()
    //{
    //    if (!withRaycast || AimDirection.magnitude < 1.0f)
    //        return;

    //    Vector2 origin = (Vector2)transform.parent.position + AimDirection * 1.1f;
    //    Vector2 end = AimDirection * 2.0f;
    //    Debug.DrawRay(origin, end);
    //    Collider2D other = Physics2D.Raycast(origin, end).collider;
    //    if (other != null && other.gameObject.tag == "Player")
    //    {
    //        Debug.Log(transform.parent.gameObject.name + " -> " + other.gameObject.name);
    //        if (other.GetComponent<PlayerManager>().Ball != null && InputKeyDown)
    //        {
    //            Debug.Log(transform.parent.gameObject.name + " [BALL] " + other.gameObject.name);
    //            GameObject ball = other.GetComponent<PlayerManager>().Ball;
    //            other.GetComponent<PlayerManager>().BallLost();
    //            ball.GetComponent<BallBehaviour>().AttachBall(transform.parent.gameObject);
    //            transform.parent.GetComponent<PlayerManager>().BallReceived();
    //        }
    //    }
    //}

    void OnTriggerStay2D(Collider2D other)
    {
        if (withRaycast)
            return;

        if (other.gameObject.tag == "Player")
        {
            //Debug.Log(transform.parent.gameObject.name + " -> " + other.gameObject.name);
            if (other.GetComponent<PlayerManager>().Ball != null && InputKeyDown)
            {
                //Debug.Log(transform.parent.gameObject.name + " [BALL] " + other.gameObject.name);
                GameObject ball = other.GetComponent<PlayerManager>().Ball;
                other.GetComponent<PlayerManager>().BallLost();
                ball.GetComponent<BallBehaviour>().AttachBall(transform.parent.gameObject);
                transform.parent.GetComponent<PlayerManager>().BallReceived();
            }
        }
    }
}
