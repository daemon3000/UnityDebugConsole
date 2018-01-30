using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Luminosity.Console.UI;

namespace LuminosityEditor.Inspectors
{
	[CustomEditor(typeof(DebugConsoleUI))]
	[CanEditMultipleObjects]
	public class DebugConsoleUIInspector : Editor
	{
		private SerializedProperty m_feedbackUI;
		private SerializedProperty m_logScrollView;
		private SerializedProperty m_canvas;
		private SerializedProperty m_panel;
		private SerializedProperty m_stackTraceField;
		private SerializedProperty m_stackTracePanel;
		private SerializedProperty m_commandField;
		private SerializedProperty m_sendErrorButton;
		private SerializedProperty m_minWindowSize;
		private SerializedProperty m_padding;
		private SerializedProperty m_minStackTraceFieldHeight;
		private SerializedProperty m_minStackTraceFieldTopPadding;
		private SerializedProperty m_stackLogMessages;
		private SerializedProperty m_filters;
		private ReorderableList m_filterList;

		private void OnEnable()
		{
			m_feedbackUI = serializedObject.FindProperty("m_feedbackUI");
			m_logScrollView = serializedObject.FindProperty("m_logScrollView");
			m_panel = serializedObject.FindProperty("m_panel");
			m_stackTracePanel = serializedObject.FindProperty("m_stackTracePanel");
			m_stackLogMessages = serializedObject.FindProperty("m_stackLogMessages");
			m_commandField = serializedObject.FindProperty("m_commandField");
			m_sendErrorButton = serializedObject.FindProperty("m_sendErrorButton");
			m_minStackTraceFieldHeight = serializedObject.FindProperty("m_minStackTraceFieldHeight");
			m_minStackTraceFieldTopPadding = serializedObject.FindProperty("m_minStackTraceFieldTopPadding");
			m_filters = serializedObject.FindProperty("m_filters");
			m_canvas = serializedObject.FindProperty("m_canvas");
			m_minWindowSize = serializedObject.FindProperty("m_minWindowSize");
			m_padding = serializedObject.FindProperty("m_padding");

			m_filterList = new ReorderableList(serializedObject, m_filters);
			m_filterList.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Filters");
			m_filterList.drawElementCallback += (rect, index, isActive, isFocused) =>
			{
				SerializedProperty item = m_filters.GetArrayElementAtIndex(index);
				rect.y += 2.0f;
				rect.height = 16.0f;

				EditorGUI.PropertyField(rect, item, GUIContent.none);
			};
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(m_feedbackUI);
			EditorGUILayout.PropertyField(m_logScrollView);
			EditorGUILayout.PropertyField(m_canvas);
			EditorGUILayout.PropertyField(m_panel);
			EditorGUILayout.PropertyField(m_stackTracePanel);
			EditorGUILayout.PropertyField(m_commandField);
			EditorGUILayout.PropertyField(m_sendErrorButton);
			EditorGUILayout.PropertyField(m_minWindowSize);
			EditorGUILayout.PropertyField(m_padding);
			EditorGUILayout.PropertyField(m_minStackTraceFieldHeight);
			EditorGUILayout.PropertyField(m_minStackTraceFieldTopPadding);
			EditorGUILayout.PropertyField(m_stackLogMessages);

			EditorGUILayout.Space();
			m_filterList.DoLayoutList();

			serializedObject.ApplyModifiedProperties();
		}
	}
}