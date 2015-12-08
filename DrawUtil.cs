using UnityEngine;
using UnityEditor;
using RelationsInspector.Extensions;

namespace RelationsInspector.Backend
{
	public static class DrawUtil
	{
		public static float sqrt2 = Mathf.Sqrt(2f);
		public static Vector2 boxIconSize = new Vector2(16, 16);
        static Vector2 shadowOffset = new Vector2( 2, 2);

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
            else
            {
                // draw shadow
                EditorGUI.DrawRect( outlineRect.Move( shadowOffset ), context.style.shadowColor );
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

            DrawBoxAndBackground( labelRect, context );

            // draw label
            content.tooltip = string.Empty; // RI dll handles tooltip drawing
            GUI.Label( labelRect, content, context.style.contentStyle );

			return labelRect;
		}

        static void DrawSquareTexture( Vector2 center, float width, Color color, Texture2D texture )
        {
            var rect = Util.CenterRect( center, new Vector2( width, width ) );
            GUI.color = color;
            GUI.DrawTexture( rect, texture );
            GUI.color = Color.white;
        }

        public static void DrawCircleAndOutline( float radius, EntityDrawContext context )
        {
            float outlineRadius = radius + 1;

            // selected items get highlighted
            if ( context.isSelected )
            {
                // draw selection aura
                DrawSquareTexture(
                    context.position,
                    2 * ( outlineRadius + context.style.highlightStrength),
                    context.style.highlightColor,
                    context.style.discImage
                    );
            }
            else if ( context.isUnexlored )
            {
                // draw unexplored aura
                DrawSquareTexture(
                    context.position,
                    2 * ( outlineRadius + context.style.highlightStrength),
                    context.style.unexploredColor,
                    context.style.discImage
                    );
            }

            // draw outline disc
            DrawSquareTexture( context.position, 2 * outlineRadius, Color.black, context.style.discImage );

            // draw entity disc
            Color entityColor = context.isTarget ? context.style.targetBackgroundColor : context.style.backgroundColor;
            DrawSquareTexture( context.position, 2* radius, entityColor, context.style.discImage);
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
