using HoudiniEngineUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CZ.HEngineTools
{
    public static class CZ_HEngineManager
    {
        #region Variables
        static string s_rootPath;
        static string s_hdaPath;
        static string s_globalExportPath;
        static List<string> s_tools = new List<string>();
        static List<string> s_hdaPaths = new List<string>();
        #endregion


        #region Getters/Setters
        public static string RootPath
        {
            get { return s_rootPath; }
            set
            {
                if(value.Length > 0)
                {
                    s_rootPath = value;
                    s_hdaPath = s_rootPath + "/hda";
                }
                else
                {
                    s_rootPath = "No Directory Selected...";
                    s_hdaPath = "No Directory Selected...";
                }
            }
        }
        public static string GlobalExportPath
        {
            get { return s_globalExportPath; }
            set { s_globalExportPath = value; }
        }
        public static List<string> HDAPaths
        {
            get { return s_hdaPaths; }
            set { s_hdaPaths = value; }
        }
        #endregion



        #region Methods
        public static void LoadConfig()
        {
            CZ_HEngineJSONData configData = new CZ_HEngineJSONData();

            if (configData.FetchJSONFile())
            {
                s_rootPath = configData.RootPath;
                s_globalExportPath = configData.ExportPath;
                s_hdaPath = s_rootPath + "/hda";
                s_tools = configData.Tools;
            }
            else
            {
                Debug.LogWarning("Can't find config.json in HEngineManager root folder!");
            }
        }

        public static void UpdateHDAFiles()
        {
            s_hdaPaths.Clear();
            
            if (!Directory.Exists(s_hdaPath))
            {
                Debug.LogWarning("HDA Path: " + s_hdaPath + " does not exist!");
                return;
            }
            string[] files = Directory.GetFiles(s_hdaPath).Where(s => s.EndsWith(".hdalc")).ToArray();
            if (s_tools != null)
            {
                // Compare the files from s_hdaPath to s_tools list and add the newest versions to hdaPaths list.
                foreach (string tool in s_tools)
                {
                    List<string> result = files.Where(s => s.Contains(tool)).ToList();
                    result = result.OrderByDescending(s => s).ToList();

                    if (result.Count == 0)
                    {
                        Debug.LogWarning("The tool " + tool + " doesn't exist in folder " + s_hdaPath + "!!!");
                    }
                    else
                    {
                        s_hdaPaths.Add(result[0]);
                    }
                }
            }
            s_hdaPaths.Add("Curve");
            //hdaPaths.Add("PolyBrush");

        }

        #endregion
    }
}