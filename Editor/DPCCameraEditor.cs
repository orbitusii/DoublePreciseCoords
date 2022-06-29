using UnityEditor;
using UnityEngine;
using DoublePreciseCoords.Cameras;

namespace DoublePreciseCoords.Editor
{
    //[CustomEditor(typeof(DPCCamera))]
    public class DPCCameraEditor : UnityEditor.Editor
    {
        protected DPCCamera cam;

        void OnEnable()
        {
            cam = target as DPCCamera;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev Target"))
            {
                cam.StepViewTarget(-1);
            }
            if (GUILayout.Button("Next Target"))
            {
                cam.StepViewTarget(1);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}