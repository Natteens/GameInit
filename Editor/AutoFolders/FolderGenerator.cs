using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameInit.Editor.AutoFolders
{
    public static class FolderGenerator
    {
        const string k_AssetsPath = "Assets";

        // Pending asmdef writes survive domain reload via SessionState
        const string k_SessionKey = "GameInit_PendingAsmdefs";

        // ── Public entry point ───────────────────────────────────────────
        // Returns folder count immediately; asmdef count is reported via onComplete
        // after the post-refresh write phase.
        public static int GenerateFolders(FolderNode virtualRoot, string assemblyPrefix,
            Action<int> onAsmdefsWritten = null)
        {
            if (virtualRoot == null) return 0;

            // Phase 1 — create directories
            int folderCount = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var child in virtualRoot.Children)
                    WalkFolders(child, k_AssetsPath, ref folderCount);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            // Collect asmdef descriptors before refresh (while node graph is intact)
            var pending = new List<PendingAsmdef>();
            foreach (var child in virtualRoot.Children)
                CollectAsmdefs(child, k_AssetsPath, assemblyPrefix, pending);

            if (pending.Count == 0)
            {
                AssetDatabase.Refresh();
                onAsmdefsWritten?.Invoke(0);
                return folderCount;
            }

            // Serialize pending list into SessionState so it survives the
            // domain reload that AssetDatabase.Refresh() may trigger.
            SessionState.SetString(k_SessionKey,
                JsonUtility.ToJson(new PendingAsmdefList { Items = pending }));

            // Phase 2 — write asmdefs after Unity has processed the new folders
            AssetDatabase.Refresh();
            EditorApplication.delayCall += () => WriteStoredAsmdefs(onAsmdefsWritten);

            return folderCount;
        }

        // Called by the delay / domain-reload hook
        [InitializeOnLoadMethod]
        static void RegisterPostReloadHook()
        {
            // If a pending list survived a domain reload, complete phase 2.
            string json = SessionState.GetString(k_SessionKey, null);
            if (!string.IsNullOrEmpty(json))
                EditorApplication.delayCall += () => WriteStoredAsmdefs(null);
        }

        static void WriteStoredAsmdefs(Action<int> onComplete)
        {
            string json = SessionState.GetString(k_SessionKey, null);
            if (string.IsNullOrEmpty(json)) return;

            SessionState.EraseString(k_SessionKey);

            PendingAsmdefList list;
            try { list = JsonUtility.FromJson<PendingAsmdefList>(json); }
            catch { list = null; }

            if (list?.Items == null || list.Items.Count == 0)
            {
                onComplete?.Invoke(0);
                return;
            }

            int written = 0;
            foreach (var item in list.Items)
            {
                if (WriteAsmdef(item))
                    written++;
            }

            AssetDatabase.Refresh();
            onComplete?.Invoke(written);
        }

        // ── Folder walk (phase 1) ────────────────────────────────────────
        static void WalkFolders(FolderNode node, string parentDb, ref int folders)
        {
            string dbPath = parentDb + "/" + node.Name;
            if (!AssetDatabase.IsValidFolder(dbPath))
            {
                AssetDatabase.CreateFolder(parentDb, node.Name);
                folders++;
            }
            foreach (var child in node.Children)
                WalkFolders(child, dbPath, ref folders);
        }

        // ── Asmdef collection (between phase 1 and phase 2) ─────────────
        static void CollectAsmdefs(FolderNode node, string parentDb,
            string prefix, List<PendingAsmdef> pending)
        {
            string dbPath = parentDb + "/" + node.Name;
            if (node.GenerateAsmdef)
            {
                var p = BuildPendingAsmdef(dbPath, node.Asmdef, prefix);
                if (p != null) pending.Add(p);
            }
            foreach (var child in node.Children)
                CollectAsmdefs(child, dbPath, prefix, pending);
        }

        static PendingAsmdef BuildPendingAsmdef(string dbFolder, AsmdefConfig cfg, string prefix)
        {
            string suffix   = string.IsNullOrWhiteSpace(cfg.AssemblyName)
                              ? Path.GetFileName(dbFolder) : cfg.AssemblyName;
            string fullName = BuildFullName(suffix, prefix);

            // Resolve references — stored as suffixes, written as full names
            var resolvedRefs = new List<string>();
            foreach (var r in cfg.References)
            {
                if (string.IsNullOrWhiteSpace(r)) continue;
                // If caller somehow stored a full name already (contains prefix+dot), keep it.
                // Otherwise treat as suffix and qualify.
                string resolved = r.Contains(".") ? r : BuildFullName(r, prefix);
                resolvedRefs.Add(resolved);
            }

            return new PendingAsmdef
            {
                DbFolder      = dbFolder,
                FullName      = fullName,
                Platform      = (int)cfg.Platform,
                AutoReferenced  = cfg.AutoReferenced,
                AllowUnsafeCode = cfg.AllowUnsafeCode,
                References    = resolvedRefs
            };
        }

        // ── Asmdef writer (phase 2) ──────────────────────────────────────
        static bool WriteAsmdef(PendingAsmdef p)
        {
            string relative = p.DbFolder.StartsWith("Assets/") || p.DbFolder.StartsWith("Assets\\")
                              ? p.DbFolder.Substring(7) : p.DbFolder;
            string absFolder = Path.Combine(Application.dataPath, relative);
            string absFile   = Path.Combine(absFolder, p.FullName + ".asmdef");

            if (File.Exists(absFile)) return false;
            if (!Directory.Exists(absFolder))
            {
                Debug.LogWarning($"[GameInit] Folder not found when writing asmdef: {absFolder}");
                return false;
            }

            AsmdefPlatform platform = (AsmdefPlatform)p.Platform;
            string include = platform == AsmdefPlatform.EditorOnly  ? "\"Editor\"" : "";
            string exclude = platform == AsmdefPlatform.RuntimeOnly ? "\"Editor\"" : "";

            string refsBlock = p.References.Count > 0
                ? "[\n" + string.Join(",\n", p.References.ConvertAll(r => $"    \"{r}\"")) + "\n  ]"
                : "[]";

            string json =
                "{\n" +
                $"  \"name\": \"{p.FullName}\",\n" +
                "  \"rootNamespace\": \"\",\n" +
                $"  \"references\": {refsBlock},\n" +
                $"  \"includePlatforms\": [{include}],\n" +
                $"  \"excludePlatforms\": [{exclude}],\n" +
                $"  \"allowUnsafeCode\": {p.AllowUnsafeCode.ToString().ToLower()},\n" +
                "  \"overrideReferences\": false,\n" +
                "  \"precompiledReferences\": [],\n" +
                $"  \"autoReferenced\": {p.AutoReferenced.ToString().ToLower()},\n" +
                "  \"defineConstraints\": [],\n" +
                "  \"versionDefines\": [],\n" +
                "  \"noEngineReferences\": false\n" +
                "}";

            File.WriteAllText(absFile, json);
            return true;
        }

        // ── Helpers ──────────────────────────────────────────────────────
        public static string BuildFullName(string suffix, string prefix)
        {
            suffix = suffix?.Trim() ?? "";
            prefix = prefix?.Trim() ?? "";
            return string.IsNullOrEmpty(prefix) ? suffix : prefix + "." + suffix;
        }

        // ── Serializable types for SessionState ──────────────────────────
        [Serializable]
        class PendingAsmdefList
        {
            public List<PendingAsmdef> Items;
        }

        [Serializable]
        class PendingAsmdef
        {
            public string       DbFolder;
            public string       FullName;
            public int          Platform;
            public bool         AutoReferenced;
            public bool         AllowUnsafeCode;
            public List<string> References = new List<string>();
        }
    }
}