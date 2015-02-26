//#define DBG

using System;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Serialization;

namespace Vexe.Runtime.Types
{
	/// <summary>
	/// Inherit from this instead of MonoBehaviour to live in a better world!
	/// </summary>
	[DefineCategory("Fields", 0f, MemberType = MemberType.Field, Exclusive = false)]
	[DefineCategory("Properties", 1f, MemberType = MemberType.Property, Exclusive = false)]
	[DefineCategory("Methods", 2f, MemberType = MemberType.Method, Exclusive = false)]
	[DefineCategory("Debug", 3f, Pattern = "^dbg")]
	public abstract class BetterBehaviour : MonoBehaviour, ISerializable, IHasUniqueId, ISerializationCallbackReceiver
	{
		/// <summary>
		/// Use this to include members to the "Debug" categories
		/// Ex:
		/// [Category(Dbg)]
		/// public Color gizmosColor;
		/// </summary>
		protected const string Dbg = "Debug";

		/// <summary>
		/// Used for debugging/logging
		/// </summary>
		[DontSerialize] public bool dbg;

		// SerializationData, Serializer
		#region
		/// <summary>
		/// It's important to let *only* unity serialize our serialization data
		/// </summary>
		[SerializeField, DontSave]
		private SerializationData _serializationData;
		private SerializationData SerializationData
		{
			get { return _serializationData ?? (_serializationData = new SerializationData()); }
		}

		private BetterSerializer _serializer;
		public BetterSerializer Serializer
		{
			get { return _serializer ?? (_serializer = new BetterSerializer()); }
		}

		#endregion

		// IHasUniqueId implementation
		#region
		/// <summary>
		/// A unique identifier used primarly from editor scripts to have editor data persist
		/// I've had some runtime usages for it too
		/// </summary>
		[SerializeField, HideInInspector, DontSave]
		private string id;
		public string ID
		{
			get
			{
				if (id.IsNullOrEmpty())
					id = Guid.NewGuid().ToString();
				return id;
			}
		}
		#endregion

		// [g|s]etters
		#region
		private Transform cachedTransform;
		new public Transform transform
		{
			get
			{
				if (!cachedTransform)
					cachedTransform = base.transform;
				return cachedTransform;
			}
		}

		public RectTransform rectTransform
		{
			get { return transform as RectTransform; }
		}

		public Transform parent
		{
			get { return transform.parent; }
		}

		public int childCount
		{
			get { return transform.childCount; }
		}

		public Vector3 forward
		{
			get { return transform.forward; }
			set { transform.forward = value; }
		}

		public Vector3 right
		{
			get { return transform.right; }
			set { transform.right = value; }
		}

		public Vector3 left
		{
			get { return -right; }
			set { right = -value; }
		}

		public Vector3 up
		{
			get { return transform.up; }
			set { transform.up = value; }
		}

		public Vector3 back
		{
			get { return -up; }
			set { up = -value; }
		}

		public Vector3 position
		{
			get { return transform.position; }
			set { transform.position = value; }
		}

		public Vector3 localPosition
		{
			get { return transform.localPosition; }
			set { transform.localPosition = value; }
		}

		public Quaternion rotation
		{
			get { return transform.rotation; }
			set { transform.rotation = value; }
		}

		public Quaternion localRotation
		{
			get { return transform.localRotation; }
			set { transform.localRotation = value; }
		}

		public Vector3 eulerAngles
		{
			get { return transform.eulerAngles; }
			set { transform.eulerAngles = value; }
		}

		public Vector3 localEulerAngles
		{
			get { return transform.localEulerAngles; }
			set { transform.localEulerAngles = value; }
		}

		public Vector3 localScale
		{
			get { return transform.localScale; }
			set { transform.localScale = value; }
		}

		new public bool active
		{
			get { return gameObject.activeSelf; }
			set { gameObject.SetActive(value); }
		}
		#endregion

		// [De]serialization
		#region 
		public void OnBeforeSerialize()
		{
			Serialize();
		}

		public void OnAfterDeserialize()
		{
			Deserialize();
		}

		public void Serialize()
		{
#if DBG
			Log("Serializing " + GetType().Name);
#endif
			SerializationData.Clear();
			Serializer.SerializeTargetIntoData(this, SerializationData);
		}

		public void Deserialize()
		{
#if DBG
			Log("Deserializing " + GetType().Name);
#endif
			Serializer.DeseiralizeDataIntoTarget(this, SerializationData);
		}
		#endregion

		// Logging
		#region
		protected void dbgLog(string msg, params object[] args)
		{
			if (dbg) Log(msg, args);
		}

		protected void dbgLog(object obj)
		{
			if (dbg) Log(obj);
		}

		protected void Log(string msg, params object[] args)
		{
			if (args.IsNullOrEmpty()) args = new object[0];
			Debug.Log(string.Format(msg, args), gameObject); // passing gameObject as context will ping the gameObject that we logged from when we click the log entry in the console!
		}

		protected void Log(object obj)
		{
			Log(obj.ToString(), null);
		}

		// static logs are useful when logging in nested system.object classes
		protected static void sLog(string msg, params object[] args)
		{
			if (args.IsNullOrEmpty()) args = new object[0];
			Debug.Log(string.Format(msg, args));
		}

		protected static void sLog(object obj)
		{
			Debug.Log(obj);
		}

		#endregion

		public static void ResetTarget(object target)
		{
			var type = target.GetType();
#if DBG
			Log("Assigning default values if any to " + type.Name);
#endif
			var members = RuntimeMember.EnumerateMemoized(type);
			for (int i = 0; i < members.Count; i++)
			{
				var member = members[i];
				var defAttr = member.Info.GetCustomAttribute<DefaultAttribute>();
				if (defAttr != null)
				{ 
					member.Target = target;
					var value = defAttr.Value;
					if (value == null && !member.Type.IsAbstract)
						value = member.Type.ActivatorInstance();
					member.Set(value);
				}
			}
		}

		public void Reset()
		{
			ResetTarget(this);
		}
	}
}