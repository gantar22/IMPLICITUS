using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TypeUtil;

public class IgnoreLayoutAdjuster : MonoBehaviour {
	public UnitEvent ignoreLayoutEvent;
	public UnitEvent stopIgnoringLayoutEvent;

	private LayoutElement layoutElement;

	private System.Action removeListener1;
	private System.Action removeListener2;


	// Init
	private void Awake() {
		layoutElement = GetComponent<LayoutElement>();
		removeListener1 = ignoreLayoutEvent.AddRemovableListener(ignoreLayout);
		removeListener2 = stopIgnoringLayoutEvent.AddRemovableListener(stopIgnoringLayout);
	}

	private void ignoreLayout(Unit _) {
		Debug.Log("ignoreLayout");
		layoutElement.ignoreLayout = true;
	}
	private void stopIgnoringLayout(Unit _) {
		Debug.Log("stopIgnoringLayout");
		layoutElement.ignoreLayout = false;
	}

	// Remove listeners on destroy
	private void OnDestroy() {
		removeListener1();
		removeListener2();
	}
}
