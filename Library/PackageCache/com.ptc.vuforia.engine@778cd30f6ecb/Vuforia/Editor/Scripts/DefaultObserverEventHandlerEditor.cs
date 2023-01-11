/*===============================================================================
Copyright (c) 2019 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Vuforia.EditorClasses
{
    [CustomEditor(typeof(DefaultObserverEventHandler))]
    [CanEditMultipleObjects]
    public class DefaultObserverEventHandlerEditor : Editor
    {
        SerializedProperty mStatusFilterProp;
        SerializedProperty mOnTargetFoundProp;
        SerializedProperty mOnTargetLostProp;
        SerializedProperty mUsePoseSmoothing;
        SerializedProperty mAnimationCurve;

        WorldCenterMode mWorldCenterMode;

        void OnEnable()
        {
            // Setup the SerializedProperties.
            mStatusFilterProp = serializedObject.FindProperty("StatusFilter");
            mUsePoseSmoothing = serializedObject.FindProperty("UsePoseSmoothing");
            mAnimationCurve = serializedObject.FindProperty("AnimationCurve");
            mOnTargetFoundProp = serializedObject.FindProperty("OnTargetFound");
            mOnTargetLostProp = serializedObject.FindProperty("OnTargetLost");

            var vuforiaBehaviours = FindObjectsOfType<VuforiaBehaviour>();

            if (vuforiaBehaviours.All(vb => vb.WorldCenterMode == WorldCenterMode.DEVICE)) // multiple VuforiaBehaviours is not supported, but if there are, remain on the strict side for enabling smoothing
                mWorldCenterMode = WorldCenterMode.DEVICE;
        }

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();
            
            // render the standard script selector so that users can find the DefaultObserverEventHandler
            // to customize it:
            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true);
            GUI.enabled = true;
            
            
            GUILayout.Label("Consider target as visible if its status is:");
            string[] options =
                new[] { "Tracked", 
                        "Tracked or Extended Tracked",
                        "Tracked, Extended Tracked or Limited"};
            mStatusFilterProp.enumValueIndex = EditorGUILayout.Popup(mStatusFilterProp.enumValueIndex, options);

            GUI.enabled = mWorldCenterMode == WorldCenterMode.DEVICE;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(mUsePoseSmoothing, new GUIContent("Use smooth transition on pose jump", "Smooth large pose jumps when transitioning from EXTENDED_TRACKED to TRACKED. Requires WorldCenterMode DEVICE to be set on the VuforiaBehaviour."));
            if (mUsePoseSmoothing.boolValue)
            {
                EditorGUILayout.PropertyField(mAnimationCurve, new GUIContent("Pose Smoothing Curve"));
            }

            EditorGUILayout.Space();
            
            GUI.enabled = true;
            
            GUILayout.Label("Event(s) when target is found:");
            EditorGUILayout.PropertyField(mOnTargetFoundProp);
            
            GUILayout.Label("Event(s) when target is lost:");
            EditorGUILayout.PropertyField(mOnTargetLostProp);

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}