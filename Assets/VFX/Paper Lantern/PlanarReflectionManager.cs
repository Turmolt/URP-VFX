using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlanarReflectionManager : MonoBehaviour
{
    Camera m_ReflectionCamera;
    private Camera mainCam;

    public GameObject ReflectionPlane;

    // Start is called before the first frame update
    void Start()
    {
        var camGo = new GameObject("Reflection Camera");
        m_ReflectionCamera = camGo.AddComponent<Camera>();
        m_ReflectionCamera.enabled = false;

        mainCam = Camera.main;


        RenderPipelineManager.endCameraRendering += EndCamRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= EndCamRendering;
    }

    private void EndCamRendering(ScriptableRenderContext arg1, Camera arg2)
    {
        RenderReflection();
    }

    void RenderReflection()
    {
        m_ReflectionCamera.CopyFrom(mainCam);

        Vector3 camDirWS = mainCam.transform.forward;
        Vector3 camUpWS = mainCam.transform.up;
        Vector3 camPosWS = mainCam.transform.position;

        // transform vectors to floor space
        Vector3 camDirPS = ReflectionPlane.transform.InverseTransformDirection(camDirWS);
        Vector3 camUpPS = ReflectionPlane.transform.InverseTransformDirection(camUpWS);
        Vector3 camPosPS = ReflectionPlane.transform.InverseTransformPoint(camPosWS);

        //mirror
        camDirPS.y *= -1f;
        camUpPS.y *= -1f;
        camPosPS.y *= -1f;

        //transform back to world space
        camDirWS = ReflectionPlane.transform.TransformDirection(camDirPS);
        camUpWS = ReflectionPlane.transform.TransformDirection(camUpPS);
        camPosWS = ReflectionPlane.transform.TransformPoint(camPosPS);

        //set cam pos and rot
        m_ReflectionCamera.transform.position = camPosWS;
        m_ReflectionCamera.transform.LookAt(camPosWS + camDirWS, camUpWS);
    }
}
