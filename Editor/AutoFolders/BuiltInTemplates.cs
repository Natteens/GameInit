using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameInit.Editor.AutoFolders
{
    public static class BuiltInTemplates
    {
        public static List<FolderTemplate> GetAll()
        {
            return new List<FolderTemplate>
            {
                BuildCurrentProject(),   // ← first
                BuildStandardUnity()
            };
        }

        // ──────────────────────────────────────────────────────────────────
        //  Current Project — reads what already exists in Assets/
        // ──────────────────────────────────────────────────────────────────
        public static FolderTemplate BuildCurrentProject()
        {
            var t = new FolderTemplate("Current Project", "", builtIn: true);
            var root = new FolderNode("__root__");

            string assetsPath = Application.dataPath;
            if (Directory.Exists(assetsPath))
                ScanDirectory(assetsPath, root, 0, 3);

            t.Root = root;
            return t;
        }

        // ──────────────────────────────────────────────────────────────────
        //  Standard Unity
        // ──────────────────────────────────────────────────────────────────
        public static FolderTemplate BuildStandardUnity()
        {
            var t = new FolderTemplate("Standard Unity", "", builtIn: true);
            var root = new FolderNode("__root__");

            var art = root.AddChild("Art");
            art.AddChild("Animations");
            art.AddChild("Fonts");
            art.AddChild("Materials");
            art.AddChild("Models");
            art.AddChild("Sprites");
            art.AddChild("Textures");
            art.AddChild("VFX");

            var audio = root.AddChild("Audio");
            audio.AddChild("Music");
            audio.AddChild("SFX");
            audio.AddChild("Ambience");

            var data = root.AddChild("Data");
            data.AddChild("ScriptableObjects");
            data.AddChild("Configs");
            data.AddChild("Saves");

            var prefabs = root.AddChild("Prefabs");
            prefabs.AddChild("Characters");
            prefabs.AddChild("Environment");
            prefabs.AddChild("UI");
            prefabs.AddChild("Effects");
            prefabs.AddChild("Gameplay");

            root.AddChild("Resources");

            var scenes = root.AddChild("Scenes");
            scenes.AddChild("Menus");
            scenes.AddChild("Gameplay");
            scenes.AddChild("Test");

            var scripts = root.AddChild("Scripts");
            AddModule(scripts, "Core",     AsmdefPlatform.Any);
            AddModule(scripts, "Systems",  AsmdefPlatform.Any,         refs: new[] { "Core" });
            AddModule(scripts, "Gameplay", AsmdefPlatform.RuntimeOnly, refs: new[] { "Core", "Systems" });
            AddModule(scripts, "UI",       AsmdefPlatform.RuntimeOnly, refs: new[] { "Core" });
            AddModule(scripts, "Utils",    AsmdefPlatform.Any);
            AddModule(scripts, "Editor",   AsmdefPlatform.EditorOnly,  autoRef: false, refs: new[] { "Core" });

            t.Root = root;
            return t;
        }

        // ──────────────────────────────────────────────────────────────────
        //  Helpers
        // ──────────────────────────────────────────────────────────────────

        // NOTE: assembly names store only the suffix (e.g. "Core").
        // The window prepends the user's assembly prefix at generation time.
        static FolderNode AddModule(FolderNode parent, string name,
            AsmdefPlatform platform, bool autoRef = true, string[] refs = null)
        {
            var n = parent.AddChild(name);
            n.GenerateAsmdef          = true;
            n.Asmdef.AssemblyName     = name;          // suffix only
            n.Asmdef.Platform         = platform;
            n.Asmdef.AutoReferenced   = autoRef;
            if (refs != null) n.Asmdef.References.AddRange(refs);
            return n;
        }

        static void ScanDirectory(string dir, FolderNode parent, int depth, int max)
        {
            if (depth > max) return;
            foreach (var sub in Directory.GetDirectories(dir))
            {
                string name = Path.GetFileName(sub);
                if (name.StartsWith(".")) continue;
                var child = parent.AddChild(name);
                ScanDirectory(sub, child, depth + 1, max);
            }
        }
    }
}