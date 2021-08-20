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
    private float captureTime = 10f;


    public GameObject testPoints = null;
    public List<Transform> correspondencePointsTrans = null;

    void Awake()
    {
        rotateAngle = 0;
        capturing = false;

        cam.targetTexture = new RenderTexture(textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32);
        cam.backgroundColor = Color.clear;
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

        string path = Application.streamingAssetsPath + "/output/"+ objectFolderName +"/"+ fileName;
        System.IO.File.WriteAllBytes(path, bytes);
        return;
    }

    public void createFolder(string folderName)
    {
        var folder = Directory.CreateDirectory(Application.streamingAssetsPath + "/output/"+ folderName); 
    }

    public void rotateObjAlongYAxis(int deg)
    {
        targetObj.transform.Rotate(new Vector3(0, deg, 0));
    }

    private void capture()
    {
        Debug.Log("Camera width" + cam.pixelWidth+ " Camera height" + cam.pixelHeight);
        if (rotateAngle <= 360)
        {
            //Debug.Log(rotateAngle);
            rotateObjAlongYAxis(1);
            string fileName = rotateAngle + ".png";
            outputCorrespondence();
            //SaveRenderTextureToFile(cam.targetTexture, fileName);
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

    private void outputCorrespondence()
    {
        for(int i = 0; i < correspondencePointsTrans.Count; i++)
        {
            Debug.Log("worldPos " + correspondencePointsTrans[i].position);
            Vector3 screenPos = cam.WorldToScreenPoint(correspondencePointsTrans[i].position);
            Debug.Log("screenSpace " + screenPos);
        }
    }

    private void sampleVertices()
    {
        Matrix4x4 localToWorld = targetObj.transform.localToWorldMatrix;
        MeshFilter mf = targetObj.GetComponentInChildren<MeshFilter>();

        int verticesLength = mf.mesh.vertices.Length;
        Debug.Log("vertice length: " + verticesLength);


        for (int i = 0; i < 1; ++i)
        {
            GameObject createPoint = Instantiate(testPoints, localToWorld.MultiplyPoint3x4(mf.mesh.vertices[i]), targetObj.transform.rotation);
            createPoint.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            createPoint.transform.parent = targetObj.transform;
            //Debug.Log("create point worldPos: " +createPoint.transform.position);
            correspondencePointsTrans.Add(createPoint.transform);
        }

        outputCorrespondence();
    }

    // Update is called once per frame
    void Update()
    {
        captureRT = cam.targetTexture;

        if (Input.GetKeyDown(KeyCode.C) && !capturing)
        {
            Debug.Log("start capture");
            sampleVertices();
            float invokeRate = captureTime / 360f;
            InvokeRepeating("capture", 0, invokeRate);
            capturing = true;
        }
        if (Input.GetKeyDown(KeyCode.V) )
        {
            sampleVertices();
        }
        if (Input.GetKeyDown(KeyCode.X) )
        {
            outputCorrespondence();
        }
    }
}
