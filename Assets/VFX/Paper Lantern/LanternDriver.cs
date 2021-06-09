using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternDriver : MonoBehaviour
{
    public Renderer LanternRenderer;
    public Renderer FireRenderer;

    public GameObject TailObject;

    private Vector3 startPos;
    private float startRot;
    private float offset;
    private float offsetX;
    private float offsetZ;
    private float speed;
    private float rotSpeed;

    private float swayX;
    private float swayZ;
    private float swaySpeedX;
    private float swaySpeedZ;
    private float riseSpeed;

    private float riseVal;

    private Vector3 pos;
    private Material tailMat;
    private Material tailMat2;
    private Renderer rend;

    private Vector3 sway;

    void Start()
    {
        if (Random.Range(0f, 1f) <= 0.15f)
        {
            gameObject.SetActive(false);
            return;
        }


        var mat = new Material(LanternRenderer.materials[0]);
        var mat2 = new Material(LanternRenderer.materials[1]);
        var f = Random.Range(-10f, 10f);
        LanternRenderer.materials[0] = mat;
        LanternRenderer.materials[1] = mat2;
        LanternRenderer.materials[0].SetFloat("Random", f);

        if (Random.Range(0f, 1f) <= 0.75f)
        {
            LanternRenderer.materials[0].SetInt("_HasSymbol", 1);
        }

        var c = Random.Range(.85f, 1f);
        var tint = new Color(c,c,c,1);

        LanternRenderer.materials[0].SetColor("_Tint", tint);
        LanternRenderer.materials[1].SetColor("_Tint", tint);

        LanternRenderer.materials[1].SetFloat("Random", f);
        var fmat = new Material(FireRenderer.material);
        FireRenderer.material = fmat;
        fmat.SetFloat("_Random", Random.Range(0, .5f));
        offset = Random.Range(-20f, 20f);
        offsetX = Random.Range(-20f, 20f);
        offsetZ = Random.Range(-20f, 20f);
        speed = Random.Range(0.15f, 0.35f);
        rotSpeed = Random.Range(-0.1f, 0.1f);
        startPos = transform.position;
        startRot = Random.Range(-180f, 180f);
        transform.eulerAngles = new Vector3(0, startRot, 0);

        swaySpeedX = Random.Range(-0.25f, 0.25f);
        swaySpeedZ = Random.Range(-0.25f, 0.25f);
        swayX = Random.Range(0.05f, .2f);
        swayZ = Random.Range(0.05f, .20f);
        riseSpeed = Random.Range(0.05f, 0.075f);

        if (Random.Range(0f, 1f) < 0.05f)
        {
            var tail = Instantiate(TailObject, transform);
            rend = tail.GetComponent<Renderer>();
            tailMat = new Material(rend.materials[0]);
            tailMat2 = new Material(rend.materials[1]);
            rend.materials[0] = tailMat;
            rend.materials[1] = tailMat2;
            tailMat.SetFloat("_Offset", Random.Range(-20f, 20f));
        }

        pos = transform.position;
    }

    void Update()
    {
        riseVal += Time.deltaTime * riseSpeed;
        var yAng = startRot + Mathf.Cos(Time.time * rotSpeed + offset) * 90f;
        transform.position = startPos + new Vector3(Mathf.Cos(Time.time * swaySpeedX + offsetX) * swayX, Mathf.Sin(Time.time * speed + offset) * 0.05f + riseVal, Mathf.Sin(Time.time * swaySpeedZ + offsetZ) * swayZ);
        transform.eulerAngles = new Vector3(0, yAng, 0);
        if (tailMat != null)
        {

            var dir = (pos - transform.position);

            dir = Quaternion.AngleAxis(-yAng, Vector3.up) * dir;

            dir = new Vector3(dir.x, -dir.z,0f);

            sway = Vector3.MoveTowards(sway, dir.normalized, 100f * Time.deltaTime);

            rend.materials[0].SetVector("_Sway", sway);
            rend.materials[1].SetVector("_Sway", sway);
        }

        pos = transform.position;
    }
}
