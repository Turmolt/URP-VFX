using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickPointDist : MonoBehaviour
{
    public GameObject Target;
    public GameObject P1;
    public GameObject P2;
    public GameObject P3;


    void Update()
    {
        var d1 = Vector3.Distance(Target.transform.position, P1.transform.position);
        var d2 = Vector3.Distance(Target.transform.position, P2.transform.position);
        var d3 = Vector3.Distance(Target.transform.position, P3.transform.position);
        var total = d1 + d2 + d3;
        var d1t = d1 / total;
        var d2t = d2 / total;
        var d3t = d3 / total;
        Debug.Log($"{.1f+(1.0f-d1t)*.7f}, {.1f + (1.0f - d2t) * .7f}, {.1f + (1.0f-d3t) * .7f}");
    }
}
