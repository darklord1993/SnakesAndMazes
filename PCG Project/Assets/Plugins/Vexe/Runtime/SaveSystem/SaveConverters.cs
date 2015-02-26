#define PROFILE
#define DBG

using System;
using System.Collections.Generic;
using FullSerializer;
using UnityEngine;
using Vexe.Runtime.Serialization;
using UnityObject = UnityEngine.Object;

/*
 * This file contains the converters used to save/load data from/into unity objects (GOs, Transforms, MonoBehaviours etc)
 * It is very easy to extend the system and add more converters as you need (2D components for ex)
 *
 * Note that it is better to write your converters somewhere in your project and not somewhere within the framework files
 * because high chances are that I haven't added what you added so if you update to a newer version,
 * the old files will get overwritten and you lose progress
 */

public abstract class SaveConverter
{
	public fsSerializer Serializer { get; set; }

	public abstract fsData SerializeInstanceIntoData(UnityObject instance);
	public abstract void DeserializeDataIntoInstance(UnityObject instance, fsData serialized);

	protected fsData Serialize<T>(T instance)
	{
		return Serializer.Serialize<T>(instance);
	}

	protected T Deserialize<T>(fsData data)
	{
		return Serializer.Deserialize<T>(data);
	}

	public fsData SerializeType(Type type)
	{
		var result   = fsData.CreateDictionary();
		var data     = result.AsDictionary;
		data["Type"] = Serializer.Serialize<Type>(type);
		return result;
	}
}

public abstract class SaveConverter<T> : SaveConverter where T : UnityObject
{
	public sealed override fsData SerializeInstanceIntoData(UnityObject instance)
	{
		var data = SerializeType(instance.GetType());
		SerializeInstanceIntoData(instance as T, data.AsDictionary);
		return data;
	}

	public sealed override void DeserializeDataIntoInstance(UnityObject instance, fsData serialized)
	{
		DeserializeDataIntoInstance(instance as T, serialized.AsDictionary);
	}

	public abstract void SerializeInstanceIntoData(T instance, Dictionary<string, fsData> data);
	public abstract void DeserializeDataIntoInstance(T instance, Dictionary<string, fsData> data);
}

public class GameObjectConverter : SaveConverter<GameObject>
{
	public override void SerializeInstanceIntoData(GameObject instance, Dictionary<string, fsData> data)
	{
		data["name"]     = Serialize<string> (instance.name);
		data["tag"]      = Serialize<string> (instance.tag);
		data["layer"]    = Serialize<int>    (instance.layer);
		data["isStatic"] = Serialize<bool>   (instance.isStatic);
		data["id"]       = Serialize<string> (instance.GetId());
		data["parentId"] = Serialize<string> (instance.GetParentId());
	}

	public override void DeserializeDataIntoInstance(GameObject instance, Dictionary<string, fsData> data)
	{
		instance.name     = Deserialize<string> (data["name"]);
		instance.tag      = Deserialize<string> (data["tag"]);
		instance.layer    = Deserialize<int>    (data["layer"]);
		instance.isStatic = Deserialize<bool>   (data["isStatic"]);
	}
}

public class TransformConverter : SaveConverter<Transform>
{
	public override void SerializeInstanceIntoData(Transform instance, Dictionary<string, fsData> data)
	{
#if !PROFILE
		data["position"] = Serialize<Vector3>(instance.localPosition);
		data["rotation"] = Serialize<Vector3>(instance.localEulerAngles);
		data["scale"]    = Serialize<Vector3>(instance.localScale);
#else
		Profiler.BeginSample("Serializing position");
		data["position"] = Serialize<Vector3>(instance.localPosition);
		Profiler.EndSample();
		Profiler.BeginSample("Serializing rotation");
		data["rotation"] = Serialize<Vector3>(instance.localEulerAngles);
		Profiler.EndSample();
		Profiler.BeginSample("Serializing scale");
		data["scale"] = Serialize<Vector3>(instance.localScale);
		Profiler.EndSample();
#endif
	}

