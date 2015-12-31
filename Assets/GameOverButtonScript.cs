using UnityEngine;
using System.Collections;

public class GameOverButtonScript : MonoBehaviour {
	private GameScript gameScript;

	public string buttonText;
	public Texture normalTexture;
	public Texture pressedTexture;

	void Awake() {
		gameScript = Camera.main.GetComponent<GameScript> ();
	}

	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0)) {
			MenuScript.PlayButtonSound();
			gameScript.Reset(buttonText);
		}
	}
}
