using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class OptionsScript : MonoBehaviour {

	// GUI stuff
	private static Rect leftBottomRect;
	private static Rect option1Rect;
	private static Rect option2Rect;
	private static Rect option3Rect;
	private static Rect option4Rect;
	private static Rect option1TextRect;
	private static Rect option2TextRect;
	private static Rect option3TextRect;
	private static Rect option4TextRect;
	private static Rect resetButtonRect;
	private static Rect resetTextRect;
	
	private static Texture2D optionButtonTexture, optionSelectedButtonTexture;
	private static Texture2D resetButtonTexture;

	private static GUIStyle backButtonChosenStyle;
	private static GUIStyle optionButtonStyle, optionSelectedButtonStyle;
	private static GUIStyle optionButton1ChosenStyle, optionButton2ChosenStyle, optionButton3ChosenStyle, optionButton4ChosenStyle;
	private static GUIStyle optionTextStyle;
	private static GUIStyle resetButtonStyle;

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
			Application.LoadLevel ("menu");
		}

		if (GUI.Button(option1Rect, "", optionButton1ChosenStyle)) {
			MenuScript.PlayButtonSound();
			MenuScript.data.playAnimations = !MenuScript.data.playAnimations;
			if (MenuScript.data.playAnimations) {
				optionButton1ChosenStyle = optionSelectedButtonStyle;

				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car2", 1));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car3", 2));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car4", 3));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car5", 4));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car6", 5));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car7", 6));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car8", 7));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car9", 8));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car10", 9));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car11", 10));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car12", 11));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car13", 12));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car14", 13));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car15", 14));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("car16", 15));

				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("puzzleBoxWorld2", 1));
				MenuScript.data.animationQueue.Enqueue (new Pair<string, int> ("puzzleBoxWorld3", 2));
			} else {
				optionButton1ChosenStyle = optionButtonStyle;
			}
		}
		GUI.Label (option1TextRect, "<removed>", optionTextStyle);

		if (GUI.Button(option2Rect, "", optionButton2ChosenStyle)) {
			MenuScript.PlayButtonSound();
			MenuScript.data.playTutorials = !MenuScript.data.playTutorials;
			if (MenuScript.data.playTutorials) {
				optionButton2ChosenStyle = optionSelectedButtonStyle;
				MenuScript.data.carsUnlocked[0] = false;
				MenuScript.data.puzzleBoxesUnlocked[0] = false;
			} else {
				optionButton2ChosenStyle = optionButtonStyle;
				MenuScript.data.carsUnlocked[0] = true;
				MenuScript.data.puzzleBoxesUnlocked[0] = true;
			}
		}
		GUI.Label (option2TextRect, "Tutorials", optionTextStyle);

		if (GUI.Button(option3Rect, "", optionButton3ChosenStyle)) {
			MenuScript.PlayButtonSound();
			MenuScript.data.playMusic = !MenuScript.data.playMusic;
			if (MenuScript.data.playMusic) {
				MenuScript.PlayMenuMusic();
				optionButton3ChosenStyle = optionSelectedButtonStyle;
			} else {
				MenuScript.StopMenuMusic();
				optionButton3ChosenStyle = optionButtonStyle;
			}
		}
		GUI.Label (option3TextRect, "Music", optionTextStyle);

		if (GUI.Button(option4Rect, "", optionButton4ChosenStyle)) {
			MenuScript.PlayButtonSound();
			MenuScript.data.playSoundEffects = !MenuScript.data.playSoundEffects;
			if (MenuScript.data.playSoundEffects)
				optionButton4ChosenStyle = optionSelectedButtonStyle;
			else
				optionButton4ChosenStyle = optionButtonStyle;
		}
		GUI.Label (option4TextRect, "Sound effects", optionTextStyle);

		GUI.Label (resetTextRect, "Resets puzzle pieces and car position in explorer mode.\nDoes not reset acquired items.", optionTextStyle);
		if (GUI.Button (resetButtonRect, "", resetButtonStyle)) {
			MenuScript.PlayButtonSound();
			MenuScript.data.board = null;
			MenuScript.Save ();
		}
	}

	private static void SetVariables() {
		int buttonSize = (int)(Screen.width / 5 * 0.7);
		float offset = (Screen.width / 5 - buttonSize) / 2;
		leftBottomRect = new Rect (offset, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		option1Rect = new Rect (buttonSize / 4, buttonSize / 4, buttonSize / 3, buttonSize / 3);
		option2Rect = new Rect (buttonSize / 4, buttonSize / 2 + 20, buttonSize / 3, buttonSize / 3);
		option3Rect = new Rect (buttonSize / 4, buttonSize * 3 / 4 + 40, buttonSize / 3, buttonSize / 3);
		option4Rect = new Rect (buttonSize / 4, buttonSize + 60, buttonSize / 3, buttonSize / 3);
		option1TextRect = new Rect (buttonSize / 2 + 30, buttonSize / 4, Screen.width - buttonSize / 2 - 20, buttonSize / 3);
		option2TextRect = new Rect (buttonSize / 2 + 30, buttonSize / 2 + 20, Screen.width - buttonSize / 2 - 20, buttonSize / 3);
		option3TextRect = new Rect (buttonSize / 2 + 30, buttonSize * 3 / 4 + 40, Screen.width - buttonSize / 2 - 20, buttonSize / 3);
		option4TextRect = new Rect (buttonSize / 2 + 30, buttonSize + 60, Screen.width - buttonSize / 2 - 20, buttonSize / 3);
		resetTextRect = new Rect (Screen.width / 2, buttonSize / 4, Screen.width / 2, buttonSize / 3);
		resetButtonRect = new Rect (Screen.width * 0.75f - buttonSize / 2, buttonSize * 0.75f, buttonSize, buttonSize / 2);

		optionButtonTexture = (Texture2D)Resources.Load ("ui_button_unchecked");
		optionSelectedButtonTexture = (Texture2D)Resources.Load ("ui_button_checked");
		resetButtonTexture = (Texture2D)Resources.Load ("ui_button_reset");
		
		optionButtonStyle = new GUIStyle();
		optionButtonStyle.normal.background = optionButtonTexture;
		optionSelectedButtonStyle = new GUIStyle();
		optionSelectedButtonStyle.normal.background = optionSelectedButtonTexture;
		if (MenuScript.data.playAnimations)
			optionButton1ChosenStyle = optionSelectedButtonStyle;
		else
			optionButton1ChosenStyle = optionButtonStyle;
		if (MenuScript.data.playTutorials)
			optionButton2ChosenStyle = optionSelectedButtonStyle;
		else
			optionButton2ChosenStyle = optionButtonStyle;
		if (MenuScript.data.playMusic)
			optionButton3ChosenStyle = optionSelectedButtonStyle;
		else
			optionButton3ChosenStyle = optionButtonStyle;
		if (MenuScript.data.playSoundEffects)
			optionButton4ChosenStyle = optionSelectedButtonStyle;
		else
			optionButton4ChosenStyle = optionButtonStyle;
		
		optionTextStyle = new GUIStyle ();
		optionTextStyle.alignment = TextAnchor.MiddleLeft;
		optionTextStyle.normal.textColor = Color.white;
		optionTextStyle.fontSize = 32;

		resetButtonStyle = new GUIStyle ();
		resetButtonStyle.alignment = TextAnchor.MiddleCenter;
		resetButtonStyle.normal.textColor = Color.black;
		resetButtonStyle.fontSize = 32;
		Texture2D texture = new Texture2D (1, 1, TextureFormat.RGBA32, false);
		texture.SetPixel (0, 0, new Color (0, 0, 0, 0.75f));
		resetButtonStyle.normal.background = resetButtonTexture;

		backButtonChosenStyle = MenuScript.backButtonStyle;
	}
}
