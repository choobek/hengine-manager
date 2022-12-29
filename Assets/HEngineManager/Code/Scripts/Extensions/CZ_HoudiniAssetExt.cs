using HoudiniEngineUnity;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CZ.HEngineTools
{
    public class CZ_HoudiniAssetExt
    {
        #region Variables
        HEU_HoudiniAsset _houdiniAsset;
        #endregion

        #region Getters/Setters
        public HEU_HoudiniAsset HoudiniAsset { get { return _houdiniAsset; } set { _houdiniAsset = value; } }
        #endregion

        #region Constructor
        public CZ_HoudiniAssetExt(HEU_HoudiniAsset houdiniAsset)
        {
            _houdiniAsset = houdiniAsset;
        }
        #endregion

        #region Methods
        public bool IsValidForInteraction(GameObject go, ref string errorMessage)
        {
            bool valid = true;
            if (HEU_EditorUtility.IsPrefabAsset(go))
            {
                // Disable UI when HDA is prefab
                errorMessage = "Houdini Engine Asset Error\n" +
                    "HDA as prefab not supported!";
                valid = false;
            }
            else
            {
#if UNITY_EDITOR && UNITY_2018_3_OR_NEWER
#if UNITY_2021_2_OR_NEWER
                var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
		var stage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif

                if (stage != null)
                {
                    // Disable UI when HDA is in prefab stage
                    errorMessage = "Houdini Engine Asset Error\n" +
                        "HDA as prefab not supported!";
                    valid = false;
                }
#endif
            }
            return valid;
        }

        public GameObject BakeScatterToNewPrefab(string destinationPrefabPath = null)
        {
            if (_houdiniAsset.PreAssetEvent != null)
            {
                _houdiniAsset.PreAssetEvent.Invoke(new HEU_PreAssetEventData(_houdiniAsset, HEU_AssetEventType.BAKE_NEW));
            }
            // This creates a temporary clone of the asset without the HDA data
            // in the scene, then creates a prefab of the cloned object.

            string bakedAssetPath = null;
            if (!string.IsNullOrEmpty(destinationPrefabPath))
            {
                char[] trimChars = { '/', '\\' };
                bakedAssetPath = destinationPrefabPath.TrimEnd(trimChars);
            }

            bool bWriteMeshesToAssetDatabase = false;
            bool bReconnectPrefabInstances = false;
            GameObject newClonedRoot = _houdiniAsset.CloneAssetWithoutHDA(ref bakedAssetPath, bWriteMeshesToAssetDatabase, bReconnectPrefabInstances);
            if (newClonedRoot != null)
            {
                try
                {
                    for (int i = 0; i < (int)newClonedRoot.transform.childCount; i++)
                    {
                        if (newClonedRoot.transform.GetChild(i).name.Contains("ScatterMasks"))
                        {
                            HEU_GeneralUtility.DestroyImmediate(newClonedRoot.transform.GetChild(i).gameObject);
                        }
                    }

                    if (string.IsNullOrEmpty(bakedAssetPath))
                    {
                        // Need to create the baked folder to store the prefab
                        bakedAssetPath = HEU_AssetDatabase.CreateUniqueBakePath(_houdiniAsset.AssetName);
                    }

                    string prefabPath = HEU_AssetDatabase.AppendPrefabPath(bakedAssetPath, _houdiniAsset.AssetName);
                    GameObject prefabGO = HEU_EditorUtility.SaveAsPrefabAsset(prefabPath, newClonedRoot);
                    if (prefabGO != null)
                    {
                        HEU_EditorUtility.SelectObject(prefabGO);

                        _houdiniAsset.InvokeBakedEvent(true, new List<GameObject>() { prefabGO }, true);

                        HEU_Logger.LogFormat("Exported prefab to {0}", bakedAssetPath);
                    }
                    return prefabGO;
                }
                finally
                {
                    // Don't need the new object anymore since its just prefab that's required
                    HEU_GeneralUtility.DestroyImmediate(newClonedRoot);
                }
            }
            return null;
        }

        public void BakeLVLToolToPrefab(string destinationPrefabPath = null)
        {
            GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject gameObject in allGameObjects)
            {
                if (gameObject.name.StartsWith("MadWar_") && gameObject.name.EndsWith("_OUTPUTS"))
                {
                    //Debug.Log("Found GameObject: " + gameObject.name);
                    Transform terrainOutGO = gameObject.transform.GetChild(0);
                    if (terrainOutGO != null && terrainOutGO.name.StartsWith("HE_OUT"))
                    {

                        for (int i = 0; i < (int)terrainOutGO.childCount; i++)
                        {
                            Transform singleTerrainOut = terrainOutGO.GetChild(i);


                            // Bake the asset using HEU_GeoSync interface
                            HEU_GeoSync assetToBake = singleTerrainOut.gameObject.GetComponent<HEU_GeoSync>();
                            if (assetToBake != null)
                            {
                                BakeGeoSync(assetToBake);
                            }
                            else
                            {
                                Debug.Log("There are no output terrains to bake.");
                            }

                            //DestroyImmediate(terrainOutGO.GetChild(i).gameObject);
                        }

                    }


                }
            }
        }

        private void BakeGeoSync(HEU_GeoSync assetToBake)
        {
            if (assetToBake._syncing)
            {
                return;
            }
            string outputPath = CZ_HEngineUtils.CreateUniqueBakePath(assetToBake.gameObject.name, _houdiniAsset.ExportPath);
            GameObject parentObj = HEU_GeneralUtility.CreateNewGameObject(assetToBake.gameObject.name);

            foreach (HEU_GeneratedOutput generatedOutput in assetToBake._generatedOutputs)
            {
                //Debug.Log(generatedOutput._outputData._gameObject.name + " is instancer?: " + generatedOutput.IsInstancer);
                if (!generatedOutput.IsInstancer)
                {
                    GameObject obj = HEU_GeneralUtility.CreateNewGameObject(generatedOutput._outputData._gameObject.name);

                    generatedOutput.WriteOutputToAssetCache(obj, outputPath, generatedOutput.IsInstancer);

                    obj.transform.parent = parentObj.transform;
                }
            }

            string prefabPath = HEU_AssetDatabase.AppendPrefabPath(outputPath, parentObj.name);
            GameObject prefabGO = HEU_EditorUtility.SaveAsPrefabAsset(prefabPath, parentObj);
            if (prefabGO != null)
            {
                HEU_EditorUtility.SelectObject(prefabGO);

                HEU_Logger.LogFormat("Exported prefab to {0}", outputPath);
            }

            GameObject.DestroyImmediate(parentObj);
        }
        #endregion
    }
}