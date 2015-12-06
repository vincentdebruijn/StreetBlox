using UnityEngine;
using System.Collections;

public class CarDisplayScript : MonoBehaviour {
	
	// private WorldSelectScript worldSelectScript;
	
	void Awake () {
		// worldSelectScript = Camera.main.GetComponent<WorldSelectScript>();
		if (name == MenuScript.data.chosenCar)
			ShowCarIsSelected ();
	}
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0)) {
			if (MenuScript.data.chosenCar != name)
				ShowCarIsSelected();
			MenuScript.data.chosenCar = name;
		}
	}

	private void ShowCarIsSelected() {
		// Remove the selector currently visible
		GameObject obj = GameObject.FindGameObjectWithTag("CarDisplaySelector");
		if (obj != null)
			Destroy (obj);
		Vector3 carPosition = gameObject.transform.position;
		GameObject carDisplaySelector = Resources.Load ("carDisplaySelector") as GameObject;
		Instantiate (carDisplaySelector, new Vector3(carPosition.x, carPosition.y + 0.6f, carPosition.z), Quaternion.Euler (0, 90, 0));
	}
}
