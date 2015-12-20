using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

public class WorldSelectScript : MonoBehaviour {

	// The names of the available worlds, in order.
	private static readonly string[] WorldNames = {"Grass World", "Lava World", "Space World"};

	// The world 1 levels
	private static readonly string[] levelsWorld1 = {
		"tutorial_1",
		"tutorial_2",
		"tutorial_3",
		"level_01",
		"level_02",
		"level_03",
		"level_04",
		"level_05",
		"level_06",
		"level_07",
		"level_08",
		"level_09",
		"level_10",
		"level_11",
		"level_12",
		"level_13",
		"level_14",
		"level_15",
		"level_16"
	};
	// The world2 levels
	private static readonly string[] levelsWorld2 = {
		"level_l01",
		"level_l02",
		"level_l03"
	};
	// The world3 levels
	private static readonly string[] levelsWorld3 = {
		"level_s01",
		"level_s_test"
	};

	private string[][] worlds = {levelsWorld1, levelsWorld2, levelsWorld3};

	// Mapping of World names to a mapping of level names to level configurations
	private static Dictionary<string, Dictionary<string, LevelConfiguration>> worldConfigurations;

	// The levels (in order) that the LevelSelect should show
	public static string[] levels;
	//  The configurations for the selected levels that LevelSelect should use
	public static Dictionary<string, LevelConfiguration> levelConfigurations;

	// GUI stuff
	private static Texture2D endlessButtonTexture, endlessButtonPressedTexture;
	
	private static Rect leftBottomRect;
	private static Rect rightBottomRect;

