using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(LineRenderer))]
public class CircleLineRendering : MonoBehaviour
{
    #region ======== Constants ==============
    const float MIN_MOVEDIST = 0.01f;
    #endregion

    [Header("===== Settings =====")]
    [SerializeField]
    [Range(4, 500)]
    [Tooltip("How smooth the circle will be. The higher the resolution, the more points in between the 4 corners of circle there will be.")]
    ///<Summary>How smooth the circle will be. The higher the resolution, the more points in between the 4 corners of circle there will be.</Summary>
    int m_Resolution = 4;

    [SerializeField]
    [Range(0.0001f, 100)]
    [Tooltip("The radius of the circle.")]
    ///<Summary>The radius of the circle.</Summary>
    float m_Radius = 5f;



    LineRenderer lineRenderer = default;
    ///<Summary>An array of vector2 which will be used to set positions in the linerenderer based on the linerenderer's current transform relation to its parent (if any).</Summary>
    Vector2[] circlePoints = default;



#if UNITY_EDITOR
    bool valueRunTimeChanged = false;

    private void OnValidate()
    {
        if (m_Resolution % 4 != 0)
        {
            Debug.LogWarning($"The resolution integer must be a multiple of 4!", this);
            return;
        }

        if (EditorApplication.isPlaying)
        {
            valueRunTimeChanged = true;
        }
    }
#endif

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        GenerateCirclePoints();
        ApplyCirclePoints();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (valueRunTimeChanged)
        {
            //If setting values has changed during runtime,
            GenerateCirclePoints();
            ApplyCirclePoints();
            return;
        }
#endif

        if (transform.hasChanged)
        {
            ApplyCirclePoints();
        }
    }

    ///<Summary>Generates the points in circlePoints with the current m_Resolution value.</Summary>
    void GenerateCirclePoints()
    {
        lineRenderer.positionCount = m_Resolution + 1;
        circlePoints = new Vector2[lineRenderer.positionCount];

        #region ------- Ploting Extreme & Inbetween Points ------------
        //So imagine a circle inside of a box where the circle is perfectly fitted inside of the square. The points where the circle touches the square is what im going to call extreme points and there is always 4 in any circle.
        //Now im trying to find out the number of points inbetween the extremes (because the extreme points can be predicted easily)
        int numOfPointsInbetwExtremes = (m_Resolution - 4) / 4;
        float angleBetwPointsInbetwExtremes = 90f / (numOfPointsInbetwExtremes + 1);
        float theta = 0;

        //Plotting extreme points + the inbetween points in a clockwise direction starting from north direction
        #region _____ More Description _____
        //when extremePointIndex  = 0. extremePointIndex is in the east direction, 
        //when extremePointIndex  = 1. extremePointIndex is in the north direction, 
        //when extremePointIndex  = 2. extremePointIndex is in the west direction, 
        //when extremePointIndex  = 3. extremePointIndex is in the south direction, 
        #endregion
        //extremePointIndex
        for (int extremePointIndex = 0, currRendererPoint = 0; extremePointIndex < 4; extremePointIndex++)
        {
            #region ________ Plotting Extreme Point _______
            //determine if the current extremePointIndex is odd or even numbered
            bool isEven = extremePointIndex % 2 == 0;

            Vector2 extremePoint = Vector2.zero;

            //Determine whether or not the offset from the center is positive or negative
            //If extremePointIndex is in the south or west direction, the offsetFromCenterRadius is negative radius
            float offsetFromCenterRadius = extremePointIndex >= 2 ? -m_Radius : m_Radius;

            //If current extremePointIndex is even numbered, 
            if (isEven)
            {
                //The offset from the center of the circle should only be on the Y axis
                extremePoint.x += offsetFromCenterRadius;
            }
            else
            {
                //The offset from the center of the circle should only be on the X axis
                extremePoint.y += offsetFromCenterRadius;
            }


            circlePoints[currRendererPoint] = extremePoint;
            currRendererPoint++;
            #endregion


            #region _________ Plotting Inbetween Points _____________
            for (int i = 0; i < numOfPointsInbetwExtremes; i++)
            {
                //Make sure inbetween point starts from center
                Vector2 inBetweenPoint = Vector2.zero;
                theta += angleBetwPointsInbetwExtremes;


                //Calculate the ratio
                float ratio = Mathf.Cos(theta * Mathf.Deg2Rad);
                float xValueOffSet = m_Radius * ratio;

                ratio = Mathf.Sin(theta * Mathf.Deg2Rad);
                float yValueOffSet = m_Radius * ratio;


                inBetweenPoint.x += xValueOffSet;
                inBetweenPoint.y += yValueOffSet;

                circlePoints[currRendererPoint] = inBetweenPoint;
                currRendererPoint++;
            }
            #endregion

            //Add angle once more to make theta start as a angle divisible by 90 degrees
            theta += angleBetwPointsInbetwExtremes;
        }
        #endregion


        //Join up the last line to the beginning.
        circlePoints[circlePoints.Length - 1] = circlePoints[0];
    }



    ///<Summary>Updates the circle points' positions by the inputed vector</Summary>
    void ApplyCirclePoints()
    {
        for (int i = 0; i < circlePoints.Length; i++)
        {
            Vector3 point = circlePoints[i];
            point = transform.TransformPoint(point);
            lineRenderer.SetPosition(i, point);
        }
    }

}
