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
    private List<Vector3> branchPos;
    
    private List<(float x, float z)> curveVals;

    private float uvVal = 0f;
    private float uv2Val = 0f;

    private float scale = 0.05f;
    private float widthScale = 1.0f;
    private float stemCurveScale = .04f;
    private float stemCurveScaleCirc = 1f;
    private float curveSpeed = 0.5f;
    private int numberOfCurves = 3;

    private float branchChance = 0.3333f;

    private float curveDelta => (Mathf.PI * 2f / numberOfCurves);
    private float curveOffset => id * curveDelta;

    private float yScale = 0.25f;
    private float runtime = 0f;
    private float delay = 1f / 45f;

    private float xDist = 1f;

    private float life;
    private float extraBranchLife;

    private float layerCurveVal = 0.0f;
    private int currentLayers = 0;
    private int maxLayers = 250;

    private float prevCurveX;
    private float prevCurveZ;

    private bool isVertical = false;

    private float smallCurveDistance = 0.01f;
    private float smallCurveOffset = 0f;
    private float smallCurveVal;

    public int SmallID;
#endregion

    void Start()
    {
        if (VineMaterial != null)
        {
            GenerateCurveImage();
        }
        life = delay * maxLayers + 5f;
        extraBranchLife = -1.75f;
        if(isVertical) CreateMeshVertical();
        else CreateMeshCirc();
    }

    void GenerateCurveImage()
    {
        var n = 20;
        Texture2D tex = new Texture2D(1, n);
        float p = 0f;
        for (int i = 0; i < n; i++)
        {
            var intensity = GrowthCurve.Evaluate(p);
            tex.SetPixel(0, i, new Color(intensity, intensity, intensity, 1));
            p += 1f / n;
        }
        tex.Apply();

        VineMaterial.SetTexture("_Curve", tex);
    }

    void CreateMeshVertical()
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

        InsertStemLayerVertical();

        UpdateMesh();
        

    }

    void CreateMeshCirc()
    {
        curveVals = new List<(float, float)>();

        branches = new List<BranchGenerator>();
        branchDir = new List<Vector3>();

        prevCurveX = 0;
        prevCurveZ = 0;

        var curveX = Mathf.Sin(layerCurveVal + curveOffset * yScale * (2 * 1f / yScale)) * stemCurveScaleCirc;
        var curveZ = Mathf.Cos(layerCurveVal + curveOffset * yScale * (2 * 1f / yScale)) * stemCurveScaleCirc;
        var p = new Vector3(curveX, curveZ, 0);
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

        InsertStemLayerVertical();

        UpdateMesh();


    }

    void Update()
    {
        runtime += Time.deltaTime;
        if (runtime >= delay)
        {
            runtime = 0f;
            if (isVertical)
            {
                InsertStemLayerVertical();
                RollForBranchVertical();
                MoveBranchesVertical();
                ReshapeMeshVertical();
            }
            else
            {
                InsertStemLayerCirc();
                RollForBranchCirc();
                MoveBranchesCirc();
                ReshapeMeshCirc();
            }

            UpdateMesh();
        }
    }

    void MoveBranchesVertical()
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
                Debug.Log(Branch.Flower.name);
                FlowerPool.Instance.PushFlower(Branch.Flower);
                targetBranch.DestroySelf();
            }
        }
    }

    void RollForBranchVertical()
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

    void RollForBranchCirc()
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

        var pos = positions[currentLayers - 1];
        var lastPos = positions[currentLayers - 2];

        var dir = (pos - lastPos).normalized;
        var directions = GetVectors(dir);

        var tiltAmt = 0.25f;

        var fb = Vector3.Lerp(directions.fwd, directions.bwd, Random.Range(0f, 1f));
        var lr = Vector3.Lerp(directions.left, directions.right, Random.Range(0f, 1f));
        var overall = Vector3.Lerp(fb, lr, Random.Range(0f, 1f)).normalized;

