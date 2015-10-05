using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/*
 * Bit of a mis-name, this class serves as the mediator between C# code and Unity
 * 
 */
public class UnityInterface : MonoBehaviour {

    public GameObject OVRcamera; 
    public GameObject OVRcamRig;

    public GameObject modelHolder;

    public GameObject t_original;
    public GameObject t_copyA;
    public GameObject t_copyB;
    public GameObject t_originalLabel;
    public GameObject t_copyALabel;
    public GameObject t_copyBLabel;

    public GameObject d_original;
    public GameObject d_copy;
    public GameObject d_originalLabel;
    public GameObject d_copyLabel;

    public GameObject stateText;
    public GameObject bigText;
    public GameObject bigPicture;

    public GameObject buttonA;
    public GameObject buttonB;
    public GameObject buttonAny;
    public GameObject slideRating;

    internal bool codeDriven = false;

    private Model[] activeTriplet;
    private Model[] activeDuplet;
    private Vector3 activeScale;

    private Vector3 currentRotEuler = new Vector3(0, 0, 0);
    private float currentZoom = 0;

    public SceneParameters sceneParameters;

    private bool zoomMode = true;

    private void checkCommandline() 
    {
        /*MonoBehaviour.print("commandline args");
        foreach (string str in System.Environment.GetCommandLineArgs())
        {
            MonoBehaviour.print(str);
        }*/
        //MonoBehaviour.print(System.Environment.GetCommandLineArgs());
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            string firstArg = args[1];
            if (firstArg.EndsWith(_Constants.datFileExtension) | firstArg.EndsWith(_Constants.objFileExtension)) 
            {
                GlobalStateHolder.activeExperiment = MCOExperiment.generateModelview(firstArg, System.IO.Path.IsPathRooted(firstArg));
            }
            else if (firstArg.EndsWith(_Constants.recordFileExtension))
            {
                GlobalStateHolder.activeExperiment = MCOExperiment.generateRecordPlayback(firstArg, System.IO.Path.IsPathRooted(firstArg));
            }
            else if (firstArg.EndsWith(_Constants.xmlFileExtension)) 
            {
                GlobalStateHolder.activeExperiment = MCOExperiment.load(firstArg);
            }
            else
            {
                GlobalStateHolder.activeExperiment = MCOExperiment.load("default.xml");
            }
        }
        else 
        {
            GlobalStateHolder.activeExperiment = MCOExperiment.load("default.xml");
        }
        
    }

	// Use this for initialization
	void Start () {
        GlobalStateHolder.unityInterface = this;
        GlobalStateHolder.recorder = this.gameObject.GetComponent<Recorder>();
        sceneParameters = this.gameObject.GetComponent<SceneParameters>();
        //GlobalStateHolder.activeExperiment = MCOExperiment.generateDefault();
        //GlobalStateHolder.activeExperiment.save("default.xml");
        checkCommandline();
        //GlobalStateHolder.activeExperiment = MCOExperiment.generateRecordPlayback(_Constants.defaultFinalRecordingFile + _Constants.recordFileExtension, false);
        GlobalStateHolder.activeScreen = GlobalStateHolder.activeExperiment.Screens[0];
        GlobalStateHolder.activeScreen.jumpedTo(null);
        GlobalStateHolder.activeExperiment.save("latestRun.xml");
	}

    void Awake()
    {
        

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //MCOExperiment.generateDefault();
    }

    /*internal void flyByWire(Vector3 camPos, Vector3 camRot, float scale, Vector3 modelRot) 
    {
        RecorderFrame frame;
        frame.
    }*/

    internal void flyByWire(RecorderFrame frameData, PreloadedModelsStorage modelStorage, bool forceCam) 
    {
        if (forceCam) 
        {
            //MonoBehaviour.print("Forcing cam to "+frameData.camPos+" , "+frameData.camRot);
            OVRcamRig.transform.position = frameData.camPos;
            OVRcamRig.transform.rotation = Quaternion.Euler(frameData.camRot);
        }
        if (frameData.textState == RecorderFrame.TEXTSTATE.BIG | frameData.textState == RecorderFrame.TEXTSTATE.BOTH) 
        {
            setBigText(frameData.bigText);
        }
        if (frameData.textState == RecorderFrame.TEXTSTATE.STATE | frameData.textState == RecorderFrame.TEXTSTATE.BOTH)
        {
            setStateText(frameData.stateText);
        }
        currentZoom = frameData.modelScale;
        currentRotEuler = frameData.modelRot;
        for (int i = 0; i < frameData.modelIDs.Length; i++)
        {
            MonoBehaviour.print("Assinging model "+frameData.modelFilenames[i]+" to "+frameData.modelIDs[i]);
            assignSpecific(frameData.modelIDs[i], modelStorage.getModelForcedLoad(frameData.modelFilenames[i]));
        }
    }

    void FixedUpdate() 
    {
        IMCOScreen screen = GlobalStateHolder.activeScreen;
        if (GlobalStateHolder.activeScreen != null)
        {
            GlobalStateHolder.activeScreen.frameEndfixed();
        }
    }
	
	// Update is called once per frame
	void Update () {
        IMCOScreen screen = GlobalStateHolder.activeScreen;
        if (GlobalStateHolder.activeScreen != null)
        {
            GlobalStateHolder.activeScreen = GlobalStateHolder.activeScreen.frameEnd();
        }

        float zoom = 0;
        float offset = 0;

        if (!codeDriven)
        {
            float horizontal = Input.GetAxis("Mouse X");
            float vertical = Input.GetAxis("Mouse Y");
            if (zoomMode)
            {
                zoom = Input.GetAxis("Mouse ScrollWheel");
                currentRotEuler.x += vertical * _Constants.rotationRateMultVertical;
                currentRotEuler.y += horizontal * _Constants.rotationRateMultHorizontal;
                currentZoom += zoom;
                if (currentZoom < _Constants.MIN_ZOOM) currentZoom = _Constants.MIN_ZOOM;
                if (currentZoom > _Constants.MAX_ZOOM) currentZoom = _Constants.MAX_ZOOM;
            }
            else
            {
                offset = Input.GetAxis("Mouse ScrollWheel");
            }

            if (Input.GetButtonDown(_Constants.cancel))
            {
                //MonoBehaviour.print("escape");
                GlobalStateHolder.exit();
            }
            else
            {
                MCOInputMask inputMask = MCOInputMask.None;
                if (Input.anyKeyDown | buttonAny.GetComponent<GazeButtonController>().isClicked())
                {
                    OVRManager.DismissHSWDisplay();
                    buttonAny.GetComponent<GazeButtonController>().lockForTime(_Constants.BUTTON_LOCK_TIME);
                    inputMask |= MCOInputMask.Any;
                }
                if (Input.GetButtonDown(_Constants.buttonA) | buttonA.GetComponent<GazeButtonController>().isClicked())
                {
                    buttonA.GetComponent<GazeButtonController>().lockForTime(_Constants.BUTTON_LOCK_TIME);
                    inputMask |= MCOInputMask.ButtonA;
                }
                if (Input.GetButtonDown(_Constants.buttonB) | buttonB.GetComponent<GazeButtonController>().isClicked())
                {
                    buttonB.GetComponent<GazeButtonController>().lockForTime(_Constants.BUTTON_LOCK_TIME);
                    inputMask |= MCOInputMask.ButtonB;
                }
                if (Input.GetButtonDown(_Constants.buttonC))
                {
                    //buttonC.GetComponent<GazeButtonController>().lockForTime(_Constants.BUTTON_LOCK_TIME);
                    inputMask |= MCOInputMask.ButtonC;
                }
                if (GlobalStateHolder.activeScreen != null)
                {
                    GlobalStateHolder.activeScreen.handleInput(inputMask);
                }
                else
                {
                    setStateText("NO STATE");
                }
            }

            if (GlobalStateHolder.activeScreen == null)
            {
                GlobalStateHolder.exit();
                return;
            }
            if (screen != GlobalStateHolder.activeScreen)
            {
                if (screen != null)
                {
                    screen.jumpedFrom();
                }
                if (GlobalStateHolder.activeScreen != null)
                {
                    GlobalStateHolder.activeScreen.jumpedTo(screen);
                }
            }
            if (GlobalStateHolder.activeScreen != null)
            {
                setStateText(GlobalStateHolder.activeScreen.GetType().ToString());
            }
            else
            {
                setStateText("NO STATE");
            }
        }      

        Vector3 offsetVec = new Vector3(0, offset, 0);
        modelHolder.transform.position = modelHolder.transform.position + offsetVec;
        Quaternion rotation = Quaternion.Euler(currentRotEuler);
        if (t_original != null)
        {
            t_original.transform.rotation = rotation;
            t_original.transform.localScale = activeScale * (currentZoom);
        }
        if (t_copyA != null)
        {
            //copyA.transform.rotation = rotation;
            t_copyA.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.left) * rotation;
            t_copyA.transform.localScale = activeScale * (currentZoom);
        }
        if (t_copyB != null)
        {
            //copyB.transform.rotation = rotation;
            t_copyB.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.right) * rotation;
            t_copyB.transform.localScale = activeScale * (currentZoom);
        }
        if (d_original != null)
        {
            d_original.transform.rotation = Quaternion.LookRotation(d_original.transform.position) * rotation;
            d_original.transform.localScale = activeScale * (currentZoom);
        }
        if (d_copy != null)
        {
            d_copy.transform.rotation = Quaternion.LookRotation(d_copy.transform.position) * rotation;
            d_copy.transform.localScale = activeScale * (currentZoom);
        }
         

        //MonoBehaviour.print(currentZoom);
        
	}

    private void destroyChildren(GameObject target) 
    {
        List<GameObject> children = new List<GameObject>();
        for (int child = 0; child < target.transform.childCount; child++)
        {
            children.Add(target.transform.GetChild(child).gameObject);
        }
        foreach (GameObject child in children.ToArray())
        {
            Destroy(child);
        }
    }

    internal void setupModel(GameObject target, Model model) 
    {
        destroyChildren(target);
        Vector3 scaleVec = new Vector3(1, 1, 1);
        if (model != null) 
        {
            target.GetComponent<MeshFilter>().mesh = model.Meshes[0];
            for (int i = 1; i < model.Meshes.Length; i++)
            {
                GameObject submesh = new GameObject();
                submesh.transform.parent = target.transform;
                submesh.transform.localScale = Vector3.one;
                submesh.transform.localPosition = Vector3.zero;
                submesh.transform.localRotation = Quaternion.identity;
                submesh.AddComponent<MeshFilter>().mesh = model.Meshes[i];
                submesh.AddComponent<MeshRenderer>().material = sceneParameters.objectMaterial;
            }
            scaleVec = new Vector3(1f / model.span, 1f / model.span, 1f / model.span);
            target.transform.localScale = scaleVec;
        }
        this.activeScale = scaleVec;
        this.currentZoom = 1;
    }

    internal void assignDuplet(Model[] duplet)
    {
        //MonoBehaviour.print("duplet assign");
        wipe();
        if (duplet == null)
        {      
            return;
        }
        setupModel(d_original, duplet[0]);
        setupModel(d_copy, duplet[1]);

        if (!sceneParameters.hide_labels)
        {
            d_originalLabel.GetComponent<TextMesh>().text = duplet[0].ToString();
            d_copyLabel.GetComponent<TextMesh>().text = duplet[1].ToString();
        }
        /*original.GetComponent<MeshFilter>().mesh = triplet[0].mesh;
        copyA.GetComponent<MeshFilter>().mesh = triplet[1].mesh;
        copyB.GetComponent<MeshFilter>().mesh = triplet[2].mesh;*/

        float biggestSpan = duplet[0].span;
        if (duplet[1].span > biggestSpan) biggestSpan = duplet[1].span;
        //MonoBehaviour.print("SPAN: " + biggestSpan);

        biggestSpan = 1 / biggestSpan;
        biggestSpan *= 0.8f;
        //MonoBehaviour.print("Scale factor: " + biggestSpan);

        Vector3 scaleVec = new Vector3(biggestSpan, biggestSpan, biggestSpan);

        d_original.transform.localScale = scaleVec;
        d_copy.transform.localScale = scaleVec;

        this.activeScale = scaleVec;
        this.activeDuplet = duplet;
        this.activeTriplet = null;
        this.currentZoom = 1;
    }

    private void assignSpecific(int id, Model model)
    {
        GameObject target;
        GameObject targetLabel;
        switch (id) 
        {
            case 0:
                target = d_original;
                targetLabel = d_originalLabel;
                break;
            case 1:
                target = d_copy;
                targetLabel = d_copyLabel;
                break;
            case 2:
                target = t_original;
                targetLabel = t_originalLabel;
                break;
            case 3:
                target = t_copyA;
                targetLabel = t_copyALabel;
                break;
            case 4:
                target = t_copyB;
                targetLabel = t_copyBLabel;
                break;
            default:
                MonoBehaviour.print("Illegal specific model assign("+id+")");
                return;
                //break;
        }
        setupModel(target, model);
        if (!sceneParameters.hide_labels)
        {
            if (model != null)
            {
                targetLabel.GetComponent<TextMesh>().text = model.ToString();
            }
            else 
            {
                targetLabel.GetComponent<TextMesh>().text = "NULL";
            }
        }
    }

    internal void assignTriplet(Model[] triplet)
    {
        wipe();
        if (triplet == null) 
        {           
            return;
        }
        setupModel(t_original, triplet[0]);
        setupModel(t_copyA, triplet[1]);
        setupModel(t_copyB, triplet[2]);

        if (!sceneParameters.hide_labels) 
        {
            t_originalLabel.GetComponent<TextMesh>().text = triplet[0].ToString();
            t_copyALabel.GetComponent<TextMesh>().text = triplet[1].ToString();
            t_copyBLabel.GetComponent<TextMesh>().text = triplet[2].ToString();
        }
        /*original.GetComponent<MeshFilter>().mesh = triplet[0].mesh;
        copyA.GetComponent<MeshFilter>().mesh = triplet[1].mesh;
        copyB.GetComponent<MeshFilter>().mesh = triplet[2].mesh;*/

        float biggestSpan = triplet[0].span;
        if (triplet[1].span > biggestSpan) biggestSpan = triplet[1].span;
        if (triplet[2].span > biggestSpan) biggestSpan = triplet[2].span;
        //MonoBehaviour.print("SPAN: " + biggestSpan);

        biggestSpan = 1 / biggestSpan;
        biggestSpan *= 0.8f;
        //MonoBehaviour.print("Scale factor: " + biggestSpan);

        Vector3 scaleVec = new Vector3(biggestSpan, biggestSpan, biggestSpan);

        t_original.transform.localScale = scaleVec;
        t_copyA.transform.localScale = scaleVec;
        t_copyB.transform.localScale = scaleVec;

        this.activeScale = scaleVec;
        this.activeTriplet = triplet;
        this.activeDuplet = null;
        this.currentZoom = 1;
    }

    internal void setStateText(string p)
    {
        stateText.GetComponent<TextMesh>().text = p;
    }

    internal void setBigText(string p)
    {
        bigText.GetComponent<TextMesh>().text = p;
    }

    internal void clearBigImage() 
    {
        bigPicture.GetComponent<Renderer>().enabled = false;
    }

    internal void loadBigImage(string path)
    {
        MonoBehaviour.print("loading image " + path);
        Texture2D texture = LoadImage(path);
        bigPicture.GetComponent<Renderer>().enabled = true;
        bigPicture.GetComponent<Renderer>().material.mainTexture = texture;
    }

    public static Texture2D LoadImage(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    internal void wipe()
    {
        this.activeTriplet = null;
        t_original.GetComponent<MeshFilter>().mesh = null;
        destroyChildren(t_original);
        t_copyA.GetComponent<MeshFilter>().mesh = null;
        destroyChildren(t_copyA);
        t_copyB.GetComponent<MeshFilter>().mesh = null;
        destroyChildren(t_copyB);

        this.activeDuplet = null;
        d_original.GetComponent<MeshFilter>().mesh = null;
        destroyChildren(d_original);
        d_copy.GetComponent<MeshFilter>().mesh = null;
        destroyChildren(d_copy);

        t_originalLabel.GetComponent<TextMesh>().text = "";
        t_copyALabel.GetComponent<TextMesh>().text = "";
        t_copyBLabel.GetComponent<TextMesh>().text = "";
        d_originalLabel.GetComponent<TextMesh>().text = "";
        d_copyLabel.GetComponent<TextMesh>().text = "";
    }

    internal Vector3 getCurrentRotation()
    {
        return currentRotEuler;
    }

    internal float getCurrentScale()
    {
        return currentZoom;
    }

    internal string getStateText()
    {
        return stateText.GetComponent<TextMesh>().text;
    }

    internal string getBigText()
    {
        return bigText.GetComponent<TextMesh>().text;
    }

    internal Model[] getCurrentDuplet()
    {
        return activeDuplet;
    }
    
    internal Model[] getCurrentTriplet()
    {
        return activeTriplet;
    }

    internal void setCodeDriven(bool state)
    {
        codeDriven = state;
    }
}
