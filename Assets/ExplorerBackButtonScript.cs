using UnityEngine;
using System.Collections;

public class ExplorerBackButtonScript : MonoBehaviour {
	private GameScript gameScript;

	// Use this for initialization
	void Start () {
		gameScript = Camera.main.GetComponent<GameScript> ();
	}

	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0)) {
			MenuScript.PlayButtonSound();
			gameScript.CloseShop();
		}
	}
}
