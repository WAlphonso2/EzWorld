using UnityEditor;
using UnityEngine;

#region PathFollwerGUI

namespace CurvedPathGenerator
{

    [CustomEditor(typeof(PathFollower))]
    internal class PathFollowerGUI : Editor
    {
        #region PathFollower_GUI_Variables

        private GUIStyle H1Text;
        private GUIStyle BoldText;

        private SerializedProperty endEvent;

        private GUIStyle ComponentTitle;

        #endregion PathFollower_GUI_Variables

        #region PathFollower_InspectorUI_Main


        public override void OnInspectorGUI()
        {
            #region PathFollower_InspectorUI_Main_StartsUp

            PathFollower pathFollower = target as PathFollower;


            if ( ComponentTitle == null )
            {
                ComponentTitle = new GUIStyle(EditorStyles.label);
                ComponentTitle.normal.textColor = new Color(0f, 0.3882f, 0.9725f, 1f);
                ComponentTitle.fontSize = 17;
                ComponentTitle.fontStyle = FontStyle.Bold;
            }

            if ( H1Text == null )
            {
                H1Text = new GUIStyle(EditorStyles.label);
                H1Text.fontStyle = FontStyle.Bold;
                H1Text.fontSize = 15;
            }

            if ( BoldText == null )
            {
                BoldText = new GUIStyle(EditorStyles.label);
                BoldText.fontStyle = FontStyle.Bold;
            }

            #endregion PathFollower_InspectorUI_Main_StartsUp

            #region PathFollower_InspectorUI_Main_Header


            Texture LogoTex = (Texture2D)Resources.Load("PathFollowerScriptImg", typeof(Texture2D));
            GUILayout.Label(LogoTex, GUILayout.Width(300f), GUILayout.Height(67.5f));


            GUILayout.BeginHorizontal();
            GUI.enabled = PathGeneratorGUILanguage.CurrentLanguage != LANGUAGE.ENG;
            if ( GUILayout.Button("English", GUILayout.Height(22f)) )
            {
                Undo.RecordObject(pathFollower, "Modify " + pathFollower.gameObject.name);
                PathGeneratorGUILanguage.CurrentLanguage = LANGUAGE.ENG;
            }
            GUI.enabled = PathGeneratorGUILanguage.CurrentLanguage != LANGUAGE.KOR;
            if ( GUILayout.Button("한국어", GUILayout.Height(22f)) )
            {
                Undo.RecordObject(pathFollower, "Modify " + pathFollower.gameObject.name);
                PathGeneratorGUILanguage.CurrentLanguage = LANGUAGE.KOR;
            }
            GUI.enabled = PathGeneratorGUILanguage.CurrentLanguage != LANGUAGE.JAP;
            if ( GUILayout.Button("日本語", GUILayout.Height(22f)) )
            {
                Undo.RecordObject(pathFollower, "Modify " + pathFollower.gameObject.name);
                PathGeneratorGUILanguage.CurrentLanguage = LANGUAGE.JAP;
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);


            GUILayout.Label(PathGeneratorGUILanguage.GetLocalText("PF_Title"), ComponentTitle);
            GUI.enabled = false;
            GUILayout.Label(PathGeneratorGUILanguage.GetLocalText("PF_SubTitle"));
            GUI.enabled = true;
            GUILayout.Space(15f);
            GuiLine();
            GUILayout.Space(15f);

            #endregion PathFollower_InspectorUI_Main_Header

            #region PathFollower_InspectorUI_Main_Info


            GUILayout.Label(PathGeneratorGUILanguage.GetLocalText("PF_H1_Info"), H1Text);
            GUILayout.Space(3f);
            GUILayout.Label(PathGeneratorGUILanguage.GetLocalText("PF_Info_Label"));
            GUILayout.Space(10f);


            GUILayout.BeginHorizontal();
            GUILayout.Label(PathGeneratorGUILanguage.GetLocalText("PF_Info_Path"), GUILayout.Width(120f));
            GUILayout.Space(15f);
            Undo.RecordObject(pathFollower, "Modify " + pathFollower.gameObject.name);
            pathFollower.Generator = EditorGUILayout.ObjectField(pathFollower.Generator, typeof(PathGenerator), true) as PathGenerator;
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label(PathGeneratorGUILanguage.GetLocalText("PF_Info_Speed"), GUILayout.Width(120f));
            GUILayout.Space(15f);
            Undo.RecordObject(pathFollower, "Modify " + pathFollower.gameObject.name);
            pathFollower.Speed = EditorGUILayout.Slider(pathFollower.Speed, 0, 600f);
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);


