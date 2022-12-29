using System.Collections;
using UnityEngine;
using UnityEditor;
using HoudiniEngineUnity;
using System;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI;
using UnityEditor.VersionControl;

namespace CZ.HEngineTools
{
    public class CZ_HEngineAssetUI : Editor
    {
        #region Variables
        CZ_HEngineAsset _asset;
        HEU_HoudiniAsset _hAsset;
        SerializedObject _hAssetSerialized;
        HEU_PDGAssetLink _pdgAssetLink;

        bool _showPDGProperties = true;
        bool _guiEnabled;
        SceneView _sceneView;
        Editor _parameterEditor;
        Editor _curveEditor;
        Editor _toolsEditor;
        Editor _handlesEditor;
        #endregion

        #region Styles Variables
        static GUIStyle _mainButtonStyle;
        static GUIStyle _mainCentredButtonStyle;
        static GUIStyle _mainButtonSetStyle;
        static GUIStyle _mainBoxStyle;
        static GUIStyle _mainPromptStyle;
        static GUIStyle _pdgBoxStyle;
        static GUIStyle _editableNodeBoxStyle;
        static GUIStyle _pdgBoxStyleStatus;
        static GUIStyle _pdgUtilButtonStyle;
        static GUIStyle _pdgButtonStyle;
        static GUIStyle _pdgFoldStyle;
        static GUIStyle _pdgBoxStyleTitle;
        static GUIStyle _pdgBoxStyleValue;

        static Color _cookedColor;
        static float _mainButtonSeparatorDistance;

        static Texture2D _reloadhdaIcon;
        static Texture2D _recookhdaIcon;
        static Texture2D _bakegameobjectIcon;
        static Texture2D _bakeprefabIcon;
        static Texture2D _bakeandreplaceIcon;
        static Texture2D _removeheIcon;
        static Texture2D _duplicateAssetIcon;
        static Texture2D _resetParamIcon;

        static GUIContent _reloadhdaContent;
        static GUIContent _recookhdaContent;
        static GUIContent _bakegameobjectContent;
        static GUIContent _bakeprefabContent;
        static GUIContent _bakeandreplaceContent;
        static GUIContent _removeheContent;
        static GUIContent _duplicateContent;
        static GUIContent _resetParamContent;
        static GUIContent _dragAndDropField;
        static GUIContent _resetMaterialOverridesButton;
        static GUIContent _projectCurvePointsButton;
        static GUIContent _savePresetButton;
        static GUIContent _loadPresetButton;
        static GUIContent _useCurveScaleRotContent;
        static GUIContent _cookCurveOnDragContent;
        static GUIContent _curveFrameSelectedNodesContent;
        static GUIContent _curveFrameSelectedNodeDistanceContent;
        static GUIContent _resetContent;
        static GUIContent _refreshContent;
        static GUIContent _buttonDirtyAllContent;
        static GUIContent _buttonCookAllContent;
        static GUIContent _buttonCancelCookContent;
        static GUIContent _buttonPauseCookContent;
        #endregion

        #region Getters/Setters
        public CZ_HEngineAsset Asset
        {
            get { return _asset; }
            set {
                _asset = value;
                if (_asset.HoudiniAssetExt != null)
                {
                    _hAsset = _asset.HoudiniAssetExt.HoudiniAsset;
                    _pdgAssetLink = _asset.PDGAssetLink;
                }
            }
        }
        #endregion


