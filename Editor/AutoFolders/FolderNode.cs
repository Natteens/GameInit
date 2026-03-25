using System;
using System.Collections.Generic;

namespace GameInit.Editor.AutoFolders
{
    [Serializable]
    public class FolderNode
    {
        public string Name;
        public bool GenerateAsmdef;
        public AsmdefConfig Asmdef = new AsmdefConfig();
        public List<FolderNode> Children = new List<FolderNode>();

        // Runtime only — not serialized
        [NonSerialized] public bool IsExpanded = true;
        [NonSerialized] public FolderNode Parent;
        [NonSerialized] public bool IsRenaming;
        [NonSerialized] public string RenameBuffer = "";

        public FolderNode() { }

        public FolderNode(string name, FolderNode parent = null)
        {
            Name   = name;
            Parent = parent;
        }

        public FolderNode AddChild(string name)
        {
            var child = new FolderNode(name, this);
            Children.Add(child);
            return child;
        }

        public void RemoveChild(FolderNode child)
        {
            Children.Remove(child);
        }

        /// <summary>Deep clone preserving hierarchy. Parent refs are re-wired. UI state is reset.</summary>
        public FolderNode DeepClone(FolderNode newParent = null)
        {
            var clone = new FolderNode(Name, newParent)
            {
                GenerateAsmdef = GenerateAsmdef,
                IsExpanded     = true,   // always reset UI state on clone
                Asmdef         = Asmdef.Clone()
            };
            foreach (var c in Children)
                clone.Children.Add(c.DeepClone(clone));
            return clone;
        }

        /// <summary>Full path relative to Assets/, e.g. "Scripts/Core"</summary>
        public string GetRelativePath()
        {
            if (Parent == null) return Name;
            var parentPath = Parent.GetRelativePath();
            return string.IsNullOrEmpty(parentPath) ? Name : parentPath + "/" + Name;
        }
    }

    [Serializable]
    public class AsmdefConfig
    {
        // Stored as suffix only (e.g. "Core"), never the full qualified name.
        // FolderGenerator.BuildFullName() applies the prefix at write time.
        public string          AssemblyName    = "";
        public AsmdefPlatform  Platform        = AsmdefPlatform.Any;
        public bool            AutoReferenced  = true;
        public bool            AllowUnsafeCode = false;

        // Stored as suffixes only (e.g. "Core", "Systems").
        // FolderGenerator resolves them to full names at write time.
        public List<string>    References      = new List<string>();

        public AsmdefConfig Clone() => new AsmdefConfig
        {
            AssemblyName    = AssemblyName,
            Platform        = Platform,
            AutoReferenced  = AutoReferenced,
            AllowUnsafeCode = AllowUnsafeCode,
            References      = new List<string>(References)
        };
    }

    public enum AsmdefPlatform { Any, EditorOnly, RuntimeOnly }

    [Serializable]
    public class FolderTemplate
    {
        public string       Name;
        public string       Description;
        public bool         IsBuiltIn;
        public FolderNode   Root;

        public FolderTemplate() { }
        public FolderTemplate(string name, string description, bool builtIn = false)
        {
            Name        = name;
            Description = description;
            IsBuiltIn   = builtIn;
        }
    }
}