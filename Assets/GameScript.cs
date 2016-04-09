using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

public class GameScript : MonoBehaviour {

	public const string explorerStartPiece = "puzzlePiece_straight_WE";
	
	// The names of the pieces in explorer mode that will trigger the shop
	public static Dictionary<String, Pair<String, int>[]> shopTriggerPieces;

	// The distance the bridge piece has to move to make the bridge completely open.
	public const float BridgeOpenDistance = 0.145f;

	public static Color backgroundColor = new Color (0, 0, 0, 0.75f);

	public static Vector3 cameraCenter = new Vector3 (0.6f, 2.2f, -1.9f);
	
	// The time it takes for the car to completely move off the previous puzzle piece.
	public float time_till_car_is_off_previous_puzzlepiece;
	
	public GameObject[] puzzlePieces;
	public GameObject[][] board;
	
	// Set this to true to start the game
	public Boolean gameStarted = false;
	
	private CarScript carScript;
	
	private LevelConfiguration levelConfiguration;

	// The time passed since the start of the game
	private float time;	
	private int movesMade;
	private int marbles;
	private Boolean processedGameOver;
	private Boolean halted;
	private Boolean showingShop;

	private List<GameObject> bridgePieces;
	private Boolean bridgesFlipped;

	public string chosenLevel;

	private static Pair<string, int> explorerPair = new Pair<string, int> ("puzzleBoxWorld4", 3);
	private static Pair<string, int> tardisPair = new Pair<String, int> ("car11", 10);

	// In game UI
	private static Texture2D quitTexture;
	private static Texture2D resetTexture;
	private static Texture2D emptyResetTexture;
	private static Texture2D goTexture0, goTexture1, goTexture2, goTexture3, goTexture4, goTexture5, goTexture6, goTexture7;
	private static Texture2D displayTexture;
	private static Texture2D emptyDisplayTexture;
	private static Texture2D boostTexture1;
	private static Texture2D boostTexture4;

	private static GUIStyle statStyle;
	private static GUIStyle timerStyle;
	private static GUIStyle quitStyle;
	private static GUIStyle resetStyle;
	private static GUIStyle emptyResetStyle;
	private static GUIStyle goStyle;
	private static GUIStyle displayStyle;
	private static GUIStyle emptyDisplayStyle;
	private static GUIStyle boostStyle1;
	private static GUIStyle boostStyle4;
	private static GUIStyle chosenBoostStyle;

	private static Rect quitRect;
	private static Rect resetRect;
	private static Rect goRect;
	private static Rect displayRect;
	private static Rect boostRect;

	private GameObject canvas;
	private GameObject scoreScreen;
	private GameObject lossScreen;
	private static GameObject tutorialBox;

	// explorer
	private static Rect explorerQuitRect;
	private static Rect explorerGoRect;
	
	private static Dictionary<String, String[]> tutorialMessages;

	private GameObject car;
	private GameObject shop;
	private GameObject iFirstCar;
	private GameObject iSecondCar;
	public GameObject puzzleBoxWorld2;
	public GameObject puzzleBoxWorld3;

	// Dynamic one time loading of all static variables
	private static Boolean staticVariablesSet = false;

	// Unity functions
	//

	void OnLevelWasLoaded(int iLevel) {
		chosenLevel = SceneManager.GetActiveScene().name;
		car = Resources.Load (MenuScript.data.chosenCar) as GameObject;
		shop = Resources.Load ("shopScreen") as GameObject;
		tutorialBox = Resources.Load ("TutorialBox") as GameObject;
		movesMade = 0;

		bridgesFlipped = false;
		if (chosenLevel == "explorer") {
			levelConfiguration = new LevelConfiguration (49, 31, 0f, 0f, 0);
			if (MenuScript.data.board != null) {
				LoadFromSave ();
				MovePiecesToCorrectPosition ();
				SetCarToCorrectPosition ();
				SetCameraToCorrectPosition ();
				SetBridgePieces ();
				if (MenuScript.data.bridgesFlipped)
					FlipBridgePositions ();
				puzzleBoxWorld2 = GameObject.Find ("puzzleBoxWorld2");
				puzzleBoxWorld3 = GameObject.Find ("puzzleBoxWorld3");
				if (MenuScript.data.puzzleBoxesUnlocked [1])
					GameObject.Destroy (puzzleBoxWorld2);
				if (MenuScript.data.puzzleBoxesUnlocked [2])
					GameObject.Destroy (puzzleBoxWorld3);
				return;
			}
		} else
			levelConfiguration = LevelSelectScript.levelConfigurations [chosenLevel];

		SetCarOnStartPiece ();
		GetPuzzlePieces ();
		SetBridgePieces ();

		Array.Sort (puzzlePieces, new PositionBasedComparer ());
		CreateBoard ();

		DisplayTutorialMessages ();
	}

