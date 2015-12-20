﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class puzzleBoxScript : MonoBehaviour {

	private Animator anim;
	private WorldSelectScript worldSelectScript;

	private Boolean startedAnimation = false;
	private Boolean animationReallyStarted = false;
	private int worldNumber;

	private Vector3 cameraPosition = new Vector3 (-2f, 4f, -2.5f);

	// Mutex lock for ensuring only one puzzlebox animation is playing at a time.
	public static Boolean animationLock;
	private Boolean moving;
	private Boolean selectedWorld;
	
	void Awake () {
		selectedWorld = false;
		animationLock = false;
		worldSelectScript = Camera.main.GetComponent<WorldSelectScript>();

		switch(gameObject.name) {
			case "puzzleBoxWorld1": worldNumber = 1; break;
			case "puzzleBoxWorld2": worldNumber = 2; break;
			case "puzzleBoxWorld3": worldNumber = 3; break;
			default: throw new System.ArgumentException("Undefined puzzle box name", gameObject.name);
		}
	}
	
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (moving) {
			transform.position = Vector3.MoveTowards (transform.position, cameraPosition, 10f * Time.deltaTime);
			Vector3 newScale = transform.localScale;
			newScale.x = newScale.x + 12f * Time.deltaTime;
			newScale.y = newScale.y + 12f * Time.deltaTime;
			newScale.z = newScale.z + 12f * Time.deltaTime;
			transform.localScale = newScale;
		}
		
		if (!selectedWorld && !animationReallyStarted && !startedAnimation && Math.Abs (transform.position.z - cameraPosition.z) < 0.01f) {
			moving = false;
			anim.Play ("open");
			startedAnimation = true;
		}
		
		if (!selectedWorld && startedAnimation && anim.GetCurrentAnimatorStateInfo (0).IsName ("open")) {
			startedAnimation = false;
			animationReallyStarted = true;
		}

		if (!selectedWorld && animationReallyStarted && !anim.GetCurrentAnimatorStateInfo (0).IsName ("open")) {
			animationReallyStarted = false;
			selectedWorld = true;
			worldSelectScript.SelectedWorld (worldNumber);
		}
	}

	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0) && !animationLock) {
			animationLock = true;
			moving = true;
			PlayBoxOpenSound();
		}
	}

	public void PlayBoxOpenSound() {
		if (MenuScript.data.playSoundEffects)
			GetComponent<AudioSource> ().Play ();
	}
}
