﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {
	
	public static GameObject canvas;
	public static Color originalCanvasColor = new Color(0.220f, 0.2f, 0.157f, 0.25f);
	public static PlayerData data;

	// GUI stuff
	public static Texture2D logoTexture;
	public static Texture2D optionButtonTexture, optionButtonPressedTexture;
	public static Texture2D selectButtonTexture, selectButtonPressedTexture;
	public static Texture2D exitButtonTexture, exitButtonPressedTexture;
	public static Texture2D vinliaTexture;
	public static Texture2D backButtonTexture, backButtonPressedTexture;

	public static GUIStyle optionButtonStyle, optionButtonPressedStyle, optionButtonChosenStyle;
	public static GUIStyle selectButtonStyle, selectButtonPressedStyle, selectButtonChosenStyle;
	public static GUIStyle exitButtonStyle, exitButtonPressedStyle, exitButtonChosenStyle;
	public static GUIStyle vinliaStyle;
	public static GUIStyle backButtonStyle, backButtonPressedStyle;
	public static GUIStyle logoStyle;

	public static GameObject soundButton;
	public static GameObject soundPuzzlePiece;
	public static GameObject soundGearShift;
	public static GameObject soundMenu;

	private static Rect logoRect;
	private static Rect leftButtonRect;
	private static Rect middleButtonRect;
	private static Rect rightButtonRect;
	private static Rect vinliaRect;

	// Dynamic one time loading of all static variables
	private static Boolean staticVariablesSet = false;

	void Awake() {
		if (!staticVariablesSet) {
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
			SetVariables ();
			Load ();
			PlayMenuMusic();
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
			if (leftButtonRect.Contains (mousePosition))
				optionButtonChosenStyle = optionButtonPressedStyle;
			if (middleButtonRect.Contains(mousePosition))
			    selectButtonChosenStyle = selectButtonPressedStyle;
			if (rightButtonRect.Contains(mousePosition))
				exitButtonChosenStyle = exitButtonPressedStyle;
		}
	}

	void OnGUI() {
		if (GUI.Button(logoRect, "", logoStyle)) {
			Application.OpenURL("https://www.facebook.com/vinliagames");
		}
		if (GUI.Button (vinliaRect, "", vinliaStyle)) {
			Application.OpenURL("https://www.facebook.com/vinliagames");
		}
		if (GUI.Button (leftButtonRect, "", optionButtonChosenStyle)) {
			optionButtonChosenStyle = optionButtonStyle;
			PlayButtonSound();
			Application.LoadLevel ("options");
		}
		if (GUI.Button (middleButtonRect, "", selectButtonChosenStyle)) {
			selectButtonChosenStyle = selectButtonStyle;
			PlayButtonSound();
			Application.LoadLevel ("world_select");
		}
		if (GUI.Button (rightButtonRect, "", exitButtonChosenStyle)) {
			exitButtonChosenStyle = exitButtonStyle;
			PlayButtonSound();
			Save ();
			Application.Quit();
		}
	}
	
	public static void Load() {
		String savePath = Application.persistentDataPath + "/playerInfo.dat";
		if (File.Exists (savePath)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (savePath, FileMode.Open);
			data = (PlayerData)bf.Deserialize(file);
			file.Close();
		} else {
			data = new PlayerData ();
			data.levelProgress = new Dictionary<string, int>();
			data.playAnimations = false;
			data.playTutorials = true;
			data.playMusic = true;
			data.playSoundEffects = true;
			data.chosenCar = "car1";
		}
	}

	public static void Save() {
		String savePath = Application.persistentDataPath + "/playerInfo.dat";
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (savePath);
		
		bf.Serialize (file, data);
		file.Close ();
	}

	public static void PlayButtonSound() {
		if (data.playSoundEffects)
			soundButton.GetComponent<AudioSource> ().Play ();
	}

	public static void PlayPuzzlePieceSound() {
		if (data.playSoundEffects)
			soundPuzzlePiece.GetComponent<AudioSource> ().Play ();
	}
	
	
	public static void PlayGearShiftSound() {
		if (data.playSoundEffects)
			soundGearShift.GetComponent<AudioSource> ().Play ();
	}
	
	public static void PlayMenuMusic() {
		if (data.playMusic)
			soundMenu.GetComponent<AudioSource> ().Play ();
	}

	public static void StopMenuMusic() {
		soundMenu.GetComponent<AudioSource> ().Stop ();
	}

	private static void SetVariables() {
		canvas = GameObject.Find ("Canvas");
		canvas.GetComponent<Image> ().color = originalCanvasColor;
		DontDestroyOnLoad (canvas);
		
		soundButton = GameObject.Find ("sound_button_click");
		DontDestroyOnLoad (soundButton);
		soundPuzzlePiece = GameObject.Find ("sound_piece_click");
		DontDestroyOnLoad (soundPuzzlePiece);
		soundGearShift = GameObject.Find ("sound_gear_shift");
		DontDestroyOnLoad (soundGearShift);
		soundMenu = GameObject.Find ("sound_menu");
		DontDestroyOnLoad (soundMenu);
		
		int buttonSize = (int)(Screen.width / 5 * 0.7);
		float offset = (Screen.width / 5 - buttonSize) / 2;
		logoTexture = (Texture2D)Resources.Load ("logo_StreetBlox_nobg");
		optionButtonTexture = (Texture2D)Resources.Load ("ui_button_options");
		optionButtonPressedTexture = (Texture2D)Resources.Load ("ui_button_options_pressed");
		selectButtonTexture = (Texture2D)Resources.Load ("ui_button_start");
		selectButtonPressedTexture = (Texture2D)Resources.Load ("ui_button_start_pressed");
		exitButtonTexture = (Texture2D)Resources.Load ("ui_button_exit");
		exitButtonPressedTexture = (Texture2D)Resources.Load ("ui_button_exit_pressed");
		vinliaTexture = (Texture2D)Resources.Load ("logo_blackWhite");		
		backButtonTexture = (Texture2D)Resources.Load("ui_button_back");
		backButtonPressedTexture = (Texture2D)Resources.Load("ui_button_back_pressed");
		
		logoRect = new Rect (Screen.width / 5, 20, Screen.width / 5 * 3 * (1f + 1f / 30f), Screen.height / 3);
		leftButtonRect = new Rect (Screen.width / 5 + offset, Screen.height / 2, buttonSize, buttonSize);
		middleButtonRect = new Rect (Screen.width / 5 * 2 + offset, Screen.height / 2, buttonSize, buttonSize);
		rightButtonRect = new Rect (Screen.width / 5 * 3 + offset, Screen.height / 2, buttonSize, buttonSize);
		vinliaRect = new Rect (Screen.width - buttonSize * 1.25f, Screen.height - buttonSize / 2 - offset / 2, 192, 108);

		optionButtonStyle = new GUIStyle ();
		optionButtonStyle.normal.background = optionButtonTexture;
		optionButtonPressedStyle = new GUIStyle ();
		optionButtonPressedStyle.normal.background = optionButtonPressedTexture;
		optionButtonChosenStyle = optionButtonStyle;

		selectButtonStyle = new GUIStyle ();
		selectButtonStyle.normal.background = selectButtonTexture;
		selectButtonPressedStyle = new GUIStyle ();
		selectButtonPressedStyle.normal.background = selectButtonPressedTexture;
		selectButtonChosenStyle = selectButtonStyle;

		exitButtonStyle = new GUIStyle ();
		exitButtonStyle.normal.background = exitButtonTexture;
		exitButtonPressedStyle = new GUIStyle ();
		exitButtonPressedStyle.normal.background = exitButtonPressedTexture;
		exitButtonChosenStyle = exitButtonStyle;

		vinliaStyle = new GUIStyle ();
		vinliaStyle.normal.background = vinliaTexture;

		logoStyle = new GUIStyle ();
		logoStyle.normal.background = logoTexture;
		
		backButtonStyle = new GUIStyle();
		backButtonStyle.normal.background = backButtonTexture;
		backButtonPressedStyle = new GUIStyle();
		backButtonPressedStyle.normal.background = backButtonPressedTexture;
	}

	[Serializable]
	public class PlayerData {
		public Dictionary<string, int> levelProgress;
		public Boolean playAnimations;
		public Boolean playTutorials;
		public Boolean playMusic;
		public Boolean playSoundEffects;
		public String chosenCar;
	}
}
