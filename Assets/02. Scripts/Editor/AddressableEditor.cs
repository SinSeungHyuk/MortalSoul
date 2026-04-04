using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AddressableEditor
{
    [MenuItem("Assets/Addressables/AddResource")]
    private static void RegisterToAddressables()
    {
        foreach (var obj in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath)) continue;

            Register(assetPath);
        }
    }

    [MenuItem("Assets/Addressables/AddResourceAndBuild")]
    private static void RegisterAndBuild()
    {
        foreach (var obj in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath)) continue;

            Register(assetPath);
        }

        AddressableAssetSettings.BuildPlayerContent();
    }

    private static void Register(string assetPath)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[Addressable] AddressableAssetSettings�� ã�� �� �����ϴ�.");
            return;
        }

        string folderName = Path.GetFileName(Path.GetDirectoryName(assetPath));
        string assetName = Path.GetFileNameWithoutExtension(assetPath);

        AddressableAssetGroup group = settings.FindGroup(folderName);
        if (group == null)
        {
            group = settings.CreateGroup(folderName, false, false, true, null);
            Debug.Log($"[Addressable] �׷� ����: {folderName}");
        }

        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        var entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = assetName;

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        AssetDatabase.SaveAssets();

        Debug.Log($"[Addressable] '{assetName}' �� �׷� '{folderName}' ��� �Ϸ�");
    }
}
