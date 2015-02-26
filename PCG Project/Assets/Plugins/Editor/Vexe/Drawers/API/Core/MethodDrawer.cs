//#define DBG

using System;
using System.Reflection;
using Fasterflect;
using Vexe.Editor.GUIs;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Drawers
{
	public class MethodDrawer
	{
		private ArgMember[] argMembers;
		private object[] argValues;
		private string[] argKeys;
		private MethodInvoker invoke;
		private bool initialized;
		private string niceName;

		private object rawTarget;
		private string id;
		private BaseGUI gui;

		private BetterPrefs prefs { get { return BetterPrefs.EditorInstance; } }

		private bool foldout
		{
			get { return prefs.Bools.ValueOrDefault(id); }
			set { prefs.Bools[id] = value; }
		}

		public void Initialize(MethodInfo method, object rawTarget, UnityObject unityTarget, string id, BaseGUI gui)
		{
			this.gui = gui;

			if (initialized) return;
			initialized = true;

			this.id        = id;
			this.rawTarget = rawTarget;

			niceName = method.GetNiceName();

			invoke	     = method.DelegateForCallMethod();
			var argInfos = method.GetParameters();
			int len      = argInfos.Length;
			argValues    = new object[len];
			argKeys      = new string[len];
			argMembers   = new ArgMember[len];

			for (int iLoop = 0; iLoop < len; iLoop++)
			{
				int i = iLoop;
				var arg = argInfos[i];
				var argType = arg.ParameterType;
				argKeys[i] = id + argType.FullName + arg.Name;

				argValues[i] = prefs.TryGet(argInfos[i].ParameterType, argKeys[i]);

				argMembers[i] = new ArgMember(
						@getter        : () => argValues[i],
						@setter        : x => argValues[i] = x,
						@target		   : argValues,
						@unityTarget   : unityTarget,
						@attributes    : new Attribute[0],
						@name          : arg.Name,
						@id            : argKeys[i],
						@dataType      : argType
					);
			}

#if DBG
			Log("Method drawer init");
#endif
		}

		public bool OnGUI()
		{
			bool changed = false;
			if (Header() && argMembers.Length > 0)
			{
				using (gui.Indent())
				{
					for (int i = 0; i < argMembers.Length; i++)
					{
						bool argChange = gui.Member(argMembers[i], false);
						changed |= argChange;
						if (argChange)
						{
							prefs.TryAdd(argValues[i], argKeys[i]);
						}
					}
				}
			}
			return changed;
		}

		private bool Header()
		{
			using (gui.Horizontal())
			{
				if (gui.Button(niceName, styles.Mini))
					invoke(rawTarget, argValues);

				gui.Space(12f);
				if (argMembers.Length > 0)
				{ 
					Foldout();
					gui.Space(-11.5f);
				}
			}
			return foldout;
		}

		private void Foldout()
		{
			foldout = gui.Foldout(foldout);
		}
	}
}
