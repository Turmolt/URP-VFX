using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerManager : MonoBehaviour
{
    public FlowerGenerator Base;
    public FlowerGenerator Outer;
    public FlowerGenerator Inner;
    public FlowerGenerator Center;
    public MeshRenderer BasePetals;
    public MeshRenderer OuterPetals;
    public MeshRenderer InnerPetals;
    public MeshRenderer CenterPetals;

    public int NumPetals = 8;
    public float PetalWidth = 0.1f;

    private Material baseMaterial;
    private Material outerMaterial;
    private Material innerMaterial;
    private Material centerMaterial;

    private float scale;
    private float scaleTime = 0.15f;
    private float maxScale = 0.6f;
    private float maxScaleDelta = 0.15f;

    private float bloom = 0f;

    private float bloomTime;
    private float baseBloomTime = 0.55f;
    private float maxBloomDelta = 0.15f;

    public AnimationCurve BloomCurve;

    public bool IsStatic;
    public Vector3 StaticStartRot;
    public Vector3 StaticStartPos;

    private void Start()
    {
        if (IsStatic)
            Initialize();
    }

    public void Initialize()
    {
        Base?.CreatePetals(NumPetals + Random.Range(-3, 3), PetalWidth + Random.Range(-0.2f, 0.2f));
        Outer?.CreatePetals(NumPetals + Random.Range(-3, 3), PetalWidth + Random.Range(-0.2f, 0.2f));
        Inner?.CreatePetals(NumPetals + Random.Range(-3, 3), PetalWidth + Random.Range(-0.2f, 0.2f));
        Center?.CreatePetals(NumPetals + Random.Range(-3, 3), PetalWidth + Random.Range(-0.2f, 0.2f));

        bloom = IsStatic ? 1f : 0f;

        if (BasePetals != null)
        {
            baseMaterial = new Material(BasePetals.material);
            BasePetals.material = baseMaterial;
        }

        if (OuterPetals != null)
        {
            outerMaterial = new Material(OuterPetals.material);
            OuterPetals.material = outerMaterial;
        }

        if (InnerPetals != null)
        {
            innerMaterial = new Material(InnerPetals.material);
            InnerPetals.material = innerMaterial;
        }

        if (CenterPetals != null)
        {
            centerMaterial = new Material(CenterPetals.material);
            CenterPetals.material = centerMaterial;
        }

        maxScale += Random.Range(-maxScaleDelta, maxScaleDelta);

        var bloomDelta = Random.Range(-maxBloomDelta, maxBloomDelta);
        bloomTime = baseBloomTime + bloomDelta;

        if (!IsStatic) transform.localScale = Vector3.zero;
        else
        {
            transform.position = StaticStartPos;
            transform.localEulerAngles = StaticStartRot;
        }
        
        SetMaterialBloom();
    }

    public void ResetFlower()
    {
        scale = 0f;
        bloom = 0f;
        transform.localScale = Vector3.zero;
        SetMaterialBloom();
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        if (!IsStatic && scale < scaleTime)
        {
            scale += Time.deltaTime;
            transform.localScale = Vector3.one * maxScale * Mathf.Clamp01(scale / scaleTime);
            return;
        }

        if (!IsStatic &&bloom < bloomTime)
        {
            bloom += Time.deltaTime;
            SetMaterialBloom();
        }

        if(!IsStatic) transform.localScale *= 1.000075f;
    }

    void SetMaterialBloom()
    {
        var bloomPercent = BloomCurve.Evaluate(Mathf.Clamp01(bloom / bloomTime));
        baseMaterial?.SetFloat("_Bloom", bloomPercent);
        outerMaterial?.SetFloat("_Bloom", bloomPercent);
        innerMaterial?.SetFloat("_Bloom", bloomPercent);
        centerMaterial?.SetFloat("_Bloom", bloomPercent);
    }
}