        #region Styles
        private void SetStylesDescriptions()
        {
            _dragAndDropField = new GUIContent("Drag & drop GameObjects / Prefabs:", "Place GameObjects and/or Prefabs here that were previously baked out and need to be updated, then click Bake Update.");
            _resetMaterialOverridesButton = new GUIContent("Reset Material Overrides", "Remove overridden materials, and replace with generated materials for this asset's output.");
            _projectCurvePointsButton = new GUIContent("Project Curve", "Project all points position.y to zero.");
            _savePresetButton = new GUIContent("Save HDA Preset", "Save the HDA's current preset to a file.");
            _loadPresetButton = new GUIContent("Load HDA Preset", "Load a HDA preset file into this asset and cook it.");
            _useCurveScaleRotContent = new GUIContent("Disable Curve scale/rot", "Disables the usage of scale/rot attributes. Useful if the scale/rot attribute values are causing issues with your curve.");
            _cookCurveOnDragContent = new GUIContent("Cook Curve on Drag", "Cooks the curve while you are dragging the curve point. Useful if you need responsiveness over performance. Disable this option to improve performance.");
            _curveFrameSelectedNodesContent = new GUIContent("Frame Selected Nodes Only", "Frames only the currently selected nodes when you press the F hotkey instead of the whole curve.");
            _curveFrameSelectedNodeDistanceContent = new GUIContent("Frame Selected Node Distance", "The distance between the selected node and the editor camera when you frame the selected node.");
            _resetContent = new GUIContent("Reset", "Reset the state and generated items. Updates from linked HDA.");
            _refreshContent = new GUIContent("Refresh", "Refresh the state and UI.");
            _buttonDirtyAllContent = new GUIContent("Dirty All", "Removes all work items.");
            _buttonCookAllContent = new GUIContent("Cook Output", "Generates and cooks all work items.");
            _buttonCancelCookContent = new GUIContent("Cancel", "Cancel PDG cook.");
            _buttonPauseCookContent = new GUIContent("Pause", "Pause PDG cook.");

            _reloadhdaIcon = Resources.Load("heu_reloadhdaIcon") as Texture2D;
            _recookhdaIcon = Resources.Load("heu_recookhdaIcon") as Texture2D;
            _bakegameobjectIcon = Resources.Load("heu_bakegameobjectIcon") as Texture2D;
            _bakeprefabIcon = Resources.Load("heu_bakeprefabIcon") as Texture2D;
            _bakeandreplaceIcon = Resources.Load("heu_bakeandreplaceIcon") as Texture2D;
            _removeheIcon = Resources.Load("heu_removeheIcon") as Texture2D;
            _duplicateAssetIcon = Resources.Load("heu_duplicateassetIcon") as Texture2D;
            _resetParamIcon = Resources.Load("heu_resetparametersIcon") as Texture2D;
            _reloadhdaContent = new GUIContent("  Rebuild  ", _reloadhdaIcon, "Reload the asset in Houdini and cook it. Current parameter values and input objects will be re-applied. Material overrides will be removed.");
            _recookhdaContent = new GUIContent("  Recook   ", _recookhdaIcon, "Force recook of the asset in Houdini with the current parameter values and specified input data. Updates asset if changed in Houdini.");
            _bakegameobjectContent = new GUIContent("  GameObject", _bakegameobjectIcon, "Bakes the output to a new GameObject. Meshes and Materials are copied.");
            _bakeprefabContent = new GUIContent("  Bake to Prefab", _bakeprefabIcon, "Bakes the output to a new Prefab to /Assets/HoudiniEngineAssetCache/Baked folder. Meshes and Materials are copied.");
            _bakeandreplaceContent = new GUIContent("  Update", _bakeandreplaceIcon, "Update existing GameObject(s) and Prefab(s). Generated components, meshes, and materials are updated. Assumes the that all asset resources are in the default cook folder indexed by the gameObject name.");
            _removeheContent = new GUIContent("  Keep Only Output", _removeheIcon, "Remove Houdini Engine data (HDA_Data, Houdini Asset Root object), and leave just the generated Unity data (meshes, materials, instances, etc.).");
            _duplicateContent = new GUIContent("  Duplicate", _duplicateAssetIcon, "Safe duplication of this asset to create an exact copy. The asset is duplicated in Houdini. All data is copied over.");
            _resetParamContent = new GUIContent("  Reset All", _resetParamIcon, "Reset all parameters, materials, and inputs to their HDA default values, clear cache, reload HDA, cook, and generate output.");
        }
        private void InitializeStyles()
        {
            _mainButtonSeparatorDistance = 5f;
            _cookedColor = new Color(0.1f, 0.9f, 0.0f, 1f);

            float screenWidth = EditorGUIUtility.currentViewWidth;
            float widthPadding = 55f;
            float doubleButtonWidth = Mathf.Round(screenWidth - widthPadding + _mainButtonSeparatorDistance);
            float singleButtonWidth = Mathf.Round((screenWidth - widthPadding) * 0.5f);

            if (_mainButtonStyle == null)
            {
                float buttonHeight = 30f;
                _mainButtonStyle = new GUIStyle(GUI.skin.button);
                _mainButtonStyle.fontStyle = FontStyle.Bold;
                _mainButtonStyle.fontSize = 12;
                _mainButtonStyle.alignment = TextAnchor.MiddleCenter;
                _mainButtonStyle.fixedHeight = buttonHeight;
                _mainButtonStyle.padding.left = 6;
                _mainButtonStyle.padding.right = 6;
                _mainButtonStyle.margin.left = 0;
                _mainButtonStyle.margin.right = 0;
                _mainButtonStyle.clipping = TextClipping.Clip;
                _mainButtonStyle.wordWrap = true;
            }
            if (_mainCentredButtonStyle == null)
            {
                _mainCentredButtonStyle = new GUIStyle(_mainButtonStyle);
                _mainCentredButtonStyle.alignment = TextAnchor.MiddleCenter;
            }
            if (_mainButtonSetStyle == null)
            {
                _mainButtonSetStyle = new GUIStyle(GUI.skin.box);
                RectOffset br = _mainButtonSetStyle.margin;
                br.left = 4;
                br.right = 4;
                _mainButtonSetStyle.margin = br;
            }
            if (_mainBoxStyle == null)
            {
                _mainBoxStyle = new GUIStyle();
                RectOffset br = _mainBoxStyle.margin;
                br.left = 4;
                br.right = 4;
                _mainBoxStyle.margin = br;
                _mainBoxStyle.padding = br;
            }
            if (_mainPromptStyle == null)
            {
                _mainPromptStyle = new GUIStyle(GUI.skin.button);
                _mainPromptStyle.fontSize = 11;
                _mainPromptStyle.alignment = TextAnchor.MiddleCenter;
                _mainPromptStyle.fixedHeight = 30;
                _mainPromptStyle.margin.left = 34;
                _mainPromptStyle.margin.right = 34;
            }
            if (_pdgBoxStyleStatus == null)
            {
                // Editable Nodes Box Style
                _editableNodeBoxStyle = new GUIStyle(GUI.skin.box);
                _editableNodeBoxStyle.margin = new RectOffset(10, 10, 0, 0);
                float c = 0.2f;
                _editableNodeBoxStyle.normal.background = HEU_GeneralUtility.MakeTexture(1, 1, new Color(c, c, c, 1f));
                _editableNodeBoxStyle.normal.textColor = Color.black;
                _editableNodeBoxStyle.fontStyle = FontStyle.Bold;
                _editableNodeBoxStyle.alignment = TextAnchor.MiddleLeft;
                _editableNodeBoxStyle.fontSize = 14;
                _editableNodeBoxStyle.stretchWidth = true;

                // PDG Box Style Status
                _pdgBoxStyleStatus = new GUIStyle(GUI.skin.box);
                c = 0.1f;
                _pdgBoxStyleStatus.normal.background = HEU_GeneralUtility.MakeTexture(1, 1, new Color(c, c, c, 1f));
                _pdgBoxStyleStatus.normal.textColor = Color.black;
                _pdgBoxStyleStatus.fontStyle = FontStyle.Bold;
                _pdgBoxStyleStatus.alignment = TextAnchor.MiddleCenter;
                _pdgBoxStyleStatus.fontSize = 14;
                _pdgBoxStyleStatus.stretchWidth = true;
            }
            if (_pdgUtilButtonStyle == null)
            {
                float buttonHeight = 24f;
                float buttonWidth = 60f;

                _pdgUtilButtonStyle = new GUIStyle(GUI.skin.button);
                _pdgUtilButtonStyle.fontStyle = FontStyle.Bold;
                _pdgUtilButtonStyle.fontSize = 12;
                _pdgUtilButtonStyle.alignment = TextAnchor.MiddleCenter;
                _pdgUtilButtonStyle.fixedHeight = buttonHeight;
                _pdgUtilButtonStyle.fixedWidth = buttonWidth;
                _pdgUtilButtonStyle.padding.left = 6;
                _pdgUtilButtonStyle.padding.right = 6;
                _pdgUtilButtonStyle.margin.top = 4;
                _pdgUtilButtonStyle.margin.bottom = 4;
                _pdgUtilButtonStyle.margin.left = 0;
                _pdgUtilButtonStyle.margin.right = 0;
                _pdgUtilButtonStyle.clipping = TextClipping.Clip;
                _pdgUtilButtonStyle.wordWrap = true;
            }
            if (_pdgBoxStyle == null)
            {
                _pdgBoxStyle = new GUIStyle();
                RectOffset br = _pdgBoxStyle.margin;
                br.left = 4;
                br.right = 4;
                _pdgBoxStyle.margin = br;
                _pdgBoxStyle.padding = br;
            }
            if (_pdgButtonStyle == null)
            {
                float buttonWidth = 68f;
                _pdgButtonStyle = new GUIStyle(GUI.skin.button);
                _pdgButtonStyle.fontStyle = FontStyle.Bold;
                _pdgButtonStyle.fontSize = 12;
                _pdgButtonStyle.alignment = TextAnchor.MiddleCenter;
                _pdgButtonStyle.fixedWidth = buttonWidth;
                _pdgButtonStyle.padding.left = 6;
                _pdgButtonStyle.padding.right = 6;
                _pdgButtonStyle.margin.top = 4;
                _pdgButtonStyle.margin.bottom = 4;
                _pdgButtonStyle.margin.left = 0;
                _pdgButtonStyle.margin.right = 0;
                _pdgButtonStyle.clipping = TextClipping.Clip;
                _pdgButtonStyle.wordWrap = true;
            }
            if (_pdgFoldStyle == null)
            {
                _pdgFoldStyle = new GUIStyle(GUI.skin.GetStyle("Foldout"));
                _pdgFoldStyle.richText = true;
                _pdgFoldStyle.fontSize = 12;
                _pdgFoldStyle.fontStyle = FontStyle.Bold;
            }
            if (_pdgBoxStyleTitle == null)
            {
                _pdgBoxStyleTitle = new GUIStyle(GUI.skin.box);
                float c = 0.35f;
                _pdgBoxStyleTitle.normal.background = HEU_GeneralUtility.MakeTexture(1, 1, new Color(c, c, c, 1f));
                _pdgBoxStyleTitle.normal.textColor = Color.black;
                _pdgBoxStyleTitle.fontStyle = FontStyle.Bold;
                _pdgBoxStyleTitle.alignment = TextAnchor.MiddleCenter;
                _pdgBoxStyleTitle.fontSize = 10;
            }
            if (_pdgBoxStyleValue == null)
            {
                _pdgBoxStyleValue = new GUIStyle(GUI.skin.box);
                float c = 0.2f;
                _pdgBoxStyleValue.normal.background = HEU_GeneralUtility.MakeTexture(1, 1, new Color(c, c, c, 1f));
                c = 0.1f;
                _pdgBoxStyleValue.normal.textColor = new Color(c, c, c, 1f);
                _pdgBoxStyleValue.fontStyle = FontStyle.Bold;
                _pdgBoxStyleValue.fontSize = 24;
            }
        }
        #endregion

