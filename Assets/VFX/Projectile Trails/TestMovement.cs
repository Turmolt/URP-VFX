using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement : MonoBehaviour
{
    // Start is called before the first frame update

    Vector3 startPos;
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = startPos + new Vector3(Mathf.Sin(Time.time) * 2, Mathf.Cos(Time.time) * 2, 0f);
    }
}
