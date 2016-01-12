using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CarScript : MonoBehaviour {

	private LevelConfiguration levelConfiguration;
	private GameScript gameScript;

	// Car movement stuff
	public PuzzlePieceScript.Coordinate currentCoordinate;
	public int currentCoordinateIndex;
	public int currentDirection;
	public PuzzlePieceScript.Connection currentConnection;
	public PuzzlePieceScript.PuzzlePieceConnections currentPuzzlePieceConnections;
	public GameObject currentPuzzlePiece;
	public GameObject previousPuzzlePiece;
	public float timeSinceOnLastPuzzlePiece = 0f;
	public float time;

	// true if we are going to the end piece
	private Boolean atEnd = false;
	private float startEnd = 0f;
	public Boolean ended = false;
	// true if we are falling
	public Boolean falling = false;
	public Boolean fell = false;
	public float startFall = 0f;
	// true if we are crashing
	public Boolean crashing = false;
	private float startCrash = 0f;
	public Boolean crashed = false;

	// When set to true, let's the car move.
	public Boolean carStarted;
	// Can be set to make the car move faster
	public float boost;

	private Boolean cameraShakeDone;

	private GameObject[] buttons;
	
	private Boolean clickedButtonWhileOnCurrentPuzzlePiece;

	private Boolean enteredPortalPiece;
	private Boolean portalEntryAnimationDone;
	private Boolean portalExitAnimationDone;

	private Boolean enteredShopPiece;
	private Boolean enteredSavePiece;

	private Transform mainCamera;

	void Awake() {
		Reset ();
		PlayCarHorn ();
		mainCamera = Camera.main.transform;
	}

	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;
		//if (time < 1.5)
		//	DecreaseTransparancy ();
		if ((!carStarted) || GameOver () || GameEnding ()) {
			return;
		// Very stupid, but needed because GameEnding could have made GameOver true now
		} else if ((!carStarted) || GameOver ()) {
			return;
		}
		// For world 3 portals
		if (!portalEntryAnimationDone) {
			ShowPortalEntryAnimation ();
			return;
		} else if (!portalExitAnimationDone) {
			ShowPortalExitAnimation ();
			return;
		}
		timeSinceOnLastPuzzlePiece += Time.deltaTime;
		UpdateCarPosition ();
	}

	private GameScript GameScript() {
		if (gameScript == null) {
			gameScript = mainCamera.GetComponent<GameScript> ();
		}
		return gameScript;
	}

	public void StartTheGame(LevelConfiguration levelConfiguration) {
		this.levelConfiguration = levelConfiguration;

		currentPuzzlePiece = ClosestPuzzlePiece (null);
		if (ExplorerLevel ())
			currentPuzzlePiece = ClosestPuzzlePiece (currentPuzzlePiece);
		Debug.Log (currentPuzzlePiece);
		currentPuzzlePieceConnections = PuzzlePieceScript.PuzzlePieceConnections.GetPuzzlePieceConnections (currentPuzzlePiece);
		Debug.Log (currentPuzzlePieceConnections);
		currentConnection = currentPuzzlePieceConnections.getConnectionForSide (PuzzlePieceScript.Coordinate.WEST);
		currentDirection = currentConnection.OtherSide (PuzzlePieceScript.Coordinate.WEST);
		currentCoordinateIndex = currentConnection.getFirstCoordinateIndexFor (currentDirection);
		Debug.Log ("First coordinate index: " + currentCoordinateIndex);
		currentCoordinate = currentConnection.coordinates [currentCoordinateIndex];
		Debug.Log ("Going to " + currentPuzzlePiece.name + ", first waypoint x: " + currentCoordinate.x + " z: " + currentCoordinate.z);
		PuzzlePieceScript.Coordinate startDestination = 
			PuzzlePieceScript.Coordinate.GetCoordinateFor (currentCoordinate.x, currentCoordinate.z, currentPuzzlePiece, levelConfiguration.PieceSize);
		Debug.Log ("Which is at: x: " + startDestination.x + " z: " + startDestination.z);
		Debug.Log ("And point of car is at: x: " + PointOfCar ().x + " z: " + PointOfCar ().z);
		buttons = GameObject.FindGameObjectsWithTag ("Button");
		carStarted = true;
		PlayEngineSound ();
	}

	public void Resume(LevelConfiguration levelConfiguration) {
		this.levelConfiguration = levelConfiguration;
		buttons = GameObject.FindGameObjectsWithTag ("Button");
		enteredPortalPiece = currentPuzzlePiece.name.Contains ("portal") && PuzzlePieceScript.GetSideOfPortal(currentPuzzlePiece) == currentDirection;
		enteredShopPiece = false;
		enteredSavePiece = false;
		clickedButtonWhileOnCurrentPuzzlePiece = ButtonNearby ();
		carStarted = true;
		PlayEngineSound ();
	}

	public Boolean GameOver() {
		return ended || crashed || fell;
	}

	public Boolean Quittable() {
		return !GameOver () && !falling && !crashing && portalEntryAnimationDone && portalExitAnimationDone;
	}

	public void Reset() {
		StopEngineSound ();
		atEnd = false;
		startEnd = 0f;
		ended = false;
		falling = false;
		fell = false;
		startFall = 0f;
		crashing = false;
		startCrash = 0f;
		crashed = false;
		carStarted = false;
		time = 0f;
		clickedButtonWhileOnCurrentPuzzlePiece = false;
		portalEntryAnimationDone = true;
		portalExitAnimationDone = true;
		enteredPortalPiece = false;
		enteredShopPiece = false;
		enteredSavePiece = false;
		cameraShakeDone = false;
	}

	Boolean GameEnding() {
		if (atEnd) {
			if (ended)
				return false;
			if (startEnd >= 3.0) {
				ended = true;
				StopEngineSound();
				return false;
			}
			if (startEnd >= 2.5) {
				foreach (Transform child in gameObject.transform) {
					child.GetComponent<Renderer> ().enabled = false;
				}
			}
			//if (startEnd >= 1.5) {
			//	IncreaseTransparancy();
			//}
			startEnd += Time.deltaTime;
			transform.Translate (Vector3.forward * levelConfiguration.movement * Time.deltaTime);
			return true;
		}
		if (falling) {
			if (fell)
				return false;
			if (startFall > 1.0) {
				StopEngineSound();
				fell = true;
				return false;
			}
			startFall += Time.deltaTime;
			transform.Translate (Vector3.forward * levelConfiguration.movement * Time.deltaTime);
			return true;
		}
		if (crashing) {
			if (crashed)
				return false;
			if (!cameraShakeDone && startCrash >= 1.2 && !ExplorerLevel()) {
				StartCoroutine(MenuScript.CameraShake(0.3f, 0.05f));
				cameraShakeDone = true;
			}
			if (startCrash > 1.5) {
				crashed = true;
				return false;
			}
			startCrash += Time.deltaTime;
			transform.Translate (Vector3.forward * levelConfiguration.movement * Time.deltaTime);
			return true;
		}
		return false;
	}

	void UpdateCarPosition() {
		float r_x = currentCoordinate.x;
		float r_z = currentCoordinate.z;
		PuzzlePieceScript.Coordinate targetCoord = PuzzlePieceScript.Coordinate.GetCoordinateFor (r_x, r_z, currentPuzzlePiece, levelConfiguration.PieceSize);
		float x = targetCoord.x;
		float z = targetCoord.z;
		MoveTowardsTarget (x, z);
		float currentX = PointOfCar ().x;
		float currentZ = PointOfCar ().z;
		if (Math.Abs (currentX - x) < 0.01f && Math.Abs (currentZ - z) < 0.01f) {
			// Get the next coordinate
			currentCoordinateIndex = currentConnection.getNextCoordinateIndex (currentCoordinateIndex, currentDirection);
			Debug.Log ("New coordinate index: " + currentCoordinateIndex);
			if (currentCoordinateIndex == currentConnection.coordinates.Length || currentCoordinateIndex == -1) {
				timeSinceOnLastPuzzlePiece = 0f;
				clickedButtonWhileOnCurrentPuzzlePiece = false;
				// Check if the end piece is nearby
				if (AtEndPiece ())
					return;
				if (enteredPortalPiece) {
					MoveCarToOtherPortal();
					return;
				}
				// Get the next puzzle piece
				previousPuzzlePiece = currentPuzzlePiece;
				Debug.Log (previousPuzzlePiece);
				currentPuzzlePiece = ClosestPuzzlePiece (previousPuzzlePiece);
				Debug.Log (currentPuzzlePiece);
				if (currentPuzzlePiece == null) {
					falling = true;
					return;
				}
				Debug.Log ("New destination: " + currentPuzzlePiece.name);
				Debug.Log ("Entering from: " + currentDirection);
				if (ExplorerLevel() && currentPuzzlePiece.tag == "UnmovablePuzzlePiece")
					enteredSavePiece = true;
				if (ExplorerLevel() && gameScript.ShopTriggerPiece(currentPuzzlePiece.name))
				    enteredShopPiece = true;
				if (!PuzzlePieceScript.PuzzlePieceConnections.HasPuzzlePieceConnections (currentPuzzlePiece) || 
				    	OnOpenBridgePiece(currentPuzzlePiece)) {
					crashing = true;
					StopEngineSound();
					PlayCrashSound();
					return;
				}
				enteredPortalPiece = currentPuzzlePiece.name.Contains("portal") && !EnteredPortalPieceFromWrongSide();

				currentPuzzlePieceConnections = PuzzlePieceScript.PuzzlePieceConnections.GetPuzzlePieceConnections (currentPuzzlePiece);
				int startSide = currentCoordinate.InverseSide (currentDirection);
				Debug.Log ("Entering at: " + startSide);
				currentConnection = currentPuzzlePieceConnections.getConnectionForSide (startSide);
				if (currentConnection == null) {
					crashing = true;
					StopEngineSound();
					PlayCrashSound ();
					return;
				}
				currentDirection = currentConnection.OtherSide (startSide);
				Debug.Log ("new bearing: " + currentDirection);
				currentCoordinateIndex = currentConnection.getFirstCoordinateIndexFor (currentDirection);
				// Get the next one, because the first one of the new piece is the same as the last of the previous piece.
				currentCoordinateIndex = currentConnection.getNextCoordinateIndex (currentCoordinateIndex, currentDirection);
			}
			currentCoordinate = currentConnection.coordinates [currentCoordinateIndex];
			RotateTowardsTarget ();
			if (enteredShopPiece) {
				gameScript.Halt();
				carStarted = false;
				gameScript.ShowShop();
				enteredShopPiece = false;
				return;
			}
			if (enteredSavePiece) {
				gameScript.RecordCurrentState();
				enteredSavePiece = false;
			}
		}
		CheckForNearbyButton();
		if (ExplorerLevel ())
			CheckForNearbyPuzzleBox ();
	}

	Boolean EnteredPortalPieceFromWrongSide() {
		return currentPuzzlePiece.name.Contains ("portal") && PuzzlePieceScript.GetSideOfPortal (currentPuzzlePiece) != currentDirection;
	}

	void MoveTowardsTarget(float x, float z) {
		Vector3 point = PointOfCar ();
		Vector3 position = transform.position;
		float movement = (levelConfiguration.movement + boost) * Time.deltaTime;
		Vector3 newPosition = Vector3.MoveTowards (point, new Vector3 (x, position.y, z), movement) + (position - point);
		transform.position = newPosition;
		if (ExplorerLevel ()) {
			Vector3 cameraPosition = mainCamera.position;
			mainCamera.position = cameraPosition + (newPosition - position);
		}
	}

	void RotateTowardsTarget() {
		PuzzlePieceScript.Coordinate targetCoord = 
			PuzzlePieceScript.Coordinate.GetCoordinateFor (currentCoordinate.x, currentCoordinate.z, currentPuzzlePiece, levelConfiguration.PieceSize);
		Vector3 point = PointOfCar ();
		float deltaX = targetCoord.x - point.x;
		float deltaZ = targetCoord.z - point.z;
		Debug.Log (deltaX + " " + deltaZ);
		float angleInDegrees = (float)(Math.Atan2 (-deltaZ, deltaX) * 180.0f / Math.PI);
		Debug.Log ("New Angle: " + (angleInDegrees + 90.0f));
		Vector3 eulerAngles = transform.eulerAngles;
		Debug.Log ("current Angle: " + eulerAngles.y);
		eulerAngles.y = angleInDegrees + 90.0f;
		transform.eulerAngles = eulerAngles;
	}

	GameObject ClosestPuzzlePiece(GameObject exclude) {
		GameObject[] pieces = GameScript ().GetPuzzlePieces();
		GameObject closest = null;
		float distance = Mathf.Infinity; 
		Vector3 carPoint = PointOfCar ();
		Vector3 center = new Vector3 (0.5f * levelConfiguration.PieceSize, 0.0f, -0.5f * levelConfiguration.PieceSize);
		// Iterate through them and find the closest one
		Debug.Log ("Closest");
		Debug.Log (pieces.Length);
		foreach (GameObject piece in pieces) {
			if (piece == exclude) continue;
			Vector3 diff = (piece.transform.position + center - carPoint);
			float curDistance = diff.sqrMagnitude;
			if (curDistance < distance && curDistance < 0.5f * levelConfiguration.PieceSize) { 
				closest = piece; 
				distance = curDistance; 
			} 
		}
		Debug.Log (closest);
		return closest;
	}

	void CheckForNearbyButton() {
		if (clickedButtonWhileOnCurrentPuzzlePiece)
			return;
		if (ButtonNearby()) {
			clickedButtonWhileOnCurrentPuzzlePiece = true;
			GameScript().FlipBridgePositions ();
		}
	}

	Boolean ButtonNearby() {
		Vector3 pos = PointOfCar ();
		foreach (GameObject button in buttons) {
			Vector3 buttonPos = button.transform.position;
			if (Math.Abs (pos.x - (buttonPos.x + 0.25f)) < 0.03f && Math.Abs (pos.z - (buttonPos.z - 0.25f)) < 0.03f)
				return true;
		}
		return false;
	}

	Boolean OnOpenBridgePiece(GameObject currentPuzzlePiece) {
		if (!currentPuzzlePiece.name.Contains ("bridge"))
			return false;
		Vector3 pos = currentPuzzlePiece.transform.Find ("bridgePiece").position;
		return GameScript().IsBridgeOpen (pos);
	}

	void CheckForNearbyPuzzleBox() {
		Vector3 carPos = transform.position;

		if (gameScript.puzzleBoxWorld2 != null) {
			Vector3 pos1 = gameScript.puzzleBoxWorld2.transform.position;
			if (Math.Abs (carPos.x - pos1.x - 0.053f) < 0.1f && Math.Abs (carPos.z - pos1.z + 0.13f) < 0.1f) {
				MenuScript.data.animationQueue.Enqueue(new Pair<String, int>("puzzleBoxWorld2", 1));
				GameObject.Destroy (gameScript.puzzleBoxWorld2);
				MenuScript.Save ();
			}
		}
		if (gameScript.puzzleBoxWorld3 != null) {
			Vector3 pos2 = gameScript.puzzleBoxWorld3.transform.position;
			if (Math.Abs ((carPos.x - pos2.x) + 0.08f) < 0.1f && Math.Abs (carPos.z - pos2.z + 0.1f) < 0.1f) {
				MenuScript.data.animationQueue.Enqueue(new Pair<String, int>("puzzleBoxWorld3", 2));
				GameObject.Destroy (gameScript.puzzleBoxWorld3);
				MenuScript.Save ();
			}
		}
	}

	Vector3 PointOfCar() {
		Vector3 center = transform.position - (new Vector3 (levelConfiguration.CarXOffset, 0.0f, - levelConfiguration.CarZOffset));
		float angle = (float) (Math.Round (transform.eulerAngles.y, 3) / 180 * Math.PI);
		float radius = 0.5f * levelConfiguration.CarLength;
		float x = center.x + (radius * (float) Math.Sin (angle));
		float z = center.z + (radius * (float) Math.Cos(angle));
		return new Vector3(x, center.y, z);
		//return center;
	}

	Boolean AtEndPiece() {
		if (ExplorerLevel())
			return false;
		atEnd = (GameObject.FindGameObjectWithTag ("EndPuzzlePiece").transform.position - transform.position).sqrMagnitude < levelConfiguration.PieceSize;
		return atEnd;
	}


	// Explorer stuff
	//

	Boolean ExplorerLevel() {
		return GameScript().chosenLevel == "explorer";
	}

	
	// Animation stuff
	//

	// TODO: actually show an animation
	void ShowPortalEntryAnimation() {
		portalEntryAnimationDone = true;
	}

	void ShowPortalExitAnimation() {
		portalExitAnimationDone = true;
	}

	void MoveCarToOtherPortal() {
		previousPuzzlePiece = currentPuzzlePiece;
		currentPuzzlePiece = GameScript().GetOtherPortalPiece (currentPuzzlePiece);

		currentPuzzlePieceConnections = PuzzlePieceScript.PuzzlePieceConnections.GetPuzzlePieceConnections (currentPuzzlePiece);
		int startSide = PuzzlePieceScript.GetSideOfPortal (currentPuzzlePiece);

		currentConnection = currentPuzzlePieceConnections.getConnectionForSide (startSide);
		currentDirection = currentConnection.OtherSide (startSide);
		currentCoordinateIndex = currentConnection.getFirstCoordinateIndexFor (currentDirection);
		currentCoordinateIndex = currentConnection.getNextCoordinateIndex (currentCoordinateIndex, currentDirection);
		currentCoordinate = currentConnection.coordinates [currentCoordinateIndex];
	
		Vector3 newPosition = new Vector3(currentPuzzlePiece.transform.position.x, transform.position.y, currentPuzzlePiece.transform.position.z);
		Vector3 newEulerAngles = transform.eulerAngles;
		if (currentDirection == PuzzlePieceScript.Coordinate.NORTH)
			newEulerAngles.y = 0f;
		else if (currentDirection == PuzzlePieceScript.Coordinate.EAST)
			newEulerAngles.y = 90f;
		else if (currentDirection == PuzzlePieceScript.Coordinate.SOUTH)
			newEulerAngles.y = 180f;
		else if (currentDirection == PuzzlePieceScript.Coordinate.WEST)
			newEulerAngles.y = -90f;
		if (ExplorerLevel ())
			mainCamera.position = new Vector3 (mainCamera.position.x + (newPosition.x - transform.position.x), mainCamera.position.y, 
			                                   mainCamera.position.z + (newPosition.z - transform.position.z));
		transform.position = newPosition;
		transform.eulerAngles = newEulerAngles;
		enteredPortalPiece = false;
	}

	// Sound stuff
	//

	public void PlayCarHorn() {
		if (MenuScript.data.playSoundEffects)
			GetComponents<AudioSource> () [0].Play ();
	}

	public void PlayCrashSound() {
		if (MenuScript.data.playSoundEffects)
			GetComponents<AudioSource> () [1].Play ();
	}

	public void PlayEngineSound() {
		if (MenuScript.data.playSoundEffects)
			GetComponents<AudioSource> () [2].Play ();
	}

	public void StopEngineSound() {
		GetComponents<AudioSource> () [2].Stop ();
	}
}
