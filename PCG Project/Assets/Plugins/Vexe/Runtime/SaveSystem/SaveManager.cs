#define PROFILE
//#define DBG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FullSerializer;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Helpers;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

public static class SaveManager
{
	/// <summary>
	/// Credits to https://github.com/jacobdufault/fullserializer#
	/// </summary>
	public static fsSerializer Serializer { get; set; }

	/// <summary>
	/// Add support to more Unity components by writing and specifying converters.
	/// For ex to add support for 2D components, say Rigidbody2D:
	///		- Write a Rigidbody2DConverter (very similar to RigidbodyConverter, take a look at it in SaveConverters.cs)
	///		- Add the converter some where from your own scripts (more preferred than hard-coding it in the Save System code):
	///			SaveManager.Converters[typeof(Rigidbody2D)] = () => new Rigidbody2DConverter();
	/// </summary>
	public static Dictionary<Type, Func<SaveConverter>> Converters { get; set; }

	/// <summary>
	/// Defaults to Application.dataPath + "/SaveData"
	/// </summary>
	public static string SaveLocation { get; set; }

	public static bool IsSaving  { get; private set; }
	public static bool IsLoading { get; private set; }

	public static string QuickSavePostfix = "_quick.txt"; // doesn't matter what the extension is, but .txt just to make it readable from the inspector
	public static string CurrentSceneName
	{
		get
		{
			string sceneName;

			#if UNITY_EDITOR
			sceneName = Application.isPlaying ? Application.loadedLevelName : Path.GetFileNameWithoutExtension(UnityEditor.EditorApplication.currentScene);
			if (string.IsNullOrEmpty(sceneName))
			{
				sceneName = "Unnamed";
			}
			#else
			sceneName = Application.loadedLevelName;
			#endif

			return sceneName;
		}
	}

	private static Func<Type, SaveConverter> getConverter;
	private static Func<Type, SaveConverter> GetConverter
	{
		get
		{
			return getConverter ?? (getConverter = new Func<Type, SaveConverter>(forType =>
			{
				Func<SaveConverter> get;
				if (!Converters.TryGetValue(forType, out get))
				{
					var key = Converters.Keys.FirstOrDefault(x => forType.IsA(x));
					if (key == null)
						return null;
						//throw new KeyNotFoundException("No suitable converter found for type " + forType.Name);
					get = Converters[key];
				}

				if (get == null)
					return null;

				var converter = get();
				converter.Serializer = Serializer;
				return converter;

			}).Memoize());
		}
	}

	static SaveManager()
	{
		Serializer = new fsSerializer();
		Serializer.AddConverter<fsGameObjectReferenceConverter>();
		Serializer.AddConverter<fsAssetReferenceConverter>();
		Serializer.AddConverter<fsComponentReferenceConverter>();

		Converters = new Dictionary<Type, Func<SaveConverter>>
		{
			{ typeof(GameObject),			() => new GameObjectConverter()             },
			{ typeof(Transform),			() => new TransformConverter()              },
			{ typeof(MonoBehaviour),		() => new MonoBehaviourConverter()          },
			{ typeof(Rigidbody),			() => new RigidbodyConverter()              },
			{ typeof(BoxCollider),			() => new BoxColliderConverter()            },
			{ typeof(SphereCollider),		() => new SphereColliderConverter()         },
			{ typeof(CapsuleCollider),		() => new CapsuleColliderConverter()        },
			{ typeof(MeshCollider),			() => new MeshColliderConverter()           },
			{ typeof(MeshFilter),			() => new MeshFilterConverter()             },
			{ typeof(AudioSource),			() => new AudioSourceConverter()            },
			{ typeof(Animator),				() => new AnimatorConverter()               },
			{ typeof(Animation),			() => new AnimationConverter()              },
			{ typeof(CharacterController),	() => new CharacterControllerConverter()    },
			{ typeof(Camera),				() => new CameraConverter()                 },
			{ typeof(MeshRenderer),			() => new RendererConverter<MeshRenderer>() },
			{ typeof(LineRenderer),			() => new RendererConverter<LineRenderer>() },
			{ typeof(SkinnedMeshRenderer),	() => new SkinnedMeshRendererConverter()    },
			{ typeof(TrailRenderer),		() => new TrailRendererConverter()          },
			{ typeof(ParticleRenderer),		() => new ParticleRendererConverter()		},
			{ typeof(Light),				() => new LightConverter()					},
			{ typeof(ParticleAnimator),		() => new ParticleAnimatorConverter()       },
			{ typeof(ParticleEmitter),		null										},
			{ typeof(GUILayer),				null 										},
			{ typeof(AudioListener),		null 										},
			{ typeof(ClothRenderer),		null 										},
			{ typeof(Cloth),				null 										},
			{ typeof(Joint),				null 										},
		};

		SaveLocation = Path.Combine(Application.dataPath, "SaveData");
	}

	/// <summary>
	/// Saves the current scene with QuickSavePostfix to SaveLocation
	/// </summary>
	public static SaveInfo QuickSave()
	{
		return SaveWithPrefix(QuickSavePostfix);
	}