	void Awake() {	
		if (!staticVariablesSet) {
			PuzzlePieceScript.MakePuzzlePieceConnections ();
			AddTutorialMessages();
			SetVariables ();
			staticVariablesSet = true;
		}

		goStyle.normal.background = goTexture0;
		chosenBoostStyle = boostStyle1;
		canvas = (GameObject)Resources.Load ("canvas");
		scoreScreen = (GameObject)Resources.Load ("scoreScreen");
		lossScreen = (GameObject)Resources.Load ("failScreen");
	}

	void Update () {		
		if (!gameStarted)
			return;

		if (!halted && !carScript.carStarted) {
			time += Time.deltaTime;
			if (chosenLevel != "explorer" && time < levelConfiguration.waitTimeAtStart)
				return;
			goStyle.normal.background = goTexture7;
			if (chosenLevel == "explorer" && MenuScript.data.board != null) {
				carScript.Resume (levelConfiguration);
			} else {
				carScript.StartTheGame (levelConfiguration);
			}
		}

		if (carScript.GameOver ()) {
			if (!processedGameOver) {
				ProcessGameOver();
				processedGameOver = true;
			}

			return;
		}
	}
	
	void OnGUI() {
		if (!carScript.ended && !carScript.crashed && !carScript.fell)
			DisplayButtonBar();

		if (chosenLevel == "explorer")
			return;
	}

	//
	// API methods

	public void ShowFirstItemObtainedMessageIfIsFirstItem() {
		if (MenuScript.data.animationQueue.Count == 0 && !MenuScript.data.puzzleBoxesUnlocked [1] && !MenuScript.data.puzzleBoxesUnlocked [2] &&
		    Array.LastIndexOf (MenuScript.data.carsUnlocked, true) <= 0) {
			String[] messages = new String[2];
			messages [0] = "Nice one! Remember, everything you\nunlock will be queued and waiting\nfor you back at your bookcase. [1/2]";
			messages [1] = "If you leave Explorer Mode,\nyour location will be saved to\nthe last unmovable piece you\nhave driven over. [2/2]";
			DisplayTutorialMessage (messages);
		}
	}

	public void ShowExplorerModeMessage(int world) {
		halted = true;
		carScript.carStarted = false;
		String[] messages = new String[1];
		if (world == 2) {
			messages [0] = "Oh my, the bridge crossing the lava is\nopened! Maybe you can find something\nbehind the fort to close the bridge?";
		} else {
			messages [0] = "Hrm, there seems to be no\nphysical road towards that puzzlebox...";
		}
		DisplayTutorialMessage(messages);
	}

	public void Reset(string button) {
		gameStarted = false;
		halted = false;
		showingShop = false;
		carScript.Reset ();
		Boolean tutorialsFinished = 
			MenuScript.InTutorialPhase() && MenuScript.data.levelProgress.Count == WorldSelectScript.levelsTutorial.Length;

		if (tutorialsFinished && !MenuScript.data.worldSelectShown) {
			MenuScript.data.worldSelectShown = true;
			MenuScript.data.animationQueue.Enqueue(new Pair<string, int>("car1", 0));
			MenuScript.data.animationQueue.Enqueue(new Pair<string, int>("puzzleBoxWorld1", 0));
			MenuScript.Save ();
			SceneManager.LoadScene ("world_select");
			return;
		}

		switch(button) {
		case "Next":
			LevelSelectScript.NextLevel();
			break;
		case "Retry":
			SceneManager.LoadScene (chosenLevel);
			break;
		case "Back":
			StopGameMusicAndPlayMenuMusic();
			if (chosenLevel == "explorer")
				SceneManager.LoadScene ("world_select");
			else if (MenuScript.InTutorialPhase())
				SceneManager.LoadScene ("menu");
			else
				SceneManager.LoadScene("level_select");
			break;
		}
	}

	public void Halt() {
		gameStarted = false;
		halted = true;
		goStyle.normal.background = goTexture0;
	}

