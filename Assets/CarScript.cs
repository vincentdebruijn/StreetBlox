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

	private Vector3 distanceMovedSinceStartPuzzlePiece;

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
		currentPuzzlePieceConnections = PuzzlePieceScript.PuzzlePieceConnections.GetPuzzlePieceConnections (currentPuzzlePiece);
		currentConnection = currentPuzzlePieceConnections.getConnectionForSide (PuzzlePieceScript.Coordinate.WEST);
		currentDirection = currentConnection.OtherSide (PuzzlePieceScript.Coordinate.WEST);
		currentCoordinateIndex = currentConnection.getFirstCoordinateIndexFor (currentDirection);
		currentCoordinate = currentConnection.coordinates [currentCoordinateIndex];
		buttons = GameObject.FindGameObjectsWithTag ("Button");
		carStarted = true;
		PlayEngineSound ();
	}

	public void Resume(LevelConfiguration levelConfiguration) {
		this.levelConfiguration = levelConfiguration;
		buttons = GameObject.FindGameObjectsWithTag ("Button");
		enteredPortalPiece = currentPuzzlePiece.name.Contains ("portal") && PuzzlePieceScript.GetSideOfPortal(currentPuzzlePiece) == currentDirection;
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
		cameraShakeDone = false;
		distanceMovedSinceStartPuzzlePiece = new Vector3 ();
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

		Boolean enteredShopPiece = false;
		Boolean enteredSavePiece = false;
		int explorerModeTriggerPiece = -1;
		if (Math.Abs (currentX - x) < 0.01f && Math.Abs (currentZ - z) < 0.01f) {
			// Get the next coordinate
			currentCoordinateIndex = currentConnection.getNextCoordinateIndex (currentCoordinateIndex, currentDirection);
			GameObject puzzleBoxObtained = null;
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
				distanceMovedSinceStartPuzzlePiece = new Vector3();
				if (ExplorerLevel()) {
					if (currentPuzzlePiece.tag == "UnmovablePuzzlePiece")
						enteredSavePiece = true;
					if (gameScript.ShopTriggerPiece(currentPuzzlePiece.name))
				    	enteredShopPiece = true;
					if (currentPuzzlePiece.name == "puzzlePiece_straight_WE (7)")
						explorerModeTriggerPiece = 2;
					if (currentPuzzlePiece.name == "puzzlePiece_straight_WE (20)")
						explorerModeTriggerPiece = 3;
					
					puzzleBoxObtained = gameScript.GetPuzzleBoxForPuzzlePiece(currentPuzzlePiece.name);
				}
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
				currentConnection = currentPuzzlePieceConnections.getConnectionForSide (startSide);
				if (currentConnection == null) {
					crashing = true;
					StopEngineSound();
					PlayCrashSound ();
					return;
				}
				currentDirection = currentConnection.OtherSide (startSide);
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
			}
			if (puzzleBoxObtained != null) {
				int index = 1;
				if (puzzleBoxObtained.name == "puzzleBoxWorld3")
					index = 2;
				gameScript.ShowFirstItemObtainedMessageIfIsFirstItem ();
				MenuScript.data.animationQueue.Enqueue(new Pair(puzzleBoxObtained.name, index));
				GameObject.Destroy (puzzleBoxObtained);
				MenuScript.Save ();
			}
			if (enteredSavePiece)
				gameScript.RecordCurrentState();
			if (explorerModeTriggerPiece != -1)
				gameScript.ShowExplorerModeMessage (explorerModeTriggerPiece);
		}
		CheckForNearbyButton();
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
		float beforeX = Mathf.Abs (distanceMovedSinceStartPuzzlePiece.x);
		float beforeZ = Mathf.Abs (distanceMovedSinceStartPuzzlePiece.z);
		distanceMovedSinceStartPuzzlePiece += (newPosition - position);
		float afterX = Mathf.Abs (distanceMovedSinceStartPuzzlePiece.x);
		float afterZ = Mathf.Abs (distanceMovedSinceStartPuzzlePiece.z);
		if (enteredPortalPiece && ((beforeX < 0.25f && afterX >= 0.25f) || (beforeZ < 0.25f && afterZ > 0.25f)))
			MenuScript.PlayPortalInSound ();
	}

	void RotateTowardsTarget() {
		PuzzlePieceScript.Coordinate targetCoord = 
			PuzzlePieceScript.Coordinate.GetCoordinateFor (currentCoordinate.x, currentCoordinate.z, currentPuzzlePiece, levelConfiguration.PieceSize);
		Vector3 point = PointOfCar ();
		float deltaX = targetCoord.x - point.x;
		float deltaZ = targetCoord.z - point.z;
		float angleInDegrees = (float)(Math.Atan2 (-deltaZ, deltaX) * 180.0f / Math.PI);
		Vector3 eulerAngles = transform.eulerAngles;
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
		foreach (GameObject piece in pieces) {
			if (piece == exclude) continue;
			Vector3 diff = (piece.transform.position + center - carPoint);
			float curDistance = diff.sqrMagnitude;
			if (curDistance < distance && curDistance < 0.5f * levelConfiguration.PieceSize) { 
				closest = piece; 
				distance = curDistance; 
			} 
		}
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
		atEnd = (GameObject.FindGameObjectWithTag ("EndPuzzlePiece").transform.position - transform.position).sqrMagnitude < (levelConfiguration.PieceSize / 2);
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
		MenuScript.PlayPortalOutSound ();
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
