using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    private Vector3 startPosition;

    public float Speed = 3f;
    public float Radius = 4f;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = startPosition + new Vector3(Mathf.Sin(Time.time * Speed) * Radius, 0f, Mathf.Cos(Time.time * Speed) * Radius);

    }
}
