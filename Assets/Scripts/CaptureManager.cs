using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureManager : MonoBehaviour
{
    public int textureWidth = 1280;
    public int textureHeight = 720;

    public Camera cam = null;
    public GameObject targetObj = null;

    private Texture2D camTex = null;
    private RenderTexture captureRT = null;
    private bool isRecordRealCam = false;

    void Awake()
    {
        camTex = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
        cam.targetTexture = new RenderTexture(textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32);
        cam.backgroundColor = Color.clear;
    }

    void LateUpdate()
    {
        captureRT = cam.targetTexture;

        if (Input.GetKeyDown(KeyCode.C) && !isRecordRealCam)
        {
            rotateTargetObj();
            Debug.Log("start capture");
            isRecordRealCam = true;
            Debug.Log(captureRT);
            toTexture2D(captureRT, camTex);
            savePic(camTex, "/output/test.png");
        }
    }

    public void savePic(Texture2D tex, string fileName)
    {
        byte[] bytes;
        bytes = tex.EncodeToPNG();

        string path = Application.streamingAssetsPath + fileName;
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("save" + path);
        return;
    }

    public void rotateTargetObj()
    {
        targetObj.transform.Rotate(new Vector3(0, 90, 0));
    }

    Texture2D toTexture2D(RenderTexture rTex, Texture2D tex)
    {
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
