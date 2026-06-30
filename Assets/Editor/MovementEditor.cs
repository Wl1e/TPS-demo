using System;
using Unity.Netcode.Editor;
using UnityEditor;

#if UNITY_EDITOR

namespace TPSDemo.Editor
{
    [CustomEditor(typeof(PlayerMovement))]
    [CanEditMultipleObjects]
    public class MovementEditor: NetworkTransformEditor
	{
        private SerializedProperty m_WalkSpeed;
        private SerializedProperty m_Acceleration;
        private SerializedProperty m_AirAcceleration;
        private SerializedProperty m_Gravity;
        private SerializedProperty m_JumpForce;
        private SerializedProperty m_RotationSmoothTime;
        private SerializedProperty m_OnMoveInput;
        private SerializedProperty m_GroundLayer;
        private SerializedProperty m_MoveAudio;
        private SerializedProperty m_AudioTime;

        public override void OnEnable()
        {
            m_WalkSpeed = serializedObject.FindProperty("m_WalkSpeed");
            m_Acceleration = serializedObject.FindProperty(nameof(PlayerMovement.Acceleration));
            m_AirAcceleration = serializedObject.FindProperty(nameof(PlayerMovement.AirAcceleration));
            m_Gravity = serializedObject.FindProperty(nameof(PlayerMovement.Gravity));
            m_JumpForce = serializedObject.FindProperty(nameof(PlayerMovement.JumpForce));
            m_RotationSmoothTime = serializedObject.FindProperty(nameof(PlayerMovement.RotationLerpSmoothing));
            m_OnMoveInput = serializedObject.FindProperty("OnMoveInput");
            m_GroundLayer = serializedObject.FindProperty(nameof(PlayerMovement.GroundLayer));
            m_MoveAudio = serializedObject.FindProperty("m_MovementAudio");
            m_AudioTime = serializedObject.FindProperty("m_AudioTime");
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            var moverScript = target as PlayerMovement;
            Action<bool> SetExpanded = (bool expanded) => moverScript.MoverScriptExpanded = expanded;
            DrawFoldOutGroup<PlayerMovement>(moverScript.GetType(), DisplayProperties, moverScript.MoverScriptExpanded, SetExpanded);
            base.OnInspectorGUI();
        }

        private void DisplayProperties() {
            EditorGUILayout.PropertyField(m_WalkSpeed);
            EditorGUILayout.PropertyField(m_Acceleration);
            EditorGUILayout.PropertyField(m_AirAcceleration);
            EditorGUILayout.PropertyField(m_Gravity);
            EditorGUILayout.PropertyField(m_JumpForce);
            EditorGUILayout.PropertyField(m_RotationSmoothTime);
            EditorGUILayout.PropertyField(m_OnMoveInput);
            EditorGUILayout.PropertyField(m_GroundLayer);
            EditorGUILayout.PropertyField(m_MoveAudio);
            EditorGUILayout.PropertyField(m_AudioTime);
        }
	}
}

#endif
