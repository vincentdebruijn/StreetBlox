using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

public class GameScript : MonoBehaviour {

	// The distance the bridge piece has to move to make the bridge completely open.
	public const float BridgeOpenDistance = 0.145f;

	public static Color backgroundColor = new Color (0, 0, 0, 0.75f);
	
	// The time it takes for the car to completely move off the previous puzzle piece.
	public float time_till_car_is_off_previous_puzzlepiece;
	
	public GameObject[] puzzlePieces;
	public GameObject[][] board;
	
	// Set this to true to start the game
	public Boolean gameStarted = false;
	
	private CarScript carScript;
	
	private LevelConfiguration levelConfiguration;
	
	// The animation at the start
	private Boolean showingAnimation;
	private float timeSinceStartAnimation = 0f;
	// The puzzle pieces that are currently in the start animation
	private GameObject[] currentPuzzlePiecesAnimation;
	// The index of the last puzzle piece being animated.
	private int puzzlePieceAnimationIndex;
	// Where the puzzle pieces need to go.
	private Vector3[] currentPuzzlePiecesDestination;
	private static Boolean dontPlayAnimation;
	// The time passed since the start of the game
	private float time;	
	private int movesMade;
	private int stars;
	private Boolean processedGameOver;

	private List<GameObject> bridgePieces;
	
	public string chosenLevel;

	// GUI stuff
	private static Texture2D retryButtonTexture, retryButtonPressedTexture;
	private static Texture2D nextButtonTexture, nextButtonPressedTexture;
	private static Texture2D oneStarMedalTexture;
	private static Texture2D twoStarMedalTexture;
	private static Texture2D threeStarMedalTexture;
	private static Texture2D statTextTexture;
	private static Texture2D quitTexture;
	private static Texture2D resetTexture;
	private static Texture2D goTexture1;
	private static Texture2D goTexture2;
	private static Texture2D goTexture3;
	private static Texture2D goTexture4;
	private static Texture2D displayTexture;
	private static Texture2D boostTexture1;
	private static Texture2D boostTexture2;
	private static Texture2D boostTexture3;
	private static Texture2D boostTexture4;
	private static Texture2D tutorialTexture;
	
	private static GUIStyle retryButtonStyle, retryButtonPressedStyle, retryButtonChosenStyle;
	private static GUIStyle nextButtonStyle, nextButtonPressedStyle, nextButtonChosenStyle;
	private static GUIStyle backButtonChosenStyle;
	private static GUIStyle textAreaStyle;
	private static GUIStyle oneStarMedalStyle;
	private static GUIStyle twoStarMedalStyle;
	private static GUIStyle threeStarMedalStyle;
	private static GUIStyle statStyle;
	private static GUIStyle statTextStyle;
	private static GUIStyle timerStyle;
	private static GUIStyle quitStyle;
	private static GUIStyle resetStyle;
	private static GUIStyle goStyle1;
	private static GUIStyle goStyle2;
	private static GUIStyle goStyle3;
	private static GUIStyle goStyle4;
	private static GUIStyle chosenGoStyle;
	private static GUIStyle displayStyle;
	private static GUIStyle boostStyle1;
	private static GUIStyle boostStyle2;
	private static GUIStyle boostStyle3;
	private static GUIStyle boostStyle4;
	private static GUIStyle chosenBoostStyle;
	private static GUIStyle tutorialStyle;
	
	private static Rect rightButtonRect;
	private static Rect leftButtonRect;
	private static Rect textArea;
	private static Rect medalRect;
	private static Rect statTextRect;
	private static Rect quitRect;
	private static Rect resetRect;
	private static Rect goRect;
	private static Rect displayRect;
	private static Rect boostRect;
	private static Rect tutorialRect;

	private int tutorialMessageCounter;
	
	private static Dictionary<String, String[]> tutorialMessages;

	// Dynamic one time loading of all static variables
	private static Boolean staticVariablesSet = false;