	/// <summary>
	/// Saves the current scene with the specified postfix to the current SaveLocation
	/// </summary>
	public static SaveInfo SaveWithPrefix(string postfix)
	{
		var path = Path.Combine(SaveLocation, CurrentSceneName + postfix);
		return SaveAtPath(path);
	}

	/// <summary>
	/// Saves the current scene to the specified path
	/// Creates the path if it doesn't exist
	/// </summary>
	public static SaveInfo SaveAtPath(string path)
	{
		if (!Directory.Exists(SaveLocation))
			Directory.CreateDirectory(SaveLocation);

		if (!File.Exists(path))
			File.Create(path).Close();

		Action<fsData, StreamWriter> writeline = (data, writer) =>
		{
			fsJsonPrinter.CompressedJson(data, writer);
			writer.WriteLine();
		};

		IsSaving = true;
		EventManager.Raise(new BeganSavingEvent());


#if !PROFILE
		var markers = UnityObject.FindObjectsOfType<SaveMarker>();
		using (var writer = new StreamWriter(path))
		{
			var goConverter = GetConverter(typeof(GameObject));
			for (int i = 0; i < markers.Length; i++)
			{
				var marker = markers[i];

				fsData data = goConverter.SerializeInstanceIntoData(marker.gameObject);
				writeline(data, writer);

				var toSave = marker.ComponentsToSave;
				for (int j = 0; j < toSave.Count; j++)
				{
					var comp = marker.GetComponent(toSave[j]);
					var type = comp.GetType();

					var converter = GetConverter(type);
					if (converter == null)
						continue;

					data = converter.SerializeInstanceIntoData(comp);
					writeline(data, writer);
				}
			}
		}
#else
		var markers = UnityObject.FindObjectsOfType<SaveMarker>();
		Profiler.BeginSample("Saving");
		using (var writer = new StreamWriter(path))
		{
			var goConverter = GetConverter(typeof(GameObject));
			for (int i = 0; i < markers.Length; i++)
			{
				var marker = markers[i];
				Profiler.BeginSample("Saving GO: " + marker.name);
#if DBG
				Debug.Log("Saving GO: " + go.name);
#endif
				fsData data = goConverter.SerializeInstanceIntoData(marker.gameObject);
				writeline(data, writer);
				Profiler.EndSample();
				var toSave = marker.ComponentsToSave;
				for (int j = 0; j < toSave.Count; j++)
				{
					var comp = marker.GetComponent(toSave[j]);
					var type = comp.GetType();

					var converter = GetConverter(type);
					if (converter == null)
						continue;
					Profiler.BeginSample("Saving Comp: " + type.Name);
#if DBG
					Debug.Log("Saving Comp: " + type.Name);
#endif
					Profiler.BeginSample("Serializing Comp: " + type.Name);
					data = converter.SerializeInstanceIntoData(comp);
					Profiler.EndSample();
					Profiler.BeginSample("Writing Comp: " + type.Name);
					writeline(data, writer);
					Profiler.EndSample();
					Profiler.EndSample();
				}
			}
		}

		Profiler.EndSample();
#endif

		IsSaving = false;
		EventManager.Raise(new FinishedSavingEvent());

		fsData.ResetPools();

		return new SaveInfo(Path.GetFileNameWithoutExtension(path), DateTime.Now);
	}

	/// <summary>
	/// Loads the current scene from the data at SaveLoction + QuickSavePostfix
	/// </summary>
	public static void QuickLoad()
	{
		var quick = CurrentSceneName + QuickSavePostfix;
		LoadByFileName(quick);
	}

	/// <summary>
	/// Loads the current scene from the data at SaveLoction/saveFileName
	/// </summary>
	public static void LoadByFileName(string saveFileName)
	{
		var path = Path.Combine(SaveLocation, saveFileName);
		LoadByPath(path);
	}

