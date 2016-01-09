using UnityEngine;
using System;
using System.Collections;

public class CarDisplayScript : MonoBehaviour {
	private static GameObject carDisplaySelector;

	public Boolean shopCar = false;
	public Boolean turn = false;

	void Awake () {
		if (carDisplaySelector == null && !shopCar)
			carDisplaySelector = Resources.Load ("carDisplaySelector") as GameObject;
	}

	void Update () {
		if (shopCar || turn)
			transform.RotateAround(transform.position, transform.up, Time.deltaTime * 45f);
	}
	
	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0) && !WorldSelectScript.showingAnimations) {
			if (MenuScript.data.playSoundEffects)
				GetComponent<AudioSource> ().Play ();
			if (!shopCar) {
				if (MenuScript.data.chosenCar != name)
					ShowCarIsSelected();
				MenuScript.data.chosenCar = name;
			}
		}
	}

	public void SelectCarIfItIsChosen() {
		if (name == MenuScript.data.chosenCar)
			ShowCarIsSelected ();
	}

	public void ShowCarIsSelected() {
		// Remove the selector currently visible
		GameObject obj = GameObject.FindGameObjectWithTag("CarDisplaySelector");
		if (obj != null)
			Destroy (obj);
		Vector3 carPosition = gameObject.transform.position;
		Instantiate (carDisplaySelector, new Vector3(carPosition.x, carPosition.y + 0.6f, carPosition.z), Quaternion.Euler (0, 90, 0));
	}
}
