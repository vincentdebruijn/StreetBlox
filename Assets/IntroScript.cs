using UnityEngine;
using System.Collections;

public class IntroScript : MonoBehaviour {

	private float time;
	private AsyncOperation async;

	// Use this for initialization
	IEnumerator Start() {
		async = Application.LoadLevelAsync("menu");
		async.allowSceneActivation = false;
		yield return async;
	}
	
	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;
		if (Input.GetMouseButtonDown(0) || time > 3) {
			async.allowSceneActivation = true;
		}
	}
}
