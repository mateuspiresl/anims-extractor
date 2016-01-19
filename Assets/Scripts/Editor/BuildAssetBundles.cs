/*************************************************
 # @Author: Mateus Pires
 # 
 # Creates MenuItem
 # Builds one asset bundle for each file
 #    inside Assets/ToBuild folder
 #
 # Attach this to nothing! It's EditorScript;
 # All bundles wil be found in
 # AssetBundles folder inside proj dir root
 #
 #Place it inside Editor Folder
 #
 *************************************************/
#pragma warning disable

using UnityEngine;
using UnityEditor;
public class BuildAssetBundles {
	
	[MenuItem ("Assets/BUILD ALL BUNDLES IN ToBuild FOLDER")]
	static void BuildBundles(){
		
		string path = "AssetBundles/";
		string[] assetsPaths = AssetDatabase.FindAssets("", new string[1] { "Assets/ToBuild" });
		
		Debug.Log("How many assets in Assets/ToBuild: " + assetsPaths.Length);
		
		foreach (string assetPathID in assetsPaths){
			
			string assetPath = AssetDatabase.GUIDToAssetPath(assetPathID);
			
			Object asset = AssetDatabase.LoadAssetAtPath(
				assetPath,
				(typeof(UnityEngine.Object))) as Object;
			
			if (asset != null){
				
				Debug.Log("Building " + assetPath);
				
				string[] tokens = assetPath.Split('/');
				tokens = tokens[tokens.Length - 1].Split('.');
				
				string filePath = path + tokens[0];
				BuildPipeline.BuildAssetBundle(asset, new Object[0] {}, filePath,

					//SE LIGA NETA PORRA DE WEBGL NEGAO
					//BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.WebGL );
					BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.WebGL );
				
			}
			
			else Debug.Log("Ignoring " + assetPath);
			
		}
		
	}
	
}