	public override void DeserializeDataIntoInstance(Transform instance, Dictionary<string, fsData> data)
	{
		instance.localPosition    = Deserialize<Vector3>(data["position"]);
		instance.localEulerAngles = Deserialize<Vector3>(data["rotation"]);
		instance.localScale       = Deserialize<Vector3>(data["scale"]);
	}
}

public class MonoBehaviourConverter : SaveConverter<MonoBehaviour>
{
#if !PROFILE
	public override void SerializeInstanceIntoData(MonoBehaviour instance, Dictionary<string, fsData> data)
	{
		var members = SerializationLogic.Default.GetMemoizedSerializableMembers(instance.GetType());
		for(int i = 0; i < members.Count; i++)
		{
			var member = members[i];
			member.Target = instance;
			data[member.Name] = Serialize(member.Value);
		}
	}

	public override void DeserializeDataIntoInstance(MonoBehaviour instance, Dictionary<string, fsData> data)
	{
		var members = SerializationLogic.Default.GetMemoizedSerializableMembers(instance.GetType());
		for(int i = 0; i < members.Count; i++)
		{
			var member = members[i];
			member.Target = instance;
			member.Value = Deserialize<object>(data[member.Name]);
		}
	}
#else
	public override void SerializeInstanceIntoData(MonoBehaviour instance, Dictionary<string, fsData> data)
	{
		Profiler.BeginSample("Saving members for: " + instance.GetType().Name);
		var members = SerializationLogic.Default.GetMemoizedSerializableMembers(instance.GetType());
		for(int i = 0; i < members.Count; i++)
		{
			var member = members[i];
			member.Target = instance;
			Profiler.BeginSample("Saving member: " + member.Name);
			data[member.Name] = Serialize(member.Value);
			Profiler.EndSample();
		}
		Profiler.EndSample();
	}

	public override void DeserializeDataIntoInstance(MonoBehaviour instance, Dictionary<string, fsData> data)
	{
		Profiler.BeginSample("Loading members for: " + instance.GetType().Name);
		var members = SerializationLogic.Default.GetMemoizedSerializableMembers(instance.GetType());
		for(int i = 0; i < members.Count; i++)
		{
			var member = members[i];
			member.Target = instance;
			Profiler.BeginSample("Loading member: " + member.Name);
			member.Value = Deserialize<object>(data[member.Name]);
			Profiler.EndSample();
		}
		Profiler.EndSample();
	}
#endif
}

public class RigidbodyConverter : SaveConverter<Rigidbody>
{
	public override void SerializeInstanceIntoData(Rigidbody instance, Dictionary<string, fsData> data)
	{
		data["mass"]          = Serialize(instance.mass);
		data["drag"]          = Serialize(instance.drag);
		data["isKinematic"]   = Serialize(instance.isKinematic);
		data["useGravity"]    = Serialize(instance.useGravity);
		data["angularDrag"]   = Serialize(instance.angularDrag);
		data["constraints"]   = Serialize((int)instance.constraints);
		data["interpolation"] = Serialize(instance.interpolation);
		data["collision"]     = Serialize(instance.collisionDetectionMode);
	}

	public override void DeserializeDataIntoInstance(Rigidbody instance, Dictionary<string, fsData> data)
	{
		instance.mass                   = Deserialize<float>(data["mass"]);
		instance.drag                   = Deserialize<float>(data["drag"]);
		instance.angularDrag            = Deserialize<float>(data["angularDrag"]);
		instance.isKinematic            = Deserialize<bool>(data["isKinematic"]);
		instance.useGravity             = Deserialize<bool>(data["useGravity"]);
		instance.constraints            = (RigidbodyConstraints)Deserialize<int>(data["constraints"]);
		instance.interpolation          = Deserialize<RigidbodyInterpolation>(data["interpolation"]);
		instance.collisionDetectionMode = Deserialize<CollisionDetectionMode>(data["collision"]);
	}
}

public class MeshFilterConverter : SaveConverter<MeshFilter>
{
	public override void SerializeInstanceIntoData(MeshFilter instance, Dictionary<string, fsData> data)
	{
		data["mesh"] = Serialize(instance.sharedMesh);
	}

