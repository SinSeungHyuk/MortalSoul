using Core;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(MSToggleButton), true)]
public class MSToggleButtonEditor : SelectableEditor
{
    private SerializedProperty activeObject;
    private SerializedProperty inactiveObject;

    protected override void OnEnable()
    {
        base.OnEnable();
        activeObject = serializedObject.FindProperty("activeObject");
        inactiveObject = serializedObject.FindProperty("inactiveObject");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.PropertyField(activeObject);
        EditorGUILayout.PropertyField(inactiveObject);
        serializedObject.ApplyModifiedProperties();
    }
}