	private static GUIStyle backButtonChosenStyle;
	private static GUIStyle endlessButtonStyle, endlessButtonPressedStyle, endlessButtonChosenStyle;

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
			else if (rightBottomRect.Contains(mousePosition))
				endlessButtonChosenStyle = endlessButtonPressedStyle;
		}
	}
	
	void OnGUI() {
		if (puzzleBoxScript.animationLock)
			return;
		if (GUI.Button (rightBottomRect, "", endlessButtonChosenStyle)) {
			MenuScript.PlayButtonSound ();
			endlessButtonChosenStyle = endlessButtonStyle;
			Application.LoadLevel ("endless");
		}
		if (GUI.Button (leftBottomRect, "", backButtonChosenStyle)) {
			backButtonChosenStyle = MenuScript.backButtonStyle;
			MenuScript.PlayButtonSound ();
			Application.LoadLevel ("menu");
		}
	}

	public void SelectedWorld(int world) {
		MenuScript.PlayButtonSound ();
		levels = worlds [world - 1];
		levelConfigurations = worldConfigurations [WorldNames [world - 1]];
		Application.LoadLevel ("level_select");
	}

	private static void SetVariables() {
		float buttonSize = (int)(Screen.width / 5 * 0.7);
		float offset = (Screen.width / 5 - buttonSize) / 2;
		leftBottomRect = new Rect (offset, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		rightBottomRect = new Rect (Screen.width - offset - buttonSize, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		
		endlessButtonTexture = (Texture2D)Resources.Load("ui_button_endless_cs");
		endlessButtonPressedTexture = (Texture2D)Resources.Load("ui_button_endless_cs");
		backButtonChosenStyle = MenuScript.backButtonStyle;
		
		endlessButtonStyle = new GUIStyle ();
		endlessButtonStyle.normal.background = endlessButtonTexture;
		endlessButtonPressedStyle = new GUIStyle ();
		endlessButtonPressedStyle.normal.background = endlessButtonPressedTexture;
		endlessButtonChosenStyle = endlessButtonStyle;

		AddLevels ();
	}

	private static void AddLevels() {
		worldConfigurations = new Dictionary<string, Dictionary<string, LevelConfiguration>>();

		Dictionary<string, LevelConfiguration> world1Configuration = new Dictionary<string, LevelConfiguration> ();
		worldConfigurations.Add (WorldNames[0], world1Configuration);

		world1Configuration.Add ("tutorial_1", new LevelConfiguration (4, 5, 0f, 0f, 5));
		world1Configuration.Add ("tutorial_2", new LevelConfiguration (4, 5, 0f, 0f, 6));
		world1Configuration.Add ("tutorial_3", new LevelConfiguration (4, 5, 0f, 0f, 8));;
		world1Configuration.Add ("level_01", new LevelConfiguration (5, 5, 0f, 0f, 5));
		world1Configuration.Add ("level_02", new LevelConfiguration (5, 5, 0f, 0f, 10));
		world1Configuration.Add ("level_03", new LevelConfiguration (5, 5, 0f, 0f, 11));
		world1Configuration.Add ("level_04", new LevelConfiguration (7, 5, 0f, 0f, 20));
		world1Configuration.Add ("level_05", new LevelConfiguration (7, 5, 0f, 0f, 11));
		world1Configuration.Add ("level_06", new LevelConfiguration (5, 5, 0f, 0f, 15));
		world1Configuration.Add ("level_07", new LevelConfiguration (5, 5, 0f, 0f, 20));
		world1Configuration.Add ("level_08", new LevelConfiguration (5, 5, 0f, 0f, 16));
		world1Configuration.Add ("level_09", new LevelConfiguration (7, 6, 0f, 0f, 35));
		world1Configuration.Add ("level_10", new LevelConfiguration (7, 5, 0f, 0f, 60));
		world1Configuration.Add ("level_11", new LevelConfiguration (5, 5, 0f, 0f, 16));
		world1Configuration.Add ("level_12", new LevelConfiguration (4, 5, 0f, 0f, 35));
		world1Configuration.Add ("level_13", new LevelConfiguration (7, 7, 0f, 0f, 30));
		world1Configuration.Add ("level_14", new LevelConfiguration (7, 7, 0f, 0f, 30));
		world1Configuration.Add ("level_15", new LevelConfiguration (7, 5, 0f, 0f, 30));
		world1Configuration.Add ("level_16", new LevelConfiguration (6, 5, 0f, 0f, 30));

		Dictionary<string, LevelConfiguration> world2Configuration = new Dictionary<string, LevelConfiguration> ();
		worldConfigurations.Add (WorldNames[1], world2Configuration);

		world2Configuration.Add ("level_l01", new LevelConfiguration (5, 5, 0f, 0f, 15));
		world2Configuration.Add ("level_l02", new LevelConfiguration (5, 5, 0f, 0f, 10));
		world2Configuration.Add ("level_l03", new LevelConfiguration (9, 5, 0f, 0f, 11));

		Dictionary<string, LevelConfiguration> world3Configuration = new Dictionary<string, LevelConfiguration> ();
		worldConfigurations.Add (WorldNames[2], world3Configuration);
		
		world3Configuration.Add ("level_s01", new LevelConfiguration (4, 5, 0f, 0f, 15));
		world3Configuration.Add ("level_s_test", new LevelConfiguration (4, 5, 0f, 0f, 15));
	}
}

public class LevelConfiguration {
	// For now just hard-code the board dimensions
	public int BoardWidth = 5;
	public int BoardHeight = 5;
	
	// The height and width of puzzlePieces
	public float PieceSize = 0.5f;
	// The x-position of the most left pieces in the board
	public float LeftXPosition = -0.5f;
	// The z-position of the most top pieces in the board
	public float TopZPosition = 1.0f;
	
	// The position of the car is in the top-left, set off-sets to get the center of the car.
	public float CarZOffset; 
	public float CarXOffset;
	
	public float CarLength;
	
	// The time the car waits before starting to move
	public float waitTimeAtStart = 6.0f;
	
	// The amount to move per frame (multiplied by Time.deltaTime)
	public float movement = 0.15f;
	
	public int par;
	
	public LevelConfiguration() {
		SetDynamicConfigurations();
	}
	
	public LevelConfiguration(int BoardWidth, int BoardHeight, float LeftXPosition, float TopZPosition, int par) {
		this.BoardWidth = BoardWidth;
		this.BoardHeight = BoardHeight;
		this.LeftXPosition = LeftXPosition;
		this.TopZPosition = TopZPosition;
		this.par = par;
		SetDynamicConfigurations();
	}
	
	private void SetDynamicConfigurations() {
		this.CarXOffset = -PieceSize * 0.5f;
		this.CarZOffset = -PieceSize * 0.5f;
		this.CarLength = 0.24f * PieceSize;
	}
}

