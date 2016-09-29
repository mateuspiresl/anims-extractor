/*
 *	 "C:\Program Files\Unity\Editor\Unity.exe" -quit -batchmode -projectPath C:\Workspace\wikilibras-player\playercore_blend -executeMethod BlendToBundlesConverter.convert
 */
#pragma warning disable

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BlendToBundlesConverter {

	private static string blendsPath = "Assets/Blends";

	private static string commonPath = "Comuns";
	private static string disambiguationPath = "Desambiguação";
	private static string compoundsPath = "Compostos";
	private static string checkPath = "Verificar";

	private static string duplicatedCommonPath = "_D_Comuns";
	private static string duplicatedDisambiguationPath = "_D_Desambiguação";
	private static string duplicatedCompoundsPath = "_D_Compostos";
	private static string duplicatedCheckPath = "_D_Verificar";

	private static string[] targets = new String[] {
		"STANDALONE", "ANDROID", "WEBGL", "IOS"
	};

	private static string fileText = "";

	private static void saveLog()
	{
		if ( ! String.IsNullOrEmpty(fileText))
		{
			System.IO.StreamWriter file = new System.IO.StreamWriter(Application.dataPath + "/../simple_log.txt");
			file.WriteLine(fileText);
			file.Close();
		}
	}

	private static void mkdir(string folder)
	{
		if ( ! System.IO.Directory.Exists(Application.dataPath + "/" + folder))
			Directory.CreateDirectory(Application.dataPath + "/" + folder);
			//AssetDatabase.CreateFolder("Assets", folder);
	}

	private static void mkdir(string path, string folder)
	{
		if ( ! System.IO.Directory.Exists(Application.dataPath + "/" + path + "/" + folder))
			Directory.CreateDirectory(Application.dataPath + "/" + path + "/" + folder);
			//AssetDatabase.CreateFolder("Assets" + "/" + path, folder);
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

		mkdir("Anims");
		mkdir("../Bundles");

		string[] folders = new String[] {
			"Comuns", "Desambiguação", "Compostos"
		};

		for (int t = 0; t < targets.Length; t++)
		{
			mkdir("../Bundles/" + targets[t]);

			for (int f = 0; f < folders.Length; f++)
			{
				string dir = "../Bundles/" + targets[t];

				mkdir(dir + "/" + folders[f]);
				mkdir(dir + "/_D_" + folders[f]);
				mkdir(dir + "/" + folders[f] + "/Verificar");
				mkdir(dir + "/_D_" + folders[f] + "/Verificar");
			}
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

	private static string getFilePath(string target, string name)
	{
		string	folder, filePath;
		bool	invalid = ! name.ToUpper().Equals(name);
		
		if (name.Contains("("))
			folder = disambiguationPath;
		
		else if (name.Contains("_"))
			folder = compoundsPath;
		
		else
			folder = commonPath;

		filePath = "Bundles/" + target + "/" + folder + (invalid ? "/Verificar/" : "/") + name;

		int dID = 1;
		while (File.Exists(Application.dataPath + filePath))
		{
			filePath = "Bundles/" + target + "/_D_" + folder + (invalid ? "/Verificar/" : "/");
			filePath += name + (dID < 10 ? "_0" : "_") + dID;
			dID++;
		}

		return filePath;
	}

	static void convert()
	{
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

				Regex rgx = new Regex("[\\<\\>\\:\\\"\\\\\\/\\|\\?\\*]");

				if (clips.Length > 0)
				{
					foreach (AnimationClip clip in clips)
					{
						AnimationClip newClip = new AnimationClip();
						EditorUtility.CopySerialized(clip, newClip);

						string name = rgx.Replace((string) names.Dequeue(), "_");
						string path = "Assets/Anims/" + name + ".anim";
						log("Saving " + path);

						AssetDatabase.CreateAsset(newClip, path);
						AssetDatabase.SaveAssets();
					}
				}
			}
			else log("Ignoring " + assetPath);
		}

		buildBundles();
		saveLog();
	}

	private static void buildBundles()
	{
		string[] assetsPaths = AssetDatabase.FindAssets("", new string[1] { "Assets/Anims" });
		
		log("How many assets in Assets/Anims: " + assetsPaths.Length);
		
		foreach (string assetPathID in assetsPaths){
			
			string assetPath = AssetDatabase.GUIDToAssetPath(assetPathID);
			
			UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(
				assetPath,
				(typeof(UnityEngine.Object))) as UnityEngine.Object;
			
			if (asset != null)
			{
				string[] tokens = assetPath.Split('/');
				tokens = tokens[tokens.Length - 1].Split('.');
				
				log("Building " + assetPath + " to " + getFilePath("STANDALONE", tokens[0]));
				
				buildBundle(asset, getFilePath("STANDALONE", tokens[0]));
				buildBundle(asset, getFilePath("ANDROID", tokens[0]), BuildTarget.Android);
				buildBundle(asset, getFilePath("WEBGL", tokens[0]), BuildTarget.WebGL);
				buildBundle(asset, getFilePath("IOS", tokens[0]), BuildTarget.iOS);
			}
			else log("Ignoring " + assetPath);
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
	
	private static int id = 1;

	private static void log(string txt)
	{
		Debug.Log((id++) + ". " + txt);
		fileText += txt + "\n";
	}
	
}