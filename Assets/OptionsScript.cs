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
	
	private static Texture2D optionButtonTexture, optionSelectedButtonTexture;

	private static GUIStyle backButtonChosenStyle;
	private static GUIStyle optionButtonStyle, optionSelectedButtonStyle;
	private static GUIStyle optionButton1ChosenStyle, optionButton2ChosenStyle, optionButton3ChosenStyle, optionButton4ChosenStyle;
	private static GUIStyle optionTextStyle;

	// Dynamic one time loading of all static variables
	private static Boolean staticVariablesSet = false;

	void Awake() {
		if (!staticVariablesSet) {
			SetVariables ();
			staticVariablesSet = true;
		}
	}

	// Use this for initialization
	void Start () {
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
			if (MenuScript.data.playAnimations)
				optionButton1ChosenStyle = optionSelectedButtonStyle;
			else
				optionButton1ChosenStyle = optionButtonStyle;
		}
		GUI.Label (option1TextRect, "Animations", optionTextStyle);

		if (GUI.Button(option2Rect, "", optionButton2ChosenStyle)) {
			MenuScript.PlayButtonSound();
			MenuScript.data.playTutorials = !MenuScript.data.playTutorials;
			if (MenuScript.data.playTutorials)
				optionButton2ChosenStyle = optionSelectedButtonStyle;
			else
				optionButton2ChosenStyle = optionButtonStyle;
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

		optionButtonTexture = (Texture2D)Resources.Load ("ui_button_unchecked");
		optionSelectedButtonTexture = (Texture2D)Resources.Load ("ui_button_checked");
		
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

		backButtonChosenStyle = MenuScript.backButtonStyle;
	}
}
