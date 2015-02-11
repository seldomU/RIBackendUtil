using UnityEngine;
using System.Collections;
using UnityEditor;
using RelationsInspector.Extensions;
using System.IO;


namespace RelationsInspector.Backend
{
	public class ScriptableObjectBackendToolbar<T> where T : ScriptableObject
	{
		static GUIContent newButtonContent = new GUIContent("New", "Create new graph");
		static GUIContent pathButtonContent = new GUIContent("", "Where to store the asset files");

		const string prefsKeyPath = "RelationsInspectorBackendPath";
		const string newEntityFieldControlName = "NewEntityField";
		const string defaultEntityName = "entityName";
		const float defaultEntityNameFieldWidth = 80;

		RelationsInspectorAPI api;
		bool waitingForEntityName;
		string entityName;
		string assetDirectory;
		Vector2 createEntityPosition;

		public ScriptableObjectBackendToolbar(RelationsInspectorAPI _api, string targetAssetDirectory)
		{
			api = _api;
			assetDirectory = targetAssetDirectory;
			if (assetDirectory == null)
			{
				// have unique path preference per entity type
				string prefsKey = Path.Combine(prefsKeyPath, typeof(T).Name);
				assetDirectory = EditorPrefs.GetString(prefsKey, string.Empty);
			}
		}

		public void OnGUI()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			// entiy name field and buttons
			GUI.enabled = (waitingForEntityName);
			bool minimizeEntityNameControls = !waitingForEntityName;

			if (!minimizeEntityNameControls)
				GUILayout.Label("Entity name", EditorStyles.toolbarButton);
			float entityNameFieldWidth = minimizeEntityNameControls ? 0 : defaultEntityNameFieldWidth;

			GUI.SetNextControlName(newEntityFieldControlName);
			bool doAddEntity = ControlGotReturnKeyEvent(newEntityFieldControlName);
			entityName = EditorGUILayout.TextField(entityName, GUILayout.Width(entityNameFieldWidth));
			if (!minimizeEntityNameControls)
				doAddEntity |= GUILayout.Button("OK", EditorStyles.miniButtonLeft);
			if (doAddEntity)
			{
				var relativeAssetDirectory = "Assets" + assetDirectory;
				var path = System.IO.Path.Combine(relativeAssetDirectory, entityName + ".asset");
				path = AssetDatabase.GenerateUniqueAssetPath(path);

				T entity = BackendUtil.CreateAssetOfType<T>(path);

				api.AddEntity(entity, createEntityPosition);
				waitingForEntityName = false;

				entityName = defaultEntityName;
				GUI.FocusControl("");
			}
			if (!minimizeEntityNameControls)
			{
				if (GUILayout.Button("X", EditorStyles.miniButtonRight))
					waitingForEntityName = false;
			}
			GUI.enabled = true;

			// new button
			if (GUILayout.Button(newButtonContent, EditorStyles.toolbarButton))
			{
				api.ResetTargets(new object[] { });
			}

			// asset path selector
			pathButtonContent.text = "Path: " + assetDirectory;
			if (GUILayout.Button(pathButtonContent, EditorStyles.toolbarButton))
			{
				string absoluteAssetDir = Path.Combine(Application.dataPath, assetDirectory);
				string userSelectedPath = EditorUtility.OpenFolderPanel("Asset directory", absoluteAssetDir, "");
				if (BackendUtil.IsValidAssetDirectory(userSelectedPath))
				{
					assetDirectory = userSelectedPath.RemovePrefix(Application.dataPath);
					string prefsKey = Path.Combine(prefsKeyPath, typeof(T).Name);
					EditorPrefs.SetString(prefsKey, assetDirectory);
				}
			}

			GUILayout.FlexibleSpace();

			EditorGUILayout.EndHorizontal();
		}


		public void ShowNamePrompt(Vector2 entityPosition)
		{
			waitingForEntityName = true;
			entityName = defaultEntityName;
			EditorGUI.FocusTextInControl(newEntityFieldControlName);
			this.createEntityPosition = entityPosition;
		}

		// utility
		// returns true if the given control has focus and received a Return key event
		public static bool ControlGotReturnKeyEvent(string controlName)
		{
			if (GUI.GetNameOfFocusedControl() != controlName)
				return false;

			if (Event.current.isKey)
				return false;

			if (Event.current.keyCode != KeyCode.Return && Event.current.keyCode != KeyCode.KeypadEnter)
				return false;

			return true;
		}
	}
}