	public override void DeserializeDataIntoInstance(MeshFilter instance, Dictionary<string, fsData> data)
	{
		instance.sharedMesh = Deserialize<Mesh>(data["mesh"]);
	}
}

public class RendererConverter<T> : SaveConverter<T> where T : Renderer
{
	public override void SerializeInstanceIntoData(T instance, Dictionary<string, fsData> data)
	{
		data["receiveShadows"] = Serialize(instance.receiveShadows);
		data["castShadows"]    = Serialize(instance.castShadows);
		data["useLightProbes"] = Serialize(instance.useLightProbes);
		data["materials"]      = Serialize(instance.sharedMaterials);
	}

	public override void DeserializeDataIntoInstance(T instance, Dictionary<string, fsData> data)
	{
		instance.receiveShadows  = Deserialize<bool>(data["receiveShadows"]);
		instance.castShadows     = Deserialize<bool>(data["castShadows"]);
		instance.useLightProbes  = Deserialize<bool>(data["useLightProbes"]);
		instance.sharedMaterials = Deserialize<Material[]>(data["materials"]);
	}
}

public class SkinnedMeshRendererConverter : RendererConverter<SkinnedMeshRenderer>
{
	public override void SerializeInstanceIntoData(SkinnedMeshRenderer instance, Dictionary<string, fsData> data)
	{
		base.SerializeInstanceIntoData(instance, data);
		data["quality"]  = Serialize(instance.quality);
		data["rootBone"] = Serialize(instance.rootBone);
	}

	public override void DeserializeDataIntoInstance(SkinnedMeshRenderer instance, Dictionary<string, fsData> data)
	{
		base.DeserializeDataIntoInstance(instance, data);
		instance.quality  = Deserialize<SkinQuality>(data["quality"]);
		instance.rootBone = Deserialize<Transform>(data["rootBone"]);
	}
}

public class TrailRendererConverter : RendererConverter<TrailRenderer>
{
	public override void SerializeInstanceIntoData(TrailRenderer instance, Dictionary<string, fsData> data)
	{
		base.SerializeInstanceIntoData(instance, data);
		data["time"]         = Serialize(instance.time);
		data["startWidth"]   = Serialize(instance.startWidth);
		data["endWidth"]     = Serialize(instance.endWidth);
		data["autodestruct"] = Serialize(instance.autodestruct);
	}

	public override void DeserializeDataIntoInstance(TrailRenderer instance, Dictionary<string, fsData> data)
	{
		base.DeserializeDataIntoInstance(instance, data);
		instance.time         = Deserialize<float>(data["time"]);
		instance.startWidth   = Deserialize<float>(data["startWidth"]);
		instance.endWidth     = Deserialize<float>(data["endWidth"]);
		instance.autodestruct = Deserialize<bool>(data["autodestruct"]);
	}
}

public class ParticleRendererConverter : RendererConverter<ParticleRenderer>
{
	public override void SerializeInstanceIntoData(ParticleRenderer instance, Dictionary<string, fsData> data)
	{
		data["uvTiles"]             = Serialize(instance.uvTiles);
		data["uvAnimationXTile"]    = Serialize(instance.uvAnimationXTile);
		data["uvAnimationYTile"]    = Serialize(instance.uvAnimationYTile);
		data["uvAnimationCycles"]   = Serialize(instance.uvAnimationCycles);
		data["velocityScale"]       = Serialize(instance.velocityScale);
		data["lengthScale"]         = Serialize(instance.lengthScale);
		data["cameraVelocityScale"] = Serialize(instance.cameraVelocityScale);
		data["maxParticleSize"]     = Serialize(instance.maxParticleSize);
	}

	public override void DeserializeDataIntoInstance(ParticleRenderer instance, Dictionary<string, fsData> data)
	{
		instance.uvTiles             = Deserialize<Rect[]>(data["uvTiles"]);
		instance.uvAnimationXTile    = Deserialize<int>(data["uvAnimationXTile"]);
		instance.uvAnimationYTile    = Deserialize<int>(data["uvAnimationYTile"]);
		instance.uvAnimationCycles   = Deserialize<float>(data["uvAnimationCycles"]);
		instance.velocityScale       = Deserialize<float>(data["velocityScale"]);
		instance.lengthScale         = Deserialize<float>(data["lengthScale"]);
		instance.cameraVelocityScale = Deserialize<float>(data["cameraVelocityScale"]);
		instance.maxParticleSize     = Deserialize<float>(data["maxParticleSize"]);
	}
}

