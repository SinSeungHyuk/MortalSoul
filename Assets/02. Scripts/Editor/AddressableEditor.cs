using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
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

    private static void Register(string _assetPath)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[Addressable] AddressableAssetSettings�� ã�� �� �����ϴ�.");
            return;
        }

        string folderName = Path.GetFileName(Path.GetDirectoryName(_assetPath));
        string assetName = Path.GetFileNameWithoutExtension(_assetPath);

        AddressableAssetGroup group = settings.FindGroup(folderName);
        if (group == null)
        {
            group = settings.CreateGroup(folderName, false, false, true, null,
                typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));

            var bundledSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (bundledSchema != null)
            {
                bundledSchema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
                bundledSchema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
                bundledSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
                bundledSchema.IncludeAddressInCatalog = true;
                bundledSchema.IncludeGUIDInCatalog = true;
                bundledSchema.IncludeLabelsInCatalog = true;
            }

            Debug.Log($"[Addressable] 그룹 생성: {folderName}");
        }

        string guid = AssetDatabase.AssetPathToGUID(_assetPath);
        var entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = assetName;

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        AssetDatabase.SaveAssets();
    }
}
