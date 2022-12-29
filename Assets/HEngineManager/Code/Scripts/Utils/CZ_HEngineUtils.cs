using UnityEngine;
using HoudiniEngineUnity;
using System.IO;
using System;
using UnityEditor;

namespace CZ.HEngineTools
{
    public static class CZ_HEngineUtils
    {
        public static void CreateHEngineSession()
        {
            bool bResult = HEU_SessionManager.CreateThriftPipeSession(
            HEU_PluginSettings.Session_PipeName,
            HEU_PluginSettings.Session_AutoClose,
            HEU_PluginSettings.Session_Timeout, true);
            if (!bResult)
            {
                HEU_EditorUtility.DisplayErrorDialog("Create Session", HEU_SessionManager.GetLastSessionError(), "OK");
            }
        }
        public static void CloseAllSessions()
        {
            HEU_SessionManager.CloseAllSessions();
            HEU_Logger.Log("Houdini Engine Sessions closed!");
        }

        public static void RestartHEngineSession()
        {
            bool bResult = HEU_SessionManager.LoadStoredDefaultSession();
            if (!bResult)
            {
                HEU_EditorUtility.DisplayDialog("Reconnecting to Session", HEU_SessionManager.GetLastSessionError(), "OK");
            }
            else
            {
                HEU_Logger.Log("Houdini Engine Session reconnected.");
            }
        }

        public static void CookAsset(CZ_HoudiniAssetExt houdiniAssetExt)
        {
            if (houdiniAssetExt.HoudiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: true, bUploadParameters: true))
            {
                //Debug.Log("Cooked HDA: " + houdiniAsset.AssetName);
            }
        }

        public static HEU_SessionBase GetHoudiniSession()
        {
            HEU_SessionBase session = HEU_SessionManager.GetOrCreateDefaultSession();
            if (!session.IsSessionValid())
            {
                Debug.LogWarning("Houdini Engine session doesnt exist!");
                return null;
            }
            return session;
        }
        public static CZ_HoudiniAssetExt GetHoudiniAssetExt(GameObject rootGO, bool showWarning = true)
        {
            HEU_HoudiniAssetRoot rootAsset = rootGO.GetComponent<HEU_HoudiniAssetRoot>();

            if (!rootAsset)
            {
                if (showWarning) { Debug.LogWarning("Couldnt get Houdini Asset Root!"); }
                return null;
            }

            if (!rootAsset.HoudiniAsset)
            {
                if (showWarning) { Debug.LogWarning("Couldnt get Houdini Asset Root!"); }
                return null;
            }

            CZ_HoudiniAssetExt houdiniAssetExt = new CZ_HoudiniAssetExt(rootAsset.HoudiniAsset);

            return houdiniAssetExt;
        }
        public static GameObject InstanceHDA(string path)
        {
            HEU_SessionBase session = GetHoudiniSession();
            if (!session.IsSessionValid())
            {
                return null;
            }

            GameObject rootGO = HEU_HAPIUtility.InstantiateHDA(path, Vector3.zero, session, true);
            if (rootGO != null)
            {
                HEU_EditorUtility.SelectObject(rootGO);
                CZ_HoudiniAssetExt houdiniAssetExt = GetHoudiniAssetExt(rootGO);
                if (houdiniAssetExt.HoudiniAsset)
                {
                    houdiniAssetExt.HoudiniAsset.ExportPath = CZ_HEngineManager.GlobalExportPath;
                    CookAsset(houdiniAssetExt);

                }
            }
            return rootGO;
        }
        public static string GetHDAVerboseNameFromPath(string path)
        {
            string filename = Path.GetFileName(path);

            string[] tokens = { ".hda", ".otl", ".hdalc" };
            string[] actualName = filename.Split(tokens, StringSplitOptions.RemoveEmptyEntries);

            tokens = new string[] { "_" };
            actualName = actualName[0].Split(tokens, StringSplitOptions.RemoveEmptyEntries);

            string finalname = actualName[0];
            if (actualName.Length > 1)
            {
                finalname = actualName[1];
            }

            return finalname;
        }
        public static string CreateUniqueBakePath(string assetName, string path = null)
        {
            string assetBakedPath;
            if (path != null)
            {
                assetBakedPath =  HEU_Platform.BuildPath(path, assetName);
            }
            assetBakedPath =  HEU_Platform.BuildPath(HEU_AssetDatabase.GetAssetBakedPath(), assetName);
            assetBakedPath = AssetDatabase.GenerateUniqueAssetPath(assetBakedPath);

            if (!HEU_Platform.DoesPathExist(assetBakedPath))
            {
                HEU_AssetDatabase.CreatePathWithFolders(assetBakedPath);
            }

            return assetBakedPath;
        }
    }

}