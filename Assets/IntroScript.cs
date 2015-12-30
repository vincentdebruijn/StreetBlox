using UnityEngine;
using System.Collections;

public class IntroScript : MonoBehaviour {

	private float time;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;
		if (Input.GetMouseButtonDown(0) || time > 3) {
			Application.LoadLevel ("menu");
		}
	}
}
