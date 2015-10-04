using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;

public class Recorder : MonoBehaviour {

    public GameObject OVRcamera;
    public string recordFile = _Constants.defaultRecordingFile;
    public string finalRecordFile = _Constants.defaultFinalRecordingFile + _Constants.recordFileExtension;

    private List<Model> usedModels = new List<Model>();

    private Model[] currentModels = new Model[5];
    private string currentStateText = "";
    private string currentBigText = "";

    private BinaryWriter writer;
    private bool open = true;

	// Use this for initialization
	void Start () {
        writer = new BinaryWriter(new FileStream(recordFile, FileMode.Create));
	}
	
	// Update is called once per frame
	void Update () {
        if (open) 
        {
            RecorderFrame frame = new RecorderFrame();
            Transform camTransform = OVRcamera.GetComponent<Transform>();
            frame.camPos = camTransform.position;
            frame.camRot = camTransform.rotation.eulerAngles;
            frame.modelScale = GlobalStateHolder.unityInterface.getCurrentScale();
            frame.modelRot = GlobalStateHolder.unityInterface.getCurrentRotation();

            Model[] duplet = GlobalStateHolder.unityInterface.getCurrentDuplet();
            if (duplet == null) duplet = new Model[2];
            Model[] triplet = GlobalStateHolder.unityInterface.getCurrentTriplet();
            if (triplet == null) triplet = new Model[3];
            Model[] models = duplet.Concat(triplet).ToArray();

            int modelDiff = 0;
            for (int i = 0; i < models.Length; i++)
            {
                if (models[i] != currentModels[i])
                {
                    modelDiff++;
                    addUsedModel(currentModels[i]);
                }
            }
            frame.modelIDs = new int[modelDiff];
            frame.modelFilenames = new string[modelDiff];
            int modelPos = 0;
            for (int i = 0; i < models.Length; i++)
            {
                if (models[i] != currentModels[i])
                {
                    currentModels[i] = models[i];
                    frame.modelIDs[modelPos] = i;
                    if (models[i] != null)
                        frame.modelFilenames[modelPos] = models[i].Path;
                    else
                        frame.modelFilenames[modelPos] = "NO_MODEL";
                    modelPos++;
                }
            }

            string stateText = GlobalStateHolder.unityInterface.getStateText();
            string bigText = GlobalStateHolder.unityInterface.getBigText();
            if (!currentStateText.Equals(stateText))
                frame.stateText = stateText;
            if (!currentBigText.Equals(currentBigText))
                frame.bigText = bigText;
            frame.textState = RecorderFrame.TEXTSTATE.NONE;
            if (frame.stateText != null & frame.bigText != null)
                frame.textState = RecorderFrame.TEXTSTATE.BOTH;
            else
            {
                if (frame.stateText != null)
                    frame.textState = RecorderFrame.TEXTSTATE.STATE;
                if (frame.bigText != null)
                    frame.textState = RecorderFrame.TEXTSTATE.BIG;
            }
            frame.write(writer);
            MonoBehaviour.print("Wrote recorder frame");
            //IMCOScreen currentScreen = unityInterface.g
        }
	}

    private void addUsedModel(Model model)
    {
        if (model != null) 
        {
            foreach (Model existing in usedModels)
            {
                if (existing.Path.Equals(model.Path)) return;
            }
            usedModels.Add(model);
       }       
    }

    internal void finalise()
    {
        if (open) 
        {
            MonoBehaviour.print("Finalising recording...");
            writer.Close();
            writer = new BinaryWriter(new FileStream(finalRecordFile, FileMode.Create));
            writer.Write(usedModels.Count);
            foreach (Model model in usedModels)
            {
                writer.Write(model.Path);
            }
            writer.Close();
            appendBinaryFiles(finalRecordFile, recordFile);
            File.Delete(recordFile);
            open = false;
        }     
    }

    static void appendBinaryFiles(string to,string from)
    {
        using (Stream original = new FileStream(to,FileMode.Append))
        {
            using (Stream extra = new FileStream(from,FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[32 * 1024];

                int blockSize;
                while ((blockSize = extra.Read
                (buffer, 0, buffer.Length)) > 0)
                {
                    original.Write(buffer, 0, blockSize);
                }
            }
        }
    }
}
