using UnityEngine;
using UnityEngine.UI;
using TypeUtil;

public class IgnoreLayoutAdjuster : MonoBehaviour {
	public UnitEvent ignoreLayoutEvent;
	public UnitEvent stopIgnoringLayoutEvent;

	private LayoutElement layoutElement;


	// Init
	private void Awake() {
		layoutElement = GetComponent<LayoutElement>();
		ignoreLayoutEvent.AddRemovableListener(ignoreLayout, this);
		stopIgnoringLayoutEvent.AddRemovableListener(stopIgnoringLayout, this);
	}

	private void ignoreLayout(Unit _) {
		layoutElement.ignoreLayout = true;
	}
	private void stopIgnoringLayout(Unit _) {
		layoutElement.ignoreLayout = false;
	}
}
