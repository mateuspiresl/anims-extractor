/*
 *	 "C:\Program Files\Unity\Editor\Unity.exe" -quit -batchmode -projectPath C:\Workspace\wikilibras-player\playercore_blend -executeMethod BlendToBundlesConverter.convert
 */

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BlendToBundlesConverter {

	private static string blendsPath = "Assets/Blends";
	private static string bundlesPath = "Assets/Bundles";

	private static string commonPath = "Assets/Bundles/Comuns";
	private static string disambiguationPath = "Assets/Bundles/Desambiguação";
	private static string compoundsPath = "Assets/Bundles/Compostos";

	private static string duplicatedCommonPath = "Assets/Bundles/_D_Comuns";
	private static string duplicatedDisambiguationPath = "Assets/Bundles/_D_Desambiguação";
	private static string duplicatedCompoundsPath = "Assets/Bundles/_D_Compostos";

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

		if ( ! System.IO.Directory.Exists(Application.dataPath + "/Bundles/Comuns"))
			AssetDatabase.CreateFolder("Assets/Bundles", "Comuns");

		if ( ! System.IO.Directory.Exists(Application.dataPath + "/Bundles/Desambiguação"))
			AssetDatabase.CreateFolder("Assets/Bundles", "Desambiguação");

		if ( ! System.IO.Directory.Exists(Application.dataPath + "/Bundles/Compostos"))
			AssetDatabase.CreateFolder("Assets/Bundles", "Compostos");

		if ( ! System.IO.Directory.Exists(Application.dataPath + "/Bundles/_D_Comuns"))
			AssetDatabase.CreateFolder("Assets/Bundles", "_D_Comuns");

		if ( ! System.IO.Directory.Exists(Application.dataPath + "/Bundles/_D_Desambiguação"))
			AssetDatabase.CreateFolder("Assets/Bundles", "_D_Desambiguação");

		if ( ! System.IO.Directory.Exists(Application.dataPath + "/Bundles/_D_Compostos"))
			AssetDatabase.CreateFolder("Assets/Bundles", "_D_Compostos");

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

	private static string getFilePath(string name)
	{
		string folder;
		bool desamb = name.Contains("(");
		bool comp = name.Contains("_");

		if (desamb)
			folder = disambiguationPath;
		else if (comp)
			folder = compoundsPath;
		else
			folder = commonPath;

		string filePath = folder + "/" + name;

		int dID = 1;
		while (File.Exists(Application.dataPath + "/../" + filePath))
		{
			if (desamb)
				folder = duplicatedDisambiguationPath;
			else if (comp)
				folder = duplicatedCompoundsPath;
			else
				folder = duplicatedCommonPath;

			filePath = folder + "/" + name + (dID < 10 ? "_0" : "_") + dID;
			dID++;
		}

		return filePath;
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

		HashSet<string> converted = new HashSet<string>();

		foreach (string assetPathID in assetsPaths)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(assetPathID);

			if (assetPath.EndsWith(".blend") && ! converted.Contains(assetPath))
			{
				converted.Add(assetPath);
				log(assetPath);
			}
		}

		converted = new HashSet<string>();

		foreach (string assetPathID in assetsPaths)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(assetPathID);
			
			if (assetPath.EndsWith(".blend") && ! converted.Contains(assetPath))
				converted.Add(assetPath);
			else
				continue;

			GameObject go = AssetDatabase.LoadAssetAtPath(
				assetPath,
				typeof(UnityEngine.Object)
			) as GameObject;
			
			if (go != null)
			{
				Queue names = readNames(assetPath.Substring(0, assetPath.Length - 6));
				AnimationClip[] clips = AnimationUtility.GetAnimationClips(go);

				log("Starting " + assetPath.Substring(0, assetPath.Length - 6));
				log(clips.Length + " animation clips found");
				
				if (clips.Length > 0)
				{
					foreach (AnimationClip clip in clips)
					{
						string filePath = getFilePath((string) names.Dequeue());

						log("Saving " + clip.name + " to " + filePath + " (" + clip.ToString() + ")");

						BuildPipeline.BuildAssetBundle(
							clip as UnityEngine.Object,
							new UnityEngine.Object[0] {},
							filePath,
							BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
							BuildTarget.WebGL
						);
					}
				}
			}
			else log("Ignoring " + assetPath);
		}

		if (hasDst)
		{
			string srcPath = Application.dataPath + "/../" + bundlesPath;
			File.Move(srcPath, dstPath);
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