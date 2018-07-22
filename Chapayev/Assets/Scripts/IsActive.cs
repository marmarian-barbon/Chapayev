using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsActive : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnEnable()
    {
        this.gameObject.SetActive(ChessBoardManager.GameStarted);
    }
}
