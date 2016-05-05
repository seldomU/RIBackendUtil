using UnityEngine;
using System.Security.Cryptography;
using UnityEditor;
using System;

namespace RelationsInspector.Backend
{
	public static class RandomColor
	{
		// base Color palettes
		static Color[] LightPalette =
			new Color[]
			{
				new Color(0.78f, 0.12f, 0.22f)
				,new Color(0.10f, 0.73f, 0.08f)
				,new Color(0.60f, 0.37f, 0.08f)
				,new Color(0.86f, 0.31f, 0.07f)
				,new Color(0.68f, 0.20f, 0.20f)
				,new Color(0.63f, 0.30f, 0.81f)
				,new Color(0.77f, 0.64f, 0.06f)
				,new Color(0.06f, 0.73f, 0.34f)
				,new Color(0.59f, 0.36f, 0.30f)
				,new Color(0.16f, 0.48f, 0.59f)
				,new Color(0.79f, 0.20f, 0.65f)
			};

		static Color[] DarkPalette =
			new Color[]
			 {
				new Color(0.78f, 0.12f, 0.22f)
				,new Color(0.11f, 0.60f, 0.10f)
				,new Color(0.60f, 0.37f, 0.08f)
				,new Color(0.18f, 0.20f, 0.56f)
				,new Color(0.68f, 0.20f, 0.20f)
				,new Color(0.63f, 0.30f, 0.81f)
				,new Color(0.60f, 0.49f, 0.05f)
				,new Color(0.10f, 0.59f, 0.31f)
				,new Color(0.59f, 0.36f, 0.30f)
				,new Color(0.16f, 0.48f, 0.59f)
				,new Color(0.66f, 0.13f, 0.54f)
			};

		public static Color[] Palette
		{
			get { return EditorGUIUtility.isProSkin ? DarkPalette : LightPalette; }
		}

		static float maxShade = 0.75f;
		static MD5 md5 = MD5.Create();

		public static Color GetHashColor( string value )
		{
			return Palette[ GetHashInRange( value, 0, Palette.Length ) ];
		}

		public static Color GetHashShade( string value, Color originColor )
		{
			Color added = EditorGUIUtility.isProSkin ?
				originColor * -1 :
				Color.white - originColor;

			float shade = GetHashInRange( value, 0, maxShade );
			return ClampColor( originColor + added * shade );
		}

		static Color ClampColor( Color c )
		{
			return new Color(
				Mathf.Clamp01( c.r ),
				Mathf.Clamp01( c.g ),
				Mathf.Clamp01( c.b ),
				Mathf.Clamp01( c.a )
				);
		}

		public static float GetHashInRange( string value, float minHash, float maxHash )
		{
			return minHash + ( GetHashUInt( value ) % ( maxHash - minHash ) );
		}

		// including min, excluding max
		public static int GetHashInRange( string value, int minHash, int maxHash )
		{
			return (int) ( minHash + ( GetHashUInt( value ) % ( maxHash - minHash ) ) );
		}

		public static uint GetHashUInt( string value )
		{
			byte[] hash = md5.ComputeHash( System.Text.Encoding.UTF8.GetBytes( value ) );
			return BitConverter.ToUInt32( hash, 0 );
		}
	}
}
