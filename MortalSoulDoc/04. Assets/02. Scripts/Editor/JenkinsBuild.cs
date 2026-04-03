using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class JenkinsBuild
{
    static string[] SCENES = FindEnabledEditorScenes();

    static void PerformBuild()
    {
        BuildPipeline.BuildPlayer(FindEnabledEditorScenes(), "Builds/Windows/MedievalSurvival.apk", BuildTarget.Android, BuildOptions.None);
    }
    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            EditorScenes.Add(scene.path);
        }
        return EditorScenes.ToArray();
    }
}