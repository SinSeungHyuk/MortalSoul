using Core;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(MSButton), true)]
public class MSButtonEditor : ButtonEditor
{
    private SerializedProperty clickSfxKey;

    protected override void OnEnable()
    {
        base.OnEnable();
        clickSfxKey = serializedObject.FindProperty("clickSfxKey");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.PropertyField(clickSfxKey);
        serializedObject.ApplyModifiedProperties();
    }
}
