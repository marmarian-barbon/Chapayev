using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ChessBoardManager.CheckerOutOfGame(other.gameObject);
    }
}
