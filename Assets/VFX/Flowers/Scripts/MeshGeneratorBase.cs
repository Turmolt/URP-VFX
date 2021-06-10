using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGeneratorBase : MonoBehaviour
{
    private MeshFilter meshFilter;
    private Mesh mesh;

    protected List<int> tris;
    protected List<Vector3> verts;
    protected List<Color> vertColors;
    protected List<Vector2> uv;
    protected List<Vector2> uv2;
    protected List<Vector3> positions;
    protected List<Vector4> layerInfo; // XYZ = dir, Z = time born

    public void Awake()
    {
        if (tris != null && mesh != null) return;
        tris = new List<int>();
        verts = new List<Vector3>();
        vertColors = new List<Color>();
        uv = new List<Vector2>();
        uv2 = new List<Vector2>();
        layerInfo = new List<Vector4>();
        positions = new List<Vector3>();
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    public void UpdateMesh()
    {
        if (mesh == null) Awake();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uv.ToArray();
        if(uv2.Count == verts.Count)
            mesh.uv2 = uv.ToArray();
        if (vertColors.Count > 0)
        {
            mesh.colors = vertColors.ToArray();
        }
        mesh.RecalculateNormals();
    }

    public static (Vector3 left, Vector3 right, Vector3 fwd, Vector3 bwd) GetVectors(Vector3 heading)
    {
        heading = heading.normalized;
        var perp = new Vector3(-heading.y, heading.x, 0).normalized;
        var right = Vector3.Cross(heading, perp);
        var left = -right;
        var fwd = Vector3.Cross(right, heading);
        var bwd = -fwd;
        return (left, right, fwd, bwd);
    }
}
