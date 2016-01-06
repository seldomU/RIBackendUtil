using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using RelationsInspector.Extensions;

namespace RelationsInspector.Backend
{
	public static class BackendUtil
	{		
		// pair collection entries with the default P value
		public static IEnumerable<Tuple<T, P>> PairWithTag<T, P>(IEnumerable<T> collection, P tag)
		{
			return collection.Select(item => new Tuple<T, P>(item, default(P)));
		}

		// create GUIContent for obj
		// use Unity's internal GUIContent if possible, fall back to ToString if not
		public static GUIContent GetContent<T>(T obj)  where T : class
		{
			var asObject = obj as Object;
			if (asObject != null)
			{
				var content = EditorGUIUtility.ObjectContent(asObject, asObject.GetType());
				content.tooltip = content.text;
				return content;
			}
					   
			return new GUIContent( obj.ToString(), null, obj.ToString() );
		}

		// returns the full window rect that isn't yet claimed by any GUILayout
		public static Rect GetMaxRect()
		{
			return GUILayoutUtility.GetRect(0, 0, new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) });
		}

		// returns the asset object's full directory path, or null
		public static string GetAssetDirectory(Object assetObj)
		{
			if (assetObj == null)
				return null;

			string path = AssetDatabase.GetAssetPath(assetObj);
			if (string.IsNullOrEmpty(path))
				return null;

			path = path.RemovePrefix("Assets");
			return Path.GetDirectoryName(path);
		}

		// returns true if the given path is a valid asset directory
		public static bool IsValidAssetDirectory(string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;

			if (!Directory.Exists(path))
				return false;

			// has to be inside assets root directory
			if (!path.StartsWith(Application.dataPath))
				return false;

			return true;
		}

		// create an asset of type T, store it at path and return the object
		public static T CreateAssetOfType<T>(string path) where T : ScriptableObject
		{
			var scriptableObject = ScriptableObject.CreateInstance<T>();
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			AssetDatabase.CreateAsset(scriptableObject, path);
			AssetDatabase.SaveAssets();
			return scriptableObject;
		}

		public static string DrawEntitySelectSearchField( string searchString, RelationsInspectorAPI api )
		{
			// when the search string changes, select the entities with matching names
			System.Action<string> onSearchStringChange = searchStr =>
			{
				if ( string.IsNullOrEmpty( searchStr ) )
					api.SelectEntityNodes( x => { return false; } );
				else
					api.SelectEntityNodes( x =>
					{
						return ( x is Object ) ?
							( x as Object ).name.ToLower().Contains( searchStr.ToLower() ) :
							false;
					} );
			};

			return DrawSearchField( searchString, onSearchStringChange );
		}

		public static string DrawSearchField( string searchString, System.Action<string> onChange)
		{
			EditorGUI.BeginChangeCheck();
			searchString = EditorGUILayout.TextField( searchString, GUI.skin.FindStyle( "ToolbarSeachTextField" ) );
			bool resetSearchString = GUILayout.Button( "", GUI.skin.FindStyle( "ToolbarSeachCancelButton" ) );
			if ( EditorGUI.EndChangeCheck() )
			{
				if ( resetSearchString )
				{
					searchString = string.Empty;
					GUI.FocusControl( null );
				}

				onChange( searchString );
			}

			return searchString;
		}
	}
}
