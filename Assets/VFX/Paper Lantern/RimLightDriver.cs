using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RimLightDriver : MonoBehaviour
{
    public Material[] Targets;
    private Vector4[] startD;

    private float dt;
    private float t;
    private float duration = 22.0f;
    private float delay = 5f;

    private string targetName = "Vector3_ec8a93c71bba4cc59f221c6b9183cd99";

    bool finished = false;
    private bool raiseLight = false;

    void Start()
    {
        startD = new Vector4[Targets.Length];
        for (int i = 0; i < Targets.Length; i++)
        {
            Targets[i].SetFloat("_Color", 0f);
            startD[i] = Targets[i].GetVector(targetName);
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < Targets.Length; i++)
        {
            Targets[i].SetVector(targetName, startD[i]);
        }
    }

    void Update()
    {
        if (finished) return;
        if (dt < delay)
        {
            dt += Time.deltaTime;
            return;
        }

        if (!raiseLight)
        {
            t += Time.deltaTime / duration;
            t = Mathf.Clamp01(t);

            for (int i = 0; i < Targets.Length; i++)
            {
                Targets[i].SetVector(targetName, startD[i] - new Vector4(0, t * 500f, 0f));

                Targets[i].SetFloat("_Color", t);
            }

            if (t >= 1f)
            {
                finished = true;
            }
        }
    }
}