public class ParticleAnimatorConverter : SaveConverter<ParticleAnimator>
{
	public override void SerializeInstanceIntoData(ParticleAnimator instance, Dictionary<string, fsData> data)
	{
		data["doesAnimateColor"]  = Serialize(instance.doesAnimateColor);
		data["colorAnimation"]    = Serialize(instance.colorAnimation);
		data["worldRotationAxis"] = Serialize(instance.worldRotationAxis);
		data["localRotationAxis"] = Serialize(instance.localRotationAxis);
		data["rndForce"]          = Serialize(instance.rndForce);
		data["force"]             = Serialize(instance.force);
		data["sizeGrow"]          = Serialize(instance.sizeGrow);
		data["damping"]           = Serialize(instance.damping);
		data["autodestruct"]      = Serialize(instance.autodestruct);
	}

	public override void DeserializeDataIntoInstance(ParticleAnimator instance, Dictionary<string, fsData> data)
	{
		instance.doesAnimateColor  = Deserialize<bool>(data["doesAnimateColor"]);
		instance.colorAnimation    = Deserialize<Color[]>(data["colorAnimation"]);
		instance.worldRotationAxis = Deserialize<Vector3>(data["worldRotationAxis"]);
		instance.localRotationAxis = Deserialize<Vector3>(data["localRotationAxis"]);
		instance.rndForce          = Deserialize<Vector3>(data["rndForce"]);
		instance.force             = Deserialize<Vector3>(data["force"]);
		instance.sizeGrow          = Deserialize<float>(data["sizeGrow"]);
		instance.damping           = Deserialize<float>(data["damping"]);
		instance.autodestruct      = Deserialize<bool>(data["autodestruct"]);
	}
}

public class LightConverter : SaveConverter<Light>
{
	public override void SerializeInstanceIntoData(Light instance, Dictionary<string, fsData> data)
	{
		var lightType = instance.type;
		data["lightType"] = Serialize(lightType);

		switch(lightType)
		{
			case LightType.Point:
				data["range"] = Serialize(instance.range);
				break;
			case LightType.Directional:
				data["cookieSize"]     = Serialize(instance.cookieSize);
				data["shadowStrength"] = Serialize(instance.shadowStrength);
				data["shadowBias"]     = Serialize(instance.shadowBias);
				if(instance.shadows == LightShadows.Soft)
				{
					data["shadowSoftness"]     = Serialize(instance.shadowSoftness);
					data["shadowSoftnessFade"] = Serialize(instance.shadowSoftnessFade);
				}
				break;
			case LightType.Spot:
				data["spotAngle"] = Serialize(instance.spotAngle);
				data["range"]     = Serialize(instance.range);
				break;
		}

		data["color"]     = Serialize(instance.color);
		data["intensity"] = Serialize(instance.intensity);

		if (lightType != LightType.Area)
		{
			data["cookie"]      = Serialize(instance.cookie);
			data["flare"]       = Serialize(instance.flare);
			data["cullingMask"] = Serialize(instance.cullingMask);
			data["renderMode"]  = Serialize(instance.renderMode);
		}
	}

	public override void DeserializeDataIntoInstance(Light instance, Dictionary<string, fsData> data)
	{
		var lightType = Deserialize<LightType>(data["lightType"]);

		switch(lightType)
		{
			case LightType.Point:
				instance.range = Deserialize<float>(data["range"]);
				break;
			case LightType.Directional:
				instance.cookieSize     = Deserialize<float>(data["cookieSize"]);
				instance.shadowStrength = Deserialize<float>(data["shadowStrength"]);
				instance.shadowBias     = Deserialize<float>(data["shadowBias"]);
				if(instance.shadows == LightShadows.Soft)
				{
					instance.shadowSoftness     = Deserialize<float>(data["shadowSoftness"]);
					instance.shadowSoftnessFade = Deserialize<float>(data["shadowSoftnessFade"]);
				}
				break;
			case LightType.Spot:
				instance.spotAngle = Deserialize<float>(data["spotAngle"]);
				instance.range     = Deserialize<float>(data["range"]);
				break;
		}

		instance.color     = Deserialize<Color>(data["color"]);
		instance.intensity = Deserialize<float>(data["intensity"]);

		if (lightType != LightType.Area)
		{
			instance.cookie      = Deserialize<Texture>(data["cookie"]);
			instance.flare       = Deserialize<Flare>(data["flare"]);
			instance.cullingMask = Deserialize<int>(data["cullingMask"]);
			instance.renderMode  = Deserialize<LightRenderMode>(data["renderMode"]);
		}
	}
}

