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
			String carName = "car" + (carIndex + 1);
			Pair pair = new Pair (carName, carIndex);
			if (MenuScript.data.marbles >= COST && !MenuScript.data.carsUnlocked[carIndex] && !MenuScript.data.animationQueue.Contains(pair)) {
				SetToBought();
				MenuScript.data.marbles -= COST;

				gameScript.ShowFirstItemObtainedMessageIfIsFirstItem ();
				MenuScript.data.animationQueue.Enqueue(pair);
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
