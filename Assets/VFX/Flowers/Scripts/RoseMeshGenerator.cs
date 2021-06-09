using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.AI;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoseMeshGenerator : MeshGeneratorBase
{
    public Material VineMaterial;

    public AnimationCurve GrowthCurve;

    public BranchGenerator Branch;

    public int id;

    #region internal_vars

    private List<BranchGenerator> branches;
    private List<Vector3> branchDir;
    
    private List<(float x, float z)> curveVals;

    private float uvVal = 0f;
    private float uv2Val = 0f;

    private float scale = 0.05f;
    private float widthScale = 1.0f;
    private float stemCurveScale = .04f;
    private float curveSpeed = 0.5f;
    private int numberOfCurves = 3;

    private float branchChance = 0.75f;

    private float curveOffset => id * (Mathf.PI * 2f / numberOfCurves);

    private float runtime = 0f;
    private float delay = 1f / 45f;

    private float xDist = 1f;

    private float life;
    private float extraBranchLife;

    private float layerCurveVal = 0.0f;
    private int currentLayers = 0;
    private int maxLayers = 300;

    private float prevCurveX;
    private float prevCurveZ;
#endregion

    void Start()
    {
        if (VineMaterial != null)
        {
            GenerateCurveImage();
        }
        life = delay * maxLayers;
        extraBranchLife = life / 2f;
        CreateMesh();
    }

    void GenerateCurveImage()
    {
        Texture2D tex = new Texture2D(1, 100);
        float p = 0f;
        for (int i = 0; i < 100; i++)
        {
            var intensity = GrowthCurve.Evaluate(p);
            tex.SetPixel(0, i, new Color(intensity, intensity, intensity, 1));
            p += 1f / 100f;
        }
        tex.Apply();

        VineMaterial.SetTexture("_Curve", tex);
    }

    void CreateMesh()
    {
        curveVals = new List<(float, float)>();

        branches = new List<BranchGenerator>();
        branchDir = new List<Vector3>();

        prevCurveX = 0;
        prevCurveZ = 0;

        var y = (maxLayers + 1) * scale / 2;
        var p = new Vector3(0, y, 0);
        //create base verts
        verts.Add(p);
        verts.Add(p);
        verts.Add(p);
        verts.Add(p);

        var c = new Color(0, 0, 0, 0);
        vertColors.Add(c);
        vertColors.Add(c);
        vertColors.Add(c);
        vertColors.Add(c);
        
        uv.Add(new Vector2(0, 0));
        uv.Add(new Vector2(0f, 0.5f));
        uv.Add(new Vector2(0f, 1f));
        uv.Add(new Vector2(0f, 0.5f));

        positions.Add(p);

        InsertStemLayer();

        UpdateMesh();
        

    }

    void Update()
    {
        runtime += Time.deltaTime;
        if (runtime >= delay)
        {
            runtime = 0f;
            InsertStemLayer();
            RollForBranch();
            MoveBranches();
            ReshapeMesh();
            UpdateMesh();
        }
    }

    void MoveBranches()
    {
        for (int i = 0; i < branches.Count; i++)
        {
            (float x, float z) = curveVals[i];
            var center = positions[i] + new Vector3(x, 0, z); 
            
            if (i < branches.Count && branches[i] != null)
            {
                var b = branches[i];
                var dir = branchDir[i];
                var lifeScale = GrowthCurve.Evaluate(Mathf.Clamp01((Time.timeSinceLevelLoad - layerInfo[i].w) / (life + extraBranchLife)));

                b.transform.position = center + dir.normalized * 0.75f * lifeScale * scale;
                var localScale = Vector3.one * lifeScale * 0.1f;
                b.transform.localScale = localScale;

            }
        }
        
        if (currentLayers >= maxLayers)
        {
            var targetBranch = branches[0];
            branches.Remove(targetBranch);
            branchDir.RemoveAt(0);
            if (targetBranch != null)
            {
                Destroy(targetBranch.gameObject);
            }
        }
    }

    void RollForBranch()
    {
        var roll = Random.Range(0f, 1f);
        var addBranch = !(roll > branchChance);
        
        if (!addBranch)
        {
            branchDir.Add(Vector2.zero);
            branches.Add(null);
            return;
        }

        (float x, float z) = curveVals[currentLayers - 1];

        var lastPos = positions[currentLayers - 1] + new Vector3(x, 0, z);

        var dir = new Vector3(lastPos.x, 0, lastPos.z);
        var directions = GetVectors(dir);
        var dirDelta = 0.05f;

        var tiltAmt = 0.25f;

        dir += Vector3.Lerp(directions.left, directions.right, Random.Range(tiltAmt, 1f-tiltAmt)) * Random.Range(0f, 1f);
        dir = dir.normalized;
        var branch = Instantiate(Branch, lastPos, Quaternion.identity, transform) as BranchGenerator;
        branch.StartGrowing(dir, Vector3.up);

        branchDir.Add(dir);
        branches.Add(branch);
    }

    void InsertStemLayer()
    {
        var vertStart = verts.Count - 4;
        
        var y = (maxLayers + 1) * scale / 2;

        layerCurveVal += scale * curveSpeed;

        var curveX = Mathf.Sin(layerCurveVal + curveOffset) * stemCurveScale;
        var curveZ = Mathf.Cos(layerCurveVal + curveOffset) * stemCurveScale;
        
        var centerPos = new Vector3(curveX, y, curveZ);
        
        var dir = new Vector3( -curveX, 1f, -curveZ).normalized;

        var left = new Vector3(-dir.y, dir.x).normalized;
        var right = new Vector3(dir.y, -dir.x).normalized;
        var fwd = new Vector3(0f, -dir.z, dir.y).normalized;
        var bwd = new Vector3(0f, dir.z, -dir.y).normalized;

        verts.Add(centerPos + right * scale);
        verts.Add(centerPos + fwd  * scale);
        verts.Add(centerPos + left * scale);
        verts.Add(centerPos + bwd  * scale);

        uvVal += scale;

        uv.Add(new Vector2(uvVal, 0));
        uv.Add(new Vector2(uvVal, 0.5f));
        uv.Add(new Vector2(uvVal, 1f));
        uv.Add(new Vector2(uvVal, .5f));

        uv2.Add(new Vector2(0, 0));
        uv2.Add(new Vector2(0, 0));
        uv2.Add(new Vector2(0, 0));
        uv2.Add(new Vector2(0, 0));

        var c = new Color(centerPos.x, centerPos.y, centerPos.z, 1);
        vertColors.Add(c);
        vertColors.Add(c);
        vertColors.Add(c);
        vertColors.Add(c);

        positions.Add(new Vector3(0, y, 0));
        curveVals.Add((curveX, curveZ));

        var newTris = new[]
        {
            vertStart + 1,
            vertStart,
            vertStart + 5,

            vertStart,
            vertStart + 4,
            vertStart + 5,

            vertStart + 2,
            vertStart + 1,
            vertStart + 5,

            vertStart + 6,
            vertStart + 2,
            vertStart + 5,

            vertStart + 2,
            vertStart + 7,
            vertStart + 3,

            vertStart + 2,
            vertStart + 6,
            vertStart + 7,

            vertStart,
            vertStart + 3,
            vertStart + 7,

            vertStart,
            vertStart + 7,
            vertStart + 4,
        };

        tris.AddRange( newTris );

        layerInfo.Add(new Vector4(dir.x, dir.y, dir.z, Time.timeSinceLevelLoad));

        currentLayers++;
    }

    void ReshapeMesh()
    {
        var haveToCull = currentLayers >= maxLayers;

        if (haveToCull)
        {
            verts.RemoveRange(0, 4);
            tris.RemoveRange(0, 24);
            layerInfo.RemoveRange(0, 1);
            positions.RemoveAt(0);
            curveVals.RemoveAt(0);
            uv.RemoveRange(0, 4);
            uv2.RemoveRange(0, 4);
            vertColors.RemoveRange(0, 4);
            currentLayers--;
        }

        for (int i = 0; i < currentLayers; i++)
        {
            var info = layerInfo[i];
            var index = i * 4;

            var triIndex = i * 24;

            var left = new Vector3(-info.y, info.x).normalized;
            var right = new Vector3(info.y, -info.x).normalized;
            var fwd = new Vector3(0f, -info.z, info.y).normalized;
            var bwd = new Vector3(0f, info.z, -info.y).normalized;

            var center = positions[i];
            var queryDir = center.normalized;


            center.y -= scale / 2;
            positions[i] = center;



            (float x, float z) = curveVals[i];

            center += new Vector3(x, 0, z);

            if (i > 0)
            {
                var lastVals = curveVals[i - 1];
                var lastPos = positions[i - 1] + new Vector3(lastVals.x, 0, lastVals.z);
                queryDir = (center - lastPos).normalized;
            }


//            (var left, var right, var fwd, var bwd) = GetVectors(queryDir);

            verts[index] = center + right  * widthScale * scale;
            verts[index + 1] = center + fwd * widthScale * scale;
            verts[index + 2] = center + left * widthScale * scale;
            verts[index + 3] = center + bwd * widthScale * scale;

            for (int j = 0; j < 4; j++)
            {
                var c =vertColors[index + j];
                c.g -= scale / 2;
                c.a = (float) i / maxLayers;
                vertColors[index + j] = c;
            }

            if (haveToCull)
            {
                for (int t = 0; t < 24; t++)
                {
                    tris[triIndex + t] -= 4;
                }
            }
        }
    }
    /*
     
//    void InsertStemLayer()
//    {
//        var vertStart = verts.Count - 4;
        
//        var y = (maxLayers + 1) * scale * yScale;

//        layerCurveVal += scale * curveSpeed * yScale;

//        var curveX = Mathf.Sin(layerCurveVal + curveOffset * yScale) * stemCurveScale;
//        var curveZ = Mathf.Cos(layerCurveVal + curveOffset * yScale) * stemCurveScale;
        
//        var centerPos = new Vector3(curveX, curveZ + yOff, 0);
//        var lastPos = positions[positions.Count - 1];
//        var dir = (centerPos - lastPos).normalized;

////        (var left, var right, var fwd, var bwd) = GetVectors(dir);
//        var left = new Vector3(-dir.y, dir.x).normalized;
//        var right = -left;
//        var fwd = new Vector3(0f, -dir.z, dir.y).normalized;
//        var bwd = -fwd;//new Vector3(0f, dir.z, -dir.y).normalized;

//        if (centerPos.x <= 0 && centerPos.y > 0 || centerPos.x < 0f && centerPos.y < 0)
//        {
//            verts.Add(centerPos + right * scale);
//            verts.Add(centerPos + fwd * scale);
//            verts.Add(centerPos + left * scale);
//            verts.Add(centerPos + bwd * scale);
//        }
//        else
//        {
//            verts.Add(centerPos + right * scale);
//            verts.Add(centerPos + bwd * scale);
//            verts.Add(centerPos + left * scale);
//            verts.Add(centerPos + fwd * scale);
//        }

//        uvVal += scale;

//        uv.Add(new Vector2(uvVal, 0));
//        uv.Add(new Vector2(uvVal, 0.5f));
//        uv.Add(new Vector2(uvVal, 1f));
//        uv.Add(new Vector2(uvVal, .5f));

//        uv2.Add(new Vector2(0, 0));
//        uv2.Add(new Vector2(0, 0));
//        uv2.Add(new Vector2(0, 0));
//        uv2.Add(new Vector2(0, 0));

//        var c = new Color(centerPos.x, centerPos.y, centerPos.z, 1);
//        vertColors.Add(c);
//        vertColors.Add(c);
//        vertColors.Add(c);
//        vertColors.Add(c);

//        positions.Add(centerPos);
//        curveVals.Add((curveX, curveZ));

//        var newTris = new[]
//        {
//            vertStart + 1,
//            vertStart,
//            vertStart + 5,

//            vertStart,
//            vertStart + 4,
//            vertStart + 5,

//            vertStart + 2,
//            vertStart + 1,
//            vertStart + 5,

//            vertStart + 6,
//            vertStart + 2,
//            vertStart + 5,

//            vertStart + 2,
//            vertStart + 7,
//            vertStart + 3,

//            vertStart + 2,
//            vertStart + 6,
//            vertStart + 7,

//            vertStart,
//            vertStart + 3,
//            vertStart + 7,

//            vertStart,
//            vertStart + 7,
//            vertStart + 4,
//        };

//        tris.AddRange( newTris );

//        layerInfo.Add(new Vector4(dir.x, dir.y, dir.z, Time.timeSinceLevelLoad));

//        currentLayers++;
//    }

//    void ReshapeMesh()
//    {
//        var haveToCull = currentLayers >= maxLayers;

//        if (haveToCull)
//        {
//            verts.RemoveRange(0, 4);
//            tris.RemoveRange(0, 24);
//            layerInfo.RemoveRange(0, 1);
//            positions.RemoveAt(0);
//            curveVals.RemoveAt(0);
//            uv.RemoveRange(0, 4);
//            uv2.RemoveRange(0, 4);
//            vertColors.RemoveRange(0, 4);
//            currentLayers--;
//        }

//        for (int i = 0; i < currentLayers; i++)
//        {
//            var info = layerInfo[i];
//            var index = i * 4;

//            var triIndex = i * 24;

//            var left = new Vector3(-info.y, info.x).normalized;
//            var right = new Vector3(info.y, -info.x).normalized;
//            var fwd = new Vector3(0f, -info.z, info.y).normalized;
//            var bwd = new Vector3(0f, info.z, -info.y).normalized;

//            var center = positions[i];
//            var queryDir = center.normalized;


//         //   center.y -= scale / 2;
//        //    positions[i] = center;



//            (float x, float z) = curveVals[i];

//            //center += new Vector3(x, z, 0);

//            if (i > 0)
//            {
//                var lastVals = curveVals[i - 1];
//                var lastPos = positions[i - 1] + new Vector3(lastVals.x, 0, lastVals.z);
//                queryDir = (center - lastPos).normalized;
//            }


////            (var left, var right, var fwd, var bwd) = GetVectors(queryDir);

//            //verts[index] = center + right  * widthScale * scale;
//            //verts[index + 1] = center + fwd * widthScale * scale;
//            //verts[index + 2] = center + left * widthScale * scale;
//            //verts[index + 3] = center + bwd * widthScale * scale;

//            for (int j = 0; j < 4; j++)
//            {
//                var c =vertColors[index + j];
//               // c.g -= scale / 2;
//                c.a = (float) i / maxLayers;
//                vertColors[index + j] = c;
//            }

//            if (haveToCull)
//            {
//                for (int t = 0; t < 24; t++)
//                {
//                    tris[triIndex + t] -= 4;
//                }
//            }
//        }
//    }
     */
}
