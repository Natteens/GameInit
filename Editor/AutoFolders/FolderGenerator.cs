using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameInit.Editor.AutoFolders
{
    public static class FolderGenerator
    {
        const string k_AssetsPath = "Assets";

        public static (int folders, int asmdefs) Generate(FolderNode virtualRoot, string assemblyPrefix = "")
        {
            int folders = 0, asmdefs = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var child in virtualRoot.Children)
                    Walk(child, k_AssetsPath, assemblyPrefix, ref folders, ref asmdefs);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
            return (folders, asmdefs);
        }

        static void Walk(FolderNode node, string parentDb, string prefix,
                         ref int folders, ref int asmdefs)
        {
            string dbPath = parentDb + "/" + node.Name;
            if (!AssetDatabase.IsValidFolder(dbPath))
            {
                AssetDatabase.CreateFolder(parentDb, node.Name);
                folders++;
            }
            if (node.GenerateAsmdef)
            {
                WriteAsmdef(dbPath, node.Asmdef, prefix);
                asmdefs++;
            }
            foreach (var child in node.Children)
                Walk(child, dbPath, prefix, ref folders, ref asmdefs);
        }

        static void WriteAsmdef(string dbFolder, AsmdefConfig cfg, string prefix)
        {
            string suffix = string.IsNullOrWhiteSpace(cfg.AssemblyName)
                ? Path.GetFileName(dbFolder) : cfg.AssemblyName;
            string fullName = string.IsNullOrWhiteSpace(prefix) ? suffix : prefix + "." + suffix;

            string absFolder = Path.Combine(Application.dataPath, dbFolder.Substring("Assets/".Length));
            string absFile   = Path.Combine(absFolder, fullName + ".asmdef");
            if (File.Exists(absFile)) return;

            string include = cfg.Platform == AsmdefPlatform.EditorOnly  ? "\"Editor\"" : "";
            string exclude = cfg.Platform == AsmdefPlatform.RuntimeOnly ? "\"Editor\"" : "";

            var resolvedRefs = new List<string>();
            foreach (var r in cfg.References)
            {
                if (string.IsNullOrWhiteSpace(r)) continue;
                bool alreadyFull = string.IsNullOrWhiteSpace(prefix) || r.Contains(".");
                resolvedRefs.Add(alreadyFull ? r : prefix + "." + r);
            }

            string refsBlock = resolvedRefs.Count > 0
                ? "[\n" + string.Join(",\n", resolvedRefs.ConvertAll(r => "    \"" + r + "\"")) + "\n  ]"
                : "[]";

            string json =
                "{\n" +
                $"  \"name\": \"{fullName}\",\n" +
                "  \"rootNamespace\": \"\",\n" +
                $"  \"references\": {refsBlock},\n" +
                $"  \"includePlatforms\": [{include}],\n" +
                $"  \"excludePlatforms\": [{exclude}],\n" +
                $"  \"allowUnsafeCode\": {cfg.AllowUnsafeCode.ToString().ToLower()},\n" +
                "  \"overrideReferences\": false,\n" +
                "  \"precompiledReferences\": [],\n" +
                $"  \"autoReferenced\": {cfg.AutoReferenced.ToString().ToLower()},\n" +
                "  \"defineConstraints\": [],\n" +
                "  \"versionDefines\": [],\n" +
                "  \"noEngineReferences\": false\n" +
                "}";

            File.WriteAllText(absFile, json);
        }
    }
}