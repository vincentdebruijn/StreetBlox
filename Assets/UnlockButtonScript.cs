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
				String carName = "";
				switch(carIndex) {
					case 1: carName = "car2"; break;
					case 2: carName = "car3"; break;
					case 3: carName = "car4"; break;
					case 4: carName = "car5"; break;
					case 5: carName = "car6"; break;
					case 6: carName = "car7"; break;
					case 7: carName = "car8"; break;
					case 8: carName = "car11"; break;
					default: 
						throw new ArgumentException("Unknown carIndex: " + carIndex);
				}

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
