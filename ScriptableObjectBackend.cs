using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector.Backend
{
	public class ScriptableObjectBackend<T, P> : IGraphBackend<T, P> where T : ScriptableObject
	{
		protected RelationsInspectorAPI api;
		ScriptableObjectBackendToolbar<T> toolbar;

		public virtual void Awake( GetAPI getAPI )
		{
			api = getAPI(1) as RelationsInspectorAPI;
			toolbar = new ScriptableObjectBackendToolbar<T>( api );
		}

		// Init turns the inspection target objects into root entities of the graph
		// we assume the two sets to be identical, so we're just pass them through
		// and initialize the toolbar
		public virtual IEnumerable<T> Init( object target )
		{
			if ( target != null )
				toolbar.SetAssetPath( BackendUtil.GetAssetDirectory( target as Object ) );

			return ( target is T ) ? new T[] { target as T } : new T[ 0 ];
		}

		// Called when the window object is being destroyed or a new backend is replacing this one
		public virtual void OnDestroy() { }

		// returns the entities that are related to the given entity, and the type of their relation
		// to be implemented by subclass
		public virtual IEnumerable<Relation<T, P>> GetRelations( T entity )
		{
			yield break; // to be implement by subclass
		}

		// UI wants to create a relation between source and target
		// to be implemented by subclass
		public virtual void CreateRelation( T source, T target ) { }

		// DrawContent is responsible for rendering entity information
		// it returns the Rect that it filled
		public virtual Rect DrawContent( T entity, EntityDrawContext drawContext )
		{
			return DrawUtil.DrawContent( new GUIContent( entity.name ), drawContext );
		}

		// GetRelationColor maps a relation tag value to a color
		// we assume only one kind of relation, so map everything to white
		public virtual Color GetRelationColor( P relationTagValue )
		{
			return Color.white;
		}

		// GetEntityTooltip returns a tooltip for the given entity, to be rendered by RI
		public virtual string GetEntityTooltip( T entity )
		{
			return entity.name;
		}

		// GetTagTooltip returns a tooltip for the given relation tag, to be rendered by RI
		public virtual string GetTagTooltip( P tag )
		{
			return ( tag == null ) ? "" : tag.ToString();
		}

		// OnGUI is responible for rendering any backend GUI, like controls or a toolbar
		// returns the rect that will be used for graph drawing
		public virtual Rect OnGUI()
		{
			toolbar.OnGUI();
			return BackendUtil.GetMaxRect();
		}

		// Event handler for when the set of selected entities has changed
		// we update Unity's object selection to match RI's
		public virtual void OnEntitySelectionChange( T[] selection )
		{
			Selection.objects = selection.ToArray();
		}

		// Event handler for when Unity's editor selection has changed
		// we ignore that (not syncing Unity's with RI's selection)
		public virtual void OnUnitySelectionChange() { }

		// Event handler for context clicks on entity widgets
		// we offer options to remove the entity or create a relation that originates from it
		public virtual void OnEntityContextClick( IEnumerable<T> entities, GenericMenu menu )
		{
			menu.AddItem( new GUIContent( "Remove entity" ), false, () => { foreach ( var e in entities ) DeleteEntity( e ); } );
			menu.AddItem( new GUIContent( "Add relation" ), false, () => api.InitRelation( entities.ToArray() ) );
		}

		// entity context menu wants to remove the entity
		public void DeleteEntity( T entity )
		{
			// first delete all relations involving the given entity
			var relations = api.FindRelations( entity ).OfType<Relation<T, P>>();
			foreach ( var rel in relations )
			{
				DeleteRelation( rel.Source, rel.Target, rel.Tag );
				api.RemoveRelation( rel.Source, rel.Target, rel.Tag );
			}

			// remove the entity
			AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( entity ) );
			AssetDatabase.SaveAssets();
			api.RemoveEntity( entity );
		}

		// Event handler for context clicks on relation widgets
		// we offer the option to remove the relation
		public virtual void OnRelationContextClick( Relation<T, P> relation, GenericMenu menu )
		{
			menu.AddItem( new GUIContent( "Remove relation" ), false, () => DeleteRelation( relation.Source, relation.Target, relation.Tag ) );
		}

		// UI wants to delete a relation between source and target (of type tag)
		// to be implemented by subclass
		public virtual void DeleteRelation( T source, T target, P tag ) { }

		// event handler for generic commands
		public virtual void OnCommand( string command ) { }
	}
}