	public void ShowShop() {
		showingShop = true;
		Vector3 cameraPosition = Camera.main.transform.position;
		Vector3 position = new Vector3 (cameraPosition.x - 0.011f, cameraPosition.y - 1.395f, cameraPosition.z - 0.15f);
		GameObject iShop = (GameObject)Instantiate (shop, position, shop.transform.rotation);
		iShop.name = "shopScreen";
		SetMarbleText ();

		Pair<String, int>[] cars = shopTriggerPieces [carScript.currentPuzzlePiece.name];

		UnlockButtonScript[] scripts = iShop.GetComponentsInChildren<UnlockButtonScript> ();
		scripts [1].carIndex = cars[0].Second;
		if (MenuScript.data.carsUnlocked[cars[0].Second])
			scripts [1].SetToBought ();
		scripts [0].carIndex = cars[1].Second;
		if (MenuScript.data.carsUnlocked[cars[1].Second])
			scripts [0].SetToBought ();

		GameObject firstCar = Resources.Load (cars [0].First) as GameObject;
		GameObject secondCar = Resources.Load (cars [1].First) as GameObject;

		Vector3 firstCarPosition = new Vector3 (position.x - 0.465f, position.y + 0.065f, position.z + 0.65f);
		Vector3 secondCarPosition = new Vector3 (position.x + 0.393f, position.y + 0.12f, position.z + 0.65f);
		Quaternion firstCarRotation = Quaternion.Euler (new Vector3 (317.585f, 211.129f, 340.11f));
		Quaternion secondCarRotation = Quaternion.Euler (new Vector3 (324.33f, 229.77f,  323.61f));

		iFirstCar = Instantiate (firstCar, firstCarPosition, firstCarRotation) as GameObject;
		iSecondCar = Instantiate (secondCar, secondCarPosition, secondCarRotation) as GameObject;
		iFirstCar.transform.localScale = iFirstCar.transform.localScale / 2.75f;
		iSecondCar.transform.localScale = iSecondCar.transform.localScale / 2.75f;

		iFirstCar.GetComponent<CarDisplayScript> ().shopCar = true;
		iSecondCar.GetComponent<CarDisplayScript> ().shopCar = true;
	}

	public void SetMarbleText() {
		GameObject.Find ("marbleCounter").GetComponent<TextMesh> ().text = "" + MenuScript.data.marbles;
	}

	public void CloseShop() {
		GameObject.Destroy (GameObject.Find ("shopScreen"));
		GameObject.Destroy (iFirstCar);
		GameObject.Destroy (iSecondCar);
		showingShop = false;
	}

	public Boolean ShopTriggerPiece(String name) {
		return shopTriggerPieces.ContainsKey (name);
	}

	public GameObject GetPuzzleBoxForPuzzlePiece(String name) {
		if (name == "puzzlePiece_turnabout_S (2)")
			return puzzleBoxWorld2;
		else if (name == "puzzlePiece_turnabout_W (3)")
			return puzzleBoxWorld3;
		else
			return null;
	}

