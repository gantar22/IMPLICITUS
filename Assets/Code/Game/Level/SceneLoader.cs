using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour {
	public void loadSceneWithTransition(string scene) {
		LoadManager.instance.LoadScene(scene);
	}
}
