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
	public static string chosenLevel;

	// Where the first level button starts and cannot go below.
	private const float startX = -7.5f;
	private const float startY = 7.5f;
	private const float startZ = -6.5f;

	// Where the last level button cannot go above
	private const float endY = 1f;
	
	// The first level the user has the focus on
	private int focus;

	private Boolean loading;

	private static GameObject[] levelButtons;

	// GUI stuff
	public static GameObject levelButton;
	
	private static Texture2D levelTextTexture;
	private static Texture tutorialTexture;
	private static Texture lavaWorldTexture;
	private static Texture spaceWorldTexture;

	private static Rect leftBottomRect;
	private static Rect rightBottomRect;
	private static Rect loadingRect;

	private static GUIStyle backButtonChosenStyle;
	private static GUIStyle loadingStyle;
	
	private static float buttonHeight;
	private static float offset;
	private static int buttonSize;

	private bool swiping = false;
	private bool eventSent = false;
	private Vector2 lastPosition;
	Vector2 lastMousePosition;

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
			Application.LoadLevel ("world_select");
		}
	}

	public void SelectedLevel(String level) {
		MenuScript.PlayButtonSound ();
		MenuScript.canvas.GetComponent<Image> ().color = GameScript.backgroundColor;
		loading = true;
		chosenLevel = level;
		Application.LoadLevel (level);
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

	public static Boolean TutorialLevel(String levelName) {
		foreach (String tutorialLevel in WorldSelectScript.levelsTutorial) {
			if (tutorialLevel == levelName)
				return true;
		}
		return false;
	}

	private static void MoveLevelButtons(Vector2 position1, Vector2 position2) {
		float yDiff = (position2.y - position1.y);
		GameObject first = levelButtons [0];
		GameObject last = levelButtons [levelButtons.Length - 1];
		if (first.transform.position.y < startY && yDiff < 0)
			return;
		else if (last.transform.position.y > endY && yDiff > 0)
			return;
		foreach (GameObject levelButton in levelButtons) {
			Vector3 position = levelButton.transform.position;
			Vector3 target = new Vector3(position.x, position.y + yDiff, position.z);
			levelButton.transform.position = Vector3.MoveTowards(position, target, 1 * Mathf.Abs(yDiff) * Time.deltaTime);
		}
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
			if (!TutorialLevel(level)) {
				int marbles = 0;
				if (MenuScript.data.levelProgress.ContainsKey(level))
				    marbles = MenuScript.data.levelProgress[level];
				marbleText = marbles + "/5";
			}
			Vector3 location = new Vector3(x, y, z);
			GameObject clone = (GameObject)Instantiate (levelButton, location, direction);
			clone.transform.FindChild("levelName").GetComponent<TextMesh>().text = WorldSelectScript.displayNames[level];
			clone.transform.FindChild("marbles").GetComponent<TextMesh>().text = marbleText;
			clone.GetComponent<LevelButtonScript> ().levelName = level;
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
		Material boxMaterial = puzzleBox.FindChild ("box").GetComponent<Renderer> ().material;
		Material lidMaterial = puzzleBox.FindChild ("lid").GetComponent<Renderer> ().material;
		
		if (WorldSelectScript.chosenWorldName == "Tutorial") {
			boxMaterial.mainTexture = tutorialTexture;
			lidMaterial.mainTexture = tutorialTexture;
		} else if (WorldSelectScript.chosenWorldName == "Lava World") {
			boxMaterial.mainTexture = lavaWorldTexture;
			lidMaterial.mainTexture = lavaWorldTexture;
		} else if (WorldSelectScript.chosenWorldName == "Space World") {
			boxMaterial.mainTexture = spaceWorldTexture;
			lidMaterial.mainTexture = spaceWorldTexture;
		}
	}

	private static void SetVariables() {
		levelButton = Resources.Load ("levelButton") as GameObject;

		buttonSize = (int)(Screen.width / 5 * 0.7);
		offset = (Screen.width / 5 - buttonSize) / 2;
		leftBottomRect = new Rect (offset, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		rightBottomRect = new Rect (Screen.width - offset - buttonSize, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
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
		
		buttonHeight =  Screen.height / 2 - buttonSize / 2;
	}

	public static void SetLevelInfo() {
		levels = WorldSelectScript.levels;
		levelConfigurations = WorldSelectScript.levelConfigurations;
	}
}
