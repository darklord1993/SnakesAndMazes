using UnityEngine;
using Vexe.Runtime.Types;
using Vexe.Runtime.Extensions;

#if UNITY_EDITOR
using UnityEditor;
using Vexe.Runtime.Helpers;
using System.IO;
using System.Linq;
#endif

[BasicView, DisallowMultipleComponent]
public class StoreManager : BetterBehaviour
{
	[Inline]
	public Store Store { get; set; }

	private static StoreManager _current;
	public static StoreManager Current
	{
		get
		{
			if (_current == null)
			{
				var name = typeof(StoreManager).Name;
				var go = GameObject.Find(name);
				if (go == null)
					go = new GameObject(name);
				_current = go.GetOrAddComponent<StoreManager>();
			}
			return _current;
		}
	}

#if UNITY_EDITOR
	private string _storesPath;
	private string storesPath
	{
		get { return DirectoryHelper.LazyGetDirectoryPath(ref _storesPath, "Stores"); }
	}

	[Hide, Serialize]
	private string _storeName;

	[Show, Popup("GetStores")]
	private string AvailableStores
	{
		get { return _storeName; }
		set
		{
			_storeName = value;
			var file = Directory.GetFiles(storesPath).FirstOrDefault(x => x.Substring(x.IndexOf("_") + 1).RemoveExtension() == value);
			if (!string.IsNullOrEmpty(file))
				Store = AssetDatabase.LoadAssetAtPath(file, typeof(Store)) as Store;
		}
	}

	private string[] GetStores()
	{
		return Directory.GetFiles(storesPath)
						.Select(x => { var result = x.Substring(x.IndexOf('_') + 1); return result.Remove(result.IndexOf('.')); })
						.ToArray();
	}

	/// <summary>
	/// Creates a Store asset named after the current scene if there's not one already and sets the current Store reference to it
	/// </summary>
	/// <returns></returns>
	[Show] void LoadOrCreateStore()
	{
		var name = typeof(Store).Name + "_" + SaveManager.CurrentSceneName + ".asset";
		var path = Path.Combine(storesPath, name);
		var store = AssetDatabase.LoadAssetAtPath(path, typeof(Store)) as Store;
		if (store == null)
		{
			store = ScriptableObject.CreateInstance<Store>();
			AssetDatabase.CreateAsset(store, path);
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		}

		this.Store = store;
	}
#endif
}
