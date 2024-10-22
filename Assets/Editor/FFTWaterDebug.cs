using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class FFTWaterDebug : EditorWindow
{
    private const int PREF_SIZE = 256;
    
    private OceanRenderer hook;
    
    [MenuItem("Tools/FFTWater Debug")]
    public static void ShowWindow()
    {
        var window = GetWindow<FFTWaterDebug>("FFTWater Debug");
        window.minSize = new Vector2(520, 540);
    }

    private void Awake()
    {
        hook = SceneView.FindFirstObjectByType<OceanRenderer>();
        if (hook == null)
        {
            Debug.LogError("WaveGenerator not found");
            return;
        }
    }

    private void OnInspectorUpdate()
    {
        if (EditorApplication.isPlaying && hook == null)
        {
            Awake();
        }
    }

    private void OnGUI()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.LabelField("Please enter play mode");
            return;
        }
        if (hook == null)
        {
            EditorGUILayout.LabelField("WaveGenerator not found");
            return;
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label(hook.GaussNoise, new GUIStyle {fixedWidth = PREF_SIZE, fixedHeight = PREF_SIZE});
        GUILayout.Label(hook.H0, new GUIStyle {fixedWidth = PREF_SIZE, fixedHeight = PREF_SIZE});
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label(hook.Displacement, new GUIStyle {fixedWidth = PREF_SIZE, fixedHeight = PREF_SIZE});
        GUILayout.Label(hook.Derivatives, new GUIStyle {fixedWidth = PREF_SIZE, fixedHeight = PREF_SIZE});
        GUILayout.EndHorizontal();
    }

    public void Update()
    {
        Repaint();
    }
}
