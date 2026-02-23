using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameInit.Editor.AutoFolders
{
    public static class TemplateStorage
    {
        static readonly string k_Path = Path.Combine(
            Application.dataPath, "..", "UserSettings", "GameInit_FolderTemplates.json");

        public static List<FolderTemplate> LoadCustomTemplates()
        {
            if (!File.Exists(k_Path))
                return new List<FolderTemplate>();
            try
            {
                var json = File.ReadAllText(k_Path);
                var wrap = JsonUtility.FromJson<Wrapper>(json);
                if (wrap?.Templates == null) return new List<FolderTemplate>();
                foreach (var tpl in wrap.Templates)
                    if (tpl.Root != null) RewireParents(tpl.Root, null);
                return wrap.Templates;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[GameInit] Could not load custom templates: " + ex.Message);
                return new List<FolderTemplate>();
            }
        }

        public static void SaveCustomTemplates(List<FolderTemplate> list)
        {
            try
            {
                var dir = Path.GetDirectoryName(k_Path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
                File.WriteAllText(k_Path, JsonUtility.ToJson(new Wrapper { Templates = list }, true));
            }
            catch (Exception ex)
            {
                Debug.LogError("[GameInit] Could not save custom templates: " + ex.Message);
            }
        }

        static void RewireParents(FolderNode n, FolderNode p)
        {
            n.Parent = p;
            foreach (var c in n.Children) RewireParents(c, n);
        }

        [Serializable] class Wrapper { public List<FolderTemplate> Templates; }
    }
}