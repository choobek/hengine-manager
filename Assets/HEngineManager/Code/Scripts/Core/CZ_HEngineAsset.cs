using System.Collections;
using UnityEngine;
using UnityEditor;
using HoudiniEngineUnity;
using System.Collections.Generic;

namespace CZ.HEngineTools
{
    public class CZ_HEngineAsset
    {
        #region Variabes
        CZ_HoudiniAssetExt _houdiniAssetExt;
        HEU_HoudiniAsset _hAsset;
        SerializedObject _hAssetSerialized;
        HEU_PDGAssetLink _pdgAssetLink;
        #endregion

        #region Getters/Setters
        public CZ_HoudiniAssetExt HoudiniAssetExt
            {
                get { return _houdiniAssetExt; }
                set { _houdiniAssetExt = value; }
            }
        public SerializedObject SerializedObject
        {
            get { return _hAssetSerialized; }
            set { _hAssetSerialized = value; }
        }
        public HEU_PDGAssetLink PDGAssetLink
        {
            get { return _pdgAssetLink;}
            set { _pdgAssetLink = value; }
        }
        #endregion

        #region Methods
        public void Set(CZ_HoudiniAssetExt houdiniAssetExt, HEU_PDGAssetLink pdgAssetLink = null)
        {
            _houdiniAssetExt = houdiniAssetExt;
            _hAsset = houdiniAssetExt.HoudiniAsset;
            _hAssetSerialized = new SerializedObject(_houdiniAssetExt.HoudiniAsset);
            _pdgAssetLink = pdgAssetLink;
        }
        public SerializedObject TryAcquiringAsset()
        {
            if(_houdiniAssetExt== null)
            {
                return null;
            }
            if (_houdiniAssetExt.HoudiniAsset != null && _hAssetSerialized == null)
            {
                _hAssetSerialized = new SerializedObject(_houdiniAssetExt.HoudiniAsset);
            }
            return _hAssetSerialized;
        }
        public void Recook(bool bSkipAutoCook)
        {

            // Check options that require a rebuild/recook if changed.
            bool oldUseOutputNodes = _hAsset.UseOutputNodes;
            bool oldUsePoints = _hAsset.GenerateMeshUsingPoints;

            _hAssetSerialized.ApplyModifiedProperties();

            bool bNeedsRebuild = false;
            bool bNeedsRecook = false;

            // UseOutputNodes is a special parameter that requires us to rebuild in order to use it.
            if (_hAsset.UseOutputNodes != oldUseOutputNodes)
            {
                bNeedsRebuild = true;
            }

            if (_hAsset.GenerateMeshUsingPoints != oldUsePoints)
            {
                bNeedsRecook = true;
            }
            if (!bSkipAutoCook)
            {
                // If we need a rebuild, do that first
                if (HEU_PluginSettings.CookingEnabled && _hAsset.AutoCookOnParameterChange && bNeedsRebuild)
                {
                    _hAsset.RequestReload(true);
                }
                else if (bNeedsRecook)
                {
                    _hAsset.RequestCook();
                }
                else if (HEU_PluginSettings.CookingEnabled && _hAsset.AutoCookOnParameterChange && _hAsset.DoesAssetRequireRecook())
                {
                    // Often times, cooking while dragging mouse results in poor UX
                    bool isDragging = (EditorGUIUtility.hotControl != 0);
                    bool blockAutoCook = _hAsset.PendingAutoCookOnMouseRelease == true || (isDragging && Event.current != null);

                    if (HEU_PluginSettings.CookOnMouseUp && blockAutoCook)
                    {
                        _hAsset.PendingAutoCookOnMouseRelease = true;
                    }
                    else
                    {
                        _hAsset.PendingAutoCookOnMouseRelease = false;
                        _hAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: false, bUploadParameters: true);
                    }
                }
            }
        }
        public void Bake()
        {
            GameObject selectedGO = Selection.activeGameObject;
            if (selectedGO.name.Contains("LVLTool"))
            {
                _houdiniAssetExt.BakeLVLToolToPrefab(_hAsset.ExportPath);
            }
            else if (selectedGO.name.Contains("Scatter"))
            {
                _houdiniAssetExt.BakeScatterToNewPrefab(_hAsset.ExportPath);
            }
            else
            {
                _hAsset.BakeToNewPrefab(_hAsset.ExportPath);
            }
        }

        internal void ProcessPendingBuildAction(
        HEU_HoudiniAsset.AssetBuildAction pendingBuildAction,
        SerializedProperty pendingBuildProperty,
        HEU_HoudiniAsset asset,
        SerializedObject assetObject)
        {
            if (pendingBuildAction != HEU_HoudiniAsset.AssetBuildAction.NONE)
            {
                // Sanity check to make sure the asset is part of the AssetUpater
                HEU_AssetUpdater.AddAssetForUpdate(asset);

                // Apply pending build action based on user UI interaction above
                pendingBuildProperty.enumValueIndex = (int)pendingBuildAction;

                if (pendingBuildAction == HEU_HoudiniAsset.AssetBuildAction.COOK)
                {
                    // Recook should only update parameters that haven't changed. Otherwise if not checking and updating parameters,
                    // then buttons will trigger callbacks on Recook which is not desired.
                    SerializedProperty checkParameterChange = HEU_EditorUtility.GetSerializedProperty(assetObject, "_checkParameterChangeForCook");
                    if (checkParameterChange != null)
                    {
                        checkParameterChange.boolValue = true;
                    }

                    // But we do want to always upload input geometry on user hitting Recook expliclity
                    SerializedProperty forceUploadInputs = HEU_EditorUtility.GetSerializedProperty(assetObject, "_forceUploadInputs");
                    if (forceUploadInputs != null)
                    {
                        forceUploadInputs.boolValue = true;
                    }
                }
            }
        }

        internal HEU_HoudiniAsset.AssetCookStatus GetCookStatusFromSerializedAsset(SerializedObject hAssetSerialized)
        {
            HEU_HoudiniAsset.AssetCookStatus cookStatus = HEU_HoudiniAsset.AssetCookStatus.NONE;

            SerializedProperty cookStatusProperty = HEU_EditorUtility.GetSerializedProperty(hAssetSerialized, "_cookStatus");
            if (cookStatusProperty != null)
            {
                cookStatus = (HEU_HoudiniAsset.AssetCookStatus)cookStatusProperty.enumValueIndex;
            }

            return cookStatus;
        }
        #endregion

    }
}