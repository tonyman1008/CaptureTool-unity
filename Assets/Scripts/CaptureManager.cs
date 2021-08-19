using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CaptureManager : MonoBehaviour
{
    public int textureWidth = 1280;
    public int textureHeight = 720;

    public Camera cam = null;
    public GameObject targetObj = null;

    private RenderTexture captureRT = null;

    private float rotateAngle = 0.0f;
    private bool capturing = false;
    private float captureTime = 10f;

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
        Debug.Log("save" + path);
        return;
    }

    public void createFolder(string folderName)
    {
        var folder = Directory.CreateDirectory(Application.streamingAssetsPath + "/output/"+ folderName); 
    }

    public void setTargetObjectRotation(Quaternion quaternion)
    {
        targetObj.transform.rotation = quaternion;
    }

    private void capture()
    {
        if (rotateAngle <= 360)
        {
            Debug.Log(rotateAngle);
            setTargetObjectRotation(Quaternion.Euler(0, rotateAngle, 0));
            string fileName = rotateAngle + ".png";
            SaveRenderTextureToFile(cam.targetTexture, fileName);
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

    // Update is called once per frame
    void Update()
    {
        captureRT = cam.targetTexture;

        if (Input.GetKeyDown(KeyCode.C) && !capturing)
        {
            Debug.Log(targetObj.name);
            Debug.Log("start capture");
            float invokeRate = captureTime / 360f;
            InvokeRepeating("capture", 0, invokeRate);
            capturing = true;
        }
    }
}
