using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class WorldSelectScript : MonoBehaviour {

	public static String chosenWorldName;

	// The names of the available worlds, in order.
	private static readonly string[] WorldNames = {"Tutorial", "Grass World", "Lava World", "Space World"};

	// The tutorial levels
	public static readonly string[] levelsTutorial = {
		"tutorial_1",
		"tutorial_2",
		"tutorial_3"
	};

	// The world 1 levels
	private static readonly string[] levelsWorld1 = {
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
		"level_16",
		"level_17",
		"level_18"
	};
	// The world2 levels
	private static readonly string[] levelsWorld2 = {
		"level_l01",
		"level_l02",
		"level_l03",
		"level_l04",
		"level_l05",
		"level_l06"
	};
	// The world3 levels
	private static readonly string[] levelsWorld3 = {
		"level_s01",
		"level_s02",
		"level_s03",
		"level_s04",
		"level_s05",
		"level_s06",
		"level_s07"
	};
	// Mapping of level names to the name that should be displayed in level select
	public static Dictionary<string, string> displayNames;
	// All the available worlds
	private static string[][] worlds = {levelsTutorial, levelsWorld1, levelsWorld2, levelsWorld3};
	// Mapping of World names to a mapping of level names to level configurations
	private static Dictionary<string, Dictionary<string, LevelConfiguration>> worldConfigurations;
	// The levels (in order) that the LevelSelect should show
	public static string[] levels;
	//  The configurations for the selected levels that LevelSelect should use
	public static Dictionary<string, LevelConfiguration> levelConfigurations;

	// GUI stuff
	private static Texture2D endlessButtonTexture, endlessButtonPressedTexture;
	private static Texture2D levelTextTexture;
	
	private static Rect leftBottomRect;
	private static Rect rightBottomRect;
	private static Rect loadingRect;

	private static GUIStyle backButtonChosenStyle;
	private static GUIStyle endlessButtonStyle, endlessButtonPressedStyle, endlessButtonChosenStyle;
	private static GUIStyle loadingStyle;

	// cars
	private static GameObject displayCar1;
	private static GameObject displayCar2;
	private static GameObject displayCar3;
	private static GameObject displayCar4;
	private static GameObject displayCar5;
	private static GameObject displayCar6;
	private static GameObject displayCar7;
	private static GameObject displayCar8;
	private static GameObject displayCar11;

	// puzzle boxes
	private static GameObject puzzleBoxWorld1;
	private static GameObject puzzleBoxWorld2;
	private static GameObject puzzleBoxWorld3;

	// Dynamic one time loading of all static variables
	private static Boolean staticVariablesSet = false;
	
	private Boolean loading;
	
	void Awake() {
		if (!staticVariablesSet) {
			SetVariables ();
			staticVariablesSet = true;
		}

		LoadCars ();
		LoadPuzzleBoxes ();

		loading = false;
	}
	
	// Use this for initialization
	void Start () {
		GameObject.Find ("marbleCounter").GetComponent<TextMesh> ().text = "" + MenuScript.data.marbles;
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
		if (loading)
			GUI.Label(loadingRect, "Loading...", loadingStyle);

		if (puzzleBoxScript.animationLock)
			return;
		if (GUI.Button (rightBottomRect, "", endlessButtonChosenStyle)) {
			MenuScript.PlayButtonSound ();
			endlessButtonChosenStyle = endlessButtonStyle;
			loading = true;
			Application.LoadLevel ("explorer");
		}
		if (GUI.Button (leftBottomRect, "", backButtonChosenStyle)) {
			backButtonChosenStyle = MenuScript.backButtonStyle;
			MenuScript.PlayButtonSound ();
			Application.LoadLevel ("menu");
		}
	}

	public void SelectedWorld(int world) {
		MenuScript.PlayButtonSound ();
		SetLevelInfo (world);
		chosenWorldName = WorldNames [world];
		Application.LoadLevel ("level_select");
	}

	public static void SetLevelInfo(int world) {
		levels = worlds [world];
		levelConfigurations = worldConfigurations [WorldNames [world]];
	}

	private static void LoadCars() {
		if (MenuScript.data.carsUnlocked [0]) {
			GameObject clone = (GameObject)Instantiate (displayCar1);
			clone.name = "car1";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked [1]) {
			GameObject clone = (GameObject)Instantiate (displayCar2);
			clone.name = "car2";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[2]) {
			GameObject clone = (GameObject)Instantiate (displayCar3);
			clone.name = "car3";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[3]) {
			GameObject clone = (GameObject)Instantiate (displayCar4);
			clone.name = "car4";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[4]) {
			GameObject clone = (GameObject)Instantiate (displayCar5);
			clone.name = "car5";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[5]) {
			GameObject clone = (GameObject)Instantiate (displayCar6);
			clone.name = "car6";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[6]) {
			GameObject clone = (GameObject)Instantiate (displayCar7);
			clone.name = "car7";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[7]) {
			GameObject clone = (GameObject)Instantiate (displayCar8);
			clone.name = "car8";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[8]) {
			GameObject clone = (GameObject)Instantiate (displayCar11);
			clone.name = "car11";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
	}

	private static void LoadPuzzleBoxes() {
		if (MenuScript.data.puzzleBoxesUnlocked [0]) {
			GameObject clone = (GameObject)Instantiate (puzzleBoxWorld1);
			clone.transform.GetComponent<puzzleBoxScript>().SetWorldNumber (1);
		}
		if (MenuScript.data.puzzleBoxesUnlocked [1]) {
			GameObject clone = (GameObject)Instantiate (puzzleBoxWorld2);
			clone.transform.GetComponent<puzzleBoxScript>().SetWorldNumber (2);
		}
		if (MenuScript.data.puzzleBoxesUnlocked [2]) {
			GameObject clone = (GameObject)Instantiate (puzzleBoxWorld3);
			clone.transform.GetComponent<puzzleBoxScript>().SetWorldNumber (3);
		}
	}

	private static void SetVariables() {
		float buttonSize = (int)(Screen.width / 5 * 0.7);
		float offset = (Screen.width / 5 - buttonSize) / 2;
		leftBottomRect = new Rect (offset, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		rightBottomRect = new Rect (Screen.width - offset - buttonSize, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		loadingRect = new Rect (Screen.width / 2 - buttonSize / 2, Screen.height / 5 - buttonSize / 6, buttonSize, buttonSize / 3);
		
		endlessButtonTexture = (Texture2D)Resources.Load("ui_button_endless_cs");
		endlessButtonPressedTexture = (Texture2D)Resources.Load("ui_button_endless_cs");
		levelTextTexture = (Texture2D) Resources.Load ("ui_border_levelname");
		backButtonChosenStyle = MenuScript.backButtonStyle;
		
		endlessButtonStyle = new GUIStyle ();
		endlessButtonStyle.normal.background = endlessButtonTexture;
		endlessButtonPressedStyle = new GUIStyle ();
		endlessButtonPressedStyle.normal.background = endlessButtonPressedTexture;
		endlessButtonChosenStyle = endlessButtonStyle;

		loadingStyle = new GUIStyle ();
		loadingStyle.normal.textColor = Color.white;
		loadingStyle.fontSize = 28;
		loadingStyle.alignment = TextAnchor.MiddleCenter;
		loadingStyle.normal.background = levelTextTexture;
	
		AddCars ();
		AddPuzzleBoxes ();
		AddLevels ();
	}

	public static void AddCars() {
		displayCar1 = Resources.Load ("displayCar1") as GameObject;
		displayCar2 = Resources.Load ("displayCar2") as GameObject;
		displayCar3 = Resources.Load ("displayCar3") as GameObject;
		displayCar4 = Resources.Load ("displayCar4") as GameObject;
		displayCar5 = Resources.Load ("displayCar5") as GameObject;
		displayCar6 = Resources.Load ("displayCar6") as GameObject;
		displayCar7 = Resources.Load ("displayCar7") as GameObject;
		displayCar8 = Resources.Load ("displayCar8") as GameObject;
		displayCar11 = Resources.Load ("displayCar11") as GameObject;
	}

	public static void AddPuzzleBoxes() {
		puzzleBoxWorld1 = Resources.Load ("puzzleBoxWorld1") as GameObject;
		puzzleBoxWorld2 = Resources.Load ("puzzleBoxWorld2") as GameObject;
		puzzleBoxWorld3 = Resources.Load ("puzzleBoxWorld3") as GameObject;
	}

	public static void AddLevels() {
		worldConfigurations = new Dictionary<string, Dictionary<string, LevelConfiguration>>();

		Dictionary<string, LevelConfiguration> world0Configuration = new Dictionary<string, LevelConfiguration> ();
		worldConfigurations.Add (WorldNames[0], world0Configuration);
		world0Configuration.Add ("tutorial_1", new LevelConfiguration (4, 5, 0f, 0f, 5));
		world0Configuration.Add ("tutorial_2", new LevelConfiguration (4, 5, 0f, 0f, 6));
		world0Configuration.Add ("tutorial_3", new LevelConfiguration (4, 5, 0f, 0f, 8));

		Dictionary<string, LevelConfiguration> world1Configuration = new Dictionary<string, LevelConfiguration> ();
		worldConfigurations.Add (WorldNames[1], world1Configuration);

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
		world1Configuration.Add ("level_17", new LevelConfiguration (8, 4, 0f, 0f, 30));
		world1Configuration.Add ("level_18", new LevelConfiguration (8, 7, 0f, 0f, 30));

		Dictionary<string, LevelConfiguration> world2Configuration = new Dictionary<string, LevelConfiguration> ();
		worldConfigurations.Add (WorldNames[2], world2Configuration);

		world2Configuration.Add ("level_l01", new LevelConfiguration (5, 5, 0f, 0f, 15));
		world2Configuration.Add ("level_l02", new LevelConfiguration (5, 5, 0f, 0f, 10));
		world2Configuration.Add ("level_l03", new LevelConfiguration (7, 5, 0f, 0f, 11));
		world2Configuration.Add ("level_l04", new LevelConfiguration (5, 5, 0f, 0f, 11));
		world2Configuration.Add ("level_l05", new LevelConfiguration (6, 5, 0f, 0f, 11));
		world2Configuration.Add ("level_l06", new LevelConfiguration (4, 5, 0f, 0f, 12));

		Dictionary<string, LevelConfiguration> world3Configuration = new Dictionary<string, LevelConfiguration> ();
		worldConfigurations.Add (WorldNames[3], world3Configuration);
		
		world3Configuration.Add ("level_s01", new LevelConfiguration (4, 5, 0f, 0f, 15));
		world3Configuration.Add ("level_s02", new LevelConfiguration (4, 5, 0f, 0f, 15));
		world3Configuration.Add ("level_s03", new LevelConfiguration (5, 5, 0f, 0f, 15));
		world3Configuration.Add ("level_s04", new LevelConfiguration (5, 5, 0f, 0f, 15));
		world3Configuration.Add ("level_s05", new LevelConfiguration (4, 5, 0f, 0f, 15));
		world3Configuration.Add ("level_s06", new LevelConfiguration (9, 5, 0f, 0f, 25));
		world3Configuration.Add ("level_s07", new LevelConfiguration (7, 4, 0f, 0f, 25));

		displayNames = new Dictionary<string, string> ();
		displayNames.Add ("tutorial_1", "0-1");
		displayNames.Add ("tutorial_2", "0-2");
		displayNames.Add ("tutorial_3", "0-3");
		displayNames.Add ("level_01", "1-1");
		displayNames.Add ("level_02", "1-2");
		displayNames.Add ("level_03", "1-3");
		displayNames.Add ("level_04", "1-4");
		displayNames.Add ("level_05", "1-5");
		displayNames.Add ("level_06", "1-6");
		displayNames.Add ("level_07", "1-7");
		displayNames.Add ("level_08", "1-8");
		displayNames.Add ("level_09", "1-9");
		displayNames.Add ("level_10", "1-10");
		displayNames.Add ("level_11", "1-11");
		displayNames.Add ("level_12", "1-12");
		displayNames.Add ("level_13", "1-13");
		displayNames.Add ("level_14", "1-14");
		displayNames.Add ("level_15", "1-15");
		displayNames.Add ("level_16", "1-16");
		displayNames.Add ("level_17", "1-17");
		displayNames.Add ("level_18", "1-18");
		displayNames.Add ("level_19", "1-19");

		displayNames.Add ("level_l01", "2-1");
		displayNames.Add ("level_l02", "2-2");
		displayNames.Add ("level_l03", "2-3");
		displayNames.Add ("level_l04", "2-4");
		displayNames.Add ("level_l05", "2-5");
		displayNames.Add ("level_l06", "2-6");

		displayNames.Add ("level_s01", "3-1");
		displayNames.Add ("level_s02", "3-2");
		displayNames.Add ("level_s03", "3-3");
		displayNames.Add ("level_s04", "3-4");
		displayNames.Add ("level_s05", "3-5");
		displayNames.Add ("level_s06", "3-6");
		displayNames.Add ("level_s07", "3-7");
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

public class Pair<T, U> {
	public Pair() {
	}
	
	public Pair(T first, U second) {
		this.First = first;
		this.Second = second;
	}
	
	public T First { get; set; }
	public U Second { get; set; }
};

