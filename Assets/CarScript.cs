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

	// Keeps track of which pieces the car touched and how often.
	public Dictionary<string, int> piecesTouched;
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

	// True if the Car is in range of a button
	private Boolean inRangeOfButton;

	private GameObject[] buttons;

	private float time;

	void Awake() {
		Reset ();
		MakeCarInivisible ();
	}

	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;
		if (time < 1.5)
			DecreaseTransparancy ();
		if ((!carStarted) || GameOver () || GameEnding ()) {
			return;
		// Very stupid, but needed because GameEnding could have made GameOver true now
		} else if ((!carStarted) || GameOver ()) {
			return;
		}
		timeSinceOnLastPuzzlePiece += Time.deltaTime;
		UpdateCarPosition ();
	}

	private GameScript GameScript() {
		if (gameScript == null) {
			gameScript = Camera.main.GetComponent<GameScript> ();
		}
		return gameScript;
	}

	public void StartTheGame(LevelConfiguration levelConfiguration) {
		this.levelConfiguration = levelConfiguration;
		piecesTouched = new Dictionary<string, int>();
		foreach (GameObject piece in GameScript ().GetPuzzlePieces()) {
			piecesTouched.Add (piece.name, 0);
		}

		currentPuzzlePiece = ClosestPuzzlePiece (null);
		currentPuzzlePieceConnections = PuzzlePieceScript.PuzzlePieceConnections.GetPuzzlePieceConnections (currentPuzzlePiece);
		currentConnection = currentPuzzlePieceConnections.getConnectionForSide (PuzzlePieceScript.Coordinate.WEST);
		currentDirection = currentConnection.OtherSide (PuzzlePieceScript.Coordinate.WEST);
		currentCoordinateIndex = currentConnection.getFirstCoordinateIndexFor (currentDirection);
		Debug.Log ("First coordinate index: " + currentCoordinateIndex);
		currentCoordinate = currentConnection.coordinates [currentCoordinateIndex];
		Debug.Log ("Going to " + currentPuzzlePiece.name + ", first waypoint x: " + currentCoordinate.x + " z: " + currentCoordinate.z);
		piecesTouched [currentPuzzlePiece.name] = 1;
		PuzzlePieceScript.Coordinate startDestination = 
			PuzzlePieceScript.Coordinate.GetCoordinateFor (currentCoordinate.x, currentCoordinate.z, currentPuzzlePiece, levelConfiguration.PieceSize);
		Debug.Log ("Which is at: x: " + startDestination.x + " z: " + startDestination.z);
		Debug.Log ("And point of car is at: x: " + PointOfCar ().x + " z: " + PointOfCar ().z);
		inRangeOfButton = false;
		buttons = GameObject.FindGameObjectsWithTag ("Button");
		carStarted = true;
	}

	public Boolean GameOver() {
		return ended || crashed || fell;
	}

	public void Reset() {
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
	}

	Boolean GameEnding() {
		if (atEnd) {
			if (ended)
				return false;
			if (startEnd >= 3.0) {
				ended = true;
				return false;
			}
			if (startEnd >= 2.5) {
				foreach (Transform child in gameObject.transform) {
					child.GetComponent<Renderer> ().enabled = false;
				}
			}
			if (startEnd >= 1.5) {
				IncreaseTransparancy();
			}
			startEnd += Time.deltaTime;
			transform.Translate (Vector3.forward * levelConfiguration.movement * Time.deltaTime);
			return true;
		}
		if (falling) {
			if (fell)
				return false;
			if (startFall > 1.0) {
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

	void IncreaseTransparancy() {
		foreach (Transform child in gameObject.transform) {
			Color tempcolor = child.GetComponent<Renderer> ().material.color;
			tempcolor.a = Mathf.MoveTowards (tempcolor.a, 0, Time.deltaTime * 4 / 5);
			child.GetComponent<Renderer> ().material.color = tempcolor;
		}
	}

	void DecreaseTransparancy() {
		foreach (Transform child in gameObject.transform) {
			Color tempcolor = child.GetComponent<Renderer> ().material.color;
			tempcolor.a = Mathf.MoveTowards (tempcolor.a, 1, Time.deltaTime);
			child.GetComponent<Renderer> ().material.color = tempcolor;
		}
	}

	void MakeCarInivisible() {
		foreach (Transform child in gameObject.transform) {
			Color tempcolor = child.GetComponent<Renderer> ().material.color;
			tempcolor.a = 0f;
			child.GetComponent<Renderer> ().material.color = tempcolor;
		}
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
				// Check if the end piece is nearby
				if (AtEndPiece ())
					return;
				// Get the next puzzle piece
				previousPuzzlePiece = currentPuzzlePiece;
				currentPuzzlePiece = ClosestPuzzlePiece (previousPuzzlePiece);
				if (currentPuzzlePiece == null) {
					falling = true;
					return;
				}
				Debug.Log ("New destination: " + currentPuzzlePiece.name);
				Debug.Log ("Entering from: " + currentDirection);
				if (!PuzzlePieceScript.PuzzlePieceConnections.HasPuzzlePieceConnections (currentPuzzlePiece) || OnOpenBridgePiece(currentPuzzlePiece)) {
					crashing = true;
					return;
				}
				currentPuzzlePieceConnections = PuzzlePieceScript.PuzzlePieceConnections.GetPuzzlePieceConnections (currentPuzzlePiece);
				int startSide = currentCoordinate.InverseSide (currentDirection);
				Debug.Log ("Entering at: " + startSide);
				currentConnection = currentPuzzlePieceConnections.getConnectionForSide (startSide);
				if (currentConnection == null) {
					crashing = true;
					return;
				}
				piecesTouched[currentPuzzlePiece.name] = piecesTouched[currentPuzzlePiece.name] + 1;
				currentDirection = currentConnection.OtherSide (startSide);
				Debug.Log ("new bearing: " + currentDirection);
				currentCoordinateIndex = currentConnection.getFirstCoordinateIndexFor (currentDirection);
				// Get the next one, because the first one of the new piece is the same as the last of the previous piece.
				currentCoordinateIndex = currentConnection.getNextCoordinateIndex (currentCoordinateIndex, currentDirection);
			}
			currentCoordinate = currentConnection.coordinates [currentCoordinateIndex];
			RotateTowardsTarget ();
		}
		CheckForNearbyButton();
	}

	void MoveTowardsTarget(float x, float z) {
		Vector3 point = PointOfCar ();
		Vector3 position = transform.position;
		float movement = (levelConfiguration.movement + boost) * Time.deltaTime;
		transform.position = Vector3.MoveTowards (point, new Vector3 (x, position.y, z), movement) + (position - point);
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
		Vector3 pos = PointOfCar ();
		Boolean nearby = false;
		foreach (GameObject button in buttons) {
			Vector3 buttonPos = button.transform.position;
			if (Math.Abs(pos.x - (buttonPos.x + 0.25f)) < 0.03f && Math.Abs(pos.z - (buttonPos.z - 0.25f)) < 0.03f)
				nearby = true;
		}
		if (!inRangeOfButton && nearby)
			gameScript.FlipBridgePositions ();
		inRangeOfButton = nearby;
	}

	Boolean OnOpenBridgePiece(GameObject currentPuzzlePiece) {
		if (!currentPuzzlePiece.name.Contains ("bridge"))
			return false;
		Vector3 pos = currentPuzzlePiece.transform.Find ("bridgePiece").position;
		return gameScript.IsBridgeOpen (pos);
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
		atEnd = (GameObject.FindGameObjectWithTag ("EndPuzzlePiece").transform.position - transform.position).sqrMagnitude < levelConfiguration.PieceSize;
		return atEnd;
	}
}
