using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public class DropboxSync : EditorWindow
{
	
	public static string dropboxAssetPath;
	public static string md5FilePath;
	public static string localAssetPath;

	//public static string localAssetPath = "/Users/jnuckolls/Documents/Programming/game/island/Assets/Binaries/";

	/// <summary>
	/// Function for assigning the different paths required for menu items to function.
	/// </summary>
	[MenuItem ("Sync/Path Settings")]
	static void SetFolder () 
	{
		dropboxAssetPath = EditorPrefs.GetString("dropbox_path", "c:/users/example/dropbox");
		md5FilePath = EditorPrefs.GetString ("md5_path", md5FilePath);
		localAssetPath = EditorPrefs.GetString ("local_asset_path", localAssetPath);
		EditorWindow.GetWindow<DropboxSync>();
	}

	/// <summary>
	/// Less intelligent version of synch.  Always prefers DropBox files and does not
	///  copy local files to DropBox.
	/// </summary>
//	[MenuItem ("Sync/Sync Binary Assets")]
//	static void SyncAssets () 
//	{
//		dropboxAssetPath = EditorPrefs.GetString("dropbox_path", "c:/users/example/dropbox");
//		Debug.Log("Syncing Assets from: " + dropboxAssetPath + "... please wait...");
//		foreach (string dirPath in Directory.GetDirectories(dropboxAssetPath, "*", SearchOption.AllDirectories))
//			Directory.CreateDirectory(dirPath.Replace(dropboxAssetPath, localAssetPath));
//		
//		foreach (string newPath in Directory.GetFiles(dropboxAssetPath, "*.*", SearchOption.AllDirectories))
//			File.Copy(newPath, newPath.Replace(dropboxAssetPath, localAssetPath), true);
//		
//		AssetDatabase.Refresh();
//		
//		Debug.Log("Done Syncing Assets.");
//	}

	/// <summary>
	/// Synch between local binary asset directory and a DropBox binary asset directory.
	/// </summary>
	[MenuItem ("Sync/Smart Sync")]
	static void SmartSync ()
	{
		dropboxAssetPath = EditorPrefs.GetString("dropbox_path", "c:/users/example/dropbox");
		md5FilePath = EditorPrefs.GetString ("md5_path", "c:/users/example/md5");
		localAssetPath = EditorPrefs.GetString ("local_asset_path", "Assets/Binaries");

		// load serialized md5
		var diskMd5 = GetMd5FromDisk (md5FilePath);

		// load current md5
		var localMd5 = ComputeBinaryDirHash (localAssetPath);

		// compare hashes, overwriting and copying new anything that
		//  is different from local to dropbox
		//  to do this we set destination Hash to the current local has, triggering
		//  a copy for any file that is different than our previous synch.
		synchDirectory (localAssetPath, diskMd5, dropboxAssetPath, localMd5);

		// load dropbox md5 after the copy
		var dropBoxMd5 = ComputeBinaryDirHash (dropboxAssetPath);

		// compare hashes overwriting and copying from dropbox to local
		synchDirectory (dropboxAssetPath, dropBoxMd5, localAssetPath, localMd5);

		// save our new localMd5 for the next comparison.
		SaveMd5ToDisk (GetMd5SavePath(md5FilePath), ComputeBinaryDirHash (localAssetPath));
	}

	/// <summary>
	/// compare originHash to destinationHash
	/// copy anything different from origin to destination.
	/// </summary>
	/// <param name="originPath">Origin path.</param>
	/// <param name="originHash">Origin hash.</param>
	/// <param name="destinationPath">Destination path.</param>
	/// <param name="destinationHash">Destination hash.</param>
	static void synchDirectory(string originPath, 
	                      Dictionary<string, byte []> originHash, 
	                      string destinationPath, 
	                      Dictionary<string, byte []> destinationHash)
	{
		var diffFiles = GetDiffFiles (originHash, destinationHash);

		foreach(string diffFilePath in diffFiles)
		{
			string fullDestinationPath = Path.Combine(destinationPath, diffFilePath);
			string destinationDir = Path.GetDirectoryName(fullDestinationPath);

			if(!Directory.Exists(destinationDir))
			{
				Directory.CreateDirectory(destinationDir);
			}

			File.Copy(
				Path.Combine(originPath, diffFilePath),
				fullDestinationPath,
				true);
		}

	}

	static string GetMd5SavePath(string md5Path)
	{
		return Path.Combine (md5Path, "localmd5.dat");
	}

	/// <summary>
	/// Retrieve a serialized Md5 from disk.
	/// </summary>
	/// <returns>The md5 from disk.</returns>
	/// <param name="md5Path">Md5 path.</param>
	static Dictionary<string, byte[]> GetMd5FromDisk(string md5Path)
	{
		if(!File.Exists(md5Path))
		{
			return SaveMd5ToDisk(GetMd5SavePath(md5Path), ComputeBinaryDirHash(localAssetPath));
		}

		Dictionary<string, byte[]> md5Dict = new Dictionary<string, byte[]>();

		using(var fileStream = new StreamReader (md5Path))
		{

			BinaryFormatter serializer = new BinaryFormatter ();

			md5Dict = (Dictionary<string, byte[]>)serializer.Deserialize(fileStream.BaseStream);
		}

		return md5Dict;
	}

	/// <summary>
	/// Serialize a binary dir Md5 and save it to disk.
	/// </summary>
	/// <returns>The md5 to disk.</returns>
	/// <param name="md5Path">Md5 path.</param>
	/// <param name="md5Dict">Md5 dict.</param>
	static Dictionary<string, byte[]> SaveMd5ToDisk(string md5Path, Dictionary<string, byte[]> md5Dict)
	{
		using(StreamWriter writer = new StreamWriter(md5Path, false))
		{
			BinaryFormatter serializer = new BinaryFormatter();
			serializer.Serialize(writer.BaseStream, md5Dict);
		}

		return md5Dict;
	}

	void OnGUI()
	{
		GUILayout.Label ("Set a path to a folder in dropbox that will contain your binary assets.", EditorStyles.boldLabel);
		dropboxAssetPath = EditorGUILayout.TextField ("DropBox Path", dropboxAssetPath);
		md5FilePath = EditorGUILayout.TextField ("TempPath", md5FilePath);
		localAssetPath = EditorGUILayout.TextField ("Local Asset Path", localAssetPath);

		if(GUILayout.Button("Save")) {
			EditorPrefs.SetString("dropbox_path", dropboxAssetPath);
			EditorPrefs.SetString ("md5_path", md5FilePath);
			EditorPrefs.SetString ("local_asset_path", localAssetPath);
		}
	}

	/// <summary>
	/// Compute a binary dir hash collection from the given path.
	/// </summary>
	/// <returns>The binary dir hash.</returns>
	/// <param name="binaryDirPath">Binary dir path.</param>
	static Dictionary<string, byte[]> ComputeBinaryDirHash(string binaryDirPath)
	{
		//path needs to end in the dir seperator in order
		// to properly replace root directory when computing hash keys.
		if(!binaryDirPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
		{
			binaryDirPath += Path.DirectorySeparatorChar;
		}

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
				string key = file.FullName.Replace(binaryDirPath, "");
				computedHashes.Add(key, Md5Sum(file.FullName));
			}

			// push sub dirs onto the stack.
			foreach(DirectoryInfo newDir in currentDir.GetDirectories())
			{
				directories.Push(newDir);
			}
		}

		return computedHashes;
	}

	/// <summary>
	/// Get the Md5 checksum of a file.
	/// </summary>
	/// <returns>The sum.</returns>
	/// <param name="fileName">File name.</param>
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