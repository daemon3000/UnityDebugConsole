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
		private SerializedProperty m_panel;
		private SerializedProperty m_logMessageTemplate;
		private SerializedProperty m_logMessageRoot;
		private SerializedProperty m_stackTracePanel;
		private SerializedProperty m_maxLogMessages;
		private SerializedProperty m_commandField;
		private SerializedProperty m_sendErrorButton;
		private SerializedProperty m_logMessageMask;
		private SerializedProperty m_logMessageScrollView;
		private SerializedProperty m_minStackTraceFieldHeight;
		private SerializedProperty m_minStackTraceFieldTopPadding;
		private SerializedProperty m_filters;
		private SerializedProperty m_canvas;
		private SerializedProperty m_minWindowSize;
		private SerializedProperty m_padding;
		private ReorderableList m_filterList;

		private void OnEnable()
		{
			m_feedbackUI = serializedObject.FindProperty("m_feedbackUI");
			m_panel = serializedObject.FindProperty("m_panel");
			m_logMessageTemplate = serializedObject.FindProperty("m_logMessageTemplate");
			m_logMessageRoot = serializedObject.FindProperty("m_logMessageRoot");
			m_stackTracePanel = serializedObject.FindProperty("m_stackTracePanel");
			m_maxLogMessages = serializedObject.FindProperty("m_maxLogMessages");
			m_commandField = serializedObject.FindProperty("m_commandField");
			m_sendErrorButton = serializedObject.FindProperty("m_sendErrorButton");
			m_logMessageMask = serializedObject.FindProperty("m_logMessageMask");
			m_logMessageScrollView = serializedObject.FindProperty("m_logMessageScrollView");
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
			EditorGUILayout.PropertyField(m_canvas);
			EditorGUILayout.PropertyField(m_panel);
			EditorGUILayout.PropertyField(m_logMessageTemplate);
			EditorGUILayout.PropertyField(m_logMessageRoot);
			EditorGUILayout.PropertyField(m_stackTracePanel);
			EditorGUILayout.PropertyField(m_commandField);
			EditorGUILayout.PropertyField(m_sendErrorButton);
			EditorGUILayout.PropertyField(m_logMessageMask);
			EditorGUILayout.PropertyField(m_logMessageScrollView);
			EditorGUILayout.PropertyField(m_minWindowSize);
			EditorGUILayout.PropertyField(m_padding);
			EditorGUILayout.PropertyField(m_minStackTraceFieldHeight);
			EditorGUILayout.PropertyField(m_minStackTraceFieldTopPadding);
			EditorGUILayout.PropertyField(m_maxLogMessages);

			EditorGUILayout.Space();
			m_filterList.DoLayoutList();

			serializedObject.ApplyModifiedProperties();
		}
	}
}