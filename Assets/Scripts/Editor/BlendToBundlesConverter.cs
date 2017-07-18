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

	private static readonly string CommonPath = "Comuns";
	private static readonly string DisambiguationPath = "Desambiguação";
	private static readonly string CompoundsPath = "Compostos";
	private static readonly string CheckPath = "Verificar";

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
		string folder, filePath;
		
		if (name.Contains("("))
			folder = DisambiguationPath;
		else if (name.Contains("_"))
			folder = CompoundsPath;
		else
			folder = CommonPath;

		if (!name.ToUpper().Equals(name))
			folder += "/Verificar";

		return "Bundles/" + target + "/" + folder;
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
			
			if (!string.IsNullOrEmpty(assetPath))
			{
				string[] tokens = assetPath.Split('/');
				string asset = tokens[tokens.Length - 1].Split('.')[0];

				// buildBundle(assetPath, asset, getFilePath("STANDALONE_WINDOWS", asset), BuildTarget.StandaloneWindows);
				// buildBundle(assetPath, asset, getFilePath("STANDALONE_WINDOWS-64", asset), BuildTarget.StandaloneWindows64);
				buildBundle(assetPath, asset, getFilePath("STANDALONE_LINUX", asset), BuildTarget.StandaloneLinux);
				// buildBundle(assetPath, asset, getFilePath("STANDALONE_LINUX-64", asset), BuildTarget.StandaloneLinux64);
				// buildBundle(assetPath, asset, getFilePath("STANDALONE_LINUX-UNI", asset), BuildTarget.StandaloneLinuxUniversal);
				buildBundle(assetPath, asset, getFilePath("ANDROID", asset), BuildTarget.Android);
				buildBundle(assetPath, asset, getFilePath("WEBGL", asset), BuildTarget.WebGL);
				buildBundle(assetPath, asset, getFilePath("IOS", asset), BuildTarget.iOS);
			}
			else log("Ignoring " + assetPath);
		}
	}

	private static void buildBundle(string asset, string name, string path, BuildTarget target)
	{
		if (!Directory.Exists(path)) Directory.CreateDirectory(path);
		else
		{
			int id = 1;
			string temp = name;
		
			while (File.Exists(temp))
			{
				temp = (id >= 10 ? "_D-" : "_D-0") + id + "__" + name;
				id++;
			}

			name = temp;
		}

		log("Building " + asset + " to " + path + "/" + name);

		AssetBundleBuild buildMap = new AssetBundleBuild();
		buildMap.assetNames = new string[1] { asset };
		buildMap.assetBundleName = name;

		BuildPipeline.BuildAssetBundles(path, new AssetBundleBuild[1] { buildMap }, BuildAssetBundleOptions.None, target);
	}

	private static void buildBundle(string asset, string name, string path) {
		buildBundle(asset, name, path, BuildTarget.NoTarget);
	}
	
	private static int id = 1;

	private static void log(string txt)
	{
		Debug.Log((id++) + ". " + txt);
		fileText += txt + "\n";
	}
	
}