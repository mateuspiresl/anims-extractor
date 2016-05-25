#pragma warning disable

using UnityEngine;
using UnityEditor;
public class BuildAssetBundles {
	
	[MenuItem ("Assets/BUILD ALL BUNDLES IN ToBuild FOLDER")]
	public static void BuildBundles()
	{
		string path = "AssetBundles/";
		string[] assetsPaths = AssetDatabase.FindAssets("", new string[1] { "Assets/ToBuild" });
		
		Debug.Log("How many assets in Assets/ToBuild: " + assetsPaths.Length);
		
		foreach (string assetPathID in assetsPaths)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(assetPathID);
			
			Object asset = AssetDatabase.LoadAssetAtPath(
					assetPath, (typeof(UnityEngine.Object))
				) as Object;
			
			if (asset != null)
			{
				Debug.Log("Building " + assetPath);
				
				string[] tokens = assetPath.Split('/');
				tokens = tokens[tokens.Length - 1].Split('.');

				buildBundle(asset, path + "STANDALONE/" + tokens[0]);
				buildBundle(asset, path + "ANDROID/" + tokens[0], BuildTarget.Android);
				buildBundle(asset, path + "WEBGL/" + tokens[0], BuildTarget.WebGL);
				buildBundle(asset, path + "IOS/" + tokens[0], BuildTarget.iOS);
			}
			else Debug.Log("Ignoring " + assetPath);
		}
	}

	private static void buildBundle(UnityEngine.Object asset, string path, BuildTarget target)
	{
		BuildPipeline.BuildAssetBundle(
			asset,
			new UnityEngine.Object[0] {},
			path,
			BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
			target
		);
	}
	private static void buildBundle(UnityEngine.Object asset, string path)
	{
		BuildPipeline.BuildAssetBundle(
			asset,
			new UnityEngine.Object[0] {},
			path,
			BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets
		);
	}
	
}