using System;
using System.Linq;
using FullSerializer;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Helpers;
using UnityObject = UnityEngine.Object;

/// <summary>
/// The FullSerializer converter to use to serialize/deserialize asset references (Textures, Meshes, Materials, AudioClips, AnimationClips, etc)
/// Instead of serializing the actual bytes of meshes, audio etc we just save thsoe references into a store object live in the scene.
/// When loading, we just ask the store to give us the item.
/// Note the use of the name of the assets meaning assets you want to save *must* have unique names
/// </summary>
public class fsAssetReferenceConverter : fsConverter
{
	public override bool RequestCycleSupport(Type storageType)
	{
		return false;
	}

	public override bool RequestInheritanceSupport(Type storageType)
	{
		return Store.ItemTypes[storageType];
	}

	public override bool CanProcess(Type type)
	{
		return canProcess(type);
	}

	private static Func<Type, bool> _canProcess;
	private static Func<Type, bool> canProcess
	{
		get
		{
			return _canProcess ?? (_canProcess = new Func<Type, bool>(x =>
			{
				if (Store.ItemTypes.ContainsKey(x))
					return true;
				return Store.ItemTypes.Keys.Any(x.IsA);
			}).Memoize());
		}
	}

	public override fsStatus TrySerialize(object instance, out fsData serialized, Type storageType)
	{
		// StoreItem gives us back the name of the asset
		serialized = Store.Current.StoreAsset(instance as UnityObject);
		return fsStatus.Success;
	}

	public override fsStatus TryDeserialize(fsData data, ref object instance, Type storageType)
	{
		// the data holds the name of the asset since that's what we serialized...
		instance = Store.Current.RetrieveItem(data.AsString);
		return fsStatus.Success;
	}
}

/// <summary>
/// This is the converter that's used to [de]serialize any Component references
/// We serialize the component's gameObject id, the parent id and the component type name
/// When deserializing, we ask our Cache for the gameObject and parent id that we serialized,
/// the Cache will create the gameObject and parent if they're non existent, 
/// which means we don't have to worry about deferring the assignment of references or anything
/// Once we have the gameObject we add the component we serialized by its name
/// this is why currently you *must* have unique components within gameObjects you save (i.e. one Rigidbody etc)
/// </summary>
public class fsComponentReferenceConverter : fsConverter
{
	public override bool CanProcess(Type type)
	{
		return type.IsA<Component>();
	}

	public override fsStatus TrySerialize(object instance, out fsData serialized, Type storageType)
	{
		serialized = fsData.CreateList();
		var list   = serialized.AsList;
		var comp   = instance as Component;

		if (comp.gameObject.IsPrefab())
		{
			string key = Store.Current.StoreComponent(comp);
			list.Add(true);
			list.Add(key);
		}
		else
		{
			list.Add(false);
			list.Add(comp.gameObject.name);
			list.Add(comp.gameObject.GetParentId());
			list.Add(comp.gameObject.GetId());
			list.Add(comp.GetType().Name);
		}

		return fsStatus.Success;
	}

	public override fsStatus TryDeserialize(fsData data, ref object instance, Type storageType)
	{
		// This is basically a hack.
		// Since this converter is for Component references and we can't have 'pure' Component objects
		// (i.e. we have Transforms, BoxCollider, Rigidbody etc all of these 'is a' Component but their GetType() != typeof(Component))
		// FullSerializer will try to create an instance of the component and pass it to TryDeserialize as ref object instance
		// we can actually fetch the serialized data in CreateInstance and connect the component to the proper gameObject
		// so the instance that is passed to us here 'should' actually be the correct component reference
		// in other words, we don't need to do anything here
		return fsStatus.Success;
	}

	public override object CreateInstance(fsData data, Type storageType)
	{
		var dict       = data.AsDictionary;
		var content    = dict["$content"].AsList;
		var isOnPrefab = content[0].AsBool;

		if (isOnPrefab)
		{
			var key = content[1].AsString;
			return Store.Current.RetrieveItem(key);
		}
		else
		{
			var goName     = content[1].AsString;
			var parentId   = content[2].AsString;
			var goId       = content[3].AsString;
			var typeName   = content[4].AsString;
			var gameObject = Cache.GetOrCreateGO(goName, goId, parentId);
			return gameObject.GetOrAddComponent(typeName);
		}
	}
}

public class fsGameObjectReferenceConverter : fsConverter
{
	public override bool RequestInheritanceSupport(Type storageType)
	{
		return false;
	}

	public override bool RequestCycleSupport(Type storageType)
	{
		return false;
	}

	public override bool CanProcess(Type type)
	{
		return type == typeof(GameObject);
	}

	public override fsStatus TrySerialize(object instance, out fsData serialized, Type storageType)
	{
		serialized = fsData.CreateList();
		var list   = serialized.AsList;
		var go     = instance as GameObject;

		if (go.IsPrefab())
		{
			string name = Store.Current.StoreAsset(go);
			list.Add(true);
			list.Add(name);
		}
		else
		{
			list.Add(false);
			list.Add(go.name);
			list.Add(go.GetParentId());
			list.Add(go.GetId());
		}

		return fsStatus.Success;
	}

	public override fsStatus TryDeserialize(fsData data, ref object instance, Type storageType)
	{
		var list     = data.AsList;
		var isPrefab = list[0].AsBool;
		var goName   = list[1].AsString;

		if (isPrefab)
		{
			instance = Store.Current.RetrieveItem(goName);
		}
		else
		{
			var parentId = list[2].AsString;
			var goId     = list[3].AsString;
			instance     = Cache.GetOrCreateGO(goName, goId, parentId);
		}

		return fsStatus.Success;
	}
}