//        dir = overall;
//        dir += Vector3.Lerp(directions.left, directions.right, Random.Range(tiltAmt, 1f - tiltAmt)) * Random.Range(0f, 1f);
  //      dir = dir.normalized;
        var branch = Instantiate(Branch, pos, Quaternion.identity, transform) as BranchGenerator;
        branch.StartGrowing(overall, dir);

        branchDir.Add(dir);
        branches.Add(branch);
    }

    void InsertStemLayerVertical()
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

    void ReshapeMeshVertical()
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

    void MoveBranchesCirc()
    {
        for (int i = 0; i < branches.Count; i++)
        {
            var center = positions[i];

            if (i < branches.Count && branches[i] != null)
            {
                var b = branches[i];
//                var lifeScale = GrowthCurve.Evaluate(Mathf.Clamp01((Time.timeSinceLevelLoad - layerInfo[i].w) / (life + extraBranchLife)));
                var lifeScale = GrowthCurve.Evaluate(1f-Mathf.Clamp01(i / (float) maxLayers));

                //b.transform.position = center + dir.normalized * 0.75f * lifeScale * scale;
                var localScale = Vector3.one * lifeScale * 0.1f;
                b.transform.localScale = localScale;

            }
        }

        if (currentLayers >= maxLayers)
        {
            var targetBranch = branches[0];
            branches.Remove(targetBranch);
            branchDir.RemoveAt(0);
            if (targetBranch != null && targetBranch.Flower !=null)
            {
                FlowerPool.Instance.PushFlower(targetBranch.Flower);
                targetBranch.DestroySelf();
            }
        }
    }

    void InsertStemLayerCirc()
    {
        var vertStart = verts.Count - 4;

        var y = (maxLayers + 1) * scale * yScale;

        layerCurveVal += scale * curveSpeed* yScale;

        var curveX = Mathf.Sin(layerCurveVal + curveOffset * yScale * (2*1f/yScale)) * stemCurveScaleCirc;
        var curveZ = Mathf.Cos(layerCurveVal + curveOffset * yScale * (2 * 1f / yScale)) * stemCurveScaleCirc;

        var centerPos = new Vector3(curveX, curveZ, 0);
        var lastPos = positions[positions.Count - 1];

        var smallCurveX = Mathf.Sin(smallCurveVal + smallCurveOffset * 2f + (SmallID * curveDelta)) * smallCurveDistance;
        var smallCurveZ = Mathf.Cos(smallCurveVal + smallCurveOffset * 2f + (SmallID * curveDelta)) * smallCurveDistance;

        smallCurveVal += scale * 2f;


        positions.Add(centerPos);

        var dir = (centerPos - lastPos).normalized;

        (var left, var right, var fwd, var bwd) = GetVectors(dir);

        var smallOffset = left * smallCurveX + fwd * smallCurveZ;

        centerPos += smallOffset;

        verts.Add(centerPos + right * scale / 4f);
        verts.Add(centerPos + fwd * scale / 4f);
        verts.Add(centerPos + left * scale / 4f);
        verts.Add(centerPos + bwd * scale / 4f);

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

        tris.AddRange(newTris);

        layerInfo.Add(new Vector4(dir.x, dir.y, dir.z, Time.timeSinceLevelLoad));

        currentLayers++;
    }

    void ReshapeMeshCirc()
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


            //   center.y -= scale / 2;
            //    positions[i] = center;



            (float x, float z) = curveVals[i];

            //center += new Vector3(x, z, 0);

            if (i > 0)
            {
                var lastVals = curveVals[i - 1];
                var lastPos = positions[i - 1] + new Vector3(lastVals.x, 0, lastVals.z);
                queryDir = (center - lastPos).normalized;
            }


            //            (var left, var right, var fwd, var bwd) = GetVectors(queryDir);

            //verts[index] = center + right  * widthScale * scale;
            //verts[index + 1] = center + fwd * widthScale * scale;
            //verts[index + 2] = center + left * widthScale * scale;
            //verts[index + 3] = center + bwd * widthScale * scale;

            for (int j = 0; j < 4; j++)
            {
                var c = vertColors[index + j];
                // c.g -= scale / 2;
                c.a = (float)i / maxLayers;
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

}
