using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public class DropboxSync : EditorWindow
{
	
	public static string dropboxPath;
	public static string md5FilePath;

	public static string destination = "Assets/Binaries";
	
	[MenuItem ("Sync/Set Asset Folder...")]
	static void SetFolder () 
	{
		dropboxPath = EditorPrefs.GetString("dropbox_path", "c:/users/example/dropbox");
		EditorWindow.GetWindow<DropboxSync>();
	}
	
	[MenuItem ("Sync/Sync Binary Assets")]
	static void SyncAssets () 
	{
		dropboxPath = EditorPrefs.GetString("dropbox_path", "c:/users/example/dropbox");
		Debug.Log("Syncing Assets from: " + dropboxPath + "... please wait...");
		foreach (string dirPath in Directory.GetDirectories(dropboxPath, "*", SearchOption.AllDirectories))
			Directory.CreateDirectory(dirPath.Replace(dropboxPath, destination));
		
		foreach (string newPath in Directory.GetFiles(dropboxPath, "*.*", SearchOption.AllDirectories))
			File.Copy(newPath, newPath.Replace(dropboxPath, destination), true);
		
		AssetDatabase.Refresh();
		
		Debug.Log("Done Syncing Assets.");
	}

	[MenuItem ("Sync/Smart Sync")]
	static void SmartSync ()
	{
		dropboxPath = EditorPrefs.GetString("dropbox_path", "c:/users/example/dropbox");
		md5FilePath = EditorPrefs.GetString ("md5_path", "c:/users/example/md5");

		// load serialized md5
		var diskMd5 = GetMd5FromDisk (md5FilePath);

		// load current md5
		var localMd5 = ComputeBinaryDirHash (destination);

		// compare hashes, overwriting and copying new anything that
		//  is different from local to dropbox

		// load dropbox md5 after the copy
		var dropBoxMd5 = ComputeBinaryDirHash (dropboxPath);

		// compare hashes overwriting and copying from dropbox to local
	}

	static void synchDirectory(string originPath, 
	                      Dictionary<string, byte []> originHash, 
	                      string destinationPath, 
	                      Dictionary<string, byte []> destinationHash)
	{

	}

	static Dictionary<string, byte[]> GetMd5FromDisk(string md5Path)
	{
		Dictionary<string, byte[]> md5Dict = new Dictionary<string, byte[]>();

		using(var fileStream = new StreamReader (md5Path))
		{

			BinaryFormatter serializer = new BinaryFormatter ();

			md5Dict = (Dictionary<string, byte[]>)serializer.Deserialize(fileStream.BaseStream);
		}

		return md5Dict;
	}

	static void SaveMd5ToDisk(string md5Path, Dictionary<string, byte[]> md5Dict)
	{
		using(StreamWriter writer = new StreamWriter(md5Path, false))
		{
			BinaryFormatter serializer = new BinaryFormatter();
			serializer.Serialize(writer.BaseStream, md5Dict);
		}
	}

	void OnGUI()
	{
		GUILayout.Label ("Set a path to a folder in dropbox that will contain your binary assets.", EditorStyles.boldLabel);
		dropboxPath = EditorGUILayout.TextField ("DropBox Path", dropboxPath);
		md5FilePath = EditorGUILayout.TextField ("TempPath", md5FilePath);

		if(GUILayout.Button("Save")) {
			EditorPrefs.SetString("dropbox_path", dropboxPath);
			EditorPrefs.SetString ("md5_path", md5FilePath);
		}
	}

	static Dictionary<string, byte[]> ComputeBinaryDirHash(string binaryDirPath)
	{
		Stack<DirectoryInfo> directories = new Stack<DirectoryInfo>();

		directories.Push (new DirectoryInfo(binaryDirPath));

		Dictionary<string, byte []> computedHashes = new Dictionary<string, byte[]> ();

		while(directories.Count > 0)
		{
			DirectoryInfo currentDir = directories.Pop();

			// add all the files to computed hash.
			foreach(FileInfo file in currentDir.GetFiles())
			{
				// we only want the sub-dir path, otherwise the keys will not
				//  match between DropBox and local.
				computedHashes.Add(file.FullName.Replace(binaryDirPath, ""), Md5Sum(file.FullName));
			}

			// push sub dirs onto the stack.
			foreach(DirectoryInfo newDir in currentDir.GetDirectories())
			{
				directories.Push(newDir);
			}
		}

		return computedHashes;
	}

	static byte [] Md5Sum(string fileName)
	{
		using (var md5 = System.Security.Cryptography.MD5.Create())
		{
			using (var stream = File.OpenRead(fileName))
			{
				return md5.ComputeHash(stream);
			}
		}
	}

	/// <summary>
	/// Gets the difference between authority and current.  Returns a list of paths from auth
	/// </summary>
	/// <returns>The diff files.</returns>
	/// <param name="authority">Authority.</param>
	/// <param name="current">Current.</param>
	static List<string> GetDiffFiles(Dictionary<string, byte []> authority, Dictionary<string, byte []> diff)
	{
		List<string> diffFiles = new List<string> ();
		foreach(string key in authority.Keys)
		{
			if(diff.ContainsKey(key))
			{
				if(CompareByteArrays(authority[key], diff[key]))
				{
					continue;
				}
			}

			diffFiles.Add(key);
		}

		return diffFiles;
	}

	static bool CompareFiles(string fileOne, string fileTwo)
	{
		byte [] fileOneBytes = Md5Sum(fileOne);
		byte [] fileTwoBytes = Md5Sum (fileTwo);

		return CompareByteArrays (fileOneBytes, fileTwoBytes);
	}

	static bool CompareByteArrays(byte [] b1, byte [] b2)
	{
		if(b1.Length != b2.Length)
		{
			return false;
		}
		
		for(int i=0; i<b1.Length; i++)
		{
			if (b1[i] != b2[i])
			{
				return false;
			}
		}
		
		return true;
	}
}