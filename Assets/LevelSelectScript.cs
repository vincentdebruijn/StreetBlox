using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

public class LevelSelectScript : MonoBehaviour {
	public static Dictionary<string, LevelConfiguration> levelConfigurations;
	private static string[] levels;
	public static string chosenLevel;

	// Where the first level button starts and cannot go below.
	private const float startX = -7.5f;
	private const float startY = 7.5f;
	private const float startZ = -6.5f;

	// Where the last level button cannot go above
	private const float endY = 1f;

	private Boolean loading;

	private static GameObject[] levelButtons;

	// GUI stuff
	public static GameObject levelButton;
	private static GameObject checkMark;
	
	private static Texture2D levelTextTexture;
	private static Texture tutorialTexture;
	private static Texture lavaWorldTexture;
	private static Texture spaceWorldTexture;

	private static Rect leftBottomRect;
	private static Rect loadingRect;

	private static GUIStyle backButtonChosenStyle;
	private static GUIStyle loadingStyle;

	private static float offset;
	private static int buttonSize;

	private bool swiping = false;
	private Vector2 lastPosition;
	Vector2 lastMousePosition;

	private GameObject canvas;

	// Dynamic one time loading of all static variables
	private static Boolean staticVariablesSet = false;

	void Awake() {
		SetLevelInfo ();
		puzzleBoxScript.animationLock = false;
		loading = false;
		if (!staticVariablesSet) {
			SetVariables ();
			staticVariablesSet = true;
		}

		SetBackgroundTexture ();
		SetLevelButtons ();

		//canvas = (GameObject)Resources.Load ("canvas");
		//Instantiate (canvas, new Vector3(0, 0, -6.2f), new Quaternion());
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

		if (Input.GetMouseButtonDown(0)) {
			//save began touch 2d point
			lastMousePosition = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
			swiping = true;
		}
		if (Input.GetMouseButtonUp(0)) {
			swiping = false;
		}
		if (swiping && Input.GetMouseButton (0)) {
			Vector2 newMousePosition = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
			MoveLevelButtons(lastMousePosition, newMousePosition);
			lastMousePosition = newMousePosition;
		}

/*
		if (Input.touchCount == 0) 
			return;
		
		Debug.Log ("HIER");
		
		if (Input.GetTouch(0).deltaPosition.sqrMagnitude != 0) {
			Debug.Log ("Inside");
			if (swiping == false){
				swiping = true;
				lastPosition = Input.GetTouch(0).position;
				return;
			} else {
				Vector2 direction = Input.GetTouch(0).position - lastPosition;
				
				if (Mathf.Abs(direction.y) > 0){
					MoveLevelButtons(lastPosition, Input.GetTouch(0).position);
				}
			}
		} else {
			swiping = false;
		}
*/
	}

	void OnGUI() {
		if (loading)
			GUI.Label(loadingRect, "Loading...", loadingStyle);
		// Back
		if (GUI.Button (leftBottomRect, "", backButtonChosenStyle)) {
			backButtonChosenStyle = MenuScript.backButtonStyle;
			MenuScript.PlayButtonSound();
			SceneManager.LoadScene ("world_select");
		}
	}

	public void SelectedLevel(String level) {
		MenuScript.PlayButtonSound ();
		loading = true;
		chosenLevel = level;

		if (WorldSelectScript.chosenWorldName == "Tutorial") {
			// just keep playing menu music
		} else if (WorldSelectScript.chosenWorldName == "Grass World") {
			MenuScript.StopMenuMusic();
			MenuScript.PlayWorld1Music();
		} else if (WorldSelectScript.chosenWorldName == "Lava World") {
			MenuScript.StopMenuMusic();
			MenuScript.PlayWorld2Music();
		} else if (WorldSelectScript.chosenWorldName == "Space World") {
			MenuScript.StopMenuMusic();
			MenuScript.PlayWorld3Music();
		}

		SceneManager.LoadScene (level);
	}
		
	public static void NextLevel() {
		int index = Array.IndexOf (levels, chosenLevel);
		if (index + 1 < levels.Length) {
			chosenLevel = levels [index + 1];
			SceneManager.LoadScene (chosenLevel);
		} else  {
			StopGameMusicAndPlayMenuMusic ();
			SceneManager.LoadScene ("level_select");
		}
	}

	public static Boolean TutorialLevel(String levelName) {
		foreach (String tutorialLevel in WorldSelectScript.levelsTutorial) {
			if (tutorialLevel == levelName)
				return true;
		}
		return false;
	}

	public static void StopGameMusicAndPlayMenuMusic() {
		if (Array.IndexOf (WorldSelectScript.levelsWorld1, chosenLevel) >= 0) {
			MenuScript.StopWorld1Music ();
			MenuScript.PlayMenuMusic ();
		} else if (Array.IndexOf (WorldSelectScript.levelsWorld2, chosenLevel) >= 0) {
			MenuScript.StopWorld2Music ();
			MenuScript.PlayMenuMusic ();
		} else if (Array.IndexOf (WorldSelectScript.levelsWorld3, chosenLevel) >= 0) {
			MenuScript.StopWorld3Music ();
			MenuScript.PlayMenuMusic ();
		}
	}

