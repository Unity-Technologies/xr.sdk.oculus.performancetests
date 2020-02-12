#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class RefreshTestObjects
{
    // This is a helper class that will reimport test files when changes have been made to the scene list. 
    // Doing so will repopulate the TestRunner window with the new test cases.
    //
    // It's meant to simplify the manual usage of these tests. If it causes problems with CI, just remove it.
    static RefreshTestObjects()
    {
        EditorBuildSettings.sceneListChanged += ReimportTestFiles;
    }

    static void ReimportTestFiles()
    {
        Debug.Log("Scene list changed. Re-importing test scripts and re-populating test cases.");
        string[] testGuids = AssetDatabase.FindAssets("t:script", new string[] {"Assets/Tests"});
        foreach (string guid in testGuids)
        {
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
    }
}
#endif
