using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlowerGenerator : MeshGeneratorBase
{

    private int npetals = 8;

    public void CreatePetals(int n, float petalWidth)
    {
        npetals = n;
        var index = 0;
        for (int id = 0; id < npetals; id++)
        {
            float p = id / (float) npetals;
            float x = Mathf.Cos(p * (Mathf.PI * 2f));
            float z = Mathf.Sin(p * (Mathf.PI * 2f));

            var dir = new Vector3(x, 0, z);
            var right = new Vector3(-z, 0, x);

            BuildPetal(5, dir, right, ref index, id, petalWidth);
        }

        UpdateMesh();
    }

    void BuildPetal(int segments, Vector3 dir, Vector3 right, ref int index, int id, float petalWidth)
    {

        var zFightFix = Vector3.up * 0.025f;
        var pos = transform.position - dir * 0.05f;

        var left = -right;

        var dUv = 1.0f / segments;
        float uvy = 0.0f;

        var dy = 0.15f/segments * Vector3.up;
        var cdy = Vector3.zero;

        var scale = petalWidth;
        var dscale = (1f - scale) / segments;

        verts.Add(pos + right * scale / 2f);
        verts.Add(pos + zFightFix / 2f);
        verts.Add(pos + left * scale / 2f + zFightFix);

        var dirColor = new Color(dir.x, dir.y, dir.z, id / (float)npetals);

        vertColors.Add(dirColor);
        vertColors.Add(dirColor);
        vertColors.Add(dirColor);

        pos += dir * (1.0f / segments);

        uv.Add(new Vector2(0f, uvy));
        uv.Add(new Vector2(0.5f, uvy));
        uv.Add(new Vector2(1f, uvy));

        uvy += dUv;

        for (int i = 0; i < segments; i++)
        {
            scale += dscale;
            verts.Add(pos + right * scale / 2f + cdy);
            verts.Add(pos + zFightFix / 2f);
            verts.Add(pos + left * scale / 2f + zFightFix + cdy);

            cdy += dy;

            vertColors.Add(dirColor);
            vertColors.Add(dirColor);
            vertColors.Add(dirColor);

            pos += dir * (1.0f / segments);

            var newTris = new[]
            {
                index, index + 3, index + 1,
                index + 3, index + 4, index + 1,

                index + 1, index + 4, index + 2,
                index + 4, index + 5, index + 2,
            };

            index += 3;

            tris.AddRange(newTris);

            uv.Add(new Vector2(0f, uvy));
            uv.Add(new Vector2(0.5f, uvy));
            uv.Add(new Vector2(1f, uvy));

            uvy += dUv;

        }

        index += 3;

    }
}
