using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameInit.Editor.AutoFolders
{
    public class ProjectStructureWindow : EditorWindow
    {
        // ── Layout constants ─────────────────────────────────────────────
        const float k_SideW    = 196f;
        const float k_AsmW     = 256f;
        const float k_HeaderH  = 52f;
        const float k_ToolbarH = 28f;
        const float k_FooterH  = 44f;
        const float k_RowH     = 22f;
        const float k_Indent   = 14f;

        const string k_PrefixKey = "GameInit_AsmPrefix";

        // ── Data ─────────────────────────────────────────────────────────
        List<FolderTemplate> _builtIn;
        List<FolderTemplate> _custom;
        int  _selIdx    = 0;
        bool _selCustom = false;

        FolderNode _root;
        FolderNode _selected;
        bool       _asmOpen;

        Vector2 _treeScroll;
        Vector2 _sideScroll;
        Vector2 _asmScroll;

        string _asmPrefix  = "Game";
        bool   _confirmGen = false;
        string _statusMsg  = "";

        FolderNode _renamingNode;

        // ── Styles ───────────────────────────────────────────────────────
        GUIStyle _sLabel;
        GUIStyle _sSection;
        GUIStyle _sTitle;
        GUIStyle _sRow;
        GUIStyle _sRowBold;
        GUIStyle _sBadge;
        GUIStyle _sCode;
        bool     _stylesOk;

        // ── Colours ──────────────────────────────────────────────────────
        static bool Pro => EditorGUIUtility.isProSkin;
        Color Bg0    => Pro ? RGB(0.18f,0.18f,0.18f) : RGB(0.76f,0.76f,0.76f);
        Color Bg1    => Pro ? RGB(0.22f,0.22f,0.22f) : RGB(0.82f,0.82f,0.82f);
        Color Bg2    => Pro ? RGB(0.25f,0.25f,0.25f) : RGB(0.87f,0.87f,0.87f);
        Color Sep    => Pro ? RGB(0.13f,0.13f,0.13f) : RGB(0.60f,0.60f,0.60f);
        Color Acc    => Pro ? RGB(0.26f,0.52f,0.96f) : RGB(0.18f,0.40f,0.85f);
        Color AccSel => new Color(Acc.r, Acc.g, Acc.b, 0.20f);
        Color Txt    => Pro ? RGB(0.85f,0.85f,0.85f) : RGB(0.08f,0.08f,0.08f);
        Color TxtDim => Pro ? RGB(0.50f,0.50f,0.50f) : RGB(0.38f,0.38f,0.38f);
        Color Hover  => Pro ? RGB(0.24f,0.24f,0.24f) : RGB(0.86f,0.86f,0.86f);
        Color BadgeBg  => Pro ? RGB(0.22f,0.36f,0.62f) : RGB(0.60f,0.74f,1.00f);
        Color BadgeTxt => Color.white;

        static Color RGB(float r, float g, float b) => new Color(r, g, b);

        // ── Menu ─────────────────────────────────────────────────────────
        [MenuItem("Tools/GameInit/Project Structure Generator")]
        public static void Open()
        {
            var w = GetWindow<ProjectStructureWindow>("Project Structure");
            w.minSize = new Vector2(580, 440);
            w.Show();
        }

        // ── Lifecycle ────────────────────────────────────────────────────
        void OnEnable()
        {
            // Persist prefix across recompiles and sessions
            _asmPrefix = EditorPrefs.GetString(k_PrefixKey, "Game");

            _builtIn = BuiltInTemplates.GetAll();
            _custom  = TemplateStorage.LoadCustomTemplates();
            ApplyTemplate(0, false);
        }

        void OnDisable() => TemplateStorage.SaveCustomTemplates(_custom);

        // ── Root GUI ─────────────────────────────────────────────────────
        void OnGUI()
        {
            BuildStyles();

            float W = position.width, H = position.height;
            float bodyY = k_HeaderH;
            float bodyH = H - k_HeaderH - k_FooterH;

            DrawHeader(new Rect(0, 0,     W, k_HeaderH));
            DrawBody  (new Rect(0, bodyY, W, bodyH));
            DrawFooter(new Rect(0, H - k_FooterH, W, k_FooterH));

            if (_renamingNode != null &&
                Event.current.type == EventType.MouseDown &&
                !string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()) == false)
            {
                CommitRename(_renamingNode);
                Repaint();
            }
        }

        // ── Header ───────────────────────────────────────────────────────
        void DrawHeader(Rect r)
        {
            EditorGUI.DrawRect(r, Bg0);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), Sep);

            GUI.Label(new Rect(r.x + 12, r.y + 8,  220, 20), "Project Structure", _sTitle);
            GUI.Label(new Rect(r.x + 12, r.y + 28, 280, 13), "GameInit  ·  Folder & Assembly scaffolding", _sLabel);

            float fx = r.xMax - 174;
            GUI.Label(new Rect(fx - 96, r.y + 18, 92, 14), "Assembly prefix", _sLabel);

            EditorGUI.BeginChangeCheck();
            string newPrefix = GUI.TextField(new Rect(fx, r.y + 14, 168, 22), _asmPrefix);
            if (EditorGUI.EndChangeCheck())
            {
                _asmPrefix = newPrefix;
                EditorPrefs.SetString(k_PrefixKey, _asmPrefix);
                Repaint();
            }
        }

        // ── Body ─────────────────────────────────────────────────────────
        void DrawBody(Rect r)
        {
            DrawSidebar(new Rect(r.x, r.y, k_SideW, r.height));
            EditorGUI.DrawRect(new Rect(r.x + k_SideW, r.y, 1, r.height), Sep);

            float asmW = _asmOpen ? k_AsmW : 0f;
            if (_asmOpen)
            {
                EditorGUI.DrawRect(new Rect(r.xMax - asmW - 1, r.y, 1, r.height), Sep);
                DrawAsmPanel(new Rect(r.xMax - asmW, r.y, asmW, r.height));
            }

            float tx = r.x + k_SideW + 1;
            float tw = r.width - k_SideW - 1 - asmW;
            DrawTree(new Rect(tx, r.y, tw, r.height));
        }

        // ── Sidebar ──────────────────────────────────────────────────────
        void DrawSidebar(Rect r)
        {
            EditorGUI.DrawRect(r, Bg0);

            float y = r.y;
            y = SideSection("TEMPLATES", r.x, y, r.width);

            float saveH   = 34f;
            float listH   = r.height - (y - r.y) - saveH - 1;
            var   listR   = new Rect(r.x, y, r.width, listH);
            int   total   = _builtIn.Count + (_custom.Count > 0 ? 1 + _custom.Count : 0);
            var   content = new Rect(0, 0, r.width - 14, total * 36f);

            _sideScroll = GUI.BeginScrollView(listR, _sideScroll, content);
            float sy = 0;
            for (int i = 0; i < _builtIn.Count; i++)
            { DrawTplRow(_builtIn[i], i, false, sy, r.width); sy += 36; }

            if (_custom.Count > 0)
            {
                EditorGUI.DrawRect(new Rect(0, sy, r.width, 16), Bg1);
                GUI.Label(new Rect(8, sy + 2, r.width, 12), "CUSTOM", _sSection);
                sy += 16;
                for (int i = 0; i < _custom.Count; i++)
                { DrawTplRow(_custom[i], i, true, sy, r.width); sy += 36; }
            }
            GUI.EndScrollView();

            EditorGUI.DrawRect(new Rect(r.x, r.yMax - saveH, r.width, 1), Sep);
            if (GUI.Button(new Rect(r.x + 8, r.yMax - saveH + 6, r.width - 16, 22), "Save as Custom Template"))
                SaveCustomTemplate();
        }

        float SideSection(string label, float x, float y, float w)
        {
            EditorGUI.DrawRect(new Rect(x, y, w, 18), Bg1);
            GUI.Label(new Rect(x + 8, y + 3, w - 8, 12), label, _sSection);
            return y + 18;
        }

        void DrawTplRow(FolderTemplate tpl, int idx, bool custom, float sy, float w)
        {
            bool active = custom == _selCustom && idx == _selIdx;
            var  rowR   = new Rect(0, sy, w, 36);

            if (active)
            {
                EditorGUI.DrawRect(rowR, AccSel);
                EditorGUI.DrawRect(new Rect(0, sy, 3, 36), Acc);
            }
            else if (rowR.Contains(Event.current.mousePosition))
                EditorGUI.DrawRect(rowR, Hover);

            string ico = custom ? "d_Favorite" : "d_Folder Icon";
            GUI.Label(new Rect(10, sy + 9, 16, 16), EditorGUIUtility.IconContent(ico), GUIStyle.none);
            GUI.Label(new Rect(30, sy + 10, w - 52, 16), tpl.Name, active ? _sRowBold : _sRow);

            if (custom)
            {
                if (GUI.Button(new Rect(w - 20, sy + 10, 14, 14),
                    EditorGUIUtility.IconContent("d_Toolbar Minus"), GUIStyle.none))
                {
                    _custom.RemoveAt(idx);
                    TemplateStorage.SaveCustomTemplates(_custom);
                    if (_selCustom && _selIdx >= _custom.Count) ApplyTemplate(0, false);
                    GUIUtility.ExitGUI(); return;
                }
            }

            if (Event.current.type == EventType.MouseDown && rowR.Contains(Event.current.mousePosition))
            {
                ApplyTemplate(idx, custom);
                _confirmGen = false;
                GUI.FocusControl(null);
                Event.current.Use();
            }
        }

        // ── Tree ─────────────────────────────────────────────────────────
        void DrawTree(Rect r)
        {
            EditorGUI.DrawRect(r, Bg1);

            var tb = new Rect(r.x, r.y, r.width, k_ToolbarH);
            DrawToolbar(tb);

            var vp      = new Rect(r.x, r.y + k_ToolbarH, r.width, r.height - k_ToolbarH);
            int rows    = CountRows(_root);
            var content = new Rect(0, 0, r.width - 16, Mathf.Max(rows * k_RowH + 8, vp.height));

            _treeScroll = GUI.BeginScrollView(vp, _treeScroll, content);
            float y = 4f;
            if (_root != null)
                foreach (var child in _root.Children)
                    y = DrawNode(child, 0, y, content.width);
            GUI.EndScrollView();
        }

        void DrawToolbar(Rect r)
        {
            EditorGUI.DrawRect(r, Bg0);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), Sep);

            float x = r.x + 8;
            if (GUI.Button(new Rect(x, r.y + 4, 108, 20),
                new GUIContent("  Add Folder", EditorGUIUtility.IconContent("d_CreateAddNew").image)))
                _root?.AddChild("New Folder");
            x += 116;

            if (GUI.Button(new Rect(x, r.y + 4, 80, 20),
                new GUIContent("  Reset", EditorGUIUtility.IconContent("d_Refresh").image)))
            { ApplyTemplate(_selIdx, _selCustom); GUI.FocusControl(null); }

            string asmLabel = _asmOpen ? "< Assemblies" : "Assemblies >";
            if (GUI.Button(new Rect(r.xMax - 120, r.y + 4, 112, 20), asmLabel))
                _asmOpen = !_asmOpen;
        }

        // ── Tree Node ────────────────────────────────────────────────────
        float DrawNode(FolderNode node, int depth, float y, float totalW)
        {
            var row = new Rect(0, y, totalW, k_RowH);
            bool sel   = _selected == node;
            bool hover = row.Contains(Event.current.mousePosition);

            if (sel)
            {
                EditorGUI.DrawRect(row, AccSel);
                EditorGUI.DrawRect(new Rect(0, y, 2, k_RowH), Acc);
            }
            else if (hover && Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(row, Hover);

            float cx = 6 + depth * k_Indent;

            if (node.Children.Count > 0)
            {
                if (GUI.Button(new Rect(cx, y + 5, 12, 12),
                    node.IsExpanded ? "v" : ">", _sLabel))
                    node.IsExpanded = !node.IsExpanded;
            }
            cx += 14;

            string iconKey = node.Children.Count > 0
                ? (node.IsExpanded ? "d_FolderOpened Icon" : "d_Folder Icon")
                : "d_Folder Icon";
            GUI.Label(new Rect(cx, y + 3, 16, 16),
                EditorGUIUtility.IconContent(iconKey), GUIStyle.none);
            cx += 18;

            const float k_BtnZoneW = 58f;
            float nameW = totalW - cx - k_BtnZoneW;

            var nameR = new Rect(cx, y + 3, nameW, 16);
            if (node.IsRenaming)
            {
                string ctrl = "rn_" + node.GetHashCode();
                GUI.SetNextControlName(ctrl);
                node.RenameBuffer = GUI.TextField(nameR, node.RenameBuffer);
                if (Event.current.type == EventType.KeyDown)
                {
                    if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                    { CommitRename(node); Event.current.Use(); Repaint(); }
                    else if (Event.current.keyCode == KeyCode.Escape)
                    { CancelRename(node); Event.current.Use(); Repaint(); }
                }
                if (GUI.GetNameOfFocusedControl() != ctrl && Event.current.type == EventType.Repaint)
                    CommitRename(node);
            }
            else
            {
                GUI.Label(nameR, node.Name, sel ? _sRowBold : _sRow);
                if (Event.current.type == EventType.MouseDown &&
                    Event.current.clickCount == 2 && nameR.Contains(Event.current.mousePosition))
                { BeginRename(node); Event.current.Use(); }
            }

            if (node.GenerateAsmdef && nameW > 40)
            {
                string badgeTxt = BuildFullName(node);
                if (badgeTxt.Length > 22) badgeTxt = "..." + badgeTxt.Substring(badgeTxt.Length - 21);
                float bw = Mathf.Min(_sBadge.CalcSize(new GUIContent(badgeTxt)).x + 10, nameW - 4);
                var   br = new Rect(cx + nameW - bw - 2, y + 4, bw, 14);
                EditorGUI.DrawRect(br, BadgeBg);
                var prev = GUI.color; GUI.color = BadgeTxt;
                GUI.Label(br, badgeTxt, _sBadge);
                GUI.color = prev;
            }

            float bx = totalW - 8;

            bx -= 14;
            if (GUI.Button(new Rect(bx, y + 4, 14, 14),
                EditorGUIUtility.IconContent("d_Toolbar Minus"), GUIStyle.none))
            {
                node.Parent?.RemoveChild(node);
                if (_selected == node)     _selected     = null;
                if (_renamingNode == node) _renamingNode = null;
                Repaint(); return y + k_RowH;
            }
            bx -= 18;

            if (GUI.Button(new Rect(bx, y + 4, 14, 14),
                EditorGUIUtility.IconContent("d_CreateAddNew"), GUIStyle.none))
            { node.IsExpanded = true; node.AddChild("New Folder"); }
            bx -= 18;

            if (GUI.Button(new Rect(bx, y + 4, 14, 14),
                EditorGUIUtility.IconContent("d_editicon.sml"), GUIStyle.none))
                BeginRename(node);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                row.Contains(Event.current.mousePosition))
            {
                _selected = node;
                if (node.GenerateAsmdef && !_asmOpen) _asmOpen = true;
                GUI.FocusControl(null);
                Repaint();
            }

            y += k_RowH;
            if (node.IsExpanded)
                foreach (var child in node.Children.ToList())
                    y = DrawNode(child, depth + 1, y, totalW);

            return y;
        }

        // ── Asmdef Panel ─────────────────────────────────────────────────
        void DrawAsmPanel(Rect r)
        {
            EditorGUI.DrawRect(r, Bg0);
            float x = r.x + 10, w = r.width - 20;

            _asmScroll = GUI.BeginScrollView(r, _asmScroll,
                new Rect(r.x, r.y, w, Mathf.Max(r.height, 600)));

            float y = r.y + 8;
            GUI.Label(new Rect(x, y, w, 13), "ASSEMBLY SETTINGS", _sSection); y += 18;
            EditorGUI.DrawRect(new Rect(x, y, w, 1), Sep); y += 8;

            if (_selected == null)
            {
                GUI.Label(new Rect(x, y, w, 30), "Select a folder in the tree.", _sLabel);
                GUI.EndScrollView(); return;
            }

            var n = _selected;

            bool was = n.GenerateAsmdef;
            n.GenerateAsmdef = EditorGUI.Toggle(new Rect(x, y + 1, 16, 16), n.GenerateAsmdef);
            GUI.Label(new Rect(x + 20, y, w - 20, 16), "Generate .asmdef", _sRow);
            y += 22;

            if (!was && n.GenerateAsmdef && string.IsNullOrWhiteSpace(n.Asmdef.AssemblyName))
                n.Asmdef.AssemblyName = n.Name;

            if (!n.GenerateAsmdef)
            {
                GUI.Label(new Rect(x, y, w, 30), "Enable to configure.", _sLabel);
                GUI.EndScrollView(); return;
            }

            // Assembly suffix (raw — prefix applied at generation time)
            GUI.Label(new Rect(x, y, w, 12), "Assembly suffix", _sLabel); y += 13;
            n.Asmdef.AssemblyName = GUI.TextField(new Rect(x, y, w, 20), n.Asmdef.AssemblyName).Trim();
            y += 26;

            // Full name preview (read-only)
            string full = BuildFullName(n);
            EditorGUI.DrawRect(new Rect(x, y, w, 28), Bg2);
            GUI.Label(new Rect(x + 4, y + 2,  w - 8, 11), "Full assembly name (preview)", _sLabel);
            GUI.Label(new Rect(x + 4, y + 13, w - 8, 13), full, _sCode);
            y += 34;

            // Namespace example
            EditorGUI.DrawRect(new Rect(x, y, w, 28), Bg2);
            GUI.Label(new Rect(x + 4, y + 2,  w - 8, 11), "Namespace example", _sLabel);
            GUI.Label(new Rect(x + 4, y + 13, w - 8, 13), "namespace " + full, _sCode);
            y += 34;

            // Platform
            GUI.Label(new Rect(x, y, w, 12), "Platform", _sLabel); y += 13;
            n.Asmdef.Platform = (AsmdefPlatform)EditorGUI.EnumPopup(
                new Rect(x, y, w, 20), n.Asmdef.Platform); y += 26;

            // Checkboxes
            n.Asmdef.AutoReferenced = EditorGUI.Toggle(
                new Rect(x, y + 1, 16, 16), n.Asmdef.AutoReferenced);
            GUI.Label(new Rect(x + 20, y, w - 20, 16), "Auto referenced", _sLabel); y += 20;

            n.Asmdef.AllowUnsafeCode = EditorGUI.Toggle(
                new Rect(x, y + 1, 16, 16), n.Asmdef.AllowUnsafeCode);
            GUI.Label(new Rect(x + 20, y, w - 20, 16), "Allow unsafe code", _sLabel); y += 28;

            // References — stored as suffixes
            EditorGUI.DrawRect(new Rect(x, y, w, 1), Sep); y += 6;
            GUI.Label(new Rect(x, y, w, 12), "REFERENCES  (suffixes only, e.g. Core)", _sSection);
            y += 16;

            for (int i = 0; i < n.Asmdef.References.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                string edited = GUI.TextField(
                    new Rect(x, y, w - 18, 18), n.Asmdef.References[i]);
                if (EditorGUI.EndChangeCheck())
                    // Strip prefix if user accidentally pastes a full name
                    n.Asmdef.References[i] = NormalizeSuffix(edited);

                if (GUI.Button(new Rect(x + w - 16, y + 1, 14, 14),
                    EditorGUIUtility.IconContent("d_Toolbar Minus"), GUIStyle.none))
                { n.Asmdef.References.RemoveAt(i); break; }
                y += 22;
            }
            y += 4;
            if (GUI.Button(new Rect(x, y, w, 20), "+ Add Reference"))
                n.Asmdef.References.Add("");
            y += 28;

            // Quick-add from other asmdef nodes in the tree
            var others = CollectAsmdefs(_root)
                .Where(m => m != n
                    && !string.IsNullOrWhiteSpace(m.Asmdef.AssemblyName)
                    && !n.Asmdef.References.Contains(m.Asmdef.AssemblyName))
                .ToList();

            if (others.Count > 0)
            {
                EditorGUI.DrawRect(new Rect(x, y, w, 1), Sep); y += 6;
                GUI.Label(new Rect(x, y, w, 12), "QUICK ADD", _sSection); y += 16;
                foreach (var m in others)
                {
                    string suffix = m.Asmdef.AssemblyName;
                    string label  = $"{suffix}  [{BuildFullName(m)}]";
                    if (GUI.Button(new Rect(x, y, w, 18), label))
                        // Always store the suffix — FolderGenerator resolves the prefix at write time
                        n.Asmdef.References.Add(suffix);
                    y += 22;
                }
            }

            GUI.EndScrollView();
        }

        // ── Footer ───────────────────────────────────────────────────────
        void DrawFooter(Rect r)
        {
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1), Sep);
            EditorGUI.DrawRect(r, Bg0);

            if (!string.IsNullOrEmpty(_statusMsg))
                GUI.Label(new Rect(r.x + 12, r.y + 14, r.width - 280, 16), _statusMsg, _sLabel);

            float bw = _confirmGen ? 210 : 174;
            float bx = r.xMax - bw - 10;
            string lbl    = _confirmGen ? "Confirm - Write to Disk" : "  Generate Structure";
            string icoKey = _confirmGen ? "console.warnicon.sml" : "d_FolderOpened Icon";

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = _confirmGen ? new Color(0.85f, 0.52f, 0.10f) : Acc;
            if (GUI.Button(new Rect(bx, r.y + 7, bw, 28),
                new GUIContent(lbl, EditorGUIUtility.IconContent(icoKey).image)))
            {
                if (_confirmGen) { DoGenerate(); _confirmGen = false; }
                else _confirmGen = true;
            }
            GUI.backgroundColor = prevBg;

            if (_confirmGen)
                if (GUI.Button(new Rect(bx - 60, r.y + 7, 54, 28), "Cancel"))
                    _confirmGen = false;
        }

        // ── Generate ─────────────────────────────────────────────────────
        void DoGenerate()
        {
            if (_root == null) return;

            _statusMsg = "Creating folders...";
            Repaint();

            int folders = 0;
            var folders1 = folders;
            folders = FolderGenerator.GenerateFolders(_root, _asmPrefix, asmdefsWritten =>
            {
                _statusMsg = $"Done  {folders1} folder(s)  {asmdefsWritten} .asmdef(s) created";
                // Window may have been destroyed during domain reload — guard
                if (this) Repaint();
            });

            // Folders are known immediately; asmdef count arrives via callback
            _statusMsg = $"Folders created: {folders}  |  Writing .asmdefs after refresh...";
            Repaint();
        }

        // ── Helpers ──────────────────────────────────────────────────────
        void ApplyTemplate(int idx, bool custom)
        {
            _selIdx = idx; _selCustom = custom;
            _selected = null;
            var list = custom ? _custom : _builtIn;
            if (list == null || idx < 0 || idx >= list.Count)
            { _root = new FolderNode("__root__"); return; }
            _root = list[idx].Root != null ? list[idx].Root.DeepClone() : new FolderNode("__root__");
        }

        void SaveCustomTemplate()
        {
            if (_root == null) return;
            _custom.Add(new FolderTemplate("Custom " + (_custom.Count + 1), "", false)
                { Root = _root.DeepClone() });
            TemplateStorage.SaveCustomTemplates(_custom);
            ApplyTemplate(_custom.Count - 1, true);
        }

        void BeginRename(FolderNode n)
        {
            if (_renamingNode != null) CommitRename(_renamingNode);
            n.IsRenaming = true; n.RenameBuffer = n.Name; _renamingNode = n;
        }

        void CommitRename(FolderNode n)
        {
            if (!string.IsNullOrWhiteSpace(n.RenameBuffer)) n.Name = n.RenameBuffer.Trim();
            n.IsRenaming = false; _renamingNode = null;
        }

        void CancelRename(FolderNode n)
        {
            n.IsRenaming = false; _renamingNode = null;
        }

        // Returns the preview full name using the current prefix
        string BuildFullName(FolderNode n)
        {
            string suffix = string.IsNullOrWhiteSpace(n.Asmdef.AssemblyName) ? n.Name : n.Asmdef.AssemblyName;
            return FolderGenerator.BuildFullName(suffix, _asmPrefix);
        }

        // If user pastes "Game.Core" into a suffix field, strip the prefix
        string NormalizeSuffix(string value)
        {
            value = value.Trim();
            string prefixDot = _asmPrefix.Trim() + ".";
            if (!string.IsNullOrEmpty(_asmPrefix) && value.StartsWith(prefixDot))
                value = value.Substring(prefixDot.Length);
            return value;
        }

        int CountRows(FolderNode n)
        {
            if (n == null) return 0;
            int c = 0;
            foreach (var ch in n.Children) { c++; if (ch.IsExpanded) c += CountRows(ch); }
            return c;
        }

        List<FolderNode> CollectAsmdefs(FolderNode n)
        {
            var r = new List<FolderNode>();
            if (n == null) return r;
            if (n.GenerateAsmdef) r.Add(n);
            foreach (var c in n.Children) r.AddRange(CollectAsmdefs(c));
            return r;
        }

        // ── Styles ───────────────────────────────────────────────────────
        void BuildStyles()
        {
            if (_stylesOk) return;
            _stylesOk = true;

            _sTitle = new GUIStyle(EditorStyles.boldLabel)
                { fontSize = 14, normal = { textColor = Txt } };

            _sLabel = new GUIStyle(EditorStyles.label)
                { fontSize = 10, wordWrap = true, normal = { textColor = TxtDim } };

            _sSection = new GUIStyle(EditorStyles.label)
                { fontSize = 9, fontStyle = FontStyle.Bold, normal = { textColor = TxtDim } };

            _sRow = new GUIStyle(EditorStyles.label)
                { fontSize = 12, normal = { textColor = Txt } };

            _sRowBold = new GUIStyle(_sRow)
                { fontStyle = FontStyle.Bold };

            _sBadge = new GUIStyle(EditorStyles.label)
                { fontSize = 9, alignment = TextAnchor.MiddleCenter, normal = { textColor = BadgeTxt } };

            _sCode = new GUIStyle(EditorStyles.label)
            {
                fontSize  = 10,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = Txt }
            };
        }
    }
}