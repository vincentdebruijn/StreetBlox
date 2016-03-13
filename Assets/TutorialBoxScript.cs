using UnityEngine;
using System.Collections;

public class TutorialBoxScript : MonoBehaviour {

	public string[] messages;

	private int messageCounter;
	private TextMesh textMesh;

	public void SetMessages(string[] messages) {
		this.messages = messages;
		messageCounter = 0;
		textMesh = transform.GetComponentInChildren<TextMesh>();
		textMesh.text = messages [0];
	}

	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0)) {
			MenuScript.PlayButtonSound();
			if (messageCounter < messages.Length - 1) {
				messageCounter += 1;
				textMesh.text = messages [messageCounter];
			} else {
				Destroy (gameObject);
			}
		}
	}
}
