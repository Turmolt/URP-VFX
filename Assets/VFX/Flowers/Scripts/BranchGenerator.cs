using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BranchGenerator : MeshGeneratorBase
{
    private Vector3 direction;

    private Material material;

    private float runtime = 0f;
    private float delay = 1f / 60f;


    private float scale = 0.1f;
    private float widthScale = 1.75f;

    private int currentLayers;
    private int targetLayerCount;
    private float layerCurveVal = 0.0f;
    private float curveOffset;

    private int minLayers = 5;
    private int maxLayers = 10;

    private float curveSpeed = 0.5f;

    private float stemCurveScale = 0.05f;

    private float uvVal = 0f;

    private bool growing = true;

    private Vector3 startDir;

    private float growScale = 0f;
    private float growDelta;

    private int layer = 1;

    private Vector3 vineDirection;

    public FlowerManager Flower;
    private GameObject f;
    public GameObject F => f;

    public void StartGrowing(Vector3 dir, Vector3 vineFwd)
    {
        vineDirection = vineFwd;    
        var rend = GetComponent<MeshRenderer>();
        material = new Material(rend.material);

        curveOffset = Random.Range(0f, 10f);

        targetLayerCount = Random.Range(minLayers, maxLayers);

        growDelta = 1f / targetLayerCount;

        direction = dir;
        startDir = dir;

        growing = true;
        
        (var l, var r, var f, var b) = GetVectors(direction);
        verts.Add(r * scale);
        verts.Add(f * scale);
        verts.Add(l * scale);
        verts.Add(b* scale);

        uv.Add(new Vector2(0, 0));
        uv.Add(new Vector2(0f, 0.5f));
        uv.Add(new Vector2(0f, 1f));
        uv.Add(new Vector2(0f, 0.5f));

        var c = Color.black;

        vertColors.Add(c);
        vertColors.Add(c);
        vertColors.Add(c);
        vertColors.Add(c);


        positions.Add(Vector3.zero);
    }


    void Update()
    {
        if (!growing) return;
        runtime += Time.deltaTime;
        if (runtime >= delay)
        {
            runtime = 0f;
            if (currentLayers < targetLayerCount)
            {
                InsertLayer();
                UpdateMesh();
            }
            else
            {
                growing = false;
                CreateFlower();
            }
        }
    }

    public void DestroySelf()
    {
        if (Flower.transform.parent != null)
        {
            Flower.transform.SetParent(null);
        }
        
        Destroy(gameObject, 1/30f);
    }

    void CreateFlower()
    {
        var highLayerCount = targetLayerCount > 3;
        var lastPos = positions[positions.Count - 1];
        var posBefore = positions[positions.Count - 2];
        var growthDir = highLayerCount ? (lastPos - posBefore).normalized : startDir.normalized;

        var v = GetVectors(growthDir);

        var rotation = Quaternion.LookRotation(v.fwd, growthDir);
        Flower = FlowerPool.Instance.PopFlower();//Instantiate(this.flower);
        f = Flower.gameObject;
        Flower.transform.parent = transform;
        Flower.transform.localScale = 0.01f * Vector3.one;
        Flower.transform.localPosition = lastPos * .95f;
        Flower.transform.rotation = rotation;
        Flower.gameObject.SetActive(true);
    }

    void InsertLayer()
    {
        var vertStart = verts.Count - 4;

        var lastPos = positions[positions.Count-1];

        layerCurveVal += scale * curveSpeed;

        var pos = lastPos + direction * scale;

        
        if (Vector3.Dot(vineDirection, direction) < 0.5f)
        {
            direction += vineDirection * 0.025f;
            //direction.y = Mathf.Clamp(direction.y,-.75f, .75f);
            //direction.x = Mathf.Clamp(direction.x,-.75f, .75f);
            direction = direction.normalized;
        }
        (var l, var r, var f, var b) = GetVectors(direction);

        verts.Add(pos + r * scale);
        verts.Add(pos + f * scale);
        verts.Add(pos + l * scale);
        verts.Add(pos + b * scale);

        uvVal += scale;

        uv.Add(new Vector2(uvVal, 0));
        uv.Add(new Vector2(uvVal, 0.5f));
        uv.Add(new Vector2(uvVal, 1f));
        uv.Add(new Vector2(uvVal, .5f));

        var c = new Color(pos.x, pos.y, pos.z, uvVal);

        vertColors.Add(c);
        vertColors.Add(c);
        vertColors.Add(c);
        vertColors.Add(c);

        material.SetFloat("_MaxUV", layer++);

        positions.Add(pos);

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

        layerInfo.Add(new Vector4(direction.x, direction.y, direction.z, Time.timeSinceLevelLoad));

        currentLayers++;
    }





}
