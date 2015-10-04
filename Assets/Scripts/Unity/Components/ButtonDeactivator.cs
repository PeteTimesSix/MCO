using UnityEngine;
using System.Collections;

public class ButtonDeactivator : MonoBehaviour {

    public DASH_COMPONENT type = DASH_COMPONENT.NONE;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        bool isActive = GlobalStateHolder.activeScreen.dashSetup.isActive(type);
        this.gameObject.SetActive(isActive);
	}
}
