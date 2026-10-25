using UnityEngine;
using UnityEditor;

public class PrefabPlacer : EditorWindow
{
    public GameObject prefab;
    public LayerMask layer = ~0;
    public bool alignToNormal = true;

    [MenuItem("Tools/Prefab Click Placer")]
    static void Init()
    {
        GetWindow<PrefabPlacer>("Prefab Placer");
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        prefab = (GameObject)EditorGUILayout.ObjectField(
            "Prefab", prefab, typeof(GameObject), false);

        layer = LayerMaskField("Target Layers", layer);

        alignToNormal = EditorGUILayout.Toggle(
            "Align To Surface", alignToNormal);

        EditorGUILayout.HelpBox(
            "Click on GameObjects with Colliders in the Scene View to place prefabs.",
            MessageType.Info);
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (prefab == null)
            return;

        Event e = Event.current;

        // Prevent placing while navigating the Scene View
        if (e.alt || e.button != 0)
            return;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, layer))
        {
            Handles.color = Color.green;

            Handles.SphereHandleCap(
                0, hit.point, Quaternion.identity, 0.15f,
                EventType.Repaint);

            // Place prefab on left-click
            if (e.type == EventType.MouseDown)
            {
                PlacePrefab(hit);
                e.Use();
            }

            // Repaint Scene View for preview
            sceneView.Repaint();
        }
    }

    void PlacePrefab(RaycastHit hit)
    {
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        Undo.RegisterCreatedObjectUndo(obj, "Place Prefab");

        obj.transform.position = hit.point;

        if (alignToNormal)
        {
            obj.transform.rotation =
                Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
    }

    LayerMask LayerMaskField(string label, LayerMask selected)
    {
        var layers = UnityEditorInternal.InternalEditorUtility.layers;

        int mask = 0;

        for (int i = 0; i < layers.Length; i++)
        {
            int layerIndex = LayerMask.NameToLayer(layers[i]);

            if ((selected.value & (1 << layerIndex)) != 0)
                mask |= 1 << i;
        }

        mask = EditorGUILayout.MaskField(label, mask, layers);

        int finalMask = 0;

        for (int i = 0; i < layers.Length; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                int layerIndex = LayerMask.NameToLayer(layers[i]);
                finalMask |= 1 << layerIndex;
            }
        }

        selected.value = finalMask;
        return selected;
    }
}