            GUILayout.BeginHorizontal();
            GUILayout.Label(PathGeneratorGUILanguage.GetLocalText("PF_Info_Threshold"), GUILayout.Width(120f));
            GUILayout.Space(15f);
            Undo.RecordObject(pathFollower, "Modify " + pathFollower.gameObject.name);
            pathFollower.DistanceThreshold = EditorGUILayout.Slider(pathFollower.DistanceThreshold, 0.001f, 100f);
            GUILayout.EndHorizontal();
            GUILayout.Space(2f);

            EditorGUILayout.HelpBox(PathGeneratorGUILanguage.GetLocalText("PF_Info_Warning"), MessageType.Info);
            GUILayout.Space(5f);

            GUILayout.BeginHorizontal();
            GUILayout.Label(PathGeneratorGUILanguage.GetLocalText("PF_Info_TurningSpeed"), GUILayout.Width(120f));
            GUILayout.Space(15f);
            Undo.RecordObject(pathFollower, "Modify " + pathFollower.gameObject.name);
            pathFollower.TurningSpeed = EditorGUILayout.Slider(pathFollower.TurningSpeed, 0.1f, 100f);
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            Undo.RecordObject(pathFollower, "Modify " + pathFollower.gameObject.name);
            pathFollower.IsMove = GUILayout.Toggle(pathFollower.IsMove, PathGeneratorGUILanguage.GetLocalText("PF_Info_IsMove"));
            GUILayout.Space(15f);
            Undo.RecordObject(pathFollower, "Modify " + pathFollower.gameObject.name);
            pathFollower.IsLoop = GUILayout.Toggle(pathFollower.IsLoop, PathGeneratorGUILanguage.GetLocalText("PF_Info_IsLoop"));
            GUILayout.EndHorizontal();
            GUILayout.Space(19f);
            GuiLine();
            GUILayout.Space(15f);

            #endregion PathFollower_InspectorUI_Main_Info

            #region PathFollower_InspectorUI_Main_EventHandler

            GUILayout.Label(PathGeneratorGUILanguage.GetLocalText("PF_H1_Events"), H1Text);
            GUILayout.Space(3f);
            GUILayout.Label(PathGeneratorGUILanguage.GetLocalText("PF_Events_Label"));
            GUILayout.Space(10f);

            Undo.RecordObject(pathFollower, "Modify " + pathFollower.gameObject.name);
            pathFollower.IsEndEventEnable =
                GUILayout.Toggle(pathFollower.IsEndEventEnable, PathGeneratorGUILanguage.GetLocalText("PF_Events_endEventLabel"));
            if ( pathFollower.IsEndEventEnable )
            {
                GUILayout.Space(10f);
                serializedObject.Update();
                EditorGUILayout.PropertyField(endEvent);
                serializedObject.ApplyModifiedProperties();
            }
            GUILayout.Space(35f);

            #endregion PathFollower_InspectorUI_Main_EventHandler
        }

        #endregion PathFollower_InspectorUI_Main

        #region PathFollower_InspectorUI_Main_Functions

        #region PathFollower_InspectorUI_Main_Functions_OnEnable


        public void OnEnable()
        {
            PathGeneratorGUILanguage.InitLocalization();                    // Language setting
            endEvent = serializedObject.FindProperty("EndEvent");           // for event handler
        }

        #endregion PathFollower_InspectorUI_Main_Functions_OnEnable

        #region PathFollower_InspectorUI_Main_Functions_GuiLine


        private void GuiLine(int i_height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        #endregion PathFollower_InspectorUI_Main_Functions_GuiLine

        #endregion PathFollower_InspectorUI_Main_Functions
    }
}

#endregion PathFollwerGUI