	private static void MoveLevelButtons(Vector2 position1, Vector2 position2) {
		float yDiff = (position2.y - position1.y);
		GameObject first = levelButtons [0];
		GameObject last = levelButtons [levelButtons.Length - 1];
		if (yDiff < 0 && CalculateNewLevelButtonPosition(first, yDiff).y < startY)
			return;
		else if (yDiff > 0 && CalculateNewLevelButtonPosition(last, yDiff).y > startY)
			return;
		foreach (GameObject levelButton in levelButtons) {
			levelButton.transform.position = CalculateNewLevelButtonPosition (levelButton, yDiff);
		}
	}

	private static Vector3 CalculateNewLevelButtonPosition(GameObject levelButton, float yDiff) {
		Vector3 position = levelButton.transform.position;
		Vector3 target = new Vector3(position.x, position.y + yDiff / 2, position.z);
		return Vector3.MoveTowards(position, target, 1 * Mathf.Abs(yDiff) * Time.deltaTime);
	}

	private static void SetLevelButtons() {
		levelButtons = new GameObject[levels.Length];
		float x = startX;
		float y = startY;
		float z = startZ;
		int levelsDone = 0;
		Quaternion direction = Quaternion.Euler (0, 0, 0);
		foreach (string level in levels) {
			string marbleText = "0/0";
			int marbles = 0;
			if (MenuScript.data.levelProgress.ContainsKey(level))
				marbles = MenuScript.data.levelProgress[level];
			if (!TutorialLevel(level))
				marbleText = marbles + "/5";

			Vector3 location = new Vector3(x, y, z);
			GameObject clone = (GameObject)Instantiate (levelButton, location, direction);
			clone.transform.FindChild("levelName").GetComponent<TextMesh>().text = WorldSelectScript.displayNames[level];
			clone.transform.FindChild("marbles").GetComponent<TextMesh>().text = marbleText;
			clone.GetComponent<LevelButtonScript> ().levelName = level;

			if (marbles == 5) {
				location.z -= 0.2f;
				location.y += 1.5f;
				GameObject iCheckMark = (GameObject)Instantiate (checkMark, location, direction);
				iCheckMark.transform.parent = clone.transform;
				Quaternion rotation = iCheckMark.transform.rotation;
				rotation.y = -180;
				iCheckMark.transform.rotation = rotation;
			}

			levelButtons[levelsDone] = clone;
			x += 3.7f;
			levelsDone += 1;
			if (levelsDone % 4 == 0) {
				x = -7.5f;
				y -= 3.7f;
			}
		}
	}

	private static void SetBackgroundTexture() {
		Transform puzzleBox = GameObject.Find ("puzzleBoxWorld").transform;
		Material lidMaterial = puzzleBox.FindChild ("lid").GetComponent<Renderer> ().material;
		
		if (WorldSelectScript.chosenWorldName == "Tutorial") {
			lidMaterial.mainTexture = tutorialTexture;
		} else if (WorldSelectScript.chosenWorldName == "Lava World") {
			lidMaterial.mainTexture = lavaWorldTexture;
		} else if (WorldSelectScript.chosenWorldName == "Space World") {
			lidMaterial.mainTexture = spaceWorldTexture;
		}
	}

	private static void SetVariables() {
		levelButton = Resources.Load ("levelButton") as GameObject;
		checkMark = Resources.Load ("check_mark") as GameObject;

		buttonSize = (int)(Screen.width / 5 * 0.7);
		offset = (Screen.width / 5 - buttonSize) / 2;
		leftBottomRect = new Rect (offset, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		loadingRect = new Rect (Screen.width / 2 - buttonSize / 2, Screen.height / 5 - buttonSize / 6, buttonSize, buttonSize / 3);

		levelTextTexture = (Texture2D) Resources.Load ("ui_border_levelname");
		tutorialTexture = (Texture) Resources.Load ("Grey");
		lavaWorldTexture = (Texture2D) Resources.Load ("lavaTexture_puzzlePiece_M");
		spaceWorldTexture = (Texture2D) Resources.Load ("spaceTexture_puzzlePiece_M");
		
		backButtonChosenStyle = MenuScript.backButtonStyle;

		loadingStyle = new GUIStyle ();
		loadingStyle.normal.textColor = Color.white;
		loadingStyle.fontSize = 28;
		loadingStyle.alignment = TextAnchor.MiddleCenter;
		loadingStyle.normal.background = levelTextTexture;
	}

	public static void SetLevelInfo() {
		levels = WorldSelectScript.levels;
		levelConfigurations = WorldSelectScript.levelConfigurations;
	}
}