        #region Methods

        public void OnSceneGUI()
        {
            if (_hAsset == null)
            {
                return;
            }

            if (!_hAsset.IsAssetValid())
            {
                return;
            }

            if (_hAsset.SerializedMetaData != null && _hAsset.SerializedMetaData.SoftDeleted == true)
            {
                return;
            }

            if ((Event.current.type == EventType.ValidateCommand && Event.current.commandName.Equals("UndoRedoPerformed")))
            {
                Event.current.Use();
            }

            if ((Event.current.type == EventType.ExecuteCommand && Event.current.commandName.Equals("UndoRedoPerformed")))
            {

                // On Undo, need to check which parameters have changed in order to update and recook.
                _hAsset.SyncInternalParametersForUndoCompare();

                _hAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: false, bUploadParameters: true);

                // Force a repaint here to update the UI when Undo is invoked. Handles case where the Inspector window is
                // no longer the focus. Without this the Inspector window still shows old value until user selects it.
                SceneView.RepaintAll();
            }

            // Draw custom scene elements. Should be called for any event, not just repaint.
            DrawSceneElements(_hAsset);
        }
        public void RefreshUI()
        {
            // Clear out the instance input cache.
            // Needed after a cook.
            //_instanceInputUI = null;

            if (_pdgAssetLink != null)
            {
                _pdgAssetLink.UpdateWorkItemTally();
            }
            SceneView.RepaintAll();
        }
        public void DrawGUI()
        {
            _hAssetSerialized = _asset.TryAcquiringAsset();

            string msg = "Houdini Engine Asset Error\n" + "No HEU_HoudiniAsset found!";
            if (_asset.HoudiniAssetExt.HoudiniAsset == null || !_asset.HoudiniAssetExt.IsValidForInteraction(Selection.activeGameObject, ref msg))
            {
                DrawMessage(msg);
                return;
            }

            _hAsset.RefreshUIDelegate = RefreshUI;
            _hAssetSerialized.Update();
            _guiEnabled = GUI.enabled;

            using (new EditorGUILayout.VerticalScope())
            {
                HEU_HoudiniAsset.AssetBuildAction pendingBuildAction = HEU_HoudiniAsset.AssetBuildAction.NONE;
                SerializedProperty pendingBuildProperty = HEU_EditorUtility.GetSerializedProperty(_hAssetSerialized, "_requestBuildAction");
                if (pendingBuildProperty != null)
                {
                    pendingBuildAction = (HEU_HoudiniAsset.AssetBuildAction)pendingBuildProperty.enumValueIndex;
                }

                //// Track changes to Houdini Asset gameobject
                EditorGUI.BeginChangeCheck();
                InitializeStyles();

                if (HEU_EditorUtility.GetSerializedProperty(_hAssetSerialized, "_cookStatus") != null)
                {
                    if (Selection.activeGameObject && _pdgAssetLink != null)
                    {
                        DrawPDGSection();
                    }
                    if (Selection.activeGameObject && _hAsset.AssetTypeInternal != HEU_HoudiniAsset.HEU_AssetType.TYPE_CURVE)
                    {
                        DrawBakeSection();
                    }

                    bool bSkipAutoCook = DrawGenerateSection(ref pendingBuildAction);
                    if (!bSkipAutoCook)
                    {
                        DrawEditableNodes();
                        // If this is a Curve asset, we don't need to draw parameters as its redundant
                        if (_hAsset.AssetTypeInternal != HEU_HoudiniAsset.HEU_AssetType.TYPE_CURVE)
                        {
                            DrawParameters(_hAsset.Parameters, ref _parameterEditor);
                            HEU_EditorUI.DrawSeparator();

                        }
                        DrawCurvesSection();
                    }

                    Asset.ProcessPendingBuildAction(pendingBuildAction, pendingBuildProperty, _hAsset, _hAssetSerialized);

                    // Check if any changes occurred, and if so, trigger a recook
                    if (EditorGUI.EndChangeCheck())
                    {
                        Asset.Recook(bSkipAutoCook);
                    }
                }
            }

            GUI.enabled = _guiEnabled;
        }
        private void DrawPDGSection()
        {
            if (_pdgAssetLink != null)
            {
                _pdgAssetLink.UpdateWorkItemTally();
            }
            EditorGUI.indentLevel++;
            _showPDGProperties = EditorGUILayout.Foldout(_showPDGProperties, "PDG PANEL", true, _pdgFoldStyle);
            if (_showPDGProperties)
            {
                HEU_PDGAssetLink.LinkState validState = _pdgAssetLink.AssetLinkStateInternal;
                using (new EditorGUILayout.HorizontalScope(_pdgBoxStyle))
                {
                    DrawPDGStatus();

                    // Refresh button re-poplates the UI data from linked asset
                    if (GUILayout.Button(_refreshContent, _pdgUtilButtonStyle))
                    {
                        _pdgAssetLink.Refresh();
                    }
                    GUILayout.Space(_mainButtonSeparatorDistance);
                    // Reset button resets and recreates the HEU_PDGAssetLink
                    if (GUILayout.Button(_resetContent, _pdgUtilButtonStyle))
                    {
                        _pdgAssetLink.Reset();
                    }
                }
                int numTopNodes = _pdgAssetLink.TopNetworkNames.Length;
                if (numTopNodes > 0)
                {
                    if (validState == HEU_PDGAssetLink.LinkState.LINKED)
                    {
                        using (new EditorGUILayout.HorizontalScope(_pdgBoxStyle))
                        {
                            if (validState == HEU_PDGAssetLink.LinkState.INACTIVE)
                            {
                                _pdgAssetLink.Refresh();
                            }
                            else if (validState == HEU_PDGAssetLink.LinkState.LINKED)
                            {
                                DrawTOPNetworkButtons();
                            }

                            DrawWorkItemTally(_pdgAssetLink._workItemTally);

                        }
                    }
                }
            }
            EditorGUI.indentLevel--;

            HEU_EditorUI.DrawSeparator();
        }
        private void DrawPDGStatus()
        {
            string pdgState = "PDG is NOT READY";
            Color stateColor = Color.red;

            HEU_PDGSession pdgSession = HEU_PDGSession.GetPDGSession();
            if (pdgSession != null)
            {
                if (pdgSession._pdgState == HAPI_PDG_State.HAPI_PDG_STATE_COOKING)
                {
                    pdgState = "PDG is COOKING";
                    stateColor = Color.yellow;
                }
                else if (pdgSession._pdgState == HAPI_PDG_State.HAPI_PDG_STATE_READY)
                {
                    pdgState = "PDG is READY";
                    stateColor = Color.green;
                }
            }

            _pdgBoxStyleStatus.normal.textColor = stateColor;
            GUILayout.Box(pdgState, _pdgBoxStyleStatus);
        }
        private void DrawTOPNetworkButtons()
        {
            _pdgAssetLink.SelectTOPNetwork(0);

            using (new EditorGUILayout.VerticalScope())
            {
                _pdgButtonStyle.fixedHeight = 68.0f;
                if (GUILayout.Button(_buttonCookAllContent, _pdgButtonStyle))
                {
                    _pdgAssetLink.CookOutput();
                }
            }
            GUILayout.Space(_mainButtonSeparatorDistance);
            _pdgButtonStyle.fixedHeight = 20.0f;
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(_buttonDirtyAllContent, _pdgButtonStyle))
                    {
                        _pdgAssetLink.DirtyAll();
                    }
                }
                using (new EditorGUILayout.HorizontalScope())
                {

                    if (GUILayout.Button(_buttonPauseCookContent, _pdgButtonStyle))
                    {
                        _pdgAssetLink.PauseCook();
                    }
                }
                using (new EditorGUILayout.HorizontalScope())
                {

                    if (GUILayout.Button(_buttonCancelCookContent, _pdgButtonStyle))
                    {
                        _pdgAssetLink.CancelCook();
                    }
                }
            }
            GUILayout.Space(_mainButtonSeparatorDistance);
        }
        private void DrawWorkItemTally(HEU_WorkItemTally workItemTally)
        {
            float totalWidth = EditorGUIUtility.currentViewWidth;
            float cellWidth = (totalWidth - 174.0f) / 3f;

            float titleCellHeight = 26;
            float cellHeight = 37;
            using (new EditorGUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    _pdgBoxStyleTitle.normal.textColor = ((workItemTally._scheduledWorkItems + workItemTally._cookingWorkItems) > 0) ? Color.yellow : Color.black;
                    GUILayout.Box("COOKING", _pdgBoxStyleTitle, GUILayout.Width(cellWidth), GUILayout.Height(titleCellHeight));

                    _pdgBoxStyleTitle.normal.textColor = (workItemTally._cookedWorkItems > 0) ? _cookedColor : Color.black;
                    GUILayout.Box("COOKED", _pdgBoxStyleTitle, GUILayout.Width(cellWidth), GUILayout.Height(titleCellHeight));

                    _pdgBoxStyleTitle.normal.textColor = (workItemTally._erroredWorkItems > 0) ? Color.red : Color.black;
                    GUILayout.Box("FAILED", _pdgBoxStyleTitle, GUILayout.Width(cellWidth), GUILayout.Height(titleCellHeight));

                    GUILayout.FlexibleSpace();
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.Box(string.Format("{0}", (workItemTally._scheduledWorkItems + workItemTally._cookingWorkItems)), _pdgBoxStyleValue, GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
                    GUILayout.Box(string.Format("{0}", workItemTally._cookedWorkItems), _pdgBoxStyleValue, GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
                    GUILayout.Box(string.Format("{0}", workItemTally._erroredWorkItems), _pdgBoxStyleValue, GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));

                    GUILayout.FlexibleSpace();
                }
            }
        }
        private void DrawBakeSection()
        {
            using (new EditorGUILayout.HorizontalScope(_pdgBoxStyle))
            {
                GUILayout.Label("Export Dir: ", EditorStyles.boldLabel, GUILayout.MaxWidth(90));
                EditorGUILayout.LabelField(_hAsset.ExportPath, EditorStyles.helpBox, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("...", GUILayout.MaxWidth(50)))
                {
                    string tempPath = EditorUtility.OpenFolderPanel("Export Directory", Application.dataPath, "");
                    string projectPath = Application.dataPath;

                    if (tempPath.Contains(projectPath))
                    {
                        _hAsset.ExportPath = "Assets" + tempPath.Replace(projectPath, "");
                    }
                    else
                    {
                        Debug.LogWarning("The path has to be inside the project.");
                    }
                }
            }
            using (new EditorGUILayout.HorizontalScope(_pdgBoxStyle))
            {
                if (GUILayout.Button(_bakeprefabContent, _mainButtonStyle))
                {
                    _asset.Bake();
                }
            }
        }
        private bool DrawGenerateSection(ref HEU_HoudiniAsset.AssetBuildAction pendingBuildAction)
        {
            bool bSkipAutoCook = false;
            _recookhdaContent.text = "  Recook";

            HEU_HoudiniAsset.AssetCookStatus cookStatus = _asset.GetCookStatusFromSerializedAsset(_hAssetSerialized);

            if (cookStatus == HEU_HoudiniAsset.AssetCookStatus.SELECT_SUBASSET)
            {
                // Prompt user to select subasset

                GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
                promptStyle.fontStyle = FontStyle.Bold;
                promptStyle.normal.textColor = HEU_EditorUI.IsEditorDarkSkin() ? Color.green : Color.blue;
                EditorGUILayout.LabelField("SELECT AN ASSET TO INSTANTIATE:", promptStyle);

                EditorGUILayout.Separator();

                int selectedIndex = -1;
                string[] subassetNames = _hAsset.SubassetNames;

                for (int i = 0; i < subassetNames.Length; ++i)
                {
                    if (GUILayout.Button(subassetNames[i], _mainPromptStyle))
                    {
                        selectedIndex = i;
                        break;
                    }

                    EditorGUILayout.Separator();
                }

                if (selectedIndex >= 0)
                {
                    SerializedProperty selectedIndexProperty = HEU_EditorUtility.GetSerializedProperty(_hAssetSerialized, "_selectedSubassetIndex");
                    if (selectedIndexProperty != null)
                    {
                        selectedIndexProperty.intValue = selectedIndex;
                    }
                }

                bSkipAutoCook = true;
            }
            else
            {
                HEU_EditorUI.BeginSection();
                {
                    if (cookStatus == HEU_HoudiniAsset.AssetCookStatus.COOKING || cookStatus == HEU_HoudiniAsset.AssetCookStatus.POSTCOOK)
                    {
                        _recookhdaContent.text = "  Cooking Asset";
                    }
                    else if (cookStatus == HEU_HoudiniAsset.AssetCookStatus.LOADING || cookStatus == HEU_HoudiniAsset.AssetCookStatus.POSTLOAD)
                    {
                        _reloadhdaContent.text = "  Loading Asset";
                    }

                    SerializedProperty showGenerateProperty = _hAssetSerialized.FindProperty("_showGenerateSection");

                    HEU_EditorUI.DrawSeparator();

                    using (var hs = new EditorGUILayout.HorizontalScope(_mainBoxStyle))
                    {
                        if (GUILayout.Button(_reloadhdaContent, _mainButtonStyle))
                        {
                            pendingBuildAction = HEU_HoudiniAsset.AssetBuildAction.RELOAD;
                            bSkipAutoCook = true;
                        }

                        GUILayout.Space(_mainButtonSeparatorDistance);

                        if (!bSkipAutoCook && GUILayout.Button(_recookhdaContent, _mainButtonStyle))
                        {
                            pendingBuildAction = HEU_HoudiniAsset.AssetBuildAction.COOK;
                            bSkipAutoCook = true;
                        }

                        GUILayout.Space(_mainButtonSeparatorDistance);

                        if (GUILayout.Button(_duplicateContent, _mainButtonStyle))
                        {
                            pendingBuildAction = HEU_HoudiniAsset.AssetBuildAction.DUPLICATE;
                            bSkipAutoCook = true;
                        }

                        GUILayout.Space(_mainButtonSeparatorDistance);

                        if (GUILayout.Button(_resetParamContent, _mainButtonStyle))
                        {
                            pendingBuildAction = HEU_HoudiniAsset.AssetBuildAction.RESET_PARAMS;
                            bSkipAutoCook = true;
                        }
                    }

                }

                HEU_EditorUI.EndSection();
            }

            return bSkipAutoCook;
        }
        private void DrawEditableNodes()
        {
            if (_hAsset.NumAttributeStores() > 0)
            {
                using (new EditorGUILayout.HorizontalScope(_editableNodeBoxStyle))
                {
                    HEU_EditorUI.DrawPropertyField(_hAssetSerialized, "_editableNodesToolsEnabled", "Enable Editable Node Tools", "Displays Editable Node Tools and generates the node's geometry, if asset has editable nodes.");
                }
                HEU_EditorUI.DrawSeparator();
            }
        }
        private void DrawParameters(HEU_Parameters parameters, ref Editor parameterEditor)
        {
            if (parameters != null)
            {
                SerializedObject paramObject = new SerializedObject(parameters);
                CreateCachedEditor(paramObject.targetObject, null, ref parameterEditor);
                parameterEditor.OnInspectorGUI();
            }
        }
        private void DrawCurvesSection()
        {
            if (!_hAsset.IsAssetValid())
            {
                return;
            }


            if (_hAsset.GetEditableCurveCount() <= 0)
            {
                return;
            }

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 11;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.fixedHeight = 24;
            buttonStyle.margin.left = 34;

            HEU_EditorUI.BeginSection();
            {
                List<HEU_Curve> curves = _hAsset.Curves;

                SerializedProperty showCurvesProperty = HEU_EditorUtility.GetSerializedProperty(_hAssetSerialized, "_showCurvesSection");
                if (showCurvesProperty != null)
                {

                    List<SerializedObject> serializedCurves = new List<SerializedObject>();
                    for (int i = 0; i < curves.Count; i++)
                    {
                        serializedCurves.Add(new SerializedObject(curves[i]));
                    }

                    bool bHasBeenModifiedInInspector = false;

                    for (int i = 0; i < serializedCurves.Count; i++)
                    {
                        HEU_Curve curve = curves[i];

                        SerializedObject serializedCurve = serializedCurves[i];
                        EditorGUI.BeginChangeCheck();

                        if (curve.CurveDataType == HEU_CurveDataType.HAPI_COORDS_PARAM)
                        {

                            // Create the UI manually to have more control
                            SerializedProperty inputCurveInfoProperty = HEU_EditorUtility.GetSerializedProperty(serializedCurve, "_inputCurveInfo");

                            System.Action<int> onCurveTypeChanged = (int value) =>
                            {
                                SerializedProperty orderProperty = inputCurveInfoProperty.FindPropertyRelative("order");
                                int curOrder = orderProperty.intValue;

                                HAPI_CurveType curveType = (HAPI_CurveType)value;
                                if (curOrder < 4 && (curveType == HAPI_CurveType.HAPI_CURVETYPE_NURBS || curveType == HAPI_CurveType.HAPI_CURVETYPE_BEZIER))
                                {
                                    orderProperty.intValue = 4;
                                }
                                else if (curveType == HAPI_CurveType.HAPI_CURVETYPE_LINEAR)
                                {
                                    orderProperty.intValue = 2;
                                }
                            };


                            HEU_EditorUtility.EnumToPopup(
                                inputCurveInfoProperty.FindPropertyRelative("curveType"),
                                "Curve Type",
                                (int)curve.InputCurveInfo.curveType,
                                HEU_InputCurveInfo.GetCurveTypeNames(),
                                true,
                                "Type of the curve. Can be Linear, NURBs or Bezier. May impose restrictions on the order depending on what you choose.",
                                onCurveTypeChanged
                            );

                            EditorGUILayout.PropertyField(inputCurveInfoProperty.FindPropertyRelative("order"));

                            EditorGUILayout.PropertyField(inputCurveInfoProperty.FindPropertyRelative("closed"));

                            EditorGUILayout.PropertyField(inputCurveInfoProperty.FindPropertyRelative("reverse"));

                            HEU_EditorUtility.EnumToPopup(
                                inputCurveInfoProperty.FindPropertyRelative("inputMethod"),
                                "Input Method",
                                (int)curve.InputCurveInfo.inputMethod,
                                HEU_InputCurveInfo.GetInputMethodNames(),
                                true,
                                "How the curve behaves with respect to the provided CVs. Can be either CVs, which influence the curve, or breakpoints, which intersects the curve."
                            );

                        }


                        if (EditorGUI.EndChangeCheck())
                        {
                            curves[i].SetEditState(HEU_Curve.CurveEditState.REQUIRES_GENERATION);
                            serializedCurve.ApplyModifiedProperties();

                            bHasBeenModifiedInInspector = true;
                        }

                    }

                    if (bHasBeenModifiedInInspector)
                    {
                        if (_hAsset.GetEditableCurveCount() > 0)
                        {
                            HEU_Curve[] curvesArray = _hAsset.Curves.ToArray();
                            Editor.CreateCachedEditor(curvesArray, null, ref _curveEditor);
                            (_curveEditor as HEU_CurveUI).RepaintCurves();

                            if (HEU_PluginSettings.CookingEnabled && _hAsset.AutoCookOnParameterChange)
                            {
                                _hAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: false, bUploadParameters: true);
                            }
                        }
                    }

                    HEU_EditorUI.DrawSeparator();

                    _projectCurvePointsButton.text = "Project to Ground";
                    using (var hs = new EditorGUILayout.HorizontalScope(_mainBoxStyle))
                    {
                        if (GUILayout.Button(_projectCurvePointsButton, _mainButtonStyle, GUILayout.MaxWidth(180)))
                        {
                            for (int i = 0; i < curves.Count; ++i)
                            {
                                CZ_CurveExt.ProjectToZero(curves[i]);
                            }
                        }
                    }

                }
            }
            HEU_EditorUI.EndSection();

            HEU_EditorUI.DrawSeparator();
        }
        private void DrawSceneElements(HEU_HoudiniAsset hAsset)
        {
            if (hAsset == null || !hAsset.IsAssetValid())
            {
                return;
            }

            // Curve Editor
            if (hAsset.CurveEditorEnabled)
            {
                if (hAsset.GetEditableCurveCount() > 0)
                {
                    HEU_Curve[] curvesArray = hAsset.Curves.ToArray();
                    Editor.CreateCachedEditor(curvesArray, null, ref _curveEditor);
                    (_curveEditor as HEU_CurveUI).UpdateSceneCurves(hAsset);

                    bool bRequiresCook = !System.Array.TrueForAll(curvesArray, c => c.EditState != HEU_Curve.CurveEditState.REQUIRES_GENERATION);
                    if (bRequiresCook && HEU_PluginSettings.CookingEnabled && hAsset.AutoCookOnParameterChange)
                    {
                        _hAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: false, bUploadParameters: true);
                    }
                }
            }

            // Tools Editor
            if (hAsset.EditableNodesToolsEnabled)
            {
                List<HEU_AttributesStore> attributesStores = hAsset.AttributeStores;
                if (attributesStores.Count > 0)
                {
                    HEU_AttributesStore[] attributesStoresArray = attributesStores.ToArray();
                    Editor.CreateCachedEditor(attributesStoresArray, null, ref _toolsEditor);
                    HEU_ToolsUI toolsUI = (_toolsEditor as HEU_ToolsUI);
                    toolsUI.DrawToolsEditor(hAsset);

                    if (hAsset.ToolsInfo._liveUpdate && !hAsset.ToolsInfo._isPainting)
                    {
                        bool bAttributesDirty = !System.Array.TrueForAll(attributesStoresArray, s => !s.AreAttributesDirty());
                        if (bAttributesDirty)
                        {
                            _hAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: false, bUploadParameters: true);
                        }
                    }
                }
            }

            // Handles
            if (hAsset.HandlesEnabled)
            {
                List<HEU_Handle> handles = hAsset.GetHandles();
                if (handles.Count > 0)
                {
                    HEU_Handle[] handlesArray = handles.ToArray();
                    Editor.CreateCachedEditor(handlesArray, null, ref _handlesEditor);
                    HEU_HandlesUI handlesUI = (_handlesEditor as HEU_HandlesUI);
                    bool bHandlesChanged = handlesUI.DrawHandles(hAsset);

                    if (bHandlesChanged)
                    {
                        _hAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: false, bUploadParameters: true);
                    }
                }
            }
        }


        private void OnEnable()
        {
            SetStylesDescriptions();
            _sceneView = SceneView.lastActiveSceneView; //EditorWindow.GetWindow<SceneView>();
            if(_asset != null)
            {
                _asset.TryAcquiringAsset(); 
            }
        }
        private void DrawMessage(string msg)
        {
            HEU_EditorUI.DrawSeparator();

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = HEU_EditorUI.IsEditorDarkSkin() ? Color.yellow : Color.red;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.wordWrap = true;
            EditorGUILayout.LabelField(msg, labelStyle);

            HEU_EditorUI.DrawSeparator();
        }
        #endregion
    }
}