using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EndlessScript : MonoBehaviour {

	private static Dictionary<int, GUIStyle> pieceStyles;
	private static Texture2D toolButtonTexture, toolButtonPressedTexture;
	
	private static Rect leftBottomRect;
	private static Rect rightBottomRect;
	
	private static GUIStyle backButtonChosenStyle;
	private static GUIStyle toolButtonStyle, toolButtonPressedStyle, toolButtonChosenStyle;

	private static int[][] page1;
	private static int[][] selectedPage;

	private Boolean toolScreenOn;

	private int chosenPiece;
	
	void Awake() {
		if (PuzzlePieceScript.Connections == null) {
			PuzzlePieceScript.MakePuzzlePieceConnections ();
		}
		if (page1 == null) {
			SetVariables ();
		}
		toolScreenOn = false;
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
			else if (rightBottomRect.Contains(mousePosition))
				toolButtonChosenStyle = toolButtonPressedStyle;
		}
	}
	
	void OnGUI() {
		if (toolScreenOn) {
			int x = 0;
			int y = 0;
			foreach (int[] row in selectedPage) {
				foreach (int piece in row) {
					if (GUI.Button (new Rect(200 + x * 100, 200, 100, 100), "", pieceStyles[piece])) {
						toolScreenOn = false;
						chosenPiece = piece;
					}
				}
				y += 1;
			}
		} else {
			if (GUI.Button (rightBottomRect, "", toolButtonChosenStyle)) {
				MenuScript.PlayButtonSound ();
				toolScreenOn = true;
				toolButtonChosenStyle = toolButtonStyle;
			}
			if (GUI.Button (leftBottomRect, "", backButtonChosenStyle)) {
				backButtonChosenStyle = MenuScript.backButtonStyle;
				MenuScript.PlayButtonSound ();
				Application.LoadLevel ("world_select");
			}
		}
	}
	
	private static void SetVariables() {
		float buttonSize = (int)(Screen.width / 5 * 0.7);
		float offset = (Screen.width / 5 - buttonSize) / 2;
		leftBottomRect = new Rect (offset, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		rightBottomRect = new Rect (Screen.width - offset - buttonSize, Screen.height - 10 - buttonSize, buttonSize, buttonSize);
		
		toolButtonTexture = (Texture2D)Resources.Load("ui_button_options");
		toolButtonPressedTexture = (Texture2D)Resources.Load("ui_button_options_pressed");
		backButtonChosenStyle = MenuScript.backButtonStyle;

		Texture2D pieceTexture = (Texture2D)Resources.Load ("ui_world1_ph");
		Texture2D piece2Texture = (Texture2D)Resources.Load ("ui_world2_ph");

		GUIStyle pieceStyle = new GUIStyle ();
		pieceStyle.normal.background = pieceTexture;
		GUIStyle piece2Style = new GUIStyle ();
		piece2Style.normal.background = piece2Texture;

		pieceStyles = new Dictionary<int, GUIStyle> ();
		pieceStyles.Add (0, pieceStyle);
		pieceStyles.Add (1, piece2Style);
		
		toolButtonStyle = new GUIStyle ();
		toolButtonStyle.normal.background = toolButtonTexture;
		toolButtonPressedStyle = new GUIStyle ();
		toolButtonPressedStyle.normal.background = toolButtonPressedTexture;
		toolButtonChosenStyle = toolButtonStyle;

		page1 = new int[1] [];
		page1[0] = new int[] {0, 1};

		selectedPage = page1;
	}
}
