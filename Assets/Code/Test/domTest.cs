using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class domTest : MonoBehaviour {
	public LevelLoader levelLoader;
	public static bool loaded = false;

	// Start is called before the first frame update
	void Update() {
		if (!loaded) {
			loaded = true;
			levelLoader.loadLevel(0, 0);
		}
	}
}
