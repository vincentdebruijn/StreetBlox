using UnityEngine;
using System;
using System.Collections;

public class TutorialScript : MonoBehaviour {
	private WorldSelectScript worldSelectScript;
	
	void Awake () {
		worldSelectScript = Camera.main.GetComponent<WorldSelectScript> ();
	}

	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0)) {
			worldSelectScript.SelectedWorld (0);
		}
	}
}
