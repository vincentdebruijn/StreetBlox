using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
	public static readonly string[] levelsWorld1 = {
		"level_01",
		"level_02",
		"level_13",
		"level_05",
		"level_07",
		"level_22",
		"level_03",
		"level_15",
		"level_23",
		"level_06",
		"level_04",
		"level_10",
		"level_11",
		"level_09",
		"level_12",
		"level_16",
		"level_08",
		"level_18",
		"level_21",
		"level_14",
		"level_17",
		"level_19",
		"level_20"
	};
	// The world2 levels
	public static readonly string[] levelsWorld2 = {
		"level_l01",
		"level_l05",
		"level_l09",
		"level_l07",
		"level_l03",
		"level_l06",
		"level_l04",
		"level_l02",
		"level_l08"
	};
	// The world3 levels
	public static readonly string[] levelsWorld3 = {
		"level_s01",
		"level_s02",
		"level_s05",
		"level_s08",
		"level_s04",
		"level_s06",
		"level_s07",
		"level_s03"
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

	private static Texture2D levelTextTexture;
	private static Texture2D itemButtonTexture;
	
	private static Rect leftBottomRect;
	private static Rect loadingRect;
	private static Rect itemButtonRect;

	private static GUIStyle backButtonChosenStyle;
	private static GUIStyle loadingStyle;
	private static GUIStyle itemButtonStyle;

	// cars
	private static GameObject displayCar1;
	private static GameObject displayCar2;
	private static GameObject displayCar3;
	private static GameObject displayCar4;
	private static GameObject displayCar5;
	private static GameObject displayCar6;
	private static GameObject displayCar7;
	private static GameObject displayCar8;
	private static GameObject displayCar9;
	private static GameObject displayCar10;
	private static GameObject displayCar11;
	private static GameObject displayCar12;
	private static GameObject displayCar13;
	private static GameObject displayCar14;
	private static GameObject displayCar15;
	private static GameObject displayCar16;

	// Car display box
	private static GameObject carBox;

	// puzzle boxes
	private static GameObject puzzleBoxWorld1;
	private static GameObject puzzleBoxWorld2;
	private static GameObject puzzleBoxWorld3;
	private static GameObject puzzleBoxWorld4;

	// Animation stuff
	private static Vector3 carPositionStartAnimation = new Vector3(-1.7f, 5f, -7f);
	private static Quaternion carRotationStartAnimation = Quaternion.Euler(new Vector3(0, 270, 0));
	private static Vector3 puzzleBoxPositionStartAnimation = new Vector3(-1.8f, 7.5f, -10f);
	private static Quaternion puzzleBoxRotationStartAnimation = Quaternion.Euler (new Vector3(70, 180, 0));
	public static Boolean showingAnimations;
	private Boolean animationPlaying;
	private Boolean showingItemButton;
	private Boolean movingItemToPlace;
	private GameObject animatedItem;
	private Animator boxAnimator;
	private Vector3 destination;
	private Quaternion targetRotation;
	private Vector3 targetScale;
	private float movingSpeed;
	private float rotationSpeed;
	private float scalingSpeed;
	private const float globalSpeed = 1f;
	
	private Boolean loading;

	// Dynamic one time loading of all static variables
	private static Boolean staticVariablesSet = false;
	
	void Awake() {
		if (!staticVariablesSet) {
			SetVariables ();
			staticVariablesSet = true;
		}

		LoadCars ();
		LoadPuzzleBoxes ();

		loading = false;
		animationPlaying = false;
		movingItemToPlace = false;
		showingItemButton = false;
	}
	
	// Use this for initialization
	void Start () {
		GameObject.Find ("marbleCounter").GetComponent<TextMesh> ().text = "" + MenuScript.data.marbles;
		showingAnimations = MenuScript.data.animationQueue.Count > 0;

		if (!MenuScript.data.showedEndText && Array.IndexOf (MenuScript.data.puzzleBoxesUnlocked, false) == -1 && Array.IndexOf (MenuScript.data.carsUnlocked, false) == -1) {
			ShowEndText ();
			MenuScript.data.showedEndText = true;
			MenuScript.Save ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (showingAnimations) {
			if (animationPlaying) {
				if (boxAnimator.GetCurrentAnimatorStateInfo (0).IsName ("End")) {
					MenuScript.PlayItemReceivedSound ();
					Destroy(boxAnimator.gameObject);
					animationPlaying = false;
					animatedItem.GetComponent<CarDisplayScript>().turn = true;
					ShowReceivedItemTextBox ();
					showingItemButton = true;
				}
			} else if (movingItemToPlace) {
				animatedItem.transform.position = Vector3.MoveTowards(animatedItem.transform.position, destination, Time.deltaTime * movingSpeed * globalSpeed);
				animatedItem.transform.rotation = Quaternion.RotateTowards(animatedItem.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed * globalSpeed);
				animatedItem.transform.localScale = Vector3.MoveTowards(animatedItem.transform.localScale, targetScale, Time.deltaTime * scalingSpeed * globalSpeed);
				if (Mathf.Abs (animatedItem.transform.position.z - destination.z) < 0.01) {
					movingItemToPlace = false;
					if (MenuScript.data.chosenCar == animatedItem.name)
						animatedItem.GetComponent<CarDisplayScript>().ShowCarIsSelected();
				}
			} else if (!showingItemButton) {
				showingAnimations = MenuScript.data.animationQueue.Count > 0;
				if (showingAnimations)
					StartItemAnimation();
				else
					MenuScript.Save ();
			}
			return;
		}
		if (Input.GetMouseButton(0)) {
			Vector3 mousePosition = new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 0);
			if (leftBottomRect.Contains (mousePosition))
				backButtonChosenStyle = MenuScript.backButtonPressedStyle;
		}
	}
	
	void OnGUI() {
		if (loading)
			GUI.Label(loadingRect, "Loading...", loadingStyle);

		if (puzzleBoxScript.animationLock)
			return;

		if (!showingAnimations) {
			if (GUI.Button (leftBottomRect, "", backButtonChosenStyle)) {
				backButtonChosenStyle = MenuScript.backButtonStyle;
				MenuScript.PlayButtonSound ();
				SceneManager.LoadScene ("menu");
			}
		}

		if (showingItemButton) {
			if (GUI.Button (itemButtonRect, "", itemButtonStyle)) {
				MenuScript.PlayButtonSound ();
				showingItemButton = false;
				DestroyTutorialMessageBox ();
				if (animatedItem.name.Contains ("car"))
					animatedItem.GetComponent<CarDisplayScript> ().turn = false;
				movingSpeed = Vector3.Distance (animatedItem.transform.position, destination);
				rotationSpeed = Vector3.Distance (animatedItem.transform.rotation.eulerAngles, targetRotation.eulerAngles);
				scalingSpeed = Vector3.Distance (animatedItem.transform.localScale, targetScale);
				movingItemToPlace = true;
			}
		}
	}

	public void SelectedWorld(int world) {
		MenuScript.PlayButtonSound ();
		if (world == 4) {
			loading = true;
			SceneManager.LoadScene ("explorer");
		} else {
			SetLevelInfo (world);
			chosenWorldName = WorldNames [world];
			SceneManager.LoadScene ("level_select");
		}
	}

	public static void SetLevelInfo(int world) {
		levels = worlds [world];
		levelConfigurations = worldConfigurations [WorldNames [world]];
	}

	private void ShowReceivedItemTextBox() {
		String text = "";
		if (animatedItem.name.Contains("puzzleBoxWorld4"))
			text = "You have unlocked Explorer mode!\nYou can find more cars/levels here.";
		else if (animatedItem.name == "car1")
			text = "You have unlocked your first car!";
		else if (animatedItem.name.Contains("puzzleBoxWorld1"))
			text = "You have unlocked a puzzle Box!\nThis contains the first levels to play.";
		else if (animatedItem.name.Contains ("car"))
			text = "You have unlocked a new car!";
		else
			text = "You have unlocked new levels!";

		ShowTextBox (text);
	}

	private void ShowEndText() {
		String text = "Wow, you unlocked everything!\nYou are winner!\nThank you for playing our little game.\nVinLia Games";
		ShowTextBox (text);
	}

	private void ShowTextBox(String text) {
		String[] messages = new String[1];
		messages [0] = text;

		GameObject tutorialBox = Resources.Load ("TutorialBox") as GameObject;
		Vector3 position = new Vector3 (-2.66f, 10.69f, -8.2f);
		Quaternion rotation = Quaternion.Euler(new Vector3(60, 180, 0));
		GameObject iTutorialBox = (GameObject)GameObject.Instantiate (tutorialBox, position, rotation);
		iTutorialBox.transform.localScale = new Vector3 (12, 4, 4);
		iTutorialBox.name = "TutorialBox";
		iTutorialBox.GetComponent<TutorialBoxScript>().SetMessages(messages);
	}
		

	private void DestroyTutorialMessageBox() {
		Destroy (GameObject.Find ("TutorialBox"));
	}

	// Animation stuff
	//

	private void StartItemAnimation() {
		Pair nextPair = MenuScript.data.animationQueue.Dequeue ();
		String itemName = nextPair.First;
		int itemIndex = nextPair.Second;
		GameObject item = null;
		switch(itemName) {
		case "car1": item = displayCar1; break;
		case "car2": item = displayCar2; break;
		case "car3": item = displayCar3; break;
		case "car4": item = displayCar4; break;
		case "car5": item = displayCar5; break;
		case "car6": item = displayCar6; break;
		case "car7": item = displayCar7; break;
		case "car8": item = displayCar8; break;
		case "car9": item = displayCar9; break;
		case "car10": item = displayCar10; break;
		case "car11": item = displayCar11; break;
		case "car12": item = displayCar12; break;
		case "car13": item = displayCar13; break;
		case "car14": item = displayCar14; break;
		case "car15": item = displayCar15; break;
		case "car16": item = displayCar16; break;
		case "puzzleBoxWorld1": item = puzzleBoxWorld1; break;
		case "puzzleBoxWorld2": item = puzzleBoxWorld2; break;
		case "puzzleBoxWorld3": item = puzzleBoxWorld3; break;
		case "puzzleBoxWorld4": item = puzzleBoxWorld4; break;
		default:
			throw new ArgumentException("Unknown item name: " + itemName);
		}
		destination = item.transform.position;
		targetRotation = item.transform.rotation;
		if (itemName.Contains ("car")) {
			MenuScript.data.carsUnlocked [itemIndex] = true;
			GameObject box = (GameObject)Instantiate(carBox);
			animatedItem = (GameObject)Instantiate (item, carPositionStartAnimation, carRotationStartAnimation);
			targetScale = animatedItem.transform.localScale;
			animatedItem.transform.localScale = targetScale * 2;
			boxAnimator = box.GetComponent<Animator>();
			boxAnimator.Play ("Take 001");
			animationPlaying = true;
		} else {
			MenuScript.PlayItemReceivedSound ();
			MenuScript.data.puzzleBoxesUnlocked [itemIndex] = true;
			animatedItem = (GameObject)Instantiate (item, puzzleBoxPositionStartAnimation, puzzleBoxRotationStartAnimation);
			targetScale = animatedItem.transform.localScale;
			animatedItem.transform.localScale = targetScale * 4;
			animatedItem.GetComponent<puzzleBoxScript>().SetWorldNumber(itemIndex + 1);
			ShowReceivedItemTextBox ();
			showingItemButton = true;
		}
		animatedItem.name = itemName;
	}

	// Resource loading stuff
	//

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
			GameObject clone = (GameObject)Instantiate (displayCar9);
			clone.name = "car9";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[9]) {
			GameObject clone = (GameObject)Instantiate (displayCar10);
			clone.name = "car10";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[10]) {
			GameObject clone = (GameObject)Instantiate (displayCar11);
			clone.name = "car11";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[11]) {
			GameObject clone = (GameObject)Instantiate (displayCar12);
			clone.name = "car12";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[12]) {
			GameObject clone = (GameObject)Instantiate (displayCar13);
			clone.name = "car13";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[13]) {
			GameObject clone = (GameObject)Instantiate (displayCar14);
			clone.name = "car14";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[14]) {
			GameObject clone = (GameObject)Instantiate (displayCar15);
			clone.name = "car15";
			clone.transform.GetComponent<CarDisplayScript>().SelectCarIfItIsChosen();
		}
		if (MenuScript.data.carsUnlocked[15]) {
			GameObject clone = (GameObject)Instantiate (displayCar16);
			clone.name = "car16";
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
		if (MenuScript.data.puzzleBoxesUnlocked [3]) {
			GameObject clone = (GameObject)Instantiate (puzzleBoxWorld4);
			clone.transform.GetComponent<puzzleBoxScript>().SetWorldNumber (4);
		}
	}

	private static void SetVariables() {
		float buttonSize = (int)(Screen.width / 5 * 0.7);
		float offset = (Screen.width / 5 - buttonSize) / 2;
		leftBottomRect = new Rect (offset, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		loadingRect = new Rect (Screen.width / 2 - buttonSize / 2, Screen.height / 5 - buttonSize / 6, buttonSize, buttonSize / 3);
		itemButtonRect = new Rect ((Screen.width - (Screen.height / 5)) / 2, Screen.height / 9 * 8 - 15, Screen.height / 4.5f, Screen.height / 9f);

		levelTextTexture = (Texture2D) Resources.Load ("ui_border_levelname");
		itemButtonTexture = (Texture2D)Resources.Load ("ui_button_awesome");
		backButtonChosenStyle = MenuScript.backButtonStyle;

		loadingStyle = new GUIStyle ();
		loadingStyle.normal.textColor = Color.white;
		loadingStyle.fontSize = 28;
		loadingStyle.alignment = TextAnchor.MiddleCenter;
		loadingStyle.normal.background = levelTextTexture;

		itemButtonStyle = new GUIStyle ();
		itemButtonStyle.normal.background = itemButtonTexture;
	
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
		displayCar9 = Resources.Load ("displayCar9") as GameObject;
		displayCar10 = Resources.Load ("displayCar10") as GameObject;
		displayCar11 = Resources.Load ("displayCar11") as GameObject;
		displayCar12 = Resources.Load ("displayCar12") as GameObject;
		displayCar13 = Resources.Load ("displayCar13") as GameObject;
		displayCar14 = Resources.Load ("displayCar14") as GameObject;
		displayCar15 = Resources.Load ("displayCar15") as GameObject;
		displayCar16 = Resources.Load ("displayCar16") as GameObject;

		carBox = Resources.Load ("carBox") as GameObject;
	}

	public static void AddPuzzleBoxes() {
		puzzleBoxWorld1 = Resources.Load ("puzzleBoxWorld1") as GameObject;
		puzzleBoxWorld2 = Resources.Load ("puzzleBoxWorld2") as GameObject;
		puzzleBoxWorld3 = Resources.Load ("puzzleBoxWorld3") as GameObject;
		puzzleBoxWorld4 = Resources.Load ("puzzleBoxWorld4") as GameObject;
	}

	public static void AddLevels() {
		worldConfigurations = new Dictionary<string, Dictionary<string, LevelConfiguration>>();

		Dictionary<string, LevelConfiguration> world0Configuration = new Dictionary<string, LevelConfiguration> ();
		worldConfigurations.Add (WorldNames[0], world0Configuration);
		world0Configuration.Add ("tutorial_1", new LevelConfiguration (4, 5, 0f, 0f, 5));
		world0Configuration.Add ("tutorial_2", new LevelConfiguration (4, 5, 0f, 0f, 5));
		world0Configuration.Add ("tutorial_3", new LevelConfiguration (4, 5, 0f, 0f, 7));

		Dictionary<string, LevelConfiguration> world1Configuration = new Dictionary<string, LevelConfiguration> ();
		worldConfigurations.Add (WorldNames[1], world1Configuration);

		world1Configuration.Add ("level_01", new LevelConfiguration (5, 5, 0f, 0f, 4));
		world1Configuration.Add ("level_02", new LevelConfiguration (5, 5, 0f, 0f, 8));
		world1Configuration.Add ("level_03", new LevelConfiguration (5, 5, 0f, 0f, 8));
		world1Configuration.Add ("level_04", new LevelConfiguration (7, 5, 0f, 0f, 15));
		world1Configuration.Add ("level_05", new LevelConfiguration (7, 5, 0f, 0f, 9));
		world1Configuration.Add ("level_06", new LevelConfiguration (5, 5, 0f, 0f, 15));
		world1Configuration.Add ("level_07", new LevelConfiguration (5, 5, 0f, 0f, 24));
		world1Configuration.Add ("level_08", new LevelConfiguration (5, 5, 0f, 0f, 14));
		world1Configuration.Add ("level_09", new LevelConfiguration (7, 6, 0f, 0f, 91));
		world1Configuration.Add ("level_10", new LevelConfiguration (7, 5, 0f, 0f, 25));
		world1Configuration.Add ("level_11", new LevelConfiguration (5, 5, 0f, 0f, 20));
		world1Configuration.Add ("level_12", new LevelConfiguration (4, 5, 0f, 0f, 12));
		world1Configuration.Add ("level_13", new LevelConfiguration (7, 7, 0f, 0f, 21));
		world1Configuration.Add ("level_14", new LevelConfiguration (7, 7, 0f, 0f, 35));
		world1Configuration.Add ("level_15", new LevelConfiguration (7, 5, 0f, 0f, 20));
		world1Configuration.Add ("level_16", new LevelConfiguration (6, 5, 0f, 0f, 33));
		world1Configuration.Add ("level_17", new LevelConfiguration (8, 4, 0f, 0f, 38));
		world1Configuration.Add ("level_18", new LevelConfiguration (8, 7, 0f, 0f, 50));
		world1Configuration.Add ("level_19", new LevelConfiguration (8, 7, 0f, 0f, 30));
		world1Configuration.Add ("level_20", new LevelConfiguration (7, 7, 0f, 0f, 45));
		world1Configuration.Add ("level_21", new LevelConfiguration (7, 5, 0f, 0f, 17));
		world1Configuration.Add ("level_22", new LevelConfiguration (6, 4, 0f, 0f, 19));
		world1Configuration.Add ("level_23", new LevelConfiguration (6, 5, 0f, 0f, 7));

		Dictionary<string, LevelConfiguration> world2Configuration = new Dictionary<string, LevelConfiguration> ();
		worldConfigurations.Add (WorldNames[2], world2Configuration);

		world2Configuration.Add ("level_l01", new LevelConfiguration (5, 5, 0f, 0f, 9));
		world2Configuration.Add ("level_l02", new LevelConfiguration (5, 5, 0f, 0f, 34));
		world2Configuration.Add ("level_l03", new LevelConfiguration (7, 5, 0f, 0f, 25));
		world2Configuration.Add ("level_l04", new LevelConfiguration (5, 5, 0f, 0f, 23));
		world2Configuration.Add ("level_l05", new LevelConfiguration (6, 5, 0f, 0f, 17));
		world2Configuration.Add ("level_l06", new LevelConfiguration (5, 5, 0f, 0f, 12));
		world2Configuration.Add ("level_l07", new LevelConfiguration (6, 6, 0f, 0f, 17));
		world2Configuration.Add ("level_l08", new LevelConfiguration (6, 6, 0f, 0f, 34));
		world2Configuration.Add ("level_l09", new LevelConfiguration (5, 4, 0f, 0f, 15));

		Dictionary<string, LevelConfiguration> world3Configuration = new Dictionary<string, LevelConfiguration> ();
		worldConfigurations.Add (WorldNames[3], world3Configuration);
		
		world3Configuration.Add ("level_s01", new LevelConfiguration (4, 5, 0f, 0f, 5));
		world3Configuration.Add ("level_s02", new LevelConfiguration (4, 5, 0f, 0f, 7));
		world3Configuration.Add ("level_s03", new LevelConfiguration (5, 5, 0f, 0f, 11));
		world3Configuration.Add ("level_s04", new LevelConfiguration (5, 5, 0f, 0f, 13));
		world3Configuration.Add ("level_s05", new LevelConfiguration (4, 5, 0f, 0f, 8));
		world3Configuration.Add ("level_s06", new LevelConfiguration (9, 5, 0f, 0f, 17));
		world3Configuration.Add ("level_s07", new LevelConfiguration (7, 4, 0f, 0f, 19));
		world3Configuration.Add ("level_s08", new LevelConfiguration (7, 6, 0f, 0f, 35));

		displayNames = new Dictionary<string, string> ();
		displayNames.Add ("tutorial_1", "0-1");
		displayNames.Add ("tutorial_2", "0-2");
		displayNames.Add ("tutorial_3", "0-3");
		displayNames.Add ("level_01", "1-1");
		displayNames.Add ("level_02", "1-2");
		displayNames.Add ("level_13", "1-3");
		displayNames.Add ("level_05", "1-4");
		displayNames.Add ("level_07", "1-5");
		displayNames.Add ("level_22", "1-6");
		displayNames.Add ("level_03", "1-7");
		displayNames.Add ("level_15", "1-8");
		displayNames.Add ("level_23", "1-9");
		displayNames.Add ("level_06", "1-10");
		displayNames.Add ("level_04", "1-11");
		displayNames.Add ("level_10", "1-12");
		displayNames.Add ("level_11", "1-13");
		displayNames.Add ("level_09", "1-14");
		displayNames.Add ("level_12", "1-15");
		displayNames.Add ("level_16", "1-16");
		displayNames.Add ("level_08", "1-17");
		displayNames.Add ("level_18", "1-18");
		displayNames.Add ("level_21", "1-19");
		displayNames.Add ("level_14", "1-20");
		displayNames.Add ("level_17", "1-21");
		displayNames.Add ("level_19", "1-22");
		displayNames.Add ("level_20", "1-23");

		displayNames.Add ("level_l01", "2-1");
		displayNames.Add ("level_l02", "2-8");
		displayNames.Add ("level_l03", "2-5");
		displayNames.Add ("level_l04", "2-7");
		displayNames.Add ("level_l05", "2-2");
		displayNames.Add ("level_l06", "2-6");
		displayNames.Add ("level_l07", "2-4");
		displayNames.Add ("level_l08", "2-9");
		displayNames.Add ("level_l09", "2-3");

		displayNames.Add ("level_s01", "3-1");
		displayNames.Add ("level_s02", "3-2");
		displayNames.Add ("level_s03", "3-8");
		displayNames.Add ("level_s04", "3-5");
		displayNames.Add ("level_s05", "3-3");
		displayNames.Add ("level_s06", "3-6");
		displayNames.Add ("level_s07", "3-7");
		displayNames.Add ("level_s08", "3-4");
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

[Serializable]
public class Pair : IEquatable<Pair> {	
	public Pair(String first, int second) {
		this.First = first;
		this.Second = second;
	}
		
	public override bool Equals(object obj)
	{
		return Equals(obj as Pair);
	}

	public bool Equals(Pair other) {
		return this.First == other.First && this.Second == other.Second;
	}

	public override int GetHashCode ()
	{
		return this.First.GetHashCode () + this.Second;
	}
	
	public String First { get; set; }
	public int Second { get; set; }
};

