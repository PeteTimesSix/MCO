using UnityEngine;
using System.Collections;

public class GazeSlideController : MonoBehaviour {

    public int segments = 5;
    public float gapMult = 0.95f;
    public bool isBadToGood = false;

    public GameObject segmentPrefab;

    internal int value = -1;
    internal bool hasValue = false;

	// Use this for initialization
	void Start () {
        float segmentScale = 1f/(float)(segments+0.5);
        for (int i = 0; i < segments; i++)
        {
            //gameObject.
            GameObject segment = (GameObject)Instantiate(segmentPrefab);
            Transform segmentTransform = segment.GetComponent<Transform>();
            segmentTransform.parent = gameObject.GetComponent<Transform>();
            segmentTransform.localRotation = Quaternion.identity;
            //MonoBehaviour.print(segmentTransform.position);
            //MonoBehaviour.print("segScale "+segmentScale);
            segmentTransform.localPosition = new Vector3(0, 0, (segmentScale * i) + (segmentScale * 0.75f));
            Vector3 localScale = segmentTransform.localScale;
            localScale.z = segmentScale * gapMult;
            localScale.x = 0.9f;
            localScale.y = 0.1f;
            segmentTransform.localScale = localScale;
            SlideSegment handler = segment.GetComponent<SlideSegment>();
            handler.slide = gameObject;
            handler.value = i;
            if (isBadToGood)
                handler.baseColor = Util.redToGreenGradient(1f - ((float)i / (float)(segments - 1f)));
            else
                handler.baseColor = Color.white;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    internal void reset() 
    {
        hasValue = false;
        value = -1;
    }

    internal void advanceValue() 
    {
        if (!hasValue)
        {
            hasValue = true;
            value = 0;
        }
        else 
        {
            value++;
            if (value >= segments) value = 0;
        }
    }

    internal void setValueFromSegment(int value)
    {
        this.value = value;
        hasValue = true;
    }

    internal bool isValueLocked(int value)
    {
        return value == this.value;
    }
}
