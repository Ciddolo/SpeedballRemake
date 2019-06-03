using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public TeamManager Team;

    public uint NetId { get; set; }
    //public int Index { get; set; }
    public bool IsSelected { get; set; }
    public GameObject Ball { get; set; }

    private PlayerMove move;
    private GameObject selection;

    void Start()
    {
        move = gameObject.GetComponent<PlayerMove>();
        selection = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        selection.SetActive(IsSelected);
    }

    //public void BallReceived()
    //{
    //    Team.SelectPlayer(gameObject);
    //    Team.IsInBallPossession = true;
    //}

    //public void BallThrown()
    //{
    //    Team.IsInBallPossession = false;
    //}

    //public void BallLost()
    //{
    //    Team.IsInBallPossession = false;
    //    Ball.GetComponent<BallBehaviour>().RemoveBall();
    //}
}
