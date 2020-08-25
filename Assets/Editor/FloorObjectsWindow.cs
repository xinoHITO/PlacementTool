using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class FloorObjectsWindow : EditorWindow
{

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Tools/Floor objects")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow window = EditorWindow.GetWindow(typeof(FloorObjectsWindow));
        window.titleContent = new GUIContent("Floor objects");
    }

    void OnGUI()
    {
        GUILayout.Label("Raycast Settings", EditorStyles.boldLabel);
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(FloorObjectsData).ToString());
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        FloorObjectsData data = AssetDatabase.LoadAssetAtPath<FloorObjectsData>(path);
        List<string> layerNames = GetLayerNames();
        data.RaycastYOffset = Mathf.Max(data.RaycastYOffset, 0);
        data.RaycastYOffset = EditorGUILayout.FloatField(new GUIContent("Y Offset"), data.RaycastYOffset);
        data.RaycastMask.value = EditorGUILayout.MaskField(new GUIContent("Raycast mask"), data.RaycastMask, layerNames.ToArray());

        GUILayout.Label("Debug", EditorStyles.boldLabel);
        data.ShowRaycast = EditorGUILayout.Toggle(new GUIContent("Show raycast"), data.ShowRaycast);

        GUIStyle style = GUI.skin.button;
        style.fixedHeight = 70;

        Transform[] selectedTransforms = Selection.transforms;
        if (selectedTransforms.Length > 0)
        {
            if (GUILayout.Button("Floor selected objects", style))
            {
                FloorTransform(data, Selection.transforms);
            }
        }
        else
        {
            if (GUILayout.Button("Floor ALL objects in scene", style))
            {
                bool dialogResult = EditorUtility.DisplayDialog("Floor ALL objects in scene",
                    "Are you sure you want to place ALL objects in scene (including, cameras, lights and others)",
                    "Yes",
                    "No");


                if (dialogResult)
                {
                    List<Transform> transformsInScene = GetObjectsInScene();
                    FloorTransform(data, transformsInScene.ToArray());
                }
            }
        }

    }

    void OnSelectionChange()
    {
        Repaint();
    }

    private static void FloorTransform(FloorObjectsData data, Transform[] selectedTransforms)
    {
        foreach (var selectedTransform in selectedTransforms)
        {
            Collider[] colliders = selectedTransform.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            Vector3 originPos = selectedTransform.position + (Vector3.up * data.RaycastYOffset);

            RaycastHit hitInfo;
            bool didRaycastHit = Physics.Raycast(originPos, Vector3.down, out hitInfo, 1000, data.RaycastMask);
            if (didRaycastHit)
            {
                if (selectedTransform.position != hitInfo.point)
                {
                    Undo.RegisterCompleteObjectUndo(selectedTransform, "Floor object");
                    selectedTransform.position = hitInfo.point;
                }
            }

            foreach (var col in colliders)
            {
                col.enabled = true;
            }

            if (data.ShowRaycast)
            {
                if (didRaycastHit)
                {
                    float distance = originPos.y - hitInfo.point.y;
                    Debug.DrawRay(originPos, Vector3.down * distance, Color.green, 0.1f);
                }
                else
                {
                    Debug.DrawRay(originPos, Vector3.down * 1000, Color.red, 0.1f);
                }
            }
        }
    }

    private static List<string> GetLayerNames()
    {
        List<string> layerNames = new List<string>();

        string layerName;
        for (int i = 0; i < 8; i++)
        {
            layerName = LayerMask.LayerToName(i);
            if (layerName != "" && layerName != null)
            {
                layerNames.Add(layerName);
            }
        }

        int counter = 8;
        layerName = LayerMask.LayerToName(counter);
        while (layerName != "" && layerName != null)
        {
            layerNames.Add(layerName);
            counter++;
            layerName = LayerMask.LayerToName(counter);
        }

        return layerNames;
    }

    private static List<Transform> GetObjectsInScene()
    {
        List<Transform> transformsInScene = new List<Transform>();
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            Scene sc = EditorSceneManager.GetSceneAt(i);
            GameObject[] rootGos = sc.GetRootGameObjects();
            foreach (var go in rootGos)
            {
                transformsInScene.Add(go.transform);
            }
        }
        return transformsInScene;
    }

}