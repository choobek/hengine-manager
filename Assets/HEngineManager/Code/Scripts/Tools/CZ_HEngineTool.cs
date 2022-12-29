using HoudiniEngineUnity;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CZ.HEngineTools
{
    public class CZ_HEngineTool
    {
        #region Variables
        string _hdaPath;
        string _name = "No name.";
        Texture _icon;

        #endregion

        #region Getters/Setters
        public string HDAPath
        {
            get { return _hdaPath; }
            set { _hdaPath = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public Texture Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }
        #endregion

        #region Constructor
        public CZ_HEngineTool(string path)
        {
            if (path == "Curve")
            {
                _hdaPath = null;
                _name = path;
                _icon = CZ_HEngineImgUtils.FetchIcon(("Assets/HEngineManager/Icons/curve-icon.png"));
            }
            else
            {
                _hdaPath = path;
                string hdaName = CZ_HEngineUtils.GetHDAVerboseNameFromPath(_hdaPath);
                _name = hdaName.Split('.')[0];
                _icon = CZ_HEngineImgUtils.FetchIcon(("Assets/HEngineManager/Icons/" + _name + "-icon.png"));
                if (_icon == null)
                {
                    _icon = CZ_HEngineImgUtils.FetchIcon(("Assets/HEngineManager/Icons/houdini-icon.png"));
                }
            }
        }
        #endregion

        #region Methods
        public GameObject Cook()
        {
            GameObject selectedGO;
            if(Name == "Curve")
            {
                selectedGO = CookCurveTool();
            }
            else if (Name == "LVLTool")
            {
                selectedGO = CookLVLTool();
            }
            else
            {
                selectedGO = CZ_HEngineUtils.InstanceHDA(_hdaPath);
            }
            return selectedGO;
        }

        private GameObject CookCurveTool()
        {
            GameObject newCurveGO = HEU_HAPIUtility.CreateNewCurveAsset();
            if (newCurveGO != null)
            {
                HEU_Curve.PreferredNextInteractionMode = HEU_Curve.Interaction.ADD;
                HEU_EditorUtility.SelectObject(newCurveGO);
                //return newCurveGO;
            }
            //newCurveGO = new GameObject("Curve");
            return newCurveGO;
        }

        private GameObject CookLVLTool()
        {
            if (IsToolAlreadyInTheScene("MadWar_LVLTool"))
            {
                Debug.LogWarning("MadWar_LVLTool is already in the scene!");
                return null;
            }
            else
            {
                GameObject selectedGO = CZ_HEngineUtils.InstanceHDA(_hdaPath);
                CZ_HoudiniAssetExt houdiniAssetExt = CZ_HEngineUtils.GetHoudiniAssetExt(selectedGO);
                HEU_ParameterUtility.SetString(houdiniAssetExt.HoudiniAsset, "pdg_workingdir", CZ_HEngineManager.RootPath);
                CZ_HEngineUtils.CookAsset(houdiniAssetExt);
                return selectedGO;
            }
        }

        private static bool IsToolAlreadyInTheScene(string name)
        {
            bool isAlreadyInScene = false;
            GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject gameObject in allGameObjects)
            {
                if (gameObject.GetComponent<HEU_HoudiniAssetRoot>() != null && gameObject.name.StartsWith(name))
                {
                    isAlreadyInScene = true;
                }
            }
            return isAlreadyInScene;
        } 
        #endregion
    }
}