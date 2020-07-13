using UnityEngine;

namespace FSM.Utility
{
	public static class PathExtension
	{
		public static string AssetsPath( string systemPath )
		{
			var assetsPath = systemPath;
			if ( assetsPath.StartsWith( Application.dataPath ) )
			{
				assetsPath = "Assets" + assetsPath.Substring( Application.dataPath.Length );
			}
			return assetsPath;
		}

		public static string SystemPath( string assetsComponentPath ) => $"{Application.dataPath}/{assetsComponentPath.Substring( 7 )}";
	}
}