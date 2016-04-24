using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.UI;

public class OptionsScript : MonoBehaviour {

	/*
		MenuScript.data.animationQueue.Enqueue (new Pair ("car2", 1));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car3", 2));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car4", 3));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car5", 4));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car6", 5));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car7", 6));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car8", 7));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car9", 8));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car10", 9));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car11", 10));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car12", 11));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car13", 12));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car14", 13));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car15", 14));
		MenuScript.data.animationQueue.Enqueue (new Pair ("car16", 15));

		MenuScript.data.animationQueue.Enqueue (new Pair ("puzzleBoxWorld2", 1));
		MenuScript.data.animationQueue.Enqueue (new Pair ("puzzleBoxWorld3", 2));
	*/

	// GUI stuff
	private static Rect leftBottomRect;
	private static Rect option1Rect;
	private static Rect option2Rect;
	private static Rect option3Rect;
	private static Rect option1TextRect;
	private static Rect option2TextRect;
	private static Rect option3TextRect;
	private static Rect resetButtonRect;
	private static Rect resetTextRect;
	private static Rect creditsRect;
	private static Rect secretRect;
	
	private static Texture2D optionButtonTexture, optionSelectedButtonTexture;
	private static Texture2D resetButtonTexture;

	private static GUIStyle backButtonChosenStyle;
	private static GUIStyle optionButtonStyle, optionSelectedButtonStyle;
	private static GUIStyle optionButton1ChosenStyle, optionButton2ChosenStyle, optionButton3ChosenStyle;
	private static GUIStyle optionTextStyle;
	private static GUIStyle resetButtonStyle;
	private static GUIStyle creditsStyle;

	private static Boolean secretTriggered;

	// Dynamic one time loading of all static variables
	private static Boolean staticVariablesSet = false;

	void Awake() {
		if (!staticVariablesSet) {
			SetVariables ();
			staticVariablesSet = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0)) {
			Vector3 mousePosition = new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 0);
			if (leftBottomRect.Contains (mousePosition))
				backButtonChosenStyle = MenuScript.backButtonPressedStyle;
		}
	}

	void OnGUI() {
		if (GUI.Button (leftBottomRect, "", backButtonChosenStyle)) {
			backButtonChosenStyle = MenuScript.backButtonStyle;
			MenuScript.PlayButtonSound();
			SceneManager.LoadScene ("menu");
		}

		if (GUI.Button(option1Rect, "", optionButton1ChosenStyle)) {
			MenuScript.PlayButtonSound();
			MenuScript.data.playTutorials = !MenuScript.data.playTutorials;
			if (MenuScript.data.playTutorials) {
				optionButton1ChosenStyle = optionSelectedButtonStyle;
				MenuScript.data.carsUnlocked[0] = false;
				MenuScript.data.puzzleBoxesUnlocked[0] = false;
			} else {
				optionButton1ChosenStyle = optionButtonStyle;
				MenuScript.data.carsUnlocked[0] = true;
				MenuScript.data.puzzleBoxesUnlocked[0] = true;
			}
		}
		GUI.Label (option1TextRect, "Tutorials", optionTextStyle);

		if (GUI.Button(option2Rect, "", optionButton2ChosenStyle)) {
			MenuScript.data.playMusic = !MenuScript.data.playMusic;
			if (MenuScript.data.playMusic) {
				MenuScript.PlayMenuMusic();
				optionButton2ChosenStyle = optionSelectedButtonStyle;
			} else {
				MenuScript.StopMenuMusic();
				optionButton2ChosenStyle = optionButtonStyle;
			}
		}
		GUI.Label (option2TextRect, "Music", optionTextStyle);

		if (GUI.Button(option3Rect, "", optionButton3ChosenStyle)) {
			MenuScript.data.playSoundEffects = !MenuScript.data.playSoundEffects;
			MenuScript.PlayButtonSound();
			if (MenuScript.data.playSoundEffects)
				optionButton3ChosenStyle = optionSelectedButtonStyle;
			else
				optionButton3ChosenStyle = optionButtonStyle;
		}
		GUI.Label (option3TextRect, "Sound effects", optionTextStyle);

		GUI.Label (resetTextRect, "Resets puzzle pieces and car position in explorer mode.\nDoes not reset acquired items.", optionTextStyle);
		if (GUI.Button (resetButtonRect, "", resetButtonStyle)) {
			MenuScript.PlayButtonSound();
			MenuScript.data.board = null;
			MenuScript.Save ();
		}

		GUI.Label (creditsRect, "Credits:\nA small learning project,\nMade with Unity and Maya.\nMusic and sfx thanks to FreeSFX\nSome altered imagery from DeviantArt and others\nInspired by Street Shuffle (1994)", optionTextStyle);

		if (!secretTriggered && GUI.Button(secretRect, "", optionTextStyle)) {
			if (!MenuScript.data.puzzleBoxesUnlocked[1])
				MenuScript.data.animationQueue.Enqueue (new Pair ("puzzleBoxWorld2", 1));
			if (!MenuScript.data.puzzleBoxesUnlocked[2])
				MenuScript.data.animationQueue.Enqueue (new Pair ("puzzleBoxWorld3", 2));
			secretTriggered = true;
		}
	}

	private static void SetVariables() {
		int buttonSize = (int)(Screen.width / 5 * 0.7);
		float offset = (Screen.width / 5 - buttonSize) / 2;
		leftBottomRect = new Rect (offset, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		option1Rect = new Rect (buttonSize / 4, buttonSize / 4, buttonSize / 3, buttonSize / 3);
		option2Rect = new Rect (buttonSize / 4, buttonSize / 2 + 20, buttonSize / 3, buttonSize / 3);
		option3Rect = new Rect (buttonSize / 4, buttonSize * 3 / 4 + 40, buttonSize / 3, buttonSize / 3);
		option1TextRect = new Rect (buttonSize / 2 + 30, buttonSize / 4, Screen.width - buttonSize / 2 - 20, buttonSize / 3);
		option2TextRect = new Rect (buttonSize / 2 + 30, buttonSize / 2 + 20, Screen.width - buttonSize / 2 - 20, buttonSize / 3);
		option3TextRect = new Rect (buttonSize / 2 + 30, buttonSize * 3 / 4 + 40, Screen.width - buttonSize / 2 - 20, buttonSize / 3);
		resetTextRect = new Rect (Screen.width / 2, buttonSize / 4, Screen.width / 2, buttonSize / 3);
		resetButtonRect = new Rect (Screen.width * 0.75f - buttonSize / 2, buttonSize * 0.75f, buttonSize, buttonSize / 2);
		creditsRect = new Rect (Screen.width / 2, Screen.height / 3 * 2, Screen.width / 2, buttonSize / 3);
		secretRect = new Rect (Screen.width - 20, Screen.height - 20, 20, 20);

		optionButtonTexture = (Texture2D)Resources.Load ("ui_button_unchecked");
		optionSelectedButtonTexture = (Texture2D)Resources.Load ("ui_button_checked");
		resetButtonTexture = (Texture2D)Resources.Load ("ui_button_reset");
		
		optionButtonStyle = new GUIStyle();
		optionButtonStyle.normal.background = optionButtonTexture;
		optionSelectedButtonStyle = new GUIStyle();
		optionSelectedButtonStyle.normal.background = optionSelectedButtonTexture;
		if (MenuScript.data.playTutorials)
			optionButton1ChosenStyle = optionSelectedButtonStyle;
		else
			optionButton1ChosenStyle = optionButtonStyle;
		if (MenuScript.data.playMusic)
			optionButton2ChosenStyle = optionSelectedButtonStyle;
		else
			optionButton2ChosenStyle = optionButtonStyle;
		if (MenuScript.data.playSoundEffects)
			optionButton3ChosenStyle = optionSelectedButtonStyle;
		else
			optionButton3ChosenStyle = optionButtonStyle;
		
		optionTextStyle = new GUIStyle ();
		optionTextStyle.alignment = TextAnchor.MiddleLeft;
		optionTextStyle.normal.textColor = Color.black;
		optionTextStyle.fontSize = 32;

		resetButtonStyle = new GUIStyle ();
		resetButtonStyle.alignment = TextAnchor.MiddleCenter;
		resetButtonStyle.normal.textColor = Color.black;
		resetButtonStyle.fontSize = 32;
		Texture2D texture = new Texture2D (1, 1, TextureFormat.RGBA32, false);
		texture.SetPixel (0, 0, new Color (0, 0, 0, 0.75f));
		resetButtonStyle.normal.background = resetButtonTexture;

		secretTriggered = false;

		backButtonChosenStyle = MenuScript.backButtonStyle;
	}
}
