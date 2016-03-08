using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace RelationsInspector.Backend
{
	// for backend api reference see https://github.com/seldomU/RIBackendUtil/wiki/IGraphBackend-members
	// for backend development guide see https://github.com/seldomU/RIBackendUtil/wiki/Backend-development

	public abstract class MinimalBackend<T, P> : IGraphBackend<T, P> where T : class
	{
		protected RelationsInspectorAPI api;

		#region graph construction

		public virtual void Awake( GetAPI getAPI )
		{
			this.api = getAPI(1) as RelationsInspectorAPI;
		}

		// Init turns the inspection target objects into root entities of the graph
		// we assume the two sets to be identical, so we're just pass them through
		public virtual IEnumerable<T> Init( object target )
		{
			return ( target is T ) ? new T[] { target as T } : new T[ 0 ];
		}

		// Called when the window object is being destroyed or a new backend is replacing this one
		public virtual void OnDestroy() { }

		// GetRelated returns the entities that are related to the given entity, and the type of their relation
		// we assume all relations to be of the same kind (default P)
		public virtual IEnumerable<Relation<T, P>> GetRelations( T entity )
		{
			yield break;
		}

		#endregion

		#region graph modification

		// UI wants to create a relation between source and target (of type tag)
		// we assume graph manipulation is unwanted and ignore the event
		public virtual void CreateRelation( T source, T target ) { } // do nothing

		#endregion

		#region content drawing

		// DrawContent is responsible for rendering entity information
		// it returns the Rect that it filled
		public virtual Rect DrawContent( T entity, EntityDrawContext drawContext )
		{
			return DrawUtil.DrawContent( GetContent( entity ), drawContext );
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
			return GetContent( entity ).tooltip;
		}

		// GetTagTooltip returns a tooltip for the given relation tag, to be rendered by RI
		public virtual string GetTagTooltip( P tag )
		{
			if ( tag == null )
				return "null";
			return tag.ToString();
		}

		// OnGUI is responible for rendering any backend GUI, like controls or a toolbar
		// returns the rect that will be used for graph drawing
		// we draw no controls, so the whole space can be used for the graph
		public virtual Rect OnGUI()
		{
			return BackendUtil.GetMaxRect();
		}

		// GetContent is a utility method that returns name, icon, toolip for the given entity
		// if possible
		public virtual GUIContent GetContent( T entity )
		{
			return BackendUtil.GetContent( entity );
		}

		#endregion

		// Event handler for when the set of selected entities has changed
		// we update Unity's object selection to match RI's
		public virtual void OnEntitySelectionChange( T[] selection )
		{
			var selectedObjects = selection.OfType<Object>();
			if ( !selectedObjects.Any() )
				return;

			Selection.objects = selectedObjects
				.Select( o => GetObjectRepresentative( o ) )
				.ToArray();
		}

		Object GetObjectRepresentative( Object obj )
		{
			var asComp = obj as Component;
			if ( asComp != null && asComp.gameObject != null )
				return asComp.gameObject;
			return obj;
		}

		// Event handler for when Unity's editor selection has changed
		// we ignore that (not syncing Unity's with RI's selection)
		public virtual void OnUnitySelectionChange() { }

		// Event handler for context clicks on entity widgets
		// we ignore those (no context menu)
		public virtual void OnEntityContextClick( IEnumerable<T> entities, GenericMenu menu ) { }

		// Event handler for context clicks on relation widgets
		// we ignore those (no context menu)
		public virtual void OnRelationContextClick( Relation<T, P> relation, GenericMenu menu ) { }

		// Event handler for generic commands
		public virtual void OnCommand( string command ) { }
	}
}
