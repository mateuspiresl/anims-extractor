/*
 *	 "C:\Program Files\Unity\Editor\Unity.exe" -quit -batchmode -projectPath C:\Workspace\wikilibras-player\playercore_blend -executeMethod BlendToBundlesConverter.convert
 */

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;

public class BlendToBundlesConverter {

	private static string blendsPath = "Assets/Blends";
	private static string bundlesPath = "Assets/Bundles";

	private static string fileText = "";

	private static bool hasDst = false;
	private static string dstPath = null;

	private static void saveLog()
	{
		if ( ! String.IsNullOrEmpty(fileText))
		{
			System.IO.StreamWriter file = new System.IO.StreamWriter(Application.dataPath + "/../simple_log.txt");
			file.WriteLine(fileText);
			file.Close();
		}
	}

	private static void parseArguments()
	{
		string[] args = System.Environment.GetCommandLineArgs();

		foreach (string arg in args)
		{
			if (hasDst && dstPath == null)
			{
				dstPath = arg;
				break;
			}
			else if ( ! hasDst && arg.Equals("-saveto", StringComparison.InvariantCultureIgnoreCase))
			{
				hasDst = true;
			}
		}
	}

	private static void createFolders()
	{
		if ( ! System.IO.Directory.Exists(Application.dataPath + "/Blends"))
		{
			log("Source folder (Assets/Blends) does not exist!");
			saveLog();

			Application.Quit();
			System.Environment.Exit(1);
		}

		if ( ! System.IO.Directory.Exists(Application.dataPath + "/Bundles"))
			AssetDatabase.CreateFolder("Assets", "Bundles");

		if (hasDst)
		{
			log("[PARAM] SaveTo: " + dstPath);

			if ( ! System.IO.Directory.Exists(dstPath))
				System.IO.Directory.CreateDirectory(dstPath);
		}
	}

	private static Queue readNames(string filename)
	{
		System.IO.StreamReader file = new System.IO.StreamReader(
				Application.dataPath + "/../" + filename + "_aninames.txt",
				System.Text.Encoding.UTF8,
				true
			);
		
		Queue names = new Queue();
		names.Enqueue("Default Take");

		string line;
		while((line = file.ReadLine()) != null)
			names.Enqueue(line);

		file.Close();
		return names;
	}

	static void convert()
	{
		parseArguments();
		createFolders();

		string[] assetsPaths = AssetDatabase.FindAssets("", new string[1] { blendsPath });
		log("How many assets in Assets/Blends: " + assetsPaths.Length);

		AssetDatabase.Refresh();

		assetsPaths = AssetDatabase.FindAssets("", new string[1] { blendsPath });
		log("How many assets in Assets/Blends: " + assetsPaths.Length);

		foreach (string assetPathID in assetsPaths)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(assetPathID);
			log(assetPath);
		
			if ( ! assetPath.EndsWith(".blend"))
				continue;

			GameObject go = AssetDatabase.LoadAssetAtPath(
				assetPath,
				typeof(UnityEngine.Object)
			) as GameObject;
			
			if (go != null)
			{
				Queue names = readNames(assetPath.Substring(0, assetPath.Length - 6));

				log("Starting " + assetPath.Substring(0, assetPath.Length - 6));
				
				AnimationClip[] clips = AnimationUtility.GetAnimationClips(go);

				log(clips.Length + " animation clips found");
				
				if (clips.Length > 0)
				{
					foreach (AnimationClip clip in clips)
					{
						string filePath = bundlesPath + "/" + names.Dequeue();//clip.name;

						log("Saving " + clip.name + " to " + filePath + " (" + clip.ToString() + ")");
						log("" + clip.name + " to " + filePath);

						BuildPipeline.BuildAssetBundle(
							clip as UnityEngine.Object,
							new UnityEngine.Object[0] {},
							filePath,
							BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
							BuildTarget.WebGL
						);

						if (hasDst)
						{
							string srcPath = Application.dataPath + "/" + bundlesPath + "/" + clip.name;
							File.Move(srcPath, dstPath);
						}
					}

					break;
				}
			}
			else log("Ignoring " + assetPath);
		}

		saveLog();
	}
	
	private static int id = 1;

	private static void log(string txt)
	{
		Debug.Log((id++) + ". " + txt);
		fileText += txt + "\n";
	}
	
}