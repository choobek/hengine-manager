using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.IO;

namespace CZ.HEngineTools
{
    class Data
    {
        public string RootPath;
        public string ExportPath;
        public List<string> Tools;
    }

    [Serializable]
    public class CZ_HEngineJSONData
    {
        #region Variables
        string _configFilepath;
        string _rootPath;
        string _exportPath;
        List<string> _tools;
        #endregion

        #region Getters/Setters
        public string ConfigFilePath
        {
            get { return _configFilepath; }
            set { _configFilepath = value; }
        }
        public string RootPath
        {
            get { return _rootPath; }
            set { _rootPath = value; }
        }
        public string ExportPath
        {
            get { return _exportPath; }
            set { _exportPath = value; }
        }
        public List<string> Tools
        {
            get { return _tools; }
            set { _tools = value; }
        }
        #endregion


        #region Methods
        public CZ_HEngineJSONData()
        {
            GetConfigFilepath();
            FetchJSONFile();
        }

        public bool FetchJSONFile()
        {
            if (File.Exists(_configFilepath))
            {
                // Read the JSON string from the file
                string json = File.ReadAllText(_configFilepath);

                // Convert the JSON string to a Data object
                Data data = JsonConvert.DeserializeObject<Data>(json);
                
                // Set the values
                _exportPath = data.ExportPath;
                _rootPath = data.RootPath;
                _tools = data.Tools;

                return true;
            }
            return false;
        }

        private void GetConfigFilepath()
        {
            string basePath = Directory.GetCurrentDirectory();
            string relativePath = "Assets/HEngineManager/config.json";
            string fullPath = System.IO.Path.Combine(basePath, relativePath);
            _configFilepath = fullPath;
        } 
        #endregion
    }
}