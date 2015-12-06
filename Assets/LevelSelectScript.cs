using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

public class LevelSelectScript : MonoBehaviour {
	
	public static Dictionary<string, LevelConfiguration> levelConfigurations;	

	private static string[] levels;
	private static string chosenLevel;
	
	// The first level the user has the focus on
	private int focus;

	private Boolean loading;

	// GUI stuff
	public static GameObject medal;

	private static Texture2D leftButtonTexture;
	private static Texture2D rightButtonTexture;
	private static Texture2D unsolvedLevelTexture, unsolvedLevelPressedTexture;
	private static Texture2D solvedLevelTexture, solvedLevelPressedTexture;
	private static Texture2D unavailableLevelTexture, unavailableLevelPressedTexture;
	private static Texture2D levelTextTexture;
	private static Texture2D oneStarMedalTexture;
	private static Texture2D twoStarMedalTexture;
	private static Texture2D threeStarMedalTexture;

	private static Rect leftBottomRect;
	private static Rect rightBottomRect;
	private static Rect leftButtonRect;
	private static Rect rightButtonRect;
	private static Rect loadingRect;

	private static GUIStyle backButtonChosenStyle;
	private static GUIStyle leftButtonStyle;
	private static GUIStyle rightButtonStyle;
	private static GUIStyle unsolvedLevelStyle, unsolvedLevelPressedStyle;
	private static GUIStyle solvedLevelStyle, solvedLevelPressedStyle, solvedLevelChosenStyle;
	private static GUIStyle unavailableLevelStyle;
	private static GUIStyle levelTextStyle;
	private static GUIStyle oneStarMedalStyle;
	private static GUIStyle twoStarMedalStyle;
	private static GUIStyle threeStarMedalStyle;
	private static GUIStyle loadingStyle;
	
	private static float buttonHeight;
	private static float offset;
	private static int buttonSize;

	// Dynamic one time loading of all static variables
	private static Boolean staticVariablesSet = false;

	void Awake() {
		levels = WorldSelectScript.levels;
		levelConfigurations = WorldSelectScript.levelConfigurations;
		loading = false;

		if (!staticVariablesSet) {
			SetVariables ();
			staticVariablesSet = true;
		}
	}

	// Use this for initialization
	void Start () {
		focus = 0;
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
		if (loading)
			GUI.Label(loadingRect, "Loading...", loadingStyle);
		// Back
		if (GUI.Button (leftBottomRect, "", backButtonChosenStyle)) {
			backButtonChosenStyle = MenuScript.backButtonStyle;
			MenuScript.PlayButtonSound();
			Application.LoadLevel ("world_select");
		}

		for (int i = 0; i < 3; i++) {
			if (focus + i >= levels.Length) {
				break;
			}
			string level = levels [focus + i];
			float buttonWidth = Screen.width / 5 * (i + 1) + offset;
			if (focus < levels.Length - i) {
				Rect buttonRect = new Rect (buttonWidth, buttonHeight, buttonSize, buttonSize);
				Rect textRect = new Rect(buttonWidth, buttonHeight + buttonSize + 10, buttonSize, 40);
				Rect medalRect = new Rect(buttonWidth, buttonHeight - buttonSize - 10, buttonSize, buttonSize);
				GUIStyle style = unsolvedLevelStyle;
				if (MenuScript.data.levelProgress.ContainsKey(level)) {
					style = solvedLevelChosenStyle;
					GUIStyle medalStyle = oneStarMedalStyle;
					if (MenuScript.data.levelProgress[level] == 2)
						medalStyle = twoStarMedalStyle;
					else if(MenuScript.data.levelProgress[level] == 3)
						medalStyle = threeStarMedalStyle;
					GUI.Label(medalRect, "", medalStyle);
				}
				if (GUI.Button (buttonRect, "", style)) {
					MenuScript.PlayButtonSound();
					MenuScript.canvas.GetComponent<Image>().color = GameScript.backgroundColor;
					loading = true;
					chosenLevel = level;
					Application.LoadLevel (level);
				}
				GUI.Label(textRect, level, levelTextStyle);
			}
		}

		if (focus > 2) {
			if (GUI.Button (leftButtonRect, "", leftButtonStyle)) {
				MenuScript.PlayButtonSound();
				focus -= 3;
			}
		}
		if (focus < levels.Length - 3) {
			if (GUI.Button (rightButtonRect, "", rightButtonStyle)) {
				MenuScript.PlayButtonSound();
				focus += 3;
			}
		}
	}

