using UnityEngine;
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
	public static GameObject soundBridgePiece;
	public static GameObject soundWorld1;
	public static GameObject soundWorld2;
	public static GameObject soundWorld3;

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
			String tutorialLevel = InTutorialPhase();
			if (tutorialLevel != null && data.playTutorials) {
				WorldSelectScript.AddLevels();
				WorldSelectScript.SetLevelInfo(0);
				LevelSelectScript.SetLevelInfo();
				LevelSelectScript.chosenLevel = tutorialLevel;
				Application.LoadLevel(tutorialLevel);
			} else {
				Application.LoadLevel ("world_select");
			}
		}
		if (GUI.Button (rightButtonRect, "", exitButtonChosenStyle)) {
			exitButtonChosenStyle = exitButtonStyle;
			PlayButtonSound();
			Save ();
			Application.Quit();
		}
	}

	public static String InTutorialPhase() {
		String tutorialLevel = null;
		foreach (String levelName in WorldSelectScript.levelsTutorial) {
			if (!data.levelProgress.ContainsKey (levelName)) {
				tutorialLevel = levelName;
				break;
			}
		}
		return tutorialLevel;
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
			data.worldSelectShown = false;
			data.marbles = 0;
			data.carsUnlocked = new Boolean[9];
			for(int i = 0; i < data.carsUnlocked.Length; i++)
				data.carsUnlocked[i] = true;
			data.puzzleBoxesUnlocked = new Boolean[3];
			for(int i = 0; i < data.puzzleBoxesUnlocked.Length; i++)
				data.puzzleBoxesUnlocked[i] = true;
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

	public static void PlayBridgeSound() {
		if (data.playSoundEffects)
			soundBridgePiece.GetComponent<AudioSource>().Play();
	}
	
	public static void PlayMenuMusic() {
		if (data.playMusic)
			soundMenu.GetComponent<AudioSource> ().Play ();
	}

	public static void StopMenuMusic() {
		soundMenu.GetComponent<AudioSource> ().Stop ();
	}

	public static void PlayWorld1Music() {
		if (data.playMusic)
			soundWorld1.GetComponent<AudioSource> ().Play ();
	}
	
	public static void StopWorld1Music() {
		soundWorld1.GetComponent<AudioSource> ().Stop ();
	}

	public static void PlayWorld2Music() {
		if (data.playMusic)
			soundWorld2.GetComponent<AudioSource> ().Play ();
	}
	
	public static void StopWorld2Music() {
		soundWorld2.GetComponent<AudioSource> ().Stop ();
	}

	public static void PlayWorld3Music() {
		if (data.playMusic)
			soundWorld3.GetComponent<AudioSource> ().Play ();
	}
	
	public static void StopWorld3Music() {
		soundWorld3.GetComponent<AudioSource> ().Stop ();
	}
	
	public static IEnumerator CameraShake(float duration, float magnitude) {
		float elapsed = 0.0f;
		
		Vector3 originalCamPos = Camera.main.transform.position;
		
		while (elapsed < duration) {
			elapsed += Time.deltaTime;          
			
			float percentComplete = elapsed / duration;         
			float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);
			
			// map value to [-1, 1]
			float x = UnityEngine.Random.value * 2.0f - 1.0f;
			float z = UnityEngine.Random.value * 2.0f - 1.0f;
			x *= magnitude * damper;
			z *= magnitude * damper;
			
			Camera.main.transform.localPosition = new Vector3(originalCamPos.x + x, originalCamPos.y, originalCamPos.z + z);
			
			yield return null;
		}
		
		Camera.main.transform.position = originalCamPos;
	}

	private static void SetVariables() {
		soundButton = GameObject.Find ("sound_button_click");
		DontDestroyOnLoad (soundButton);
		soundPuzzlePiece = GameObject.Find ("sound_piece_click");
		DontDestroyOnLoad (soundPuzzlePiece);
		soundGearShift = GameObject.Find ("sound_gear_shift");
		DontDestroyOnLoad (soundGearShift);
		soundMenu = GameObject.Find ("sound_menu");
		DontDestroyOnLoad (soundMenu);
		soundBridgePiece = GameObject.Find ("sound_bridge_piece");
		DontDestroyOnLoad (soundBridgePiece);
		soundWorld1 = GameObject.Find ("sound_world1");
		DontDestroyOnLoad (soundWorld1);
		soundWorld2 = GameObject.Find ("sound_world2");
		DontDestroyOnLoad (soundWorld2);
		soundWorld3 = GameObject.Find ("sound_world3");
		DontDestroyOnLoad (soundWorld3);
		
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

		public Boolean worldSelectShown;
		public int marbles;
		
		public String chosenCar;
		public Boolean[] carsUnlocked;

		public Boolean[] puzzleBoxesUnlocked;

		// Explorer save stuff
		public string[] puzzlePieces;
		public string[][] board;

		public float carPositionX;
		public float carPositionY;
		public float carPositionZ;
		public float carRotationW;
		public float carRotationX;
		public float carRotationY;
		public float carRotationZ;

		public float cameraPositionX;
		public float cameraPositionZ;

		public PuzzlePieceScript.Coordinate currentCoordinate;
		public int currentCoordinateIndex;
		public int currentDirection;
		public PuzzlePieceScript.Connection currentConnection;
		public PuzzlePieceScript.PuzzlePieceConnections currentPuzzlePieceConnections;
		public string currentPuzzlePiece;
		public string previousPuzzlePiece;
		public float timeSinceOnLastPuzzlePiece = 0f;
		public float time;
	}
}
