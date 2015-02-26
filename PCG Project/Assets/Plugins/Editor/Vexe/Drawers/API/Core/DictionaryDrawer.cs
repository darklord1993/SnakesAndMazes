//#define PROFILE
//#define DBG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Fasterflect;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Drawers
{
	public class DictionaryDrawer<TKey, TValue> : ObjectDrawer<Dictionary<TKey, TValue>>
	{
		private List<ElementMember<TKey>> keyElements;
		private List<ElementMember<TValue>> valueElements;
		private EditorMember addKeyInfo, addValueInfo;
		private AddInfo addInfo;
		private KVPList<TKey, TValue> kvpList;
		private string dictionaryName;
		private string pairFormatPattern;
		private MethodInvoker pairFormatMethod;
		private bool perKeyDrawing, perValueDrawing, ignoreAddArea;
		private bool shouldRead = true, shouldWrite;

		public bool Readonly { get; set; }

		protected override void OnSingleInitialization()
		{
			keyElements   = new List<ElementMember<TKey>>();
			valueElements = new List<ElementMember<TValue>>();
			addInfo       = new AddInfo();
			addKeyInfo    = new EditorMember(typeof(AddInfo).Field("key"), addInfo, unityTarget, id);
			addValueInfo  = new EditorMember(typeof(AddInfo).Field("value"), addInfo, unityTarget, id);

			perKeyDrawing   = attributes.AnyIs<PerKeyAttribute>();
			perValueDrawing = attributes.AnyIs<PerValueAttribute>();
			ignoreAddArea   = attributes.AnyIs<IgnoreAddAreaAttribute>();
			Readonly		= attributes.AnyIs<ReadonlyAttribute>();

			var formatMember	= attributes.OfType<FormatMemberAttribute>().FirstOrDefault();
			if (formatMember == null || string.IsNullOrEmpty(formatMember.pattern))
			{ 
				dictionaryName  = niceName;
				dictionaryName += " (" + memberType.GetNiceName() + ")";
			}
			else
			{
				dictionaryName = formatMember.Format(niceName, memberType.GetNiceName());
			}

			var pairFormat = attributes.GetAttribute<FormatPairAttribute>();
			if (pairFormat != null)
			{
				if (!string.IsNullOrEmpty(pairFormat.Method))
					pairFormatMethod = rawTarget.GetType().DelegateForCallMethod(pairFormat.Method, Flags.InstanceAnyVisibility, typeof(TKey), typeof(TValue));
				else if (!string.IsNullOrEmpty(pairFormat.Pattern))
					pairFormatPattern = pairFormat.Pattern;
			}

			if (Readonly)
				dictionaryName += " (Readonly)";

			#if DBG
			Log("Dictionary drawer Initialized (" + dictionaryName + ")");
			#endif
		}

		public class AddInfo
		{
			public TKey key;
			public TValue value;
		}

		public override void OnGUI()
		{
			if (!(foldout = gui.Foldout(dictionaryName, foldout, Layout.sExpandWidth())))
				return;

			if (memberValue == null)
			{ 
				#if DBG
				Log("Dictionary null " + dictionaryName);
				#endif
				memberValue = new Dictionary<TKey, TValue>();
			}

			shouldRead |= (kvpList == null || memberValue.Count != kvpList.Count);

			if (shouldRead)
			{
				#if DBG
				Log("Reading " + dictionaryName);
				#endif
				kvpList = memberValue.ToKVPList();
				shouldRead = false;
			}

			if (!Readonly)
			{
				#if PROFILE
				Profiler.BeginSample("DictionaryDrawer Header");
				#endif
				using (gui.Indent())
				{
					var pStr   = FormatPair(addInfo.key, addInfo.value);
					var addKey = id + "add";

					using (gui.Horizontal())
					{
						foldouts[addKey] = gui.Foldout("Add pair:", foldouts[addKey], Layout.sWidth(65f));

						gui.TextLabel(pStr);

						using (gui.State(kvpList.Count > 0))
						{
							if (gui.ClearButton("entries"))
							{
								kvpList.Clear();
								shouldWrite = true;
							}

							if (gui.RemoveButton("Last dictionary pair"))
							{
								kvpList.RemoveLast();
								shouldWrite = true;
							}
						}

						if (gui.AddButton("pair", MiniButtonStyle.ModRight))
						{
							AddPair(addInfo.key, addInfo.value);
							shouldWrite = true;
						}
					}

					if (foldouts[addKey])
					{
						#if PROFILE
						Profiler.BeginSample("DictionaryDrawer AddingPair");
						#endif
						using (gui.Indent())
						{
							gui.Member(addKeyInfo, attributes, ignoreAddArea || !perKeyDrawing);
							gui.Member(addValueInfo, attributes, ignoreAddArea || !perValueDrawing);
						}
						#if PROFILE
						Profiler.EndSample();
						#endif
					}
				}
				#if PROFILE
				Profiler.EndSample();
				#endif
			}

			if (kvpList.Count == 0)
			{
				gui.HelpBox("Dictionary is empty");
			}
			else
			{ 
				#if PROFILE
				Profiler.BeginSample("DictionaryDrawer Pairs");
				#endif
				using (gui.Indent())
				{
					for (int i = 0; i < kvpList.Count; i++)
					{
						var dKey   = kvpList.Keys[i];
						var dValue = kvpList.Values[i];

						TValue val;
						if (memberValue.TryGetValue(dKey, out val))
							shouldRead |= !dValue.GenericEqual(val);

						#if PROFILE
						Profiler.BeginSample("DictionaryDrawer KVP assignments");
						#endif

						var pairStr        = FormatPair(dKey, dValue);
						var entryKey       = id + i + "entry";
						foldouts[entryKey] = gui.Foldout(pairStr, foldouts[entryKey], Layout.sExpandWidth());

						#if PROFILE
						Profiler.EndSample();
						#endif

						if (!foldouts[entryKey])
							continue;

						#if PROFILE
						Profiler.BeginSample("DictionaryDrawer SinglePair");
						#endif
						using (gui.Indent())
						{
							var keyMember = GetElement(keyElements, kvpList.Keys, i, entryKey + "key");
							shouldWrite |= gui.Member(keyMember, !perKeyDrawing);

							var valueMember = GetElement(valueElements, kvpList.Values, i, entryKey + "value");
							shouldWrite |= gui.Member(valueMember, !perValueDrawing);
						}
						#if PROFILE
						Profiler.EndSample();
						#endif
					}
				}
				#if PROFILE
				Profiler.EndSample();
				#endif

				shouldWrite |= memberValue.Count > kvpList.Count;
			}

			if (shouldWrite)
			{
				#if DBG
				Log("Writing " + dictionaryName);
				#endif
				memberValue = kvpList.ToDictionary();
				shouldWrite = false;
			}
		}

		private ElementMember<T> GetElement<T>(List<ElementMember<T>> elements, List<T> source, int index, string id)
		{
			if (index >= elements.Count)
			{
				var element = new ElementMember<T>(
					@id          : id + index,
					@attributes  : attributes,
					@name        : string.Empty
				);
				elements.Add(element);
			}

			var e = elements[index];
			e.Initialize(source, index, rawTarget, unityTarget);
			return e;
		}

		private string FormatPair(TKey key, TValue value)
		{
			return formatPair(new KeyValuePair<TKey, TValue>(key, value));
		}

		private Func<KeyValuePair<TKey, TValue>, string> _formatPair;
		private Func<KeyValuePair<TKey, TValue>, string> formatPair
		{
			get
			{
				return _formatPair ?? (_formatPair = new Func<KeyValuePair<TKey, TValue>, string>(pair =>
				{
					var key = pair.Key;
					var value = pair.Value;

					if (pairFormatPattern == null)
					{ 
						if (pairFormatMethod == null)
							return string.Format("[{0}, {1}]", GetObjectString(key), GetObjectString(value));
						return pairFormatMethod(rawTarget, pair.Key, pair.Value) as string;
					}

					var result = pairFormatPattern;
					result = Regex.Replace(result, @"\$keyType", key == null ? "null" : key.GetType().GetNiceName());
					result = Regex.Replace(result, @"\$valueType", value == null ? "null" : value.GetType().GetNiceName());
					result = Regex.Replace(result, @"\$key", GetObjectString(key));
					result = Regex.Replace(result, @"\$value", GetObjectString(value));
					return result;
				}).Memoize());
			}
		}


		private string GetObjectString(object from)
		{
			if (from.IsObjectNull())
				return "null";
			var obj = from as UnityObject;
			return (obj != null) ? (obj.name + " (" + obj.GetType().Name + ")") : from.ToString();
		}

		private void AddPair(TKey key, TValue value)
		{
			try
			{
				if (typeof(TKey) == typeof(string))
				{
					var str = key as string;
					if (str == null)
						key = (TKey)(object)string.Empty;
				}
				kvpList.Add(key, value);
			}
			catch (ArgumentException e)
			{
				Log(e.Message);
			}
		}
	}
}
