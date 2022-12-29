using UnityEngine;
using UnityEditor;
using HoudiniEngineUnity;

namespace CZ.HEngineTools
{
    public class CZ_HEngineMenu
    {
        [MenuItem("HEngine/HEngine Manager")]
        public static void ShowHEngineManager()
        {
            bool bUtility = false;
            bool bFocus = true;
            string title = "HEngine Manager";
            CZ_HEngineManagerUI window = EditorWindow.GetWindow<CZ_HEngineManagerUI>(bUtility, title, bFocus);
            window.autoRepaintOnSceneChange = true;
            window.minSize = new Vector2(400.0f, 400.0f);
            window.Show();
        }

        [MenuItem("HEngine/Create Session")]
        static void CreateSession()
        {
          //  HEU_SessionBase session = CZ_HEngineUtils.GetHoudiniSession();
        }

        [MenuItem("HEngine/Close All Sessions")]
        static void CloseAllSessions()
        {
            HEU_SessionManager.CloseAllSessions();
        }
    }
}