using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Correspondence : MonoBehaviour
{
    public Transform camTransform = null;
    public bool canBeRaycasted = false;
    public string defaultTagName = "CorrespondingPoint";
    public int index = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void checkPointCanBeRaycasted()
    {
        Vector3 dir = (transform.position - camTransform.position);
        float dis = Vector3.Distance(transform.position, camTransform.position);

        RaycastHit hitInfo;
        if (Physics.Raycast(camTransform.position, dir, out hitInfo))
        {
            //Debug.Log("Hit");
            //Debug.Log("hit object tag: " + hitInfo.collider.tag);

            //Debug.Log("hit point: " + hitInfo.point);
            //Debug.Log("object position: " + transform.position);

            if (hitInfo.collider.tag == defaultTagName)
            {
                canBeRaycasted = true;
                gameObject.GetComponent<Renderer>().material.color = Color.red;
                //Debug.DrawLine(camTransform.position, hitInfo.point, Color.red, Mathf.Infinity);
            }
            else
            {
                canBeRaycasted = false;
                gameObject.GetComponent<Renderer>().material.color = Color.green;
                //Debug.DrawLine(camTransform.position, hitInfo.point, Color.white, Mathf.Infinity);
            }
        }
        else
        {
            //Debug.Log("Not Hit");

            canBeRaycasted = false;

            //gameObject.GetComponent<Renderer>().material.color = Color.white;
            //Debug.DrawLine(camTransform.position, transform.position, Color.white, Mathf.Infinity);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }
}