public class AnimationConverter : SaveConverter<Animation>
{
	public override void SerializeInstanceIntoData(Animation instance, Dictionary<string, fsData> data)
	{
		data["playAutomatically"] = Serialize(instance.playAutomatically);
		data["wrapMode"]          = Serialize(instance.wrapMode);
		data["clips"]             = fsData.CreateList();

		var clips = data["clips"].AsList;

		foreach(AnimationState anim in instance)
		{
			clips.Add(Serialize(anim.clip));
		}
	}

	public override void DeserializeDataIntoInstance(Animation instance, Dictionary<string, fsData> data)
	{
		instance.playAutomatically = Deserialize<bool>(data["playAutomatically"]);
		instance.wrapMode          = Deserialize<WrapMode>(data["wrapMode"]);

		var clips = Deserialize<List<AnimationClip>>(data["clips"]);

		for (int i = 0; i < clips.Count; i++)
		{
			var clip = clips[i];
			if (instance.GetClip(clip.name) == null)
				instance.AddClip(clip, clip.name);
		}
	}
}

public class CameraConverter : SaveConverter<Camera>
{
	public override void SerializeInstanceIntoData(Camera instance, Dictionary<string, fsData> data)
	{
		bool ortho = instance.isOrthoGraphic;
		data["fovOrSize"] = Serialize(ortho ? instance.fieldOfView : instance.orthographicSize);
		data["ortho"]     = Serialize(ortho);
		data["nearClip"]  = Serialize(instance.nearClipPlane);
		data["farClip"]   = Serialize(instance.farClipPlane);
		data["bgColor"]   = Serialize(instance.backgroundColor);
		data["flags"]     = Serialize(instance.clearFlags);
		data["rect"]      = Serialize(instance.rect);
		data["depth"]     = Serialize(instance.depth);
		data["path"]      = Serialize(instance.renderingPath);
		data["hdr"]       = Serialize(instance.hdr);
		data["texture"]   = Serialize(instance.targetTexture);
	}

	public override void DeserializeDataIntoInstance(Camera instance, Dictionary<string, fsData> data)
	{
		bool ortho = Deserialize<bool>(data["ortho"]);
		float fovOrSize = Deserialize<float>(data["fovOrSize"]);
		instance.isOrthoGraphic = ortho;
		if (ortho)
			instance.orthographicSize = fovOrSize;
		else instance.fieldOfView = fovOrSize;

		instance.nearClipPlane   = Deserialize<float>(data["nearClip"]);
		instance.farClipPlane    = Deserialize<float>(data["farClip"]);
		instance.backgroundColor = Deserialize<Color>(data["bgColor"]);
		instance.clearFlags      = Deserialize<CameraClearFlags>(data["flags"]);
		instance.rect            = Deserialize<Rect>(data["rect"]);
		instance.depth           = Deserialize<float>(data["depth"]);
		instance.renderingPath   = Deserialize<RenderingPath>(data["path"]);
		instance.hdr             = Deserialize<bool>(data["hdr"]);
		instance.targetTexture   = Deserialize<RenderTexture>(data["texture"]);
	}
}

public class CharacterControllerConverter : SaveConverter<CharacterController>
{
	public override void SerializeInstanceIntoData(CharacterController instance, Dictionary<string, fsData> data)
	{
		data["slopeLimit"] = Serialize(instance.slopeLimit);
		data["stepOffset"] = Serialize(instance.stepOffset);
		data["radius"]     = Serialize(instance.radius);
		data["height"]     = Serialize(instance.height);
		data["center"]     = Serialize(instance.center);
	}

