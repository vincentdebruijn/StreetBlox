using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class puzzleBoxScript : MonoBehaviour {

	private Animator anim;
	private WorldSelectScript worldSelectScript;

	private Boolean startedAnimation = false;
	private Boolean animationReallyStarted = false;
	private int worldNumber;

	// Mutex lock for ensuring only one puzzlebox animation is playing at a time.
	private static Boolean animationLock;
	
	void Awake () {
		animationLock = false;
		worldSelectScript = Camera.main.GetComponent<WorldSelectScript>();

		switch(gameObject.name) {
			case "puzzleBoxWorld1": worldNumber = 1; break;
			case "puzzleBoxWorld2": worldNumber = 2; break;
			default: throw new System.ArgumentException("Undefined puzzle box name", gameObject.name);
		}
	}
	
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (startedAnimation && anim.GetCurrentAnimatorStateInfo (0).IsName ("open")) {
			startedAnimation = false;
			animationReallyStarted = true;
		}
		if (animationReallyStarted && !anim.GetCurrentAnimatorStateInfo (0).IsName ("open")) {
			animationReallyStarted = false;
			animationLock = false;
			worldSelectScript.SelectedWorld (worldNumber);
		}
	}

	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0) && !animationLock) {
			animationLock = true;
			anim.Play ("open");
			startedAnimation = true;
		}
	}
}
