using UnityEngine;
using System.Collections;

public class SlideSegment : MonoBehaviour {

    public GameObject slide;
    public int value = 0; 
    public Color baseColor;

    private GazeSlideController controller;

    private float timeSinceGazeStart = 0;
    private float lockdownTime = 0;
    private bool isGazeClicked = false;
    private bool isGazedOn = false;

    private static Color dark_red = new Color(0.3f, 0.1f, 0.1f);


	// Use this for initialization
	void Start () {
        controller = slide.GetComponent<GazeSlideController>();
	}

    // Update is called once per frame
    void Update()
    {
        //MonoBehaviour.print("Active");
        if (lockdownTime > 0)
        {
            isGazedOn = false;
            isGazeClicked = false;
            timeSinceGazeStart = 0;
            if (_Constants.BUTTON_LOCK_TIME - lockdownTime < _Constants.BUTTON_PICK_STICK_TIME)
                setColor(Color.green);
            else
                setColor(Color.Lerp(dark_red, baseColor, (_Constants.BUTTON_LOCK_TIME - lockdownTime) / _Constants.SLIDE_LOCK_TIME));
        }
        else if (!isGazedOn)
        {
            setColor(baseColor);
        }
        else
        {
            timeSinceGazeStart += Time.deltaTime;
            if (timeSinceGazeStart < _Constants.SLIDE_GAZE_TIME)
            {
                setColor(Color.Lerp(baseColor, Color.cyan, timeSinceGazeStart / _Constants.SLIDE_GAZE_TIME));
            }
            else
            {
                isGazeClicked = true;
                setColor(Color.black);
                controller.setValueFromSegment(value);
            }
        }
        lockdownTime -= Time.deltaTime;
        if (lockdownTime < 0) lockdownTime = 0;
        if (controller.isValueLocked(value)) setColor(Color.black);

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
