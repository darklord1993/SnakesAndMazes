//#define DBG

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

[BasicView]
public class Store : BetterScriptableObject
{
	public Dictionary<string, UnityObject> Items
	{
		get { return _items ?? (_items = new Dictionary<string, UnityObject>()); }
	}

	[Serialize, Readonly, FormatMember("$name"), FormatPair("$valueType: $key")]
	private Dictionary<string, UnityObject> _items;

	/// <summary>
	/// A dictionary of keys representing all the item types we could store
	/// and values being whether or not the type needs inheritance support from the serializer
	/// </summary>
	public static readonly Dictionary<Type, bool> ItemTypes = new Dictionary<Type, bool>()
	{
		{ typeof(Mesh),						 false },
		{ typeof(AudioClip),      			 false },
		{ typeof(Material),       			 false },
		{ typeof(PhysicMaterial), 			 false },
		{ typeof(Flare), 					 false },
		{ typeof(GUIStyle), 				 false },
		{ typeof(Texture),                   true  },
		{ typeof(RuntimeAnimatorController), true  },
		{ typeof(AnimationClip),             true  },
	};

	public static Store Current
	{
		get { return StoreManager.Current.Store; }
	}

	public string StoreAsset(UnityObject asset)
	{
		var name = asset.name;
		Items[name] = asset;
		return name;
	}

	//public string StoreGameObject(GameObject go)
	//{
	//	var id = go.GetId();
	//	Items[id] = go;
	//	return id;
	//}

	public string StoreComponent(Component comp)
	{
		var key = comp.gameObject.name + comp.GetType().Name;
		Items[key] = comp;
		return key;
	}

	public UnityObject RetrieveItem(string key)
	{
		UnityObject result;
		if (!Items.TryGetValue(key, out result))
		{
			Debug.LogError("Trying to retrieve an item that doesn't exist: " + key);
		}
		return result;
	}

#if UNITY_EDITOR
#if DBG
	[Show] void Print()
	{
		Items.Foreach(x => Debug.Log(x.Key + " " + x.Value));
	}
#endif

	/// <summary>
	/// Captures the state of this scene to populate the Store references (essentially saves the scene and serializes asset references)
	/// This needs to be done only once when you have your scene configured
	/// If you later on change things in the scene (add new scripts with AudioClips, Textures, Materials etc) you need to capture again
	/// Just make sure that whatever audio clips, materials, textures, etc your scripts references are available in the store
	/// </summary>
	[Show] public void Populate()
	{
		Items.Clear();

		SaveManager.SaveWithPrefix("_populate");

		// sort pairs just to make the dictionary look nicer in the inspector
		_items = Items.Select(x => new { x.Key, x.Value })
					 .OrderBy(x => x.Value.GetType().Name)
					 .ToDictionary(x => x.Key, x => x.Value);
	}
#endif
}