	private void OnLevelWasLoaded(int iLevel) {
		chosenLevel = Application.loadedLevelName;
		if (chosenLevel == "explorer")
			levelConfiguration = new LevelConfiguration (60, 60, 0f, 0f, 0);
		else
			levelConfiguration = LevelSelectScript.levelConfigurations[chosenLevel];

		SetCarOnStartPiece ();
		GetPuzzlePieces ();
		SetBridgePieces ();
		chosenGoStyle = goStyle1;
		chosenBoostStyle = boostStyle1;
		Array.Sort (puzzlePieces, new PositionBasedComparer ());
		MenuScript.canvas.GetComponent<Image>().color = Color.clear;
		tutorialMessageCounter = 0;
		showingAnimation = false;
		CreateBoard ();

		if (!dontPlayAnimation && MenuScript.data.playAnimations) {
			showingAnimation = true;
			HidePuzzlePieces ();
			StartPuzzlePieceAnimation ();
		}
	}

	void Awake() {	
		if (!staticVariablesSet) {
			PuzzlePieceScript.MakePuzzlePieceConnections ();
			AddTutorialMessages();
			SetVariables ();
			staticVariablesSet = true;
		}
	}

	// Use this for initialization
	void Start () {
		dontPlayAnimation = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (showingAnimation) {
			timeSinceStartAnimation += Time.deltaTime;
			if (timeSinceStartAnimation > 0.125f && puzzlePieceAnimationIndex == 0) {
				StartSecondPuzzlePiece();
			}
			MoveCurrentPuzzlePiece();
		}

		if (!gameStarted)
			return;

		if (!carScript.carStarted) {
			time += Time.deltaTime;
			if (time < levelConfiguration.waitTimeAtStart)
				return;
			carScript.StartTheGame (levelConfiguration);
		}
		if (carScript.GameOver ()) {
			if (Input.GetMouseButton(0)) {
				Vector3 mousePosition = new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 0);
				if (rightButtonRect.Contains (mousePosition)) {
					retryButtonChosenStyle = retryButtonPressedStyle;
					nextButtonChosenStyle = nextButtonPressedStyle;
				} else if (leftButtonRect.Contains (mousePosition)) {
					backButtonChosenStyle = MenuScript.backButtonPressedStyle;
				}
			}
			return;
		}
	}
	
	void OnGUI() {
		if (!showingAnimation && !gameStarted && !carScript.carStarted)
			DisplayTutorialMessages ();

		if (!carScript.ended && !carScript.crashed && !carScript.fell)
			DisplayButtonBar();

		if (!processedGameOver && carScript.GameOver ()) {
			SetBackgroundColor();
			processedGameOver = true;
			SetStars ();
			LevelSelectScript.UpdateProgress (carScript.ended, stars);
		}

		string text = null;
		if (carScript.crashed || carScript.fell) {
			text = "The Car didn't make it!";
			if (GUI.Button (rightButtonRect, "", retryButtonChosenStyle)) {
				retryButtonChosenStyle = retryButtonStyle;
				nextButtonChosenStyle = nextButtonStyle;
				MenuScript.PlayButtonSound();
				Reset("Retry");
			}
		}
		if (carScript.ended) {
			text = "Success!";
			if (GUI.Button (rightButtonRect, "", nextButtonChosenStyle)) {
				nextButtonChosenStyle = nextButtonStyle;
				retryButtonChosenStyle = retryButtonStyle;
				MenuScript.PlayButtonSound();
				Reset("Next");
			}
			int total = 0;
			int touchedTotal = 0;
			foreach(GameObject piece in GetPuzzlePieces()) {
				if (carScript.piecesTouched[piece.name] > 0)
					touchedTotal += 1;
				if (PuzzlePieceScript.PuzzlePieceConnections.GetPuzzlePieceConnections(piece) != null)
					total += 1;
			}
			GUIStyle medalStyle = oneStarMedalStyle;
			if (stars == 2)
				medalStyle = twoStarMedalStyle;
			else if(stars == 3)
				medalStyle = threeStarMedalStyle;
			GUI.Label(medalRect, "", medalStyle);
			GUI.Label (statTextRect, "     Moves made: " + movesMade+"\n     Level par: " + levelConfiguration.par + "\n     Pieces touched: " + touchedTotal+"/"+total, statTextStyle);
		}
		if (text != null) {
			GUI.Label (textArea, text, textAreaStyle);
			if (GUI.Button (leftButtonRect, "", backButtonChosenStyle)) {
				backButtonChosenStyle = MenuScript.backButtonStyle;
				MenuScript.PlayButtonSound();
				Reset("Back");
			}
		}
	}

	private void DisplayTutorialMessages() {
		if (!MenuScript.data.playTutorials || !tutorialMessages.ContainsKey(chosenLevel))
			return;

		String[] messagesForCurrentLevel = tutorialMessages [chosenLevel];
		if (tutorialMessageCounter < messagesForCurrentLevel.Length) {
			if (GUI.Button (tutorialRect, messagesForCurrentLevel[tutorialMessageCounter], tutorialStyle)) {
				MenuScript.PlayButtonSound();
				tutorialMessageCounter += 1;
			}
		}
	}
		
	private void SetStars() {	
		stars = 2;
		if (movesMade < levelConfiguration.par)
			stars = 3;
		else if (movesMade > levelConfiguration.par)
			stars = 1;
	}

	private void DisplayButtonBar() {
		if (GUI.Button (quitRect, "", quitStyle)) {
			MenuScript.PlayButtonSound ();
			if (chosenLevel == "explorer")
				Application.LoadLevel ("world_select");
			else if (MenuScript.InTutorialPhase() != null)
				Application.LoadLevel ("menu");
			else
				Application.LoadLevel ("level_select");
		}

		if (GUI.Button (resetRect, "", resetStyle)) {
			MenuScript.PlayButtonSound ();
			Reset ("Retry");
		}
		
		if (gameStarted && time < levelConfiguration.waitTimeAtStart) {
			double restTime = Math.Ceiling (levelConfiguration.waitTimeAtStart - time);
			if (restTime == 3)
				chosenGoStyle = goStyle3;
			else if (restTime == 1)
				chosenGoStyle = goStyle4;
			if (GUI.Button (goRect, restTime + "   ", chosenGoStyle)) {
				time += (float)restTime;
			}
		} else if (gameStarted) {
			GUI.Label (goRect, "", chosenGoStyle);
		} else {
			if (GUI.Button (goRect, "", chosenGoStyle)) {
				MenuScript.PlayButtonSound ();
				chosenGoStyle = goStyle2;
				StartTheGame();
			}
		}

		GUI.Label (displayRect, "" + movesMade, displayStyle);

		if (gameStarted) {
			if (GUI.Button (boostRect, "", chosenBoostStyle)) {
				if (carScript.boost < 3f) {
					MenuScript.PlayGearShiftSound ();
					if (chosenBoostStyle == boostStyle1)
						chosenBoostStyle = boostStyle2;
					else if (chosenBoostStyle == boostStyle2)
						chosenBoostStyle = boostStyle3;
					else if (chosenBoostStyle == boostStyle3)
						chosenBoostStyle = boostStyle4;
					carScript.boost += 1f;
				}
			}
		} else {
			GUI.Label (boostRect, "", chosenBoostStyle);
		}
	}

	// Start the counter!
	private void StartTheGame() {
		EndAnimation ();
		time = 0.0f;
		movesMade = 0;
		processedGameOver = false;
		carScript.carStarted = false;
		carScript.boost = 0f;
		gameStarted = true;
	}

	private void SetCarOnStartPiece() {
		GameObject startPiece = GameObject.FindGameObjectWithTag ("StartPuzzlePiece");
		Vector3 pos = startPiece.transform.position;
		GameObject car = Resources.Load (MenuScript.data.chosenCar) as GameObject;
		car = (GameObject)Instantiate (car, new Vector3 (pos.x, 0.105f, pos.z), Quaternion.Euler (0, 90, 0));
		carScript = car.GetComponent<CarScript> ();
	}

	private void SetBackgroundColor() {
		Image image = MenuScript.canvas.GetComponent<Image> ();
		image.color = backgroundColor;
	}
	
	public void Reset(string button) {
		gameStarted = false;
		carScript.Reset ();
		Boolean tutorialsFinished = 
			MenuScript.InTutorialPhase() == null && MenuScript.data.levelProgress.Count == WorldSelectScript.levelsTutorial.Length;
		
		if (tutorialsFinished && !MenuScript.data.worldSelectShown) {
			MenuScript.canvas.GetComponent<Image> ().color = MenuScript.originalCanvasColor;
			dontPlayAnimation = false;
			MenuScript.data.worldSelectShown = true;
			MenuScript.Save ();
			Application.LoadLevel ("world_select");
			return;
		}

		switch(button) {
		case "Next":
			dontPlayAnimation = false;
			LevelSelectScript.NextLevel();
			break;
		case "Retry":
			dontPlayAnimation = true;
			Application.LoadLevel (chosenLevel);
			break;
		case "Back":
			MenuScript.canvas.GetComponent<Image>().color = MenuScript.originalCanvasColor;
			dontPlayAnimation = false;
			if (chosenLevel == "explorer")
				Application.LoadLevel ("world_select");
			else if (MenuScript.InTutorialPhase() != null)
				Application.LoadLevel ("menu");
			else
				Application.LoadLevel("level_select");
			break;
		}
	}

	public void ClickedPuzzlePiece(GameObject puzzlePiece) {
		Debug.Log("Clicked: " + puzzlePiece.name);
		if (carScript.GameOver () || !gameStarted || carScript.falling || carScript.crashing)
			return;
		if (carScript.currentPuzzlePiece == puzzlePiece || (carScript.previousPuzzlePiece == puzzlePiece && 
		    	carScript.timeSinceOnLastPuzzlePiece < 0.1 / (levelConfiguration.movement + carScript.boost))) {
			carScript.PlayCarHorn ();
			return;
		}
		 if (puzzlePiece.CompareTag ("UnmovablePuzzlePiece"))
			return;
		
		int boardHeight = levelConfiguration.BoardHeight;
		int boardWidth = levelConfiguration.BoardWidth;
		float leftXPosition = levelConfiguration.LeftXPosition;
		float pieceSize = levelConfiguration.PieceSize;
		float topZPosition = levelConfiguration.TopZPosition;

		Vector3 temp = puzzlePiece.transform.position;
		int x = (int)((temp.x - leftXPosition) / pieceSize);
		int y = (int)(-(temp.z - topZPosition) / pieceSize);
		int new_x = -1;
		int new_y = -1;
		if (puzzlePiece.name.Contains ("beacon")) {
			Boolean done = false;
			foreach(GameObject[] row in board) {
				new_y += 1;
				new_x = -1;
				foreach(GameObject obj in row) {
					new_x += 1;
					if (obj == null) {
						done = true;
						break;
					}
				}
				if (done)
					break;
			}
			board [new_y] [new_x] = puzzlePiece;
			board [y] [x] = null;
			puzzlePiece.transform.position = GetPuzzlePiecePosition(new_x, new_y);
			MenuScript.PlayPuzzlePieceSound();
			movesMade += 1;
		} else {
			int[][] points = new int[][] {new int[] {1,0}, new int[] {0, 1}, new int[] {-1,0}, new int[] {0,-1}};
			int[] point2 = new int[0];
			foreach (int[] point in points) {
				new_x = x + point [0];
				new_y = y + point [1];
				if (new_x >= 0 && new_y >= 0 && new_x < boardWidth && new_y < boardHeight && board [new_y] [new_x] == null) {
					point2 = point;
					break;
				}
			}
			if (new_x >= 0 && new_y >= 0 && new_x < boardWidth && new_y < boardHeight && board [new_y] [new_x] == null) {
				board [new_y] [new_x] = puzzlePiece;
				board [y] [x] = null;
				temp.x += point2 [0] * pieceSize;
				temp.z -= point2 [1] * pieceSize;
				puzzlePiece.transform.position = temp;
				
				MenuScript.PlayPuzzlePieceSound();
				movesMade += 1;
			}
		}
	}

	// Closes the bridges that are open, opens the bridges that are closed.
	public void FlipBridgePositions() {
		foreach (GameObject bridgePiece in bridgePieces) {
			Vector3 pos = bridgePiece.transform.position;
			if (IsBridgeOpen(pos))
				pos.x -= BridgeOpenDistance;
			else
				pos.x += BridgeOpenDistance;
			bridgePiece.transform.position = pos;
		}
	}

	// return true when  the bridge is open, false otherwise.
	public Boolean IsBridgeOpen(Vector3 pos) {
		float relativeX = pos.x % 0.5f;
		if (Math.Abs (relativeX - BridgeOpenDistance) < 0.01)
			return true;
		else if (relativeX < 0.01)
			return false;
		else
			throw new System.InvalidOperationException ("Bridge is in invalid position: " + relativeX);
	}

	public GameObject GetOtherPortalPiece(GameObject portalPiece) {
		GameObject portal = null;
		foreach(Transform child in portalPiece.transform) {
			if (child.gameObject.name == "Portal") {
				portal = child.gameObject;
				break;
			}
		}

		GameObject[] portals = GameObject.FindGameObjectsWithTag (portal.tag);
		GameObject otherPortal = null;
		if (portals [0] == portal)
			otherPortal = portals [1];
		else
			otherPortal = portals [0];

		return otherPortal.transform.parent.gameObject;
	}

	public GameObject[] GetPuzzlePieces() {
		if (puzzlePieces != null && puzzlePieces.Length > 0) 
			return puzzlePieces;
		GameObject[] movablePuzzlePieces = GameObject.FindGameObjectsWithTag ("PuzzlePiece");
		GameObject[] unmovablePuzzlePieces = GameObject.FindGameObjectsWithTag ("UnmovablePuzzlePiece");
		puzzlePieces = new GameObject[movablePuzzlePieces.Length + unmovablePuzzlePieces.Length];
		int i = 0;
		foreach (GameObject piece in movablePuzzlePieces) {
			puzzlePieces [i] = piece;
			i++;
		}
		foreach (GameObject piece in unmovablePuzzlePieces) {
			puzzlePieces [i] = piece;
			i++;
		}
		return puzzlePieces;
	}

	// Find and set the bridge pieces for easy access later
	private void SetBridgePieces() {
		bridgePieces = new List<GameObject>();
		GameObject[] puzzlePieces = GetPuzzlePieces ();
		foreach (GameObject piece in puzzlePieces) {
			if (piece.name.Contains("bridge")) {
				GameObject bridgePiece = piece.transform.Find ("bridgePiece").gameObject;
				bridgePieces.Add(bridgePiece);
			}
		}
	}
	
	private void CreateBoard() {
		Debug.Log ("Number of puzzle pieces: " + puzzlePieces.Length);
		int boardHeight = levelConfiguration.BoardHeight;
		int boardWidth = levelConfiguration.BoardWidth;
		float leftXPosition = levelConfiguration.LeftXPosition;
		float pieceSize = levelConfiguration.PieceSize;
		board = new GameObject[boardHeight][];
		board [0] = new GameObject[boardWidth];
		// We always expect the x-position of the next piece to be PieceSize away from the last one.
		// If not, then we found our empty piece.
		float expected_x = leftXPosition;
		int x = 0;
		int y = 0;
		foreach (GameObject piece in puzzlePieces) {
			if (piece.transform.position.x != expected_x) {
				Debug.Log ("Empty piece added at: " + x + " " + y);
				board [y] [x] = null;
				x += 1;
				expected_x += pieceSize;
				if (x == boardWidth) {
					x = 0;
					expected_x = leftXPosition;
					y += 1;
					if (y < boardHeight) {
						board [y] = new GameObject[boardWidth];
					}
				}
			}
			board [y] [x] = piece;
			x += 1;
			expected_x += pieceSize;
			if (x == boardWidth) {
				x = 0;
				expected_x = leftXPosition;
				y += 1;
				if (y < boardHeight) {
					board [y] = new GameObject[boardWidth];
				}
			}
		}
	}

	private Vector3 GetPuzzlePiecePosition(int x, int y) {
		return new Vector3 (x * levelConfiguration.PieceSize + levelConfiguration.LeftXPosition, 0f, 
		                    -y * levelConfiguration.PieceSize + levelConfiguration.TopZPosition);
	}

	// ANIMATION
	//

	private void HidePuzzlePieces() {
		foreach (GameObject piece in puzzlePieces) {
			piece.SetActive (false);
		}
	}

	private void StartPuzzlePieceAnimation() {
		currentPuzzlePiecesAnimation = new GameObject[2];
		currentPuzzlePiecesAnimation[0] = puzzlePieces[0];
		puzzlePieceAnimationIndex = 0;
		currentPuzzlePiecesDestination = new Vector3[2];
		currentPuzzlePiecesDestination[0]  = currentPuzzlePiecesAnimation[0].transform.position;
		MovePuzzlePieceAnimationToStartPosition ();
		timeSinceStartAnimation = 0f;
	}

	private void StartSecondPuzzlePiece() {
		currentPuzzlePiecesAnimation[1] = puzzlePieces[1];
		puzzlePieceAnimationIndex = 1;
		currentPuzzlePiecesDestination[1]  = currentPuzzlePiecesAnimation[1].transform.position;
		MovePuzzlePieceAnimationToStartPosition ();
	}

	private void EndAnimation() {
		showingAnimation = false;
	}

	private void MovePuzzlePieceAnimationToStartPosition() {
		GameObject puzzlePiece = puzzlePieces [puzzlePieceAnimationIndex];
		puzzlePiece.transform.position = puzzlePiece.transform.position - new Vector3 (1f, -1f, -1f);
		puzzlePiece.SetActive (true);
	}

	private void MoveCurrentPuzzlePiece() {
		for (int i = 0; i < currentPuzzlePiecesAnimation.Length; i++) {
			GameObject puzzlePiece = currentPuzzlePiecesAnimation [i];
			if (puzzlePiece == null) {
				break;
			}
			puzzlePiece.transform.position = Vector3.MoveTowards (puzzlePiece.transform.position, 
		                                                            currentPuzzlePiecesDestination [i],
		                                                            6f * Time.deltaTime);
		}
		if (currentPuzzlePiecesAnimation[0].transform.position == currentPuzzlePiecesDestination[0]) {
			puzzlePieceAnimationIndex += 1;
			if (puzzlePieceAnimationIndex >= puzzlePieces.Length && currentPuzzlePiecesAnimation[1] == null) {
				EndAnimation();
				return;
			}
			if (puzzlePieceAnimationIndex < puzzlePieces.Length && puzzlePieces[puzzlePieceAnimationIndex] == null) {
				puzzlePieceAnimationIndex += 1;
			}
			if (puzzlePieceAnimationIndex >= puzzlePieces.Length && currentPuzzlePiecesAnimation[1] == null) {
				EndAnimation();
				return;
			}
			currentPuzzlePiecesAnimation[0] = currentPuzzlePiecesAnimation[1];
			currentPuzzlePiecesDestination[0] = currentPuzzlePiecesDestination[1];
			if (puzzlePieceAnimationIndex < puzzlePieces.Length) {
				currentPuzzlePiecesAnimation[1] = puzzlePieces [puzzlePieceAnimationIndex];
				currentPuzzlePiecesDestination[1] = currentPuzzlePiecesAnimation[1].transform.position;
				MovePuzzlePieceAnimationToStartPosition ();
			} else {
				currentPuzzlePiecesAnimation[1] = null;
			}
		}
	}
	
	private static void SetVariables() {
		int buttonSize = (int)(Screen.width / 5 * 0.7);
		float offset = (Screen.width / 5 - buttonSize) / 2;
		rightButtonRect = new Rect (Screen.width / 5 * 4 - offset, Screen.height / 2 - (buttonSize / 2), buttonSize, buttonSize);
		leftButtonRect = new Rect (Screen.width / 5 + offset - buttonSize, Screen.height / 2 - (buttonSize / 2), buttonSize, buttonSize);
		textArea = new Rect (0, 10, Screen.width, buttonSize);
		medalRect = new Rect (Screen.width / 2 - buttonSize / 2, buttonSize + 20, buttonSize, buttonSize);
		statTextRect = new Rect (Screen.width / 5 + offset, Screen.height / 2 + buttonSize / 2, Screen.width / 5 * 3 - 2 * offset, buttonSize * 1.5f);

		float uiButtonSize = Screen.height / 10;
		quitRect = new Rect (0, uiButtonSize, uiButtonSize * 2, uiButtonSize);
		resetRect = new Rect (0, uiButtonSize * 2, uiButtonSize * 2, 2 * uiButtonSize);
		goRect = new Rect (0, uiButtonSize * 4, uiButtonSize * 2, 2 * uiButtonSize);
		displayRect = new Rect (0, uiButtonSize * 6, uiButtonSize * 2, uiButtonSize);
		boostRect = new Rect (0, uiButtonSize * 7, uiButtonSize * 2, 2 * uiButtonSize);
		tutorialRect = new Rect (Screen.width / 2 - 450, 50, 900, 100);
		
		retryButtonTexture = (Texture2D)Resources.Load ("ui_button_retry");
		retryButtonPressedTexture = (Texture2D)Resources.Load ("ui_button_retry_pressed");
		nextButtonTexture = (Texture2D)Resources.Load ("ui_button_next");
		nextButtonPressedTexture = (Texture2D)Resources.Load ("ui_button_next_pressed");
		oneStarMedalTexture = (Texture2D)Resources.Load ("ui_medal_painted_star1");
		twoStarMedalTexture = (Texture2D)Resources.Load ("ui_medal_painted_star2");
		threeStarMedalTexture = (Texture2D)Resources.Load ("ui_medal_painted_star3");
		statTextTexture = (Texture2D)Resources.Load ("ui_border_levelname");
		quitTexture = (Texture2D)Resources.Load ("quit");
		resetTexture = (Texture2D)Resources.Load ("reset");
		goTexture1 = (Texture2D)Resources.Load ("go1");
		goTexture2 = (Texture2D)Resources.Load ("go2");
		goTexture3 = (Texture2D)Resources.Load ("go3");
		goTexture4 = (Texture2D)Resources.Load ("go4");
		displayTexture = (Texture2D)Resources.Load ("counter");
		boostTexture1 = (Texture2D)Resources.Load ("speed1");
		boostTexture2 = (Texture2D)Resources.Load ("speed2");
		boostTexture3 = (Texture2D)Resources.Load ("speed3");
		boostTexture4 = (Texture2D)Resources.Load ("speed4");
		tutorialTexture = displayTexture;
		
		retryButtonStyle = new GUIStyle ();
		retryButtonStyle.normal.background = retryButtonTexture;
		retryButtonPressedStyle = new GUIStyle ();
		retryButtonPressedStyle.normal.background = retryButtonPressedTexture;
		retryButtonChosenStyle = retryButtonStyle;
		
		nextButtonStyle = new GUIStyle ();
		nextButtonStyle.normal.background = nextButtonTexture;
		nextButtonPressedStyle = new GUIStyle ();
		nextButtonPressedStyle.normal.background = nextButtonPressedTexture;
		nextButtonChosenStyle = nextButtonStyle;
		
		oneStarMedalStyle = new GUIStyle ();
		oneStarMedalStyle.normal.background = oneStarMedalTexture;
		twoStarMedalStyle = new GUIStyle ();
		twoStarMedalStyle.normal.background = twoStarMedalTexture;
		threeStarMedalStyle = new GUIStyle ();
		threeStarMedalStyle.normal.background = threeStarMedalTexture;
		
		textAreaStyle = new GUIStyle ();
		textAreaStyle.fontSize = 64;
		textAreaStyle.normal.textColor = Color.white;
		textAreaStyle.alignment = TextAnchor.MiddleCenter;

		tutorialStyle = new GUIStyle ();
		tutorialStyle.fontSize = 32;
		tutorialStyle.normal.textColor = Color.white;
		tutorialStyle.alignment = TextAnchor.MiddleCenter;
		Texture2D texture = new Texture2D (1, 1, TextureFormat.RGBA32, false);
		texture.SetPixel (0, 0, new Color (0, 0, 0, 0.75f));
		texture.Apply ();
		tutorialStyle.normal.background = texture;
		
		statStyle = new GUIStyle ();
		statStyle.fontSize = 32;
		statStyle.normal.textColor = Color.white;
		statStyle.alignment = TextAnchor.MiddleLeft;
		
		statTextStyle = new GUIStyle ();
		statTextStyle.normal.textColor = Color.white;
		statTextStyle.fontSize = 24;
		statTextStyle.alignment = TextAnchor.MiddleLeft;
		statTextStyle.normal.background = statTextTexture;

		timerStyle = new GUIStyle ();
		timerStyle.fontSize = 32;
		timerStyle.normal.textColor = Color.white;
		texture = new Texture2D (1, 1, TextureFormat.RGBA32, false);
		texture.SetPixel (0, 0, new Color (0, 0, 0, 0.75f));
		texture.Apply ();
		timerStyle.normal.background = texture;

		quitStyle = new GUIStyle ();
		quitStyle.normal.background = quitTexture;

		resetStyle = new GUIStyle ();
		resetStyle.normal.background = resetTexture;

		goStyle1 = new GUIStyle ();
		goStyle1.normal.background = goTexture1;
		goStyle2 = new GUIStyle ();
		goStyle2.normal.background = goTexture2;
		goStyle2.fontSize = 36;
		goStyle2.normal.textColor = Color.black;
		goStyle2.fontStyle = FontStyle.Bold;
		goStyle2.alignment = TextAnchor.MiddleRight;
		goStyle3 = new GUIStyle ();
		goStyle3.normal.background = goTexture3;
		goStyle3.fontSize = 36;
		goStyle3.normal.textColor = Color.black;
		goStyle3.fontStyle = FontStyle.Bold;
		goStyle3.alignment = TextAnchor.MiddleRight;
		goStyle4 = new GUIStyle ();
		goStyle4.normal.background = goTexture4;
		goStyle4.fontSize = 36;
		goStyle4.normal.textColor = Color.black;
		goStyle4.fontStyle = FontStyle.Bold;
		goStyle4.alignment = TextAnchor.MiddleRight;

		displayStyle = new GUIStyle ();
		displayStyle.normal.background = displayTexture;
		displayStyle.fontSize = 16;
		displayStyle.normal.textColor = Color.green;
		displayStyle.alignment = TextAnchor.MiddleCenter;

		boostStyle1 = new GUIStyle ();
		boostStyle1.normal.background = boostTexture1;
		boostStyle2 = new GUIStyle ();
		boostStyle2.normal.background = boostTexture2;
		boostStyle3 = new GUIStyle ();
		boostStyle3.normal.background = boostTexture3;
		boostStyle4 = new GUIStyle ();
		boostStyle4.normal.background = boostTexture4;
		
		backButtonChosenStyle = MenuScript.backButtonStyle;
	}

	private static void AddTutorialMessages() {
		tutorialMessages = new Dictionary<string, string[]> ();

		// tutorial level 1
		String[] messages1 = new String[3];
		messages1 [0] = "The goal of this game is to let the car reach\nthe portal at the other end of the map.";
		messages1 [1] = "You can start the game by pressing the 'GO' button\non the left. You cannot move the puzzle pieces\nuntil you pressed this button.";
		messages1 [2] = "Move the puzzle pieces next to the empty spot by clicking on them.\nClick 'Reset' to restart or 'Quit' to go back to the menu.";
		tutorialMessages.Add ("tutorial_1", messages1);

		// tutorial level 2
		String[] messages2 = new String[1];
		messages2 [0] = "The beacon piece always moves to the empty spot.\nIt does not have to be next to the empty spot.";
		tutorialMessages.Add ("tutorial_2", messages2);
	}

	// Comparer to sort puzzle pieces
	private class PositionBasedComparer : System.Collections.Generic.IComparer<GameObject> {
		public int Compare(GameObject a, GameObject b) {
			float a_x = a.transform.position.x;
			float a_z = a.transform.position.z;
			float b_x = b.transform.position.x;
			float b_z = b.transform.position.z;
			if(a_z < b_z) {
				return 1;
			} else if(a_z > b_z) {
				return -1;
			} else if (a_x < b_x) {
				return -1;
			} else if (a_x > b_x) {
				return 1;
			}
			return 0;
		}
	}
}