	public static void UpdateProgress(Boolean won, int stars) {
		if (won) {
			if (MenuScript.data.levelProgress.ContainsKey (chosenLevel)) {
				if (MenuScript.data.levelProgress [chosenLevel] < stars)
					MenuScript.data.levelProgress [chosenLevel] = stars;
			} else {
				MenuScript.data.levelProgress.Add (chosenLevel, stars);
			}
			MenuScript.Save ();
		}
	}
	
	public static void NextLevel() {
		int index = Array.IndexOf (levels, chosenLevel);
		if (index + 1 < levels.Length) {
			chosenLevel = levels [index + 1];
			Application.LoadLevel (chosenLevel);
		} else {
			MenuScript.canvas.GetComponent<Image>().color = MenuScript.originalCanvasColor;
			Application.LoadLevel ("level_select");
		}
	}

	private static void SetVariables() {
		buttonSize = (int)(Screen.width / 5 * 0.7);
		offset = (Screen.width / 5 - buttonSize) / 2;
		leftBottomRect = new Rect (offset, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		rightBottomRect = new Rect (Screen.width - offset - buttonSize, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		leftButtonRect = new Rect (offset, Screen.height / 2 - buttonSize / 2, buttonSize / 2, buttonSize);
		rightButtonRect = new Rect (Screen.width - offset - buttonSize / 2, Screen.height / 2 - buttonSize / 2, buttonSize / 2, buttonSize);
		loadingRect = new Rect (Screen.width / 2 - buttonSize / 2, Screen.height / 5 - buttonSize / 6, buttonSize, buttonSize / 3);

		leftButtonTexture = (Texture2D)Resources.Load("ui_button_arrow_left");
		rightButtonTexture = (Texture2D)Resources.Load("ui_button_arrow_right");
		unsolvedLevelTexture = (Texture2D) Resources.Load ("ui_button_level_enabled");
		unsolvedLevelPressedTexture = (Texture2D) Resources.Load ("ui_button_level_enabled");
		unavailableLevelTexture = (Texture2D) Resources.Load ("ui_button_level_disabled");
		solvedLevelTexture = (Texture2D) Resources.Load ("ui_button_level_cleared");
		solvedLevelPressedTexture = (Texture2D) Resources.Load ("ui_button_level_cleared");
		levelTextTexture = (Texture2D) Resources.Load ("ui_border_levelname");
		oneStarMedalTexture = (Texture2D) Resources.Load ("ui_medal_painted_star1");
		twoStarMedalTexture = (Texture2D) Resources.Load ("ui_medal_painted_star2");
		threeStarMedalTexture = (Texture2D) Resources.Load ("ui_medal_painted_star3");
		
		backButtonChosenStyle = MenuScript.backButtonStyle;

		leftButtonStyle = new GUIStyle ();
		leftButtonStyle.normal.background = leftButtonTexture;
		
		rightButtonStyle = new GUIStyle ();
		rightButtonStyle.normal.background = rightButtonTexture;
		
		unsolvedLevelStyle = new GUIStyle();
		unsolvedLevelStyle.normal.background = unsolvedLevelTexture;
		unsolvedLevelPressedStyle = new GUIStyle();
		unsolvedLevelPressedStyle.normal.background = unsolvedLevelPressedTexture;
		
		solvedLevelStyle = new GUIStyle();
		solvedLevelStyle.normal.background = solvedLevelTexture;
		solvedLevelPressedStyle = new GUIStyle();
		solvedLevelPressedStyle.normal.background = solvedLevelPressedTexture;
		solvedLevelChosenStyle = solvedLevelStyle;
		
		unavailableLevelStyle = new GUIStyle();
		unavailableLevelStyle.normal.background = unavailableLevelTexture;
		
		levelTextStyle = new GUIStyle();
		levelTextStyle.normal.textColor = Color.white;
		levelTextStyle.fontSize = 28;
		levelTextStyle.alignment = TextAnchor.MiddleCenter;
		levelTextStyle.normal.background = levelTextTexture;
		
		oneStarMedalStyle = new GUIStyle();
		oneStarMedalStyle.normal.background = oneStarMedalTexture;
		twoStarMedalStyle = new GUIStyle();
		twoStarMedalStyle.normal.background = twoStarMedalTexture;
		threeStarMedalStyle = new GUIStyle();
		threeStarMedalStyle.normal.background = threeStarMedalTexture;

		loadingStyle = new GUIStyle ();
		loadingStyle.normal.textColor = Color.white;
		loadingStyle.fontSize = 28;
		loadingStyle.alignment = TextAnchor.MiddleCenter;
		loadingStyle.normal.background = levelTextTexture;
		
		buttonHeight =  Screen.height / 2 - buttonSize / 2;
	}
}
