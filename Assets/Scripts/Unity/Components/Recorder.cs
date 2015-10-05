using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System.Collections.Generic;

public class Recorder : MonoBehaviour {

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
	void FixedUpdate () {
        if (open) 
        {
            RecorderFrame frame = new RecorderFrame();
            Transform camTransform = GlobalStateHolder.unityInterface.OVRcamera.GetComponent<Transform>();
            frame.camPos = camTransform.position;
            //MonoBehaviour.print(camTransform.name+"(camPos) is "+frame.camPos);
            frame.camRot = camTransform.rotation.eulerAngles;
            frame.modelScale = GlobalStateHolder.unityInterface.getCurrentScale();
            frame.modelRot = GlobalStateHolder.unityInterface.getCurrentRotation();
            //MonoBehaviour.print("scale " + frame.modelScale + " rot " + frame.modelRot);

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
                    if (models[i] != null)
                        addUsedModel(models[i]);
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
            //MonoBehaviour.print("bigText:" + bigText);
            if ((currentStateText.CompareTo(stateText) != 0))
            {
                frame.stateText = stateText;
                currentStateText = stateText;
            }
            if ((currentBigText.CompareTo(bigText) != 0))
            {
                frame.bigText = bigText;
                currentBigText = bigText;
            }
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
        MonoBehaviour.print("used model call");
        if (model != null)
        {

            foreach (Model existing in usedModels)
            {
                if (existing.Path.Equals(model.Path)) return;
            }
            usedModels.Add(model);
        }
        else 
        {
            MonoBehaviour.print("model is null!");
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
                MonoBehaviour.print("Adding used model "+model.Path);
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
