using UnityEngine;
using System;
using System.Collections;

public class UnlockButtonScript : MonoBehaviour {
	public const int COST = 1;

	public int carIndex;

	private GameScript gameScript;

	// Use this for initialization
	void Start () {
		gameScript = Camera.main.GetComponent<GameScript> ();
	}

	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0)) {
			if (MenuScript.data.marbles >= COST && !MenuScript.data.carsUnlocked[carIndex]) {
				SetToBought();
				MenuScript.data.marbles -= COST;
				String carName = "car" + (carIndex + 1);

				gameScript.ShowFirstItemObtainedMessageIfIsFirstItem ();
				MenuScript.data.animationQueue.Enqueue(new Pair<string, int>(carName, carIndex));
				MenuScript.Save();
				gameScript.SetMarbleText();
			}
		}
	}

	public void SetToBought() {
		GetComponentInChildren<TextMesh> ().text = "\nBOUGHT";
		GetComponentInChildren<TextMesh> ().color = Color.red;
	}
}
