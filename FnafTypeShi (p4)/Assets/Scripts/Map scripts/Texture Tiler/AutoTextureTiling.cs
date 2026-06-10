using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class AutoTextureTiling : MonoBehaviour
{
    [Header("Tiling Settings")]
    [Tooltip("How many texture tiles fit in 1 Unity unit of scale.")]
    public float tilesPerUnit = 1f;

    [Tooltip("Which world axis maps to texture U (horizontal).")]
    public Axis uAxis = Axis.X;

    [Tooltip("Which world axis maps to texture V (vertical).")]
    public Axis vAxis = Axis.Z;

    [Header("Material Settings")]
    [Tooltip("Leave empty to use the first material on the Renderer.")]
    public string texturePropertyName = "_BaseMap";

    [Tooltip("Use a MaterialPropertyBlock instead of modifying the shared material.")]
    public bool usePropertyBlock = true;

    public enum Axis { X, Y, Z }

    private Renderer _renderer;
    private MaterialPropertyBlock _block;
    private Vector3 _lastScale;

    private void OnEnable()
    {
        _renderer = GetComponent<Renderer>();
        _block = new MaterialPropertyBlock();
        ApplyTiling();
    }

    private void Update()
    {
        if (transform.lossyScale != _lastScale)
            ApplyTiling();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Make sure everything is initialized before applying
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_block == null) _block = new MaterialPropertyBlock();
        ApplyTiling();
    }
#endif

    public void ApplyTiling()
    {
        // Guard against any nulls
        if (_renderer == null || _block == null) return;

        Vector3 scale = transform.lossyScale;
        _lastScale = scale;

        float uTile = Mathf.Max(GetAxisValue(scale, uAxis) * tilesPerUnit, 0.001f);
        float vTile = Mathf.Max(GetAxisValue(scale, vAxis) * tilesPerUnit, 0.001f);

        var tiling = new Vector2(uTile, vTile);

        if (usePropertyBlock)
        {
            _renderer.GetPropertyBlock(_block);
            _block.SetVector(texturePropertyName + "_ST", new Vector4(tiling.x, tiling.y, 0f, 0f));
            _renderer.SetPropertyBlock(_block);
        }
        else
        {
            _renderer.material.SetTextureScale(texturePropertyName, tiling);
        }
    }

    private static float GetAxisValue(Vector3 v, Axis axis) => axis switch
    {
        Axis.X => v.x,
        Axis.Y => v.y,
        Axis.Z => v.z,
        _ => v.x
    };
}