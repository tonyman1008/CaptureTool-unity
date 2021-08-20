using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSampler : MonoBehaviour
{
    public Transform target = null;
    public Camera cam = null;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 screenPos = cam.WorldToScreenPoint(target.position);
        //Debug.Log("target is " + screenPos.x + " pixels from the left");
    }
}
