using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PuzzlePieceScript : MonoBehaviour {

	public static Dictionary<int, string> puzzlePieces;
	public static Dictionary<string, PuzzlePieceConnections> Connections;

	private GameScript gameScript;

	void Awake () {
		gameScript = Camera.main.GetComponent<GameScript>();
	}

	// Use this for initialization
	void Start () {
	}

	void OnMouseOver () {
	 	if (Input.GetMouseButtonDown(0)) {
			gameScript.ClickedPuzzlePiece(gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
	}

	public static void MakePuzzlePieceConnections() {
		Connections = new Dictionary<string, PuzzlePieceConnections>();
		puzzlePieces = new Dictionary<int, string> ();
		
		Coordinate coordinate1 = new Coordinate(0.0f, 0.5f);
		Coordinate coordinate2 = new Coordinate(1.0f, 0.5f);
		Coordinate[] coordinates = new Coordinate[] {coordinate1, coordinate2};
		Connection connection = new Connection (coordinates);
		Connection[] connections = new Connection[] {connection};
		PuzzlePieceConnections puzzlePieceConnections = new PuzzlePieceConnections (connections);
		Connections.Add ("puzzlePiece_straight_WE", puzzlePieceConnections);
		Connections.Add ("puzzlePiece_straight_EW", puzzlePieceConnections);
		Connections.Add ("puzzlePiece_trainCrossing_EW", puzzlePieceConnections);
		Connections.Add ("puzzlePiece_bridge", puzzlePieceConnections);
		// w.i.p Exploration mode
		puzzlePieces.Add (0, "puzzlePiece_straight_EW");
		puzzlePieces.Add (1, "puzzlePiece_trainCrossing_EW");
		
		coordinate1 = new Coordinate(0.5f, 0.0f);
		coordinate2 = new Coordinate(0.5f, 1.0f);
		coordinates = new Coordinate[] {coordinate1, coordinate2};
		connection = new Connection (coordinates);
		connections = new Connection[] {connection};
		puzzlePieceConnections = new PuzzlePieceConnections (connections);
		Connections.Add ("puzzlePiece_straight_NS", puzzlePieceConnections);
		
		coordinate1 = new Coordinate(0.0f, 0.5f);
		coordinate2 = new Coordinate(1.0f, 0.5f);
		coordinates = new Coordinate[] {coordinate1, coordinate2};
		connection = new Connection (coordinates);
		coordinate1 = new Coordinate(0.5f, 0.0f);
		coordinate2 = new Coordinate(0.5f, 1.0f);
		coordinates = new Coordinate[] {coordinate1, coordinate2};
		Connection connection2 = new Connection (coordinates);
		connections = new Connection[] {connection, connection2};
		puzzlePieceConnections = new PuzzlePieceConnections (connections);
		Connections.Add ("puzzlePiece_crossing_NS_EW", puzzlePieceConnections);
		Connections.Add ("puzzlePiece_crossing", puzzlePieceConnections);
		
		coordinate1 = new Coordinate(0.0f, 0.5f);
		Coordinate coordinate1a = new Coordinate (0.2f, 0.475f); 
		coordinate2 = new Coordinate(0.375f, 0.375f);
		Coordinate coordinate2a = new Coordinate(0.475f, 0.2f);
		Coordinate coordinate3 = new Coordinate(0.5f, 0.0f);
		coordinates = new Coordinate[] {coordinate1, coordinate1a, coordinate2, coordinate2a, coordinate3};
		connection = new Connection (coordinates);
		connections = new Connection[] {connection};
		puzzlePieceConnections = new PuzzlePieceConnections (connections);
		Connections.Add ("puzzlePiece_oneBend_NW", puzzlePieceConnections);
		
		coordinate1 = new Coordinate(0.5f, 1.0f);
		coordinate1a = new Coordinate (0.525f, 0.8f); 
		coordinate2 = new Coordinate(0.625f, 0.625f);
		coordinate2a = new Coordinate(0.8f, 0.525f);
		coordinate3 = new Coordinate(1.0f, 0.5f);
		coordinates = new Coordinate[] {coordinate1, coordinate1a, coordinate2, coordinate2a, coordinate3};
		connection = new Connection (coordinates);
		connections = new Connection[] {connection};
		puzzlePieceConnections = new PuzzlePieceConnections (connections);
		Connections.Add ("puzzlePiece_oneBend_SE", puzzlePieceConnections);
		
		coordinate1 = new Coordinate(0.5f, 1.0f);
		coordinate1a = new Coordinate (0.475f, 0.8f);
		coordinate2 = new Coordinate(0.375f, 0.625f);
		coordinate2a = new Coordinate (0.2f, 0.525f);
		coordinate3 = new Coordinate(0.0f, 0.5f);
		coordinates = new Coordinate[] {coordinate1, coordinate1a, coordinate2, coordinate2a, coordinate3};
		connection = new Connection (coordinates);
		connections = new Connection[] {connection};
		puzzlePieceConnections = new PuzzlePieceConnections (connections);
		Connections.Add ("puzzlePiece_oneBend_SW", puzzlePieceConnections);
		
		coordinate1 = new Coordinate(0.5f, 0.0f);
		coordinate1a = new Coordinate (0.525f, 0.2f);
		coordinate2 = new Coordinate(0.625f, 0.375f);
		coordinate2a = new Coordinate (0.8f, 0.475f);
		coordinate3 = new Coordinate(1.0f, 0.5f);
		coordinates = new Coordinate[] {coordinate1, coordinate1a, coordinate2, coordinate2a, coordinate3};
		connection = new Connection (coordinates);
		connections = new Connection[] {connection};
		puzzlePieceConnections = new PuzzlePieceConnections (connections);
		Connections.Add ("puzzlePiece_oneBend_NE", puzzlePieceConnections);
		
		// NW
		coordinate1 = new Coordinate(0.0f, 0.5f);
		coordinate1a = new Coordinate (0.2f, 0.475f); 
		coordinate2 = new Coordinate(0.375f, 0.375f);
		coordinate2a = new Coordinate(0.475f, 0.2f);
		coordinate3 = new Coordinate(0.5f, 0.0f);
		coordinates = new Coordinate[] {coordinate1, coordinate1a, coordinate2, coordinate2a, coordinate3};
		connection = new Connection (coordinates);
		// SE
		coordinate1 = new Coordinate(0.5f, 1.0f);
		coordinate1a = new Coordinate (0.525f, 0.8f); 
		coordinate2 = new Coordinate(0.625f, 0.625f);
		coordinate2a = new Coordinate(0.8f, 0.525f);
		coordinate3 = new Coordinate(1.0f, 0.5f);
		coordinates = new Coordinate[] {coordinate1, coordinate1a, coordinate2, coordinate2a, coordinate3};
		connection2 = new Connection (coordinates);
		connections = new Connection[] {connection, connection2};
		puzzlePieceConnections = new PuzzlePieceConnections (connections);
		Connections.Add ("puzzlePiece_twoBends_NW_SE", puzzlePieceConnections);
		
		// SW
		coordinate1 = new Coordinate(0.5f, 1.0f);
		coordinate1a = new Coordinate (0.475f, 0.8f);
		coordinate2 = new Coordinate(0.375f, 0.625f);
		coordinate2a = new Coordinate (0.2f, 0.525f);
		coordinate3 = new Coordinate(0.0f, 0.5f);
		coordinates = new Coordinate[] {coordinate1, coordinate2, coordinate3};
		connection = new Connection (coordinates);
		// NE
		coordinate1 = new Coordinate(0.5f, 0.0f);
		coordinate1a = new Coordinate (0.525f, 0.2f);
		coordinate2 = new Coordinate(0.625f, 0.375f);
		coordinate2a = new Coordinate (0.8f, 0.475f);
		coordinate3 = new Coordinate(1.0f, 0.5f);
		coordinates = new Coordinate[] {coordinate1, coordinate2, coordinate3};
		connection2 = new Connection (coordinates);
		connections = new Connection[] {connection, connection2};
		puzzlePieceConnections = new PuzzlePieceConnections (connections);
		Connections.Add ("puzzlePiece_twoBends_NE_SW", puzzlePieceConnections);
		
		coordinate1 = new Coordinate(0.5f, 0.0f);
		coordinate1a = new Coordinate (0.5f, 0.1f);
		coordinate2 = new Coordinate(0.6f, 0.4f);
		coordinate2a = new Coordinate (0.5f, 0.8f);
		coordinate3 = new Coordinate(0.4f, 0.4f);
		Coordinate coordinate4 = new Coordinate(0.5f, 0.1f);
		Coordinate coordinate5 = new Coordinate(0.5f, 0.0f);
		coordinates = new Coordinate[] {coordinate1, coordinate1a, coordinate2, coordinate2a, coordinate3, coordinate4, coordinate5};
		connection = new Connection (coordinates);
		connections = new Connection[] {connection};
		puzzlePieceConnections = new PuzzlePieceConnections (connections);
		Connections.Add ("puzzlePiece_turnabout_N", puzzlePieceConnections);

		coordinate1 = new Coordinate(0.5f, 1.0f);
		coordinate1a = new Coordinate (0.5f, 0.9f);
		coordinate2 = new Coordinate(0.6f, 0.6f);
		coordinate2a = new Coordinate (0.5f, 0.2f);
		coordinate3 = new Coordinate(0.4f, 0.6f);
		coordinate4 = new Coordinate(0.5f, 0.9f);
		coordinate5 = new Coordinate(0.5f, 1.0f);
		coordinates = new Coordinate[] {coordinate1, coordinate1a, coordinate2, coordinate2a, coordinate3, coordinate4, coordinate5};
		connection = new Connection (coordinates);
		connections = new Connection[] {connection};
		puzzlePieceConnections = new PuzzlePieceConnections (connections);
		Connections.Add ("puzzlePiece_turnabout_S", puzzlePieceConnections);
	}
	
	public class PuzzlePieceConnections {
		public Connection[] connections;
		
		public PuzzlePieceConnections(Connection[] connections) {
			this.connections = connections;
		}
		
		public Connection getConnectionForSide(int side) {
			Connection ret = null;
			foreach (Connection conn in connections) {
				if (conn.hasSide (side))
					ret = conn;
			}
			return ret;
		}
		
		public static PuzzlePieceConnections GetPuzzlePieceConnections(GameObject puzzlePiece) {
			string name = FormattedName (puzzlePiece);
			if (!Connections.ContainsKey (name))
				return null;
			return Connections [name];
		}

		public static Boolean HasPuzzlePieceConnections(GameObject puzzlePiece) {
			string name = FormattedName(puzzlePiece);
			return Connections.ContainsKey (name);
		}

		private static string FormattedName(GameObject puzzlePiece) {
			string name = puzzlePiece.name;
			return Regex.Replace (name, "\\s*\\(\\d+\\)\\s*", "");
		}
	}
	
	public class Connection {
		public Coordinate[] coordinates;
		
		public Connection(Coordinate[] coordinates) {
			this.coordinates = coordinates;
		}
		
		public Boolean hasSide(int side) {
			return (coordinates [0].Place () == side || coordinates [coordinates.Length - 1].Place () == side);
		}
		
		public int OtherSide(int side) {
			if (coordinates[0].Place() == side) {
				return coordinates[coordinates.Length - 1].Place();
			} else { 
				return coordinates[0].Place();
			}
		}
		
		public int getFirstCoordinateIndexFor (int direction) {
			if (coordinates [0].Place () == direction)
				return coordinates.Length - 1;
			else
				return 0;
		}
		
		public int getNextCoordinateIndex(int oldIndex, int direction) {
			if (getFirstCoordinateIndexFor (direction) == 0)
				return oldIndex + 1;
			else
				return oldIndex - 1;
		}
	}
	
	public class Coordinate {
		public const int NORTH = 0;
		public const int EAST = 1;
		public const int SOUTH = 2;
		public const int WEST = 3;
		public const int CENTER = 4;
		
		public float x;
		public float z;
		
		public Coordinate(float x, float z) {
			this.x = x;
			this.z = z;
		}
		
		public int Place() {
			if (x == 0.0f) {
				return WEST;
			} else if (x == 1.0f) {
				return EAST;
			} else if (z == 0.0f) {
				return NORTH;
			} else if (z == 1.0f) {
				return SOUTH;
			} else {
				return CENTER;
			}
		}
		
		public int InverseSide(int side) {
			return (side + 2) % 4;
		}

		// Get the absolute coordinate in the current puzzlepiece given the relative coordinates.
		public static Coordinate GetCoordinateFor(float r_x, float r_z, GameObject puzzlePiece, float pieceSize) {
			float x = puzzlePiece.transform.position.x + pieceSize * r_x;
			float z = puzzlePiece.transform.position.z - pieceSize * r_z;
			return new Coordinate (x, z);
		}
	}
}