	public void ClickedPuzzlePiece(GameObject puzzlePiece) {
		Debug.Log("Clicked: " + puzzlePiece.name);
		if (carScript.GameOver () || carScript.falling || carScript.crashing)
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
		//if (board [y] [x] != puzzlePiece)
		//		throw new ArgumentException ("Clicked piece " + puzzlePiece.name + ", but calculated position at x: " + x + " y: " + y + ", which contains piece " + board [y] [x]);
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
				if (!gameStarted && chosenLevel != "explorer") {
					goStyle.normal.background = goTexture1;
					StartTheGame ();
				}
			}
		}
	}

	// Closes the bridges that are open, opens the bridges that are closed.
	public void FlipBridgePositions() {
		MenuScript.PlayBridgeSound ();
		foreach (GameObject bridgePiece in bridgePieces) {
			// bridgePiece.transform.GetComponent<AudioSource>().Play();
			Vector3 pos = bridgePiece.transform.position;
			if (bridgePiece.transform.parent.name.Contains("NS")) {
				if (IsBridgeOpen(pos))
					pos.z += BridgeOpenDistance;
				else
					pos.z -= BridgeOpenDistance;
			} else {
				if (IsBridgeOpen(pos))
					pos.x -= BridgeOpenDistance;
				else
					pos.x += BridgeOpenDistance;
			}
			bridgePiece.transform.position = pos;
		}
		bridgesFlipped = !bridgesFlipped;
	}

	// return true when  the bridge is open, false otherwise.
	public Boolean IsBridgeOpen(Vector3 pos) {
		float relativeX = pos.x % 0.5f;
		float relativeZ = pos.z % 0.5f;
		if (Math.Abs (relativeX - BridgeOpenDistance) < 0.01)
			return true;
		else if (Math.Abs (relativeZ + BridgeOpenDistance) < 0.01)
			return true;
		else if (relativeX < 0.01 && relativeZ < 0.01)
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

	// Static Macros 
	// 

	private static void DisplayTutorialMessage(String[] message) {
		if (!MenuScript.data.playTutorials)
			return;
		
		Vector3 position = Camera.main.transform.position + new Vector3 (-0.2f, -0.7f, 0.5f);
		GameObject iTutorialBox = (GameObject)GameObject.Instantiate (tutorialBox, position, tutorialBox.transform.rotation);
		iTutorialBox.name = "TutorialBox";
		iTutorialBox.GetComponent<TutorialBoxScript>().SetMessages(message);
	}

	private static void DestroyTutorialMessageBox() {
		Destroy (GameObject.Find ("TutorialBox"));
	}

	// Macros 
	// 

	private void DisplayTutorialMessages() {
		if (!tutorialMessages.ContainsKey(chosenLevel))
			return;

		DisplayTutorialMessage(tutorialMessages [chosenLevel]);
	}

	private void ProcessGameOver() {	
		if (chosenLevel == "explorer") {
			gameStarted = false;
			carScript.Reset ();
			goStyle.normal.background = goTexture0;
			chosenBoostStyle = boostStyle1;
			GameObject.Destroy(carScript.gameObject);
			if (MenuScript.data.board != null) {
				Debug.Log ("Setting back to saved position");
				SetCarToCorrectPosition ();
				SetCameraToCorrectPosition ();
			} else {
				Debug.Log ("Setting back to start location");
				SetCarOnStartPiece();
				SetCameraToStartPosition();
			}
			return;
		}

		Instantiate (canvas, new Vector3 (0, 1.3f, -8f), Quaternion.Euler (277, 0, 180));

		Transform iScoreScreen;
		Vector3 position = scoreScreen.transform.position;
		position += (Camera.main.transform.position - cameraCenter);

		if (carScript.ended) {
			UpdateProgress ();

			iScoreScreen = ((GameObject)Instantiate (scoreScreen, position, scoreScreen.transform.rotation)).transform;
		} else {
			iScoreScreen = ((GameObject)Instantiate (lossScreen, position, lossScreen.transform.rotation)).transform;
		}

		iScoreScreen.Find ("levelVariable").GetComponentInChildren<TextMesh> ().text = "LEVEL " + WorldSelectScript.displayNames [chosenLevel];
		iScoreScreen.Find ("movesVariable").GetComponentInChildren<TextMesh> ().text = "" + movesMade;
		iScoreScreen.Find ("parVariable").GetComponentInChildren<TextMesh> ().text = "" + levelConfiguration.par;
		int marblesGained = 0;
		if (MenuScript.data.levelProgress.ContainsKey (chosenLevel))
			marblesGained = MenuScript.data.levelProgress [chosenLevel];
		string marbleText = marblesGained + "/" + 5;
		if (LevelSelectScript.TutorialLevel (chosenLevel))
			marbleText = "0/0";
		iScoreScreen.Find ("marblesVariable").GetComponentInChildren<TextMesh> ().text = marbleText;

	}
	
	private void UpdateProgress() {
		marbles = 0;
		if (MenuScript.data.levelProgress.ContainsKey (chosenLevel)) {
			if (MenuScript.data.levelProgress [chosenLevel] == 2 && movesMade <= levelConfiguration.par) {
				MenuScript.data.levelProgress [chosenLevel] = 5;
				marbles = 3;
			}
		} else {
			if (movesMade <= levelConfiguration.par)
				marbles = 5;
			else
				marbles = 2;

			MenuScript.data.levelProgress.Add (chosenLevel, marbles);
		}
		if (LevelSelectScript.TutorialLevel (chosenLevel))
			marbles = 0;
		MenuScript.data.marbles += marbles;

		if (MenuScript.data.marbles >= UnlockButtonScript.COST && !MenuScript.data.puzzleBoxesUnlocked[3] && !MenuScript.data.animationQueue.Contains (explorerPair)) {
			String[] messages = new String[1];
			messages [0] = "All those shiny marbles you have\ncollected! How about you check back\nat the bookcase, and there might\nbe something waiting for you..."; 
			DisplayTutorialMessage (messages);
			MenuScript.data.animationQueue.Enqueue (explorerPair);
		}
		// Give the Tardis when a space level was completed
		if (Array.IndexOf (WorldSelectScript.levelsWorld3, chosenLevel) >= 0 && !MenuScript.data.carsUnlocked [10] && !MenuScript.data.animationQueue.Contains (tardisPair)) {
			MenuScript.data.animationQueue.Enqueue (tardisPair);
			GameObject obj = (GameObject)GameObject.Instantiate (((GameObject)Resources.Load ("displayCar11")), new Vector3 (-100, -100, -100), new Quaternion ());
			obj.GetComponent<AudioSource> ().Play ();
		}
			
		MenuScript.Save ();
	}

	private void DisplayButtonBar() {
		if (!showingShop) {
			if (GUI.Button (boostRect, "", chosenBoostStyle)) {
				if (carScript.boost == 0f) {
					MenuScript.PlayGearShiftSound ();
					chosenBoostStyle = boostStyle4;
					carScript.boost = 1f;
				} else if (carScript.boost == 1f) {
					MenuScript.PlayGearShiftSound ();
					chosenBoostStyle = boostStyle1;
					carScript.boost = 0f;
				}
			}
		}

		if (chosenLevel == "explorer") {
			DisplayExplorerButtonBar ();
			return;
		}

		if (GUI.Button (quitRect, "", quitStyle)) {
			MenuScript.PlayButtonSound ();
			StopGameMusicAndPlayMenuMusic();
			if (MenuScript.InTutorialPhase ())
				SceneManager.LoadScene ("menu");
			else
				SceneManager.LoadScene ("level_select");
		}

		if (GUI.Button (resetRect, "", resetStyle)) {
			MenuScript.PlayButtonSound ();
			Reset ("Retry");
		}
		
		if (gameStarted && time < levelConfiguration.waitTimeAtStart) {
			double restTime = Math.Ceiling (levelConfiguration.waitTimeAtStart - time);
			if (restTime == 5)
				goStyle.normal.background = goTexture2;
			else if (restTime == 4)
				goStyle.normal.background = goTexture3;
			if (restTime == 3)
				goStyle.normal.background = goTexture4;
			else if (restTime == 2)
				goStyle.normal.background = goTexture5;
			else if (restTime == 1)
				goStyle.normal.background = goTexture6;
			if (GUI.Button (goRect,"", goStyle)) {
				time += (float)restTime;
			}
		} else if (gameStarted) {
			GUI.Label (goRect, "", goStyle);
		} else {
			if (GUI.Button (goRect, "", goStyle)) {
				MenuScript.PlayButtonSound ();
				StartTheGame();
			}
		}

		GUI.Label (displayRect, "" + movesMade + "             " + levelConfiguration.par + "\n             ", displayStyle);
	}

	private void DisplayExplorerButtonBar() {
		if (showingShop)
			return;
		if (carScript.Quittable () && GUI.Button (explorerQuitRect, "", quitStyle)) {
			MenuScript.PlayButtonSound ();
			MenuScript.Save ();
			SceneManager.LoadScene ("world_select");
		}

		GUI.Label (resetRect, "", emptyResetStyle);

		if (GUI.Button (explorerGoRect, "", goStyle)) {
			MenuScript.PlayButtonSound ();
			if (gameStarted && !halted) {
				goStyle.normal.background = goTexture0;
				halted = true;
				carScript.carStarted = false;
			} else if (gameStarted && halted) {
				goStyle.normal.background = goTexture7;
				halted = false;
				showingShop = false;
				carScript.carStarted = true;
				gameStarted = true;
			} else if (halted) {
				gameStarted = true;
				halted = false;
				goStyle.normal.background = goTexture7;
			} else { 
				goStyle.normal.background = goTexture7;
				StartTheGame ();
			}
		}

		GUI.Label (displayRect, "", emptyDisplayStyle);


		if (gameStarted) {
			if (GUI.Button (boostRect, "", chosenBoostStyle)) {
				if (carScript.boost == 0f) {
					MenuScript.PlayGearShiftSound ();
					chosenBoostStyle = boostStyle4;
					carScript.boost = 1f;
				} else if (carScript.boost == 1f) {
					MenuScript.PlayGearShiftSound ();
					chosenBoostStyle = boostStyle1;
					carScript.boost = 0f;
				}
			}
		} else {
			GUI.Label (boostRect, "", chosenBoostStyle);
		}
	}

	// Start the counter!
	private void StartTheGame() {
		time = 0.0f;
		processedGameOver = false;
		carScript.carStarted = false;
		DestroyTutorialMessageBox ();
		gameStarted = true;
	}

	private void SetCarOnStartPiece() {
		GameObject startPiece;
		if (chosenLevel == "explorer")
			startPiece = GameObject.Find (explorerStartPiece);
		else
			startPiece = GameObject.FindGameObjectWithTag ("StartPuzzlePiece");
		Vector3 pos = startPiece.transform.position;
		GameObject instantiatedCar = (GameObject)Instantiate (car, new Vector3(pos.x, 0.105f, pos.z), Quaternion.Euler (0, 90, 0));
		carScript = instantiatedCar.GetComponent<CarScript> ();
		carScript.boost = 0f;
	}

	private void SetCarToCorrectPosition() {
		Vector3 position = new Vector3 (MenuScript.data.carPositionX, MenuScript.data.carPositionY, MenuScript.data.carPositionZ);
		Quaternion rotation = new Quaternion (MenuScript.data.carRotationX, MenuScript.data.carRotationY, MenuScript.data.carRotationZ, MenuScript.data.carRotationW);
		GameObject instantiatedCar = (GameObject)Instantiate (car, position, rotation);
		carScript = instantiatedCar.GetComponent<CarScript> ();

		carScript.currentCoordinate = MenuScript.data.currentCoordinate;
		carScript.currentCoordinateIndex = MenuScript.data.currentCoordinateIndex;		
		carScript.currentDirection = MenuScript.data.currentDirection;
		carScript.currentConnection = MenuScript.data.currentConnection;
		carScript.currentPuzzlePieceConnections = MenuScript.data.currentPuzzlePieceConnections;
		carScript.currentPuzzlePiece = GameObject.Find (MenuScript.data.currentPuzzlePiece);
		if (MenuScript.data.previousPuzzlePiece == null)
			carScript.previousPuzzlePiece = null;
		else
			carScript.previousPuzzlePiece = GameObject.Find (MenuScript.data.previousPuzzlePiece);
		carScript.timeSinceOnLastPuzzlePiece = MenuScript.data.timeSinceOnLastPuzzlePiece;
		carScript.time = MenuScript.data.time;
	}

	private void SetCameraToCorrectPosition() {
		Vector3 position = new Vector3 (MenuScript.data.cameraPositionX, Camera.main.transform.position.y, MenuScript.data.cameraPositionZ);
		Camera.main.transform.position = position;
	}

	private void SetCameraToStartPosition() {
		Vector3 position = new Vector3 (10.5f, 2.7f, -11);
		Camera.main.transform.position = position;
	}
	
	private void MovePiecesToCorrectPosition() {
		float pieceSize = levelConfiguration.PieceSize;
		for (int y = 0; y < board.Length; y++) {
			for (int x = 0; x < board[y].Length; x++) {
				GameObject piece = board [y] [x];
				if (piece != null)
					piece.transform.position = new Vector3 (x * pieceSize, 0f, -(y * pieceSize));
			}
		}
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
			Debug.Log ("handling piece: " + piece.name);
			while (piece.transform.position.x != expected_x) {
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
		for (int i = 0; i < board.Length; i++) {
			if (board [i] == null)
				board [i] = new GameObject[boardWidth];
		}
	}

	private Vector3 GetPuzzlePiecePosition(int x, int y) {
		return new Vector3 (x * levelConfiguration.PieceSize + levelConfiguration.LeftXPosition, 0f, 
		                    -y * levelConfiguration.PieceSize + levelConfiguration.TopZPosition);
	}

	private void StopGameMusicAndPlayMenuMusic() {
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

	// VARIABLE STUFF
	//

	private void LoadFromSave() {
		string[][] savedBoard = MenuScript.data.board;
		board = new GameObject[savedBoard.Length][];
		for (int y = 0; y < savedBoard.Length; y++) {
			board[y] = new GameObject[savedBoard[y].Length];
			for (int x = 0; x < savedBoard[y].Length; x++) {
				GameObject result = null;
				if (savedBoard [y] [x] != null)
					result = GameObject.Find (savedBoard [y] [x]);
				board [y] [x] = result;
			}
		}

		string[] savedPuzzlePieces = MenuScript.data.puzzlePieces;
		puzzlePieces = new GameObject[savedPuzzlePieces.Length];
		for (int i = 0; i < savedPuzzlePieces.Length; i++) {
			puzzlePieces [i] = GameObject.Find (savedPuzzlePieces [i]);
		}
	}

	public void RecordCurrentState() {
		string[][] serializableBoard = new string[board.Length][];
		for (int y = 0; y < board.Length; y++) {
			serializableBoard[y] = new string[board[y].Length];
			for (int x = 0; x < board[y].Length; x++) {
				String result = null;
				if (board [y] [x] != null)
					result = board [y] [x].name;
				serializableBoard [y] [x] = result;
			}
		}
		
		string[] serializablePuzzlePieces = new string[puzzlePieces.Length];
		for (int i = 0; i < puzzlePieces.Length; i++) {
			serializablePuzzlePieces [i] = puzzlePieces [i].name;
		}

		MenuScript.data.board = serializableBoard;
		MenuScript.data.puzzlePieces = serializablePuzzlePieces;
		MenuScript.data.bridgesFlipped = bridgesFlipped;
		
		MenuScript.data.carPositionX = carScript.gameObject.transform.position.x;
		MenuScript.data.carPositionY = carScript.gameObject.transform.position.y;
		MenuScript.data.carPositionZ = carScript.gameObject.transform.position.z;
		
		MenuScript.data.carRotationW = carScript.gameObject.transform.rotation.w;
		MenuScript.data.carRotationX = carScript.gameObject.transform.rotation.x;
		MenuScript.data.carRotationY = carScript.gameObject.transform.rotation.y;
		MenuScript.data.carRotationZ = carScript.gameObject.transform.rotation.z;
		
		MenuScript.data.cameraPositionX = Camera.main.transform.position.x;
		MenuScript.data.cameraPositionZ = Camera.main.transform.position.z;
		
		MenuScript.data.currentCoordinate = carScript.currentCoordinate;
		MenuScript.data.currentCoordinateIndex = carScript.currentCoordinateIndex;
		MenuScript.data.currentDirection = carScript.currentDirection;
		MenuScript.data.currentConnection = carScript.currentConnection;
		MenuScript.data.currentPuzzlePieceConnections = carScript.currentPuzzlePieceConnections;
		MenuScript.data.currentPuzzlePiece = carScript.currentPuzzlePiece.name;
		if (carScript.previousPuzzlePiece == null)
			MenuScript.data.previousPuzzlePiece = null;
		else
			MenuScript.data.previousPuzzlePiece = carScript.previousPuzzlePiece.name;
		MenuScript.data.timeSinceOnLastPuzzlePiece = carScript.timeSinceOnLastPuzzlePiece;
		MenuScript.data.time = carScript.time;
	}
	
	private static void SetVariables() {
		float uiButtonSize = Screen.height / 10;
		quitRect = new Rect (0, uiButtonSize, uiButtonSize * 2, uiButtonSize);
		resetRect = new Rect (0, uiButtonSize * 2, uiButtonSize * 2, 2 * uiButtonSize);
		goRect = new Rect (0, uiButtonSize * 4, uiButtonSize * 2, 2 * uiButtonSize);
		displayRect = new Rect (0, uiButtonSize * 6, uiButtonSize * 2, uiButtonSize);
		boostRect = new Rect (0, uiButtonSize * 7, uiButtonSize * 2, 2 * uiButtonSize);

		explorerQuitRect = quitRect;
		explorerGoRect = goRect;

		quitTexture = (Texture2D)Resources.Load ("quit");
		resetTexture = (Texture2D)Resources.Load ("reset");
		emptyResetTexture = (Texture2D)Resources.Load ("resetempty");
		goTexture0 = (Texture2D)Resources.Load ("go0");
		goTexture1 = (Texture2D)Resources.Load ("go1");
		goTexture2 = (Texture2D)Resources.Load ("go2");
		goTexture3 = (Texture2D)Resources.Load ("go3");
		goTexture4 = (Texture2D)Resources.Load ("go4");
		goTexture5 = (Texture2D)Resources.Load ("go5");
		goTexture6 = (Texture2D)Resources.Load ("go6");
		goTexture7 = (Texture2D)Resources.Load ("go7");
		displayTexture = (Texture2D)Resources.Load ("counter");
		emptyDisplayTexture = (Texture2D)Resources.Load ("counterempty");
		boostTexture1 = (Texture2D)Resources.Load ("speed1");
		boostTexture4 = (Texture2D)Resources.Load ("speed2");

		goStyle = new GUIStyle ();

		statStyle = new GUIStyle ();
		statStyle.fontSize = 32;
		statStyle.normal.textColor = Color.white;
		statStyle.alignment = TextAnchor.MiddleLeft;

		timerStyle = new GUIStyle ();
		timerStyle.fontSize = 32;
		timerStyle.normal.textColor = Color.white;
		Texture2D texture = new Texture2D (1, 1, TextureFormat.RGBA32, false);
		texture.SetPixel (0, 0, new Color (0, 0, 0, 0.75f));
		texture.Apply ();
		texture = new Texture2D (1, 1, TextureFormat.RGBA32, false);
		texture.SetPixel (0, 0, new Color (0, 0, 0, 0.75f));
		texture.Apply ();
		timerStyle.normal.background = texture;

		quitStyle = new GUIStyle ();
		quitStyle.normal.background = quitTexture;

		resetStyle = new GUIStyle ();
		resetStyle.normal.background = resetTexture;

		emptyResetStyle = new GUIStyle ();
		emptyResetStyle.normal.background = emptyResetTexture;

		displayStyle = new GUIStyle ();
		displayStyle.normal.background = displayTexture;
		displayStyle.fontSize = 24;
		displayStyle.normal.textColor = Color.green;
		displayStyle.alignment = TextAnchor.LowerCenter;

		emptyDisplayStyle = new GUIStyle ();
		emptyDisplayStyle.normal.background = emptyDisplayTexture;

		boostStyle1 = new GUIStyle ();
		boostStyle1.normal.background = boostTexture1;
		boostStyle4 = new GUIStyle ();
		boostStyle4.normal.background = boostTexture4;

		shopTriggerPieces = new Dictionary<String, Pair<String, int>[]> ();
		Pair<String, int>[] carsShop1 = {new Pair<String, int>("displayCar2", 1), new Pair<String, int>("displayCar3", 2)};
		Pair<String, int>[] carsShop2 = {new Pair<String, int>("displayCar4", 3), new Pair<String, int>("displayCar5", 4)};
		Pair<String, int>[] carsShop3 = {new Pair<String, int>("displayCar6", 5), new Pair<String, int>("displayCar7", 6)};
		Pair<String, int>[] carsShop4 = {new Pair<String, int>("displayCar8", 7), new Pair<String, int>("displayCar9", 8)};
		Pair<String, int>[] carsShop5 = {new Pair<String, int>("displayCar10", 9), new Pair<String, int>("displayCar12", 10)};
		Pair<String, int>[] carsShop6 = {new Pair<String, int>("displayCar13", 11), new Pair<String, int>("displayCar14", 12)};
		Pair<String, int>[] carsShop7 = {new Pair<String, int>("displayCar15", 13), new Pair<String, int>("displayCar16", 14)};
		// Center (start)
		shopTriggerPieces.Add ("puzzlePiece_turnabout_W", carsShop1);
		// Center
		shopTriggerPieces.Add ("puzzlePiece_turnabout_E (4)", carsShop2);
		// Top left (lava)
		shopTriggerPieces.Add ("puzzlePiece_turnabout_S (1)", carsShop3);
		// Bottom left
		shopTriggerPieces.Add ("puzzlePiece_turnabout_W (2)", carsShop4);
		// Right
		shopTriggerPieces.Add ("puzzlePiece_turnabout_W (1)", carsShop5);
		// Bottom right
		shopTriggerPieces.Add ("puzzlePiece_turnabout_W (10)", carsShop6);
		// Top right
		shopTriggerPieces.Add ("puzzlePiece_turnabout_W (5)", carsShop7);
	}

	private static void AddTutorialMessages() {
		tutorialMessages = new Dictionary<string, string[]> ();

		// tutorial level 1
		String[] messages1 = new String[3];
		messages1 [0] = "Tap highlighted puzzlepieces\nto shuffle them around.\nCreate a road for your car to ride on,\ntowards the Portal. [1/3]";
		messages1 [1] = "When you touch the puzzle the car\nwill start its engine and after a few \nseconds it will start moving. [2/3]";
		messages1 [2] = "You can only move pieces next\nto the empty spot. You cannot move\npieces the car is currently on. [3/3]";
		tutorialMessages.Add ("tutorial_1", messages1);

		// tutorial level 2
		String[] messages2 = new String[2];
		messages2 [0] = "The piece with the tower on it is the\nBeacon piece. This piece will\nalways move to the empty spot,\nregardless of its current position. [1/2]";
		messages2 [1] = "Tired of waiting for your\ncar to start driving? Tap the\narea of the steering wheel to\nskip the startup time. [2/2]";
		tutorialMessages.Add ("tutorial_2", messages2);

		// tutorial level 3
		String[] messages3 = new String[1];
		messages3 [0] = "Sometimes you will lack pieces\nto complete your road. It all comes\ndown to timing and quick shuffling.";
		tutorialMessages.Add ("tutorial_3", messages3);

		// level 1-1
		String[] messages4 = new String[2];
		messages4 [0] = "Complete levels to gain Marbles.\nYou can obtain a collection\nof cars with them! [1/2]";
		messages4 [1] = "Beating the PAR awards you\nwith even more Marbles! [2/2]";
		tutorialMessages.Add ("level_01", messages4);

		// level 2-1
		String[] messages5 = new String[1];
		messages5 [0] = "Drive over buttons to open\nand close bridges.";
		tutorialMessages.Add ("level_l01", messages5);

		// level 3-1
		String[] messages6 = new String[2];
		messages6 [0] = "Driving through portals from the\ncolored side in will move you\nto the other portal with the same color. [1/2]";
		messages6 [1] = "Driving through the grey circle first\nmakes the portal ignore you. [2/2]";
		tutorialMessages.Add ("level_s01", messages6);

		// Explorer Mode - first entry
		String[] messages7 = new String[3];
		messages7 [0] = "Welcome to Streettopia! This world\ncomes with its own rules.\nYou can shuffle puzzlepieces\nwhile standing still! [1/3]";
		messages7 [1] = "You can also stop and start your car\nwhenever it pleases you! Tap the\nsteering wheel in your interface for that.\n[2/3]";
		messages7 [2] = "How about you try and reach\nthat shop northeast of you?\nSpend your acquired marbles\non cool new cars! [3/3]";
		tutorialMessages.Add ("explorer", messages7);
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
