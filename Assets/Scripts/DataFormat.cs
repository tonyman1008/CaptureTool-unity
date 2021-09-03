using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace DataFormat
{
    public class SamplePointsData
    {
        public Vector3 worldPos;
        public int index;
        public SamplePointsData(Vector3 _worldPos, int _index)
        {
            worldPos = _worldPos;
            index = _index;
        }
    }
    
    //json format
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
}
