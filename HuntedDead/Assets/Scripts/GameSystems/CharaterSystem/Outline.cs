using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class Outline : MonoBehaviour
{
    public enum Mode { OutlineAll, OutlineVisible, OutlineHidden, OutlineAndSilhouette, SilhouetteOnly }

    public Mode OutlineMode = Mode.OutlineAll;
    public Color OutlineColor = Color.white;
    public float OutlineWidth = 2f;

    private Renderer[] renderers;
    private Material outlineMaterial;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        // Создаём простой материал для контура
        Shader outlineShader = Shader.Find("Hidden/Internal-Colored");
        if (outlineShader == null)
        {
            Debug.LogError("Outline shader не найден!");
            return;
        }
        outlineMaterial = new Material(outlineShader);
        outlineMaterial.SetColor("_Color", OutlineColor);
    }

    void OnRenderObject()
    {
        if (outlineMaterial == null) return;

        outlineMaterial.SetPass(0);

        foreach (var rend in renderers)
        {
            for (int i = 0; i < rend.materials.Length; i++)
            {
                Graphics.DrawMesh(rend.GetComponent<MeshFilter>().sharedMesh, rend.transform.localToWorldMatrix, outlineMaterial, gameObject.layer);
            }
        }
    }
}