	public override void DeserializeDataIntoInstance(CharacterController instance, Dictionary<string, fsData> data)
	{
		instance.slopeLimit = Deserialize<float>(data["slopeLimit"]);
		instance.stepOffset = Deserialize<float>(data["stepOffset"]);
		instance.radius     = Deserialize<float>(data["radius"]);
		instance.height     = Deserialize<float>(data["height"]);
		instance.center     = Deserialize<Vector3>(data["center"]);
	}
}

public class AudioSourceConverter : SaveConverter<AudioSource>
{
	public override void SerializeInstanceIntoData(AudioSource instance, Dictionary<string, fsData> data)
	{
		data["clip"]        = Serialize(instance.clip);
		data["volume"]      = Serialize(instance.volume);
		data["pitch"]       = Serialize(instance.pitch);
		data["time"]        = Serialize(instance.time);
		data["mute"]        = Serialize(instance.mute);
		data["playOnAwake"] = Serialize(instance.playOnAwake);
		data["loop"]        = Serialize(instance.loop);
	}

	public override void DeserializeDataIntoInstance(AudioSource instance, Dictionary<string, fsData> data)
	{
		instance.clip        = Deserialize<AudioClip>(data["clip"]);
		instance.volume      = Deserialize<float>(data["volume"]);
		instance.pitch 		 = Deserialize<float>(data["pitch"]);
		instance.time        = Deserialize<float>(data["time"]);
		instance.mute        = Deserialize<bool>(data["mute"]);
		instance.playOnAwake = Deserialize<bool>(data["playOnAwake"]);
		instance.loop        = Deserialize<bool>(data["loop"]);
	}
}

public class AnimatorConverter : SaveConverter<Animator>
{
	public override void SerializeInstanceIntoData(Animator instance, Dictionary<string, fsData> data)
	{
		data["controller"] = Serialize(instance.runtimeAnimatorController);

		//var receiver = src.gameObject.GetInterface<IAnimatorSerializationReceiver>();
		//if (receiver == null)
		//	throw new InvalidOperationException("Can't save animator without knowing what variables to save. Please let there be an implementer of IAnimatorSerializationReceiver on the gameObject this animator is attached to");

		//receiver.SaveAnimatorVariables(ref variables);

		//for (int i = 0, layerCount = src.layerCount; i < layerCount; i++)
		//{
		//	var currentInfo = src.GetCurrentAnimatorStateInfo(i);
		//	states[currentInfo.tagHash] = currentInfo.normalizedTime;
		//}
	}

	public override void DeserializeDataIntoInstance(Animator instance, Dictionary<string, fsData> data)
	{
		instance.runtimeAnimatorController = Deserialize<RuntimeAnimatorController>(data["runtimeAnimatorController"]);

		//var receiver = dest.gameObject.GetInterface<IAnimatorSerializationReceiver>();
		//if (receiver == null)
		//	throw new OperationCanceledException("Can't load animator. No animator serialization receiver found");

		//(receiver as MonoBehaviour).StartCoroutine(receiver.LoadAnimatorVariables(variables.Values.ToArray()));

		//for (int i = 0, layerCount = dest.layerCount; i < layerCount; i++)
		//{
		//	var tagHash = states.Keys[i];
		//	var normalizedTime = states.Values[i];
		//	dest.Play("Move", i, normalizedTime);
		//	dest.SetInteger("Speed", 1);
		//}

		//for (int i = 0; i < variables.Count; i++)
		//{
		//	var name = variables.Keys[i];
		//	var value = variables.Values[i];
		//	if (value.GetType() == typeof(int))
		//	{
		//		dest.SetInteger(name, (int)value);
		//	}
		//	else if (value.GetType() == typeof(float))
		//	{
		//		dest.SetFloat(name, (float)value);
		//	}
		//	else if (value.GetType() == typeof(bool))
		//	{
		//		dest.SetBool(name, (bool)value);
		//	}
		//}

		//public interface IAnimatorSerializationReceiver
		//{
		//	void SaveAnimatorVariables(ref KVPList<string, object> variables);
		//	IEnumerator LoadAnimatorVariables(object[] variables);
		//}
	}
}

