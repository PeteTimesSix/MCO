using UnityEngine;
using System.Collections;

public class GazeButtonController : MonoBehaviour {

    private float timeSinceGazeStart = 0;
    private float lockdownTime = 0;
    private bool isGazeClicked = false;
    private bool isGazedOn = false;
   

    private static Color dark_red = new Color(0.3f, 0.1f, 0.1f);
	// Update is called once per frame
	void Update () {
        if (lockdownTime > 0)
        {
            isGazedOn = false;
            isGazeClicked = false;
            timeSinceGazeStart = 0;
            if (_Constants.BUTTON_LOCK_TIME - lockdownTime < _Constants.BUTTON_PICK_STICK_TIME)
                setColor(Color.green);
            else
                setColor(Color.Lerp(dark_red, Color.gray, (_Constants.BUTTON_LOCK_TIME - lockdownTime) / _Constants.BUTTON_LOCK_TIME));
        }
        else if (!isGazedOn)
        {
            setColor(Color.gray);
        }
        else 
        {
            timeSinceGazeStart += Time.deltaTime;
            if (timeSinceGazeStart < _Constants.BUTTON_GAZE_TIME)
            {
                setColor(Color.Lerp(Color.white, Color.cyan, timeSinceGazeStart / _Constants.BUTTON_GAZE_TIME));
            }
            else
            {
                isGazeClicked = true; 
                setColor(Color.green);
            }
        }
        lockdownTime -= Time.deltaTime;
        if (lockdownTime < 0) lockdownTime = 0;
        
	}

    private void setColor(Color color) 
    {
        GetComponent<Renderer>().material.color = color;
    }

    public bool isClicked() { return isGazeClicked; }

    public void MyPointerEnter()
    {
        isGazedOn = true;
        //MonoBehaviour.print("in");
    }

    public void MyPointerLeave()
    {
        timeSinceGazeStart = 0;
        isGazedOn = false;
        isGazeClicked = false;

        //MonoBehaviour.print("out");
    }

    public void lockForTime(float time) 
    {
        lockdownTime = Mathf.Max(time, lockdownTime);
    }
}
