using HoudiniEngineUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CZ.HEngineTools
{
    public class CZ_HEngineManagerUI : EditorWindow
    {
        #region Variables
        bool _isInitialized;
        bool _showSession;
        bool _showTools;
        bool _showSelectedSection;

        int _toolsCols;
        int _toolsRows;

        GameObject _selectedGO;
        CZ_HEngineAsset _asset;
        #endregion

        #region Style Variables
        GUIStyle _bgStyle = new GUIStyle();
        GUIStyle _horizontalStyle = new GUIStyle();
        GUIStyle _headerStyle = new GUIStyle();
        GUIStyle _sessionBtnStyle = new GUIStyle();
        GUIStyle _btnGridStyle = new GUIStyle();
        GUIStyle _pathBtnStyle = new GUIStyle();
        GUIStyle _hdaBtnStyle = new GUIStyle();
        GUIStyle _hdaBtnLabelStyle = new GUIStyle();

        GUIContent _startSessionContent = new GUIContent();
        GUIContent _closeSessionContent = new GUIContent();
        GUIContent _restartSessionContent = new GUIContent();
        GUIContent _hdaBtnContent = new GUIContent();

        Rect _btnGridRect = new Rect();

        string _goLabel;
        CZ_HEngineAssetUI _assetUI;
        #endregion

        #region Builtin Methods
        private void OnEnable()
        {
            _isInitialized = false;
            Selection.selectionChanged += SelectionChangedCallback;
            _asset = new CZ_HEngineAsset();
            _assetUI = CreateInstance("CZ_HEngineAssetUI") as CZ_HEngineAssetUI;
            _assetUI.Asset = _asset;
            SelectionChangedCallback();
        }
        private void OnDisable()
        {
            Selection.selectionChanged -= SelectionChangedCallback;
        }
        private void OnGUI()
        {
            if (!_isInitialized)
            {
                CZ_HEngineManager.LoadConfig();
                InitStyles();
                _isInitialized = true;
            }

            CZ_HEngineManager.UpdateHDAFiles();
            DrawUI();
        }
        #endregion

        #region Custom Methods
        private void InitStyles()
        {
            // BG Style
            Color bgColor = new Color(0.15f, 0.15f, 0.15f);
            _bgStyle.padding = new RectOffset(0, 0, 0, 0);
            _bgStyle.normal.background = CZ_HEngineImgUtils.MakeBgTexture(bgColor);

            // Button Grid Style
            _btnGridStyle.padding = new RectOffset(10, 10, 10, 10);

            // Horizontal Style
            _horizontalStyle.padding = new RectOffset(10, 10, 10, 10);

            // Header Style
            _headerStyle = new GUIStyle(EditorStyles.foldoutHeader);
            _headerStyle.padding = new RectOffset(20, 0, 0, 0);
            _headerStyle.fixedHeight = 30;
            _headerStyle.fontSize = 14;

            // Session Button Style
            _sessionBtnStyle = new GUIStyle(GUI.skin.button);
            _sessionBtnStyle.fixedHeight = 40;
            _sessionBtnStyle.padding = new RectOffset(0, 0, 10, 10);
            _sessionBtnStyle.fontStyle = FontStyle.Bold;

            // Path Button Style
            _pathBtnStyle = new GUIStyle(GUI.skin.button);
            _pathBtnStyle.fixedHeight = 18;

            // HDA Button Style
            float buttonHDASize = 70.0f;
            _hdaBtnStyle = new GUIStyle(GUI.skin.button);
            _hdaBtnStyle.fixedHeight = buttonHDASize;

            // HDA Button Label Style
            _hdaBtnLabelStyle.alignment = TextAnchor.MiddleCenter;
            _hdaBtnLabelStyle.fontStyle = FontStyle.Bold;
            _hdaBtnLabelStyle.margin = new RectOffset(0, 0, 0, 10);
            _hdaBtnLabelStyle.normal.textColor = Color.white;

            // Start Session Content
            _startSessionContent.text = " Start Session";
            _startSessionContent.image = CZ_HEngineImgUtils.FetchIcon("Assets/HEngineManager/Icons/start-icon.png");

            // Close Session Content
            _closeSessionContent.text = " Close Session";
            _closeSessionContent.image = CZ_HEngineImgUtils.FetchIcon("Assets/HEngineManager/Icons/close-icon.png");

            // Restart Session Content
            _restartSessionContent.text = " Restart Session";
            _restartSessionContent.image = CZ_HEngineImgUtils.FetchIcon("Assets/HEngineManager/Icons/restart-icon.png");

            // HDA Button Content
            _hdaBtnContent.text = "";
            _hdaBtnContent.image = CZ_HEngineImgUtils.FetchIcon(("Assets/HEngineManager/Icons/houdini-icon.png"));
        }



        private void DrawUI()
        {
            DrawSessionSection();
            DrawToolsSection();
            DrawSelectionSection();
        }

        private void DrawSessionSection()
        {
            HEU_SessionData sessionData = HEU_SessionManager.GetSessionData();
            using (new EditorGUILayout.VerticalScope(_bgStyle))
            {
                _showSession = EditorGUILayout.BeginFoldoutHeaderGroup(_showSession, "Session", _headerStyle);
                if (_showSession)
                {
                    using (new EditorGUILayout.HorizontalScope(_horizontalStyle))
                    {
                        if (sessionData == null)
                        {
                            // Start Session Button
                            if (GUILayout.Button(_startSessionContent, _sessionBtnStyle))
                            {
                                CZ_HEngineUtils.CreateHEngineSession();
                            }
                        }
                        else
                        {
                            // Close All Sessions Button
                            if (GUILayout.Button(_closeSessionContent, _sessionBtnStyle))
                            {
                                CZ_HEngineUtils.CloseAllSessions();
                            }
                            // Restart Session Button
                            if (GUILayout.Button(_restartSessionContent, _sessionBtnStyle))
                            {
                                CZ_HEngineUtils.RestartHEngineSession();
                            }
                        }


                    }
                    using (new EditorGUILayout.HorizontalScope(_horizontalStyle))
                    {
                        MessageType message = MessageType.Info;
                        if (sessionData == null)
                        {
                            message = MessageType.Warning;
                        }
                        EditorGUILayout.HelpBox(HEU_SessionManager.GetSessionInfo() + "\n", message, true);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
        private void DrawToolsSection()
        {
            using (var ButtonScope = new EditorGUILayout.VerticalScope(_bgStyle))
            {
                _btnGridRect = ButtonScope.rect;
                UpdateButtonGrid();

                _showTools = EditorGUILayout.BeginFoldoutHeaderGroup(_showTools, "Tools", _headerStyle);
                if (_showTools)
                {
                    using (new EditorGUILayout.HorizontalScope(_horizontalStyle))
                    {
                        GUILayout.Label("Working Dir: ", EditorStyles.boldLabel, GUILayout.MaxWidth(90));
                        EditorGUILayout.LabelField(CZ_HEngineManager.RootPath, EditorStyles.helpBox, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("...", _pathBtnStyle, GUILayout.MaxWidth(50)))
                        {
                            CZ_HEngineManager.RootPath = EditorUtility.OpenFolderPanel("Working Directory", Application.dataPath, "");
                            CZ_HEngineManager.UpdateHDAFiles();
                        }
                    }
                    EditorGUILayout.BeginVertical(_btnGridStyle);
                    int index = 0;
                    for (int i = 0; i < _toolsRows; i++)
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal();
                        for (int x = 0; x < _toolsCols; x++)
                        {
                            if (index < CZ_HEngineManager.HDAPaths.Count)
                            { 
                                CZ_HEngineTool tool = new CZ_HEngineTool(CZ_HEngineManager.HDAPaths[index]);
                                _hdaBtnContent.image = tool.Icon;
                                EditorGUILayout.BeginVertical();

                                if (GUILayout.Button(_hdaBtnContent, _hdaBtnStyle))
                                {
                                    _selectedGO = tool.Cook();
                                }

                                GUILayout.Label(tool.Name, _hdaBtnLabelStyle);
                                EditorGUILayout.EndVertical();

                            }
                            index++;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        private void UpdateButtonGrid()
        {
            if (Event.current.type == EventType.Repaint)
            {
                int numButtons = CZ_HEngineManager.HDAPaths.Count;
                _toolsCols = Mathf.Clamp(((int)((_btnGridRect.width - 100.0f) / 100.0f)), 1, numButtons);
                if (_toolsCols > 0)
                {
                    _toolsRows = numButtons / _toolsCols;
                    int remainder = numButtons % _toolsCols;
                    if (remainder > 0)
                    {
                        _toolsRows += 1;

                    }
                }
            }
            Repaint();
        }

        private void DrawSelectionSection()
        {
            HEU_SessionData sessionData = HEU_SessionManager.GetSessionData();
            using (new EditorGUILayout.VerticalScope(_bgStyle))
            {
                _showSelectedSection = EditorGUILayout.BeginFoldoutHeaderGroup(_showSelectedSection, _goLabel, _headerStyle);

                // If the foldout header group is expanded and there is some object selected
                if (_showSelectedSection && _selectedGO)
                {
                    // If the selected GameObject has a HoudiniAsset component attached to it
                    if (CZ_HEngineUtils.GetHoudiniAssetExt(_selectedGO, false) != null)
                    {
                        if (sessionData != null && _asset != null)
                        {
                            _assetUI.Asset = _asset;
                            _assetUI.DrawGUI();
                            void OnSceneChanged(Scene previousScene, Scene newScene) => _assetUI.OnSceneGUI();
                            SceneManager.activeSceneChanged += OnSceneChanged;
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("No Houdini Engine Session", MessageType.Warning, true);
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Selected object is not an HDA", MessageType.Warning, true);

                    }
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        void SelectionChangedCallback()
        {
            _selectedGO = Selection.activeGameObject;
            if (_selectedGO)
            {
                _goLabel = _selectedGO.name;
                CZ_HoudiniAssetExt houdiniAssetExt = CZ_HEngineUtils.GetHoudiniAssetExt(_selectedGO, false);
                if (houdiniAssetExt != null)
                {
                    HEU_PDGAssetLink pdgAssetLink = _selectedGO.GetComponent<HEU_PDGAssetLink>();
                    _asset.Set(houdiniAssetExt, pdgAssetLink);

                }
            } else
            {
                _goLabel = "No Object Selected...";
            }
            Repaint();
        }

        #endregion
    }
}