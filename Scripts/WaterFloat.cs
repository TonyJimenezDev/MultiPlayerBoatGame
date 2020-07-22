using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WaterFloat : MonoBehaviourPun
{
    [SerializeField] float AirDrag = 1;
    [SerializeField] float WaterDrag = 10;
    [SerializeField] Transform[] FloatPoints;
    //[SerializeField] bool AffectDirection = true;
    [SerializeField] bool attachtoSurface;
    protected Rigidbody playerBody;
    protected Waves waves;
    protected float waterLine;
    protected Vector3[] waterLinePoints;
    protected Vector3 centerOffSet;
    protected Vector3 smoothVectorRotation;
    protected Vector3 targetUp;

    public Vector3 Center
    {
        get { return transform.position + centerOffSet; }
    }

    private void Awake()
    {
        waves = FindObjectOfType<Waves>();
        playerBody = GetComponent<Rigidbody>();
        playerBody.useGravity = false;

        waterLinePoints = new Vector3[FloatPoints.Length];
        for (int i = 0; i < FloatPoints.Length; ++i)
        {
            waterLinePoints[i] = FloatPoints[i].position;
            centerOffSet = GetCenter(waterLinePoints) - transform.position;
        }
    }

    private void Update()
    {
        float newWaterLine = 0f;
        bool pointUnderwater = false;

        for(int i = 0; i < FloatPoints.Length; ++i)
        {
            waterLinePoints[i] = FloatPoints[i].position;
            waterLinePoints[i].y = waves.GetHeight(FloatPoints[i].position);
            newWaterLine += waterLinePoints[i].y / FloatPoints.Length;
            if(waterLinePoints[i].y > FloatPoints[i].position.y)
            {
                pointUnderwater = true;
            }
        }
        float waterLineDelta = newWaterLine - waterLine;
        waterLine = newWaterLine;

        Vector3 gravity = Physics.gravity;
        playerBody.drag = AirDrag;
        if (waterLine > Center.y)
        {
            playerBody.drag = WaterDrag;
            if(attachtoSurface)
            {
                // Sticks to waves 
                playerBody.position = new Vector3(playerBody.position.x, waterLine - centerOffSet.y, playerBody.position.z);
                
            }
            else
            {
                // pushes up with waves
                gravity = -Physics.gravity;
                transform.Translate(Vector3.up * waterLineDelta * 0.9f);
            }
        }
        playerBody.AddForce(gravity * Mathf.Clamp(Mathf.Abs(waterLine - Center.y), 0, 1));

        // Up vector
        targetUp = GetNormal(waterLinePoints);

        #region Keeps ship upright
        if (pointUnderwater)
        {
            //Sticks to the top of the water
            targetUp = Vector3.SmoothDamp(transform.up, targetUp, ref smoothVectorRotation, .2f);
            playerBody.rotation = Quaternion.FromToRotation(transform.up, targetUp) * playerBody.rotation;
        }
        #endregion
    }

    public Vector3 GetCenter(Vector3[] points)
    {
        Vector3 center = Vector3.zero;
        for (int i = 0; i < points.Length; ++i)
        {
            center += points[i] / points.Length;
        }            
        return center;
    }

    // Credit to https://www.ilikebigbits.com/2015_03_04_plane_from_points.html
    public Vector3 GetNormal(Vector3[] points)
    {
        
        if (points.Length < 3)
            return Vector3.up;

        Vector3 center = GetCenter(points);

        float xx = 0f, xy = 0f, xz = 0f, yy = 0f, yz = 0f, zz = 0f;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 r = points[i] - center;
            xx += r.x * r.x;
            xy += r.x * r.y;
            xz += r.x * r.z;
            yy += r.y * r.y;
            yz += r.y * r.z;
            zz += r.z * r.z;
        }

        float det_x = yy * zz - yz * yz;
        float det_y = xx * zz - xz * xz;
        float det_z = xx * yy - xy * xy;

        if (det_x > det_y && det_x > det_z)
            return new Vector3(det_x, xz * yz - xy * zz, xy * yz - xz * yy).normalized;
        if (det_y > det_z)
            return new Vector3(xz * yz - xy * zz, det_y, xy * xz - yz * xx).normalized;
        else
            return new Vector3(xy * yz - xz * yy, xy * xz - yz * xx, det_z).normalized;

    }

}
