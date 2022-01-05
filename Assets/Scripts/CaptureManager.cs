using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using DataFormat;

public class CaptureManager : MonoBehaviour
{
    // GameObject
    public Camera cam = null;
    public GameObject targetObj = null;

    public GameObject testPoints = null;

    //parameter
    public int textureWidth = 1000;
    public int textureHeight = 1000;
    private float rotateAngle = 0.0f;
    private float captureTime = 36f;
    public int sampleVerticesAmount = 250;
    public int correspondenceDiffDeg = 5;
    public int testDeg = 360;

    // data
    public List<Transform> samplePointsTrans = new List<Transform>();
    [SerializeField] public List<List<SamplePointsData>> samplePointsDataSeq = new List<List<SamplePointsData>>();
    [SerializeField] public MatchPointSeqData _matchPointSeqData = new MatchPointSeqData();

    //state
    private bool capturing = false;
    private bool corrTransStoreComplete = false;

    void Awake()
    {
        rotateAngle = 0;
        capturing = false;

        cam.targetTexture = new RenderTexture(textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32);
        cam.backgroundColor = Color.clear;
    }

    private void capture()
    {
        if (rotateAngle < testDeg)
        {
            Debug.Log("angle:" + rotateAngle);
            updateCorrespondenceRaycastState();

            string fileName = rotateAngle + ".png";
            //SaveRenderTextureToFile(cam.targetTexture, fileName);

            // store last frame correspondence trans
            storeVisiblePointsInPerFrame();

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

            if (corrTransStoreComplete)
            {
                outputCorrespondenceData();
            }

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
        foreach (Transform tran in samplePointsTrans)
        {
            tran.GetComponent<Correspondence>().checkPointCanBeRaycasted();
        }
    }

    private void sampleVertices()
    {
        MeshFilter mf = targetObj.GetComponentInChildren<MeshFilter>();

        int verticesLength = mf.mesh.vertices.Length;
        Debug.Log("vertice length: " + verticesLength);

        // get random vertices in all vertices
        int randomItemCount = sampleVerticesAmount;
        List<Vector3> tempVertices = new List<Vector3>(mf.mesh.vertices);
        List<Vector3> randomVertices = new List<Vector3>();

        for (int q = 0; q < randomItemCount; q++)
        {
            int randomIndex = Random.Range(0, tempVertices.Count);
            randomVertices.Add(tempVertices[randomIndex]);
            tempVertices.RemoveAt(randomIndex);
        }

        Debug.Log("randomVertices: " + randomVertices.Count);

        // get points world position
        Matrix4x4 localToWorld = targetObj.transform.localToWorldMatrix;
        for (int i = 0; i < randomItemCount; i++)
        {
            GameObject createPoint = Instantiate(testPoints, localToWorld.MultiplyPoint3x4(randomVertices[i]), targetObj.transform.rotation);
            createPoint.transform.parent = targetObj.transform;
            createPoint.tag = testPoints.GetComponent<Correspondence>().defaultTagName;

            // attach camera
            createPoint.GetComponent<Correspondence>().camTransform = cam.transform;
            createPoint.GetComponent<Correspondence>().index = i;
            samplePointsTrans.Add(createPoint.transform);
        }
    }

    private void storeVisiblePointsInPerFrame()
    {
        Debug.Log("storeVisiblePointsInPerFrame");

        // check all points can raycast(visible in every frame)
        List<SamplePointsData> canRaycastPointsData_temp = new List<SamplePointsData>();
        for (int i = 0; i < samplePointsTrans.Count; i++)
        {

            bool canRaycast = samplePointsTrans[i].GetComponent<Correspondence>().canBeRaycasted;
            int index = samplePointsTrans[i].GetComponent<Correspondence>().index;
            Vector3 worldPos = samplePointsTrans[i].position;

            // check if the samplePoint can be raycast
            if (canRaycast)
            {
                canRaycastPointsData_temp.Add(new SamplePointsData(worldPos, index));
            }
        }
        samplePointsDataSeq.Add(canRaycastPointsData_temp);

        if (samplePointsDataSeq.Count >= testDeg)
        {
            Debug.Log("storeVisiblePointsInPerFrame Complete !!");
            corrTransStoreComplete = true;
        }
    }

    private void outputCorrespondenceData()
    {
        Debug.Log("Comparing correspondence !!");
        Debug.Log("samplePointsDataSeq.Count : " + samplePointsDataSeq.Count);

        // parse data to output correspondence data between two frames.
        for (int i = 0; i < testDeg; i += correspondenceDiffDeg)
        {
            Debug.Log("i + correspondenceDiffDeg" + (i + correspondenceDiffDeg));
            List<SamplePointsData> srcSampleData = new List<SamplePointsData>(samplePointsDataSeq[i]);
            List<SamplePointsData> tgtSampleData;

            MatchPointArray _matchPointArray = new MatchPointArray();

            // set data index
            if (i == samplePointsDataSeq.Count - correspondenceDiffDeg)
            {
                // last frame's target correspondence data is first frame. 
                tgtSampleData = new List<SamplePointsData>(samplePointsDataSeq[0]);
            }
            else
            {
                tgtSampleData = new List<SamplePointsData>(samplePointsDataSeq[i + correspondenceDiffDeg]);
            }

            //compare TODO:rewrite logic
            for (int j = 0; j < srcSampleData.Count; j++)
            {
                for (int k = 0; k < tgtSampleData.Count; k++)
                {
                    int srcIndex = srcSampleData[j].index;
                    int tgtIndex = tgtSampleData[k].index;

                    // get correspondence by sample points index
                    if (srcIndex == tgtIndex)
                    {
                        MatchPoint _matchPoint = new MatchPoint();

                        // covert to sceen point in capture view
                        Vector3 srcCorrespondenceScreenPos = cam.WorldToScreenPoint(srcSampleData[j].worldPos);
                        Vector3 tgtCorrespondenceScreenPos = cam.WorldToScreenPoint(tgtSampleData[k].worldPos);

                        _matchPoint.keyPointOne[0] = srcCorrespondenceScreenPos.x;
                        _matchPoint.keyPointOne[1] = textureHeight - srcCorrespondenceScreenPos.y;
                        _matchPoint.keyPointTwo[0] = tgtCorrespondenceScreenPos.x;
                        _matchPoint.keyPointTwo[1] = textureHeight - tgtCorrespondenceScreenPos.y;
                        _matchPointArray.matchPoints.Add(_matchPoint);
                    }
                }
            }
            _matchPointSeqData.matchPointSeqData.Add(_matchPointArray);
        }

        Debug.Log("matchPointsSeqDataLength : " + _matchPointSeqData.matchPointSeqData.Count);
        Debug.Log("writing json");
        string matchDataStr = JsonUtility.ToJson(_matchPointSeqData);

        string filename = Application.streamingAssetsPath + "/output/PotionData.json";
        if (File.Exists(filename))
        {
            File.Delete(filename);
        }
        File.WriteAllText(filename, matchDataStr);
        Debug.Log("Output json complete !!");

    }

    // Update is called once per frame
    void Update()
    {
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
            updateCorrespondenceRaycastState();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            sampleVertices();
        }
    }
}

//[System.Serializable]
//public class MatchPointSeqData
//{
//    public List<MatchPointArray> matchPointSeqData = new List<MatchPointArray>();
//}

//[System.Serializable]
//public class MatchPointArray
//{
//    public List<MatchPoint> matchPoints = new List<MatchPoint>();
//}

//[System.Serializable]
//public class MatchPoint
//{
//    public float[] keyPointOne = new float[2];
//    public float[] keyPointTwo = new float[2];
//}

//public class SamplePointsData
//{
//    public Vector3 worldPos;
//    public int index;
//    public SamplePointsData(Vector3 _worldPos, int _index)
//    {
//        worldPos = _worldPos;
//        index = _index;
//    }
//}

