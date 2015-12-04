using UnityEngine;
using UnityEditor;
using RelationsInspector.Extensions;

namespace RelationsInspector.Backend
{
	public static class DrawUtil
	{
		public static float sqrt2 = Mathf.Sqrt(2f);
		public static Vector2 boxIconSize = new Vector2(16, 16);

		// draw the content in a rect or circle widget, depending on context
		public static Rect DrawContent(GUIContent content, EntityDrawContext context)
		{
			switch (context.widgetType)
			{
				case EntityWidgetType.Circle:
					return DrawCircleWidget(content, context);

				case EntityWidgetType.Rect:
				default:
					return DrawRectWidget(content, context);
			}
		}

        // draw content box background, outline and selection/unexplored aura
        public static void DrawBoxAndBackground(Rect contentRect, EntityDrawContext context )
        {
            Rect outlineRect = contentRect.AddBorder( 1 );

            // selected items get highlighted
            if ( context.isSelected )
            {
                var auraRect = outlineRect.AddBorder( context.style.highlightStrength );
                EditorGUI.DrawRect( auraRect, context.style.highlightColor );
            }
            else if ( context.isUnexlored )
            {
                var auraRect = outlineRect.AddBorder( context.style.highlightStrength );
                EditorGUI.DrawRect( auraRect, context.style.unexploredColor );
            }

            // draw outline rect
            EditorGUI.DrawRect( outlineRect, Color.black );

            // draw content rect
            EditorGUI.DrawRect( contentRect, context.isTarget ? context.style.targetBackgroundColor : context.style.backgroundColor );
        }

		// draw content in rect widget
		public static Rect DrawRectWidget(GUIContent content, EntityDrawContext context)
		{
			// determine the space required for drawing the content
			EditorGUIUtility.SetIconSize(boxIconSize);
			Vector2 contentExtents = context.style.contentStyle.CalcSize(content);
			Rect labelRect = Util.CenterRect(context.position, contentExtents);

			// find a box around it, with some padding
			Rect contentRect = labelRect.AddBorder(context.style.contentPadding);
            DrawBoxAndBackground( contentRect, context );

            // draw label
            content.tooltip = string.Empty; // RI dll handles tooltip drawing
            GUI.Label( labelRect, content, context.style.contentStyle );

			return contentRect;
		}

        public static void DrawCircleAndOutline( float radius, EntityDrawContext context )
        {
            // selected items get highlighted
            if ( context.isSelected )
            {
                // draw selection aura
                float highlightRadius = radius + context.style.highlightStrength;
                Handles.color = context.style.highlightColor;
                Handles.DrawSolidDisc( context.position, Vector3.forward, highlightRadius );

                Handles.color = Color.white;
            }
            else if ( context.isUnexlored )
            {
                // draw unexplored aura
                float highlightRadius = radius + context.style.highlightStrength;
                Handles.color = context.style.unexploredColor;
                Handles.DrawSolidDisc( context.position, Vector3.forward, highlightRadius );

                Handles.color = Color.white;
            }

            // draw entity disc
            Handles.color = context.isTarget ? context.style.targetBackgroundColor : context.style.backgroundColor;
            Handles.DrawSolidDisc( context.position, Vector3.forward, radius );

            // draw disc outline
            Handles.color = Color.black;
            Handles.DrawWireDisc( context.position, Vector3.forward, radius );
            Handles.color = Color.white;
        }

        // draw content in circle widget
        public static Rect DrawCircleWidget(GUIContent content, EntityDrawContext context)
		{
			float contentSize = 2 * context.style.widgetRadius;
			float radius = context.style.widgetRadius * sqrt2;

            DrawCircleAndOutline( radius, context );

			// draw content icon, if any
			if (content.image != null)
			{
                //2*radius/sqrt2
				Rect contentRect = Util.CenterRect(context.position, new Vector2(contentSize, contentSize));
				GUI.DrawTexture(contentRect, content.image, ScaleMode.ScaleToFit);
			}

			return Util.CenterRect(context.position, new Vector2(radius * 2, radius * 2));
		}
	}
}
