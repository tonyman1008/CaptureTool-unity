using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CaptureManager : MonoBehaviour
{
    public int textureWidth = 1000;
    public int textureHeight = 1000;

    public Camera cam = null;
    public GameObject targetObj = null;

    private RenderTexture captureRT = null;

    private float rotateAngle = 0.0f;
    private bool capturing = false;
    private float captureTime = 36f;

    public GameObject testPoints = null;
    public List<Transform> correspondencePointsTrans = new List<Transform>();
    [SerializeField] public List<List<Transform>> correspondencePointsTransSeq = new List<List<Transform>>();

    [SerializeField] public MatchPointSeqData _matchPointSeqData = new MatchPointSeqData();


    void Awake()
    {
        rotateAngle = 0;
        capturing = false;

        cam.targetTexture = new RenderTexture(textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32);
        cam.backgroundColor = Color.clear;
    }

    private void capture()
    {
        if (rotateAngle < 360)
        {
            //Debug.Log("angle:" + rotateAngle);
            updateCorrespondenceRaycastState();

            string fileName = rotateAngle + ".png";
            //SaveRenderTextureToFile(cam.targetTexture, fileName);

            // store last frame correspondence trans
            storeCorrespondenceInPerFrame();

            // ---- next capture ----

            // rotate object
            rotateObjAlongYAxis(1);
            rotateAngle++;
        }
        else
        {
            CancelInvoke();
            rotateAngle = 0;
            capturing = false;
            Debug.Log("capture complete");
        }
    }

    public void SaveRenderTextureToFile(RenderTexture rTex, string fileName)
    {
        RenderTexture.active = rTex;
        Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        byte[] bytes;
        bytes = tex.EncodeToPNG();

        //create folder
        string objectFolderName = targetObj.name;
        createFolder(objectFolderName);

        string path = Application.streamingAssetsPath + "/output/" + objectFolderName + "/" + fileName;
        System.IO.File.WriteAllBytes(path, bytes);
        return;
    }

    public void createFolder(string folderName)
    {
        var folder = Directory.CreateDirectory(Application.streamingAssetsPath + "/output/" + folderName);
    }

    public void rotateObjAlongYAxis(int deg)
    {
        targetObj.transform.Rotate(new Vector3(0, deg, 0));
    }

    private void updateCorrespondenceRaycastState()
    {
        foreach (Transform tran in correspondencePointsTrans)
        {
            tran.GetComponent<Correspondence>().checkPointCanBeRaycasted();
        }
    }

    private void sampleVertices()
    {
        MeshFilter mf = targetObj.GetComponentInChildren<MeshFilter>();

        int verticesLength = mf.mesh.vertices.Length;
        Debug.Log("vertice length: " + verticesLength);

        // random
        int randomItemCount = 100;
        List<Vector3> tempVertices = new List<Vector3>(mf.mesh.vertices);
        List<Vector3> randomVertices = new List<Vector3>();

        for (int q = 0; q < randomItemCount; q++)
        {
            int randomIndex = Random.Range(0, tempVertices.Count);
            randomVertices.Add(tempVertices[randomIndex]);
            tempVertices.RemoveAt(randomIndex);
        }

        Debug.Log("randomVertices: " + randomVertices.Count);

        Matrix4x4 localToWorld = targetObj.transform.localToWorldMatrix;
        for (int i = 0; i < randomItemCount; ++i)
        {
            GameObject createPoint = Instantiate(testPoints, localToWorld.MultiplyPoint3x4(randomVertices[i]), targetObj.transform.rotation);
            createPoint.transform.parent = targetObj.transform;
            createPoint.tag = testPoints.GetComponent<Correspondence>().defaultTagName;

            // attach camera
            createPoint.GetComponent<Correspondence>().camTransform = cam.transform;
            createPoint.GetComponent<Correspondence>().index = i;
            correspondencePointsTrans.Add(createPoint.transform);
        }
    }

    private void storeCorrespondenceInPerFrame()
    {
        List<Transform> correspondencePointsTrans_temp = new List<Transform>();
        for(int i=0;i<correspondencePointsTrans.Count;i++)
        {
            bool canRaycast = correspondencePointsTrans[i].GetComponent<Correspondence>().canBeRaycasted;
            if (canRaycast)
            {
                correspondencePointsTrans_temp.Add(correspondencePointsTrans[i]);
            }
        }
        correspondencePointsTransSeq.Add(correspondencePointsTrans_temp);
    }

    private void outputCorrespondenceData()
    {
        Debug.Log("Comparing correspondence !!");

        for (int i = 0; i < correspondencePointsTransSeq.Count; i++)
        {
            List<Transform> srcCorrespondencePointsTrans = new List<Transform>(correspondencePointsTransSeq[i]);
            List<Transform> tgtCorrespondencePointsTrans;

            MatchPointArray _matchPointArray = new MatchPointArray();

            if (i == correspondencePointsTransSeq.Count - 1)
            {
                tgtCorrespondencePointsTrans = new List<Transform>(correspondencePointsTransSeq[0]);
            }
            else
            {
                tgtCorrespondencePointsTrans = new List<Transform>(correspondencePointsTransSeq[i + 1]);
            }

            //compare TODO:rewrite logic
            for (int j = 0; j < srcCorrespondencePointsTrans.Count; j++)
            {
                for (int k = 0; k < tgtCorrespondencePointsTrans.Count; k++)
                {
                    int srcIndex = srcCorrespondencePointsTrans[j].GetComponent<Correspondence>().index;
                    int tgtIndex = tgtCorrespondencePointsTrans[k].GetComponent<Correspondence>().index;
                    if (srcIndex == tgtIndex)
                    {
                        MatchPoint _matchPoint = new MatchPoint();

                        Vector3 srcCorrespondenceScreenPos = cam.WorldToScreenPoint(srcCorrespondencePointsTrans[j].position);
                        Vector3 tgtCorrespondenceScreenPos = cam.WorldToScreenPoint(tgtCorrespondencePointsTrans[k].position);

                        _matchPoint.keyPointOne[0] = srcCorrespondenceScreenPos.x;
                        _matchPoint.keyPointOne[1] = srcCorrespondenceScreenPos.y;
                        _matchPoint.keyPointTwo[0] = tgtCorrespondenceScreenPos.x;
                        _matchPoint.keyPointTwo[1] = tgtCorrespondenceScreenPos.y;
                        _matchPointArray.matchPoints.Add(_matchPoint);
                    }
                }
            }
            _matchPointSeqData.matchPointSeqData.Add(_matchPointArray);
        }


        Debug.Log("write json");
        string matchDataStr = JsonUtility.ToJson(_matchPointSeqData);

        string filename = Application.streamingAssetsPath + "/output/PotionData.json";
        if (File.Exists(filename))
        {
            File.Delete(filename);
        }
        File.WriteAllText(filename, matchDataStr);
        Debug.Log("write json complete !!");

    }

    // Update is called once per frame
    void Update()
    {
        captureRT = cam.targetTexture;

        if (Input.GetKeyDown(KeyCode.C) && !capturing)
        {
            Debug.Log("Camera width " + cam.pixelWidth + " Camera height " + cam.pixelHeight);
            Debug.Log("start capture");
            sampleVertices();

            // do capture after 2 seconds to make sure smaple vertice finished.
            float invokeRate = captureTime / 360f;
            InvokeRepeating("capture", 2, invokeRate);
            capturing = true;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            outputCorrespondenceData();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            sampleVertices();
        }

    }
}

[System.Serializable]
public class MatchPointSeqData
{
    public List<MatchPointArray> matchPointSeqData = new List<MatchPointArray>();
}

[System.Serializable]
public class MatchPointArray
{
    public List<MatchPoint> matchPoints = new List<MatchPoint>();
}

[System.Serializable]
public class MatchPoint
{
    public float[] keyPointOne = new float[2];
    public float[] keyPointTwo = new float[2];
}

