using UnityEngine;
using System.Collections;

public class ButtonDeactivator : MonoBehaviour {

    public DASH_COMPONENT type = DASH_COMPONENT.NONE;
    public GameObject button;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (GlobalStateHolder.activeScreen != null) 
        {
            bool isActive = GlobalStateHolder.activeScreen.dashSetup.isActive(type);
            button.SetActive(isActive);
        }      
	}
}
