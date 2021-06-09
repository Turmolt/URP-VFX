using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PotionDriver : MonoBehaviour
{
    public Material PotionMaterial;

    private Vector3 lastPosition;
    private Vector3 lastRotation;

    private Vector3 velocity;
    private Vector3 angularVelocity;

    public float MaxWobble = 0.003f;
    public float WobbleSpeed = 0.25f;
    public float Recovery = 2f;
    private float speed = 1f;

    private float wobbleAmountX;
    private float wobbleAmountZ;
    private float wobbleAmountToAddX;
    private float wobbleAmountToAddZ;
    private float pulse;
    private float time = 0.5f;

    private float rotX;
    private float rotZ;

    private float noiseTime = 0f;
    private float disturbance;
    float parabola;

    private Vector3 mouseStart;
    private Vector3 startRotation;

    private Vector3 startPos;
    private Vector3 mouseStartPos;
    private Vector3 targetRot;

    public Transform CameraParent;
    private Vector3 camStartRotation;
    private Vector3 camMouseStart;
    private Vector3 camTargetRot;

    void Start()
    {
        transform.eulerAngles = new Vector3(0f, 220f, 0f);
        camStartRotation = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0f, Time.deltaTime * Recovery);
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0f, Time.deltaTime * Recovery);

        pulse = 2 * Mathf.PI * WobbleSpeed;

        wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
        wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);

        var radX = (Mathf.Deg2Rad * lastRotation.x);
        var radZ = (Mathf.Deg2Rad * lastRotation.z);

        PotionMaterial.SetFloat("_Emission", 1.0f + Mathf.Clamp01(disturbance) * .75f);
        PotionMaterial.SetFloat("_Disturbance", disturbance);
        PotionMaterial.SetFloat("_Foam", Mathf.Lerp(0.1f, 0.2f, Mathf.Clamp01(parabola)));
        PotionMaterial.SetFloat("_Delay", Mathf.Clamp01(parabola));
        PotionMaterial.SetFloat("_RotationX", radX + wobbleAmountX);
        PotionMaterial.SetFloat("_RotationZ", radZ + wobbleAmountZ);
        PotionMaterial.SetFloat("_NoiseTime", noiseTime);

        //        renderer.material.SetFloat("_WobbleX", wobbleAmountX);
        //      renderer.material.SetFloat("_WobbleZ", wobbleAmountZ);

        //        velocity = (lastPosition - transform.position) / Time.deltaTime;

        // angularVelocity = transform.rotation.eulerAngles - lastRotation;

        if (Input.GetMouseButtonDown(0))
        {
            mouseStart = Input.mousePosition;
            startRotation = transform.eulerAngles;
        }

        if (Input.GetMouseButton(0))
        {
            UpdateRotation();
        }

        if (Input.GetMouseButtonDown(1))
        {
            mouseStartPos = Input.mousePosition;
            startPos = transform.position;
        }

        if (Input.GetMouseButton(1))
        {
            UpdatePosition();
        }

        velocity = (lastPosition - transform.position) / Time.deltaTime;

        transform.eulerAngles = targetRot;
        angularVelocity = targetRot - lastRotation;

        wobbleAmountToAddX += Mathf.Clamp((velocity.z * 0.5f + (angularVelocity.z * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);
        wobbleAmountToAddZ += Mathf.Clamp((velocity.x * 0.5f + (angularVelocity.x * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);

        lastPosition = transform.position;

        var dVel = Mathf.Clamp01(velocity.magnitude / 100f);
        var dAng = Mathf.Clamp01(angularVelocity.magnitude / 150f);

        disturbance = Mathf.MoveTowards(disturbance, disturbance + Mathf.Max(dVel, dAng), 10f * Time.deltaTime);
        parabola = Mathf.MoveTowards(parabola, parabola + Mathf.Max(dVel, dAng), 10f * Time.deltaTime);

        disturbance = Mathf.Lerp(disturbance, 0f, Time.deltaTime * Recovery * 0.25f);
        parabola = Mathf.Lerp(parabola, 0f, Time.deltaTime * Recovery * 2.5f);

        noiseTime += Mathf.Abs(Time.deltaTime * disturbance * speed);

        lastPosition = transform.position;
        lastRotation = targetRot;

        if (Input.GetMouseButtonDown(2))
        {
            camStartRotation = camTargetRot;
            camMouseStart = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            UpdateCamera();
        }
    }

    void UpdateRotation()
    {
        var deltaMouse = Input.mousePosition - mouseStart;
        rotZ = deltaMouse.x;
        targetRot = startRotation + new Vector3(0f, 0f, rotZ);
    }

    void UpdatePosition()
    {
        var deltaMouse = Input.mousePosition - mouseStartPos;
        deltaMouse *= -0.005f;
        var newPos = startPos + new Vector3(deltaMouse.x, 0f, deltaMouse.y);
        transform.position = newPos;
    }

    void UpdateCamera()
    {
        
        var deltaMouse = Input.mousePosition - camMouseStart;
        deltaMouse *= 0.1f;
        camTargetRot = camStartRotation + new Vector3(deltaMouse.y, deltaMouse.x, 0f);
        camTargetRot.x = Mathf.Clamp(camTargetRot.x, -90f, 90f);
        CameraParent.eulerAngles = camTargetRot;
    }
}
