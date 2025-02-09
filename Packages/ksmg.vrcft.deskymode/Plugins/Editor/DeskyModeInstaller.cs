#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;

[InitializeOnLoad]
public class DeskyModeInstaller
{
    static string deskyModeInstallLocation = "Assets/Gimmicks/DeskyMode/";
    static string[] deskyModeInstalledScriptFiles = { string.Concat(deskyModeInstallLocation, "Editor/DeskyModeSetup.cs"),
                                              string.Concat(deskyModeInstallLocation, "Editor/DeskyModeEditor.cs") };

    static string thisFilePath = AssetDatabase.FindAssets("DeskyModeInstaller", new[] { "Packages", "Assets" }).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
    static string thisPackageDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFilePath), "../../"));
    static string[] deskyModeSourceScriptFiles = { string.Concat(thisPackageDirectory, "Editor/DeskyModeSetup.cs.no"),
                                              string.Concat(thisPackageDirectory, "Editor/DeskyModeEditor.cs.no") };

    // used for the initial copy
    static DeskyModeInstaller()
    {
        // thanks FIK stub
        // see if DeskyMode scripts were setup
        bool deskyModeScriptsInLocation = System.IO.File.Exists(deskyModeInstalledScriptFiles[0]) ||
            System.IO.File.Exists(deskyModeInstalledScriptFiles[1]);

        // see if DeskyMode exists in current context
        bool deskyModePresent = AppDomain.CurrentDomain.GetAssemblies()
            .Any(x => x.GetTypes().Any(y => y.FullName == "DeskyMode.DeskyModeSetup"));

        if (deskyModePresent)
        {
            if (!deskyModeScriptsInLocation)
            {
                EditorUtility.DisplayDialog("DeskyMode in unexpected location", "Please remove old DeskyMode scripts", "Cancel");
            }
            // skip doing anything if stuff in Assets folder seems right
            return;
        }
        // copied Final IK assembly check from VRLab's FinalIKStubInstaller
        // not foolproof but if you're trying to fool this script... why? 
        bool fikPresent = AppDomain.CurrentDomain.GetAssemblies()
            .Any(x => x.GetTypes().Any(y => y.FullName == "RootMotion.FinalIK.AimIK"));
        if (!fikPresent)
        {

            EditorUtility.DisplayDialog("Missing Final IK", "Please install Final IK or Final IK Stub", "Ok");
        }
        else
        {
            // copy over the scripts
            // copied again from FIK Stub <3
            CopyFiles(deskyModeSourceScriptFiles, deskyModeInstallLocation);
            AssetDatabase.Refresh();
        }
    }

    private static void CopyFiles(string[] files, string finalPath)
    {
        foreach (var file in files)
        {
            if (file.EndsWith(".cs.no"))
            {
                string partialPath = file.Substring(file.IndexOf("Editor", StringComparison.Ordinal));
                string directory = Path.GetDirectoryName(partialPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(finalPath + directory);
                }

                File.Copy(file, finalPath + partialPath.Replace(".cs.no", ".cs"));
                File.Copy(file.Replace(".cs.no", ".cs.meta.no"), finalPath + partialPath.Replace(".cs.no", ".cs.meta"));
            }
        }

        // check if Final IK was installed or the stub
        // can check by looking for the editor scripts
        // copied Final IK assembly check from VRLab's FinalIKStubInstaller
        bool realFIKPresent = AppDomain.CurrentDomain.GetAssemblies()
            .Any(x => x.GetTypes().Any(y => y.FullName == "RootMotion.FinalIK.IKInspector"));
        if (realFIKPresent)
        {
            // "uncomment" the #define ActualFinalIK in DeskyModeSetup.cs
            StreamReader reader = new StreamReader(deskyModeInstalledScriptFiles[0]);
            string fileText = reader.ReadToEnd();
            reader.Close();
            //File.Delete(deskyModeInstalledScriptFiles[0]);
            StreamWriter writer = new StreamWriter(deskyModeInstalledScriptFiles[0], false);
            writer.WriteLine("#define ActualFinalIK\n");
            writer.WriteLine(fileText);
            writer.Close();
            //AssetDatabase.ImportAsset(deskyModeInstalledScriptFiles[0]);
        }
    }

    private static void RemoveFiles(string[] files)
    {
        foreach (var file in files)
        {
            if (file.EndsWith(".cs") && System.IO.File.Exists(file))
            {
                File.Delete(file);
                File.Delete(file.Replace(".cs", ".cs.meta"));
            }
        }
    }

    [MenuItem("Tools/DeskyMode/Refresh Scripts")]
    static void ForceRefresh()
    {
        // clear out the existing DeskyMode scripts in Assets
        RemoveFiles(deskyModeInstalledScriptFiles);

        // copy them back in
        CopyFiles(deskyModeSourceScriptFiles, deskyModeInstallLocation);
        AssetDatabase.Refresh();
    }
}

#endif
