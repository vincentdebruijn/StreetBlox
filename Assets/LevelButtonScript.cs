using UnityEngine;
using System;
using System.Collections;

public class LevelButtonScript : MonoBehaviour {
	private LevelSelectScript levelSelectScript;

	public string levelName;

	private Boolean clicked;
	private float time;
	
	void Awake () {
		levelSelectScript = Camera.main.GetComponent<LevelSelectScript> ();
		time = 0f;
	}

	void Update() {
		time += Time.deltaTime;
		if (time >= 0.1f)
			clicked = false;
	}

	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0)) {
			clicked = true;
			time = 0f;
		}
		if (Input.GetMouseButtonUp (0) && clicked && time < 0.1f) {
			clicked = false;
			levelSelectScript.SelectedLevel (levelName);
		}
	}
}
