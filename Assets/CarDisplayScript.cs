using UnityEngine;
using System.Collections;

public class CarDisplayScript : MonoBehaviour {
	
	private WorldSelectScript worldSelectScript;
	private string name;
	
	void Awake () {
		worldSelectScript = Camera.main.GetComponent<WorldSelectScript>();
		name = gameObject.name;
	}
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0)) {
			MenuScript.data.chosenCar = name;
		}
	}
}
