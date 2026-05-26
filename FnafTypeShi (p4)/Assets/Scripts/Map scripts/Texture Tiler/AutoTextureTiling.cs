using UnityEngine;

/// <summary>
/// Automatically tiles a texture based on the object's world scale,
/// so the texture density stays consistent no matter how the object is stretched.
/// 
/// SETUP:
///   1. Attach this script to any GameObject with a Renderer (MeshRenderer, etc.)
///   2. Assign a Material that uses a shader supporting _MainTex tiling (e.g. Standard, URP/Lit)
///   3. Choose which axes drive the tiling (default: X → U, Z → V, good for floors/terrain)
///   4. Adjust 'tilesPerUnit' to control how zoomed in/out the texture appears
/// </summary>
[ExecuteAlways] // Also runs in the Editor so you see changes live
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
    public string texturePropertyName = "_BaseMap"; // URP default; use "_MainTex" for Built-in/HDRP

    [Tooltip("Use a MaterialPropertyBlock instead of modifying the shared material.\n" +
             "Recommended: keeps materials unaffected at the asset level.")]
    public bool usePropertyBlock = true;

    // ── Enums ─────────────────────────────────────────────────────────────────

    public enum Axis { X, Y, Z }

    // ── Private ───────────────────────────────────────────────────────────────

    private Renderer _renderer;
    private MaterialPropertyBlock _block;
    private Vector3 _lastScale;

    // ── Unity Messages ────────────────────────────────────────────────────────

    private void OnEnable()
    {
        _renderer = GetComponent<Renderer>();
        _block    = new MaterialPropertyBlock();
        ApplyTiling();
    }

    private void Update()
    {
        // Only recalculate when scale actually changes (cheap check)
        if (transform.lossyScale != _lastScale)
            ApplyTiling();
    }

#if UNITY_EDITOR
    // Respond to transform changes made in the Inspector while in Edit Mode
    private void OnValidate() => ApplyTiling();
#endif

    // ── Core Logic ────────────────────────────────────────────────────────────

    public void ApplyTiling()
    {
        if (_renderer == null) return;

        Vector3 scale = transform.lossyScale; // world scale (accounts for parent scaling)
        _lastScale    = scale;

        float uTile = GetAxisValue(scale, uAxis) * tilesPerUnit;
        float vTile = GetAxisValue(scale, vAxis) * tilesPerUnit;

        // Clamp to avoid zero/negative tiling (causes visual artifacts)
        uTile = Mathf.Max(uTile, 0.001f);
        vTile = Mathf.Max(vTile, 0.001f);

        var tiling = new Vector2(uTile, vTile);

        if (usePropertyBlock)
        {
            _renderer.GetPropertyBlock(_block);
            _block.SetVector(texturePropertyName + "_ST", new Vector4(tiling.x, tiling.y, 0f, 0f));
            _renderer.SetPropertyBlock(_block);
        }
        else
        {
            // Modifies the instance material (creates a copy at runtime)
            _renderer.material.SetTextureScale(texturePropertyName, tiling);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static float GetAxisValue(Vector3 v, Axis axis) => axis switch
    {
        Axis.X => v.x,
        Axis.Y => v.y,
        Axis.Z => v.z,
        _      => v.x
    };
}