using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContourSampler : MonoBehaviour
{
    public TextAsset jsonFile;
    public Camera cam;
    public GameObject targetObj = null;
    public GameObject testPoints = null;

    public List<Vector2> contour2DPoints = new List<Vector2>();
    public int imgHeight = 1000;
    public int imgWidth = 1000;
    public string objectMeshTagName = "ObjectMesh";
    public string defaultTagName = "CorrespondingPoint";

    // Start is called before the first frame update
    void Start()
    {
        //ContourPointsArray contourPointsInJson = JsonUtility.FromJson<ContourPointsArray>(jsonFile.text);
        //foreach (ContourPoint contourPoint in contourPointsInJson.contourPoints)
        //{
        //    // convert to unity coordinate system
        //    Vector2 point = new Vector2(contourPoint.x, imgHeight - contourPoint.y);
        //    contour2DPoints.Add(point);
        //}
        //sampleContourPoints();
    }

    private void sampleContourPoints()
    {
        Debug.Log("sampel contour points count " + contour2DPoints.Count);
        for (int i = 0; i < contour2DPoints.Count; i++)
        {
            // raycast from camera 2d space
            RaycastHit hitInfo;
            Ray ray = cam.ScreenPointToRay(contour2DPoints[i]);

            if (Physics.Raycast(ray, out hitInfo))
            {

                if (hitInfo.collider.tag == objectMeshTagName)
                {
                    Transform objectHit = hitInfo.transform;
                    Vector3 hit3DPoint = hitInfo.point;

                    // Instantiate sample point
                    GameObject createPoint = Instantiate(testPoints, hit3DPoint, targetObj.transform.rotation);
                    createPoint.transform.parent = targetObj.transform;
                    createPoint.tag = testPoints.GetComponent<Correspondence>().defaultTagName;

                }
                else
                {
                    Debug.Log("hit other things");
                }
            }
            else
            {
                Debug.Log("not hit");
            }
        }
    }
}

[System.Serializable]
public class ContourPointsArray
{
    public ContourPoint[] contourPoints;
}

[System.Serializable]
public class ContourPoint
{
    public int x;
    public int y;
}
