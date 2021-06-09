using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotator : MonoBehaviour
{

    public Vector3 RotationValue;

    void Update()
    {
        transform.localEulerAngles += RotationValue * Time.deltaTime;
    }
}