public class BoxColliderConverter : SaveConverter<BoxCollider>
{
	public override void SerializeInstanceIntoData(BoxCollider instance, Dictionary<string, fsData> data)
	{
		data["isTrigger"] = Serialize<bool>(instance.isTrigger);
		data["center"]    = Serialize<Vector3>(instance.center);
		data["size"]      = Serialize<Vector3>(instance.size);
		data["material"]  = Serialize<PhysicMaterial>(instance.sharedMaterial);
	}

	public override void DeserializeDataIntoInstance(BoxCollider instance, Dictionary<string, fsData> data)
	{
		instance.isTrigger      = Deserialize<bool>(data["isTrigger"]);
		instance.center         = Deserialize<Vector3>(data["center"]);
		instance.size           = Deserialize<Vector3>(data["size"]);
		instance.sharedMaterial = Deserialize<PhysicMaterial>(data["material"]);
	}
}

public class CapsuleColliderConverter : SaveConverter<CapsuleCollider>
{
	public override void SerializeInstanceIntoData(CapsuleCollider instance, Dictionary<string, fsData> data)
	{
		data["isTrigger"] = Serialize(instance.isTrigger);
		data["center"]    = Serialize(instance.center);
		data["radius"]    = Serialize(instance.radius);
		data["height"]    = Serialize(instance.height);
		data["direction"] = Serialize(instance.direction);
		data["material"]  = Serialize(instance.sharedMaterial);
	}

	public override void DeserializeDataIntoInstance(CapsuleCollider instance, Dictionary<string, fsData> data)
	{
		instance.isTrigger      = Deserialize<bool>(data["isTrigger"]);
		instance.center         = Deserialize<Vector3>(data["center"]);
		instance.radius         = Deserialize<float>(data["radius"]);
		instance.height         = Deserialize<float>(data["height"]);
		instance.direction      = Deserialize<int>(data["direction"]);
		instance.sharedMaterial = Deserialize<PhysicMaterial>(data["material"]);
	}
}

public class MeshColliderConverter : SaveConverter<MeshCollider>
{
	public override void SerializeInstanceIntoData(MeshCollider instance, Dictionary<string, fsData> data)
	{
		data["isTrigger"]              = Serialize(instance.isTrigger);
		data["convex"]                 = Serialize(instance.convex);
		data["smoothSphereCollisions"] = Serialize(instance.smoothSphereCollisions);
		data["material"]               = Serialize(instance.sharedMaterial);
		data["mesh"]                   = Serialize(instance.sharedMesh);
	}

	public override void DeserializeDataIntoInstance(MeshCollider instance, Dictionary<string, fsData> data)
	{
		instance.isTrigger              = Deserialize<bool>(data["isTrigger"]);
		instance.convex                 = Deserialize<bool>(data["convex"]);
		instance.smoothSphereCollisions = Deserialize<bool>(data["smoothSphereCollisions"]);
		instance.sharedMaterial         = Deserialize<PhysicMaterial>(data["material"]);
		instance.sharedMesh             = Deserialize<Mesh>(data["mesh"]);
	}
}

public class SphereColliderConverter : SaveConverter<SphereCollider>
{
	public override void SerializeInstanceIntoData(SphereCollider instance, Dictionary<string, fsData> data)
	{
		data["isTrigger"] = Serialize(instance.isTrigger);
		data["radius"]    = Serialize(instance.radius);
		data["center"]    = Serialize(instance.center);
		data["material"]  = Serialize(instance.sharedMaterial);
	}

	public override void DeserializeDataIntoInstance(SphereCollider instance, Dictionary<string, fsData> data)
	{
		instance.isTrigger      = Deserialize<bool>(data["isTrigger"]);
		instance.radius         = Deserialize<float>(data["radius"]);
		instance.center         = Deserialize<Vector3>(data["center"]);
		instance.sharedMaterial = Deserialize<PhysicMaterial>(data["material"]);
	}
}