	/// <summary>
	/// Loads the current scene from the data at the specified path
	/// Throws an InvalidOperationException if the path doesn't exist
	/// </summary>
	public static void LoadByPath(string path)
	{
		Assert.PathExists(path);

		Cache.UpdateGOsCache();

		IsLoading = true;
		EventManager.Raise(new BeganLoadingEvent());

#if !PROFILE
		using (var reader = new StreamReader(path))
		{
			GameObject lastRead = null;
			string line;
			while (reader.TryReadLine(out line))
			{
				fsData serialized;
				var status = fsJsonParser.Parse(line, out serialized);
				if (status.Failed)
					throw new Exception(status.FailureReason);

				Assert.True(line != "null");
				Assert.True(serialized.IsDictionary);

				var data      = serialized.AsDictionary;
				var type      = Serializer.Deserialize<Type>(data["Type"]);
				var converter = GetConverter(type);

				if (type == typeof(GameObject))
				{
					string id = Serializer.Deserialize<string>(data["id"]);
					string parentId = Serializer.Deserialize<string>(data["parentId"]);
					lastRead = Cache.GetGO(id, parentId);
					converter.DeserializeDataIntoInstance(lastRead, serialized);
				}
				else
				{
					Assert.NotNull(lastRead);
					Assert.True(type.IsA<Component>());
					var component = lastRead.GetOrAddComponent(type);
					converter.DeserializeDataIntoInstance(component, serialized);
				}
			}
		}
#else
		using (var reader = new StreamReader(path))
		{
			Profiler.BeginSample("Loading");
			GameObject lastRead = null;
			while (true)
			{
				Profiler.BeginSample("Reading line");
				string line = reader.ReadLine();
				Profiler.EndSample();
				if (line == null)
					break;
				fsData serialized;
				Profiler.BeginSample("Parsing: " + line);
				var status = fsJsonParser.Parse(line, out serialized);
				Profiler.EndSample();
				if (status.Failed)
					throw new Exception(status.FailureReason);

				Assert.True(line != "null");
				Assert.True(serialized.IsDictionary);

				Profiler.BeginSample("Deserializing type");
				var data      = serialized.AsDictionary;
				var type      = Serializer.Deserialize<Type>(data["Type"]);
				var converter = GetConverter(type);
				Profiler.EndSample();

				if (type == typeof(GameObject))
				{
#if DBG
					Debug.Log("Loading GO: " + goName);
#endif
					string goName = Serializer.Deserialize<string>(data["name"]);
					Profiler.BeginSample("Loading GO: " + goName);
					string id = Serializer.Deserialize<string>(data["id"]);
					string parentId = Serializer.Deserialize<string>(data["parentId"]);
					lastRead = Cache.GetOrCreateGO(goName, id, parentId);
					converter.DeserializeDataIntoInstance(lastRead, serialized);
					Profiler.EndSample();
				}
				else
				{
#if DBG
					Debug.Log("Loading Comp: " + type.Name);
#endif
					Profiler.BeginSample("Loading Comp: " + type.Name);
					Assert.NotNull(lastRead);
					Assert.True(type.IsA<Component>());
					var component = lastRead.GetOrAddComponent(type);
					converter.DeserializeDataIntoInstance(component, serialized);
					Profiler.EndSample();
				}
			}

			Profiler.EndSample();
		}
#endif

		fsData.ResetPools();

		IsLoading = false;
		EventManager.Raise(new FinishedLoadingEvent());
	}
}

public struct SaveInfo
{
	public readonly string name;
	public readonly DateTime time;

	public SaveInfo(string name, DateTime time)
	{
		this.name = name;
		this.time = time;
	}
}

public static class GameObjectExtensions
{
	private static Func<GameObject, string> _getId;
	private static Func<GameObject, string> getId
	{
		get
		{
			return _getId ?? (_getId = new Func<GameObject, string>(go =>
			{
				return go.GetOrAddComponent<UniqueId>().Id;
			}).Memoize());
		}
	}

	public static string GetId(this GameObject go)
	{
		return getId(go);
	}

	public static void SetId(this GameObject go, string id)
	{
		go.GetOrAddComponent<UniqueId>().Id = id;
	}

	public static string GetParentId(this GameObject go)
	{
		var parent = go.transform.parent;
		if (parent == null) return string.Empty;
		return GetId(parent.gameObject);
	}

	public static bool IsPrefab(this GameObject go)
	{
		var prefab = go.GetComponent<PrefabMarker>();
		return prefab != null && !prefab.IsAlive;
	}
}

public static class Cache
{
	public static Dictionary<string, GameObject> GameObjects = new Dictionary<string, GameObject>();

	public static void UpdateGOsCache()
	{
		GameObjects.Clear();

		var gos = UnityObject.FindObjectsOfType<GameObject>();
		for (int i = 0; i < gos.Length; i++)
		{
			var go = gos[i];
			var id = go.GetId();
			GameObjects[id] = go;
		}
	}

	public static GameObject GetOrCreateGO(string name, string goId, string parentId)
	{
		Func<string, GameObject> fetch = id =>
		{
			GameObject result;
			if (!GameObjects.TryGetValue(id, out result))
			{
				result = new GameObject(name);
				result.SetId(id);
				GameObjects[id] = result;
			}
			return result;
		};

		var go = fetch(goId);

		if (!string.IsNullOrEmpty(parentId))
		{
			var parent = fetch(parentId);
			go.transform.parent = parent.transform;
		}

		return go;
	}
}

public struct BeganSavingEvent : IGameEvent
{
}

public struct FinishedSavingEvent : IGameEvent
{
}

public struct BeganLoadingEvent : IGameEvent
{
}

public struct FinishedLoadingEvent : IGameEvent
{
}

#if UNITY_EDITOR
public static class SaveMenus
{
	[UnityEditor.MenuItem("Tools/Vexe/SaveSystem/QuickSave &#s")]
	public static void Save()
	{
		SaveManager.QuickSave();
	}

	[UnityEditor.MenuItem("Tools/Vexe/SaveSystem/QuickLoad &#l")]
	public static void Load()
	{
		SaveManager.QuickLoad();
	}
}
#endif
