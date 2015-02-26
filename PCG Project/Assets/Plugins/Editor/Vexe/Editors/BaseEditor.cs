//#define DBG

using System;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Helpers;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Editors
{
	using UnityEditor;
	using Vexe.Editor.GUIs;
	using Vexe.Runtime.Types;
	using Editor = UnityEditor.Editor;

	/// <summary>
	/// A better editor class that has a GLWrapper that uses the same API that GUIWrapper does to render GUI controls
	/// with a bunch of most-often used properties and methods for convenience
	/// </summary>
	public class BaseEditor<T> : Editor where T : UnityObject
	{
		public BaseGUI gui;

		public bool useUnityGUI
		{
			get { return prefs.Bools.ValueOrDefault("UnityGUI"); }
			set { prefs.Bools["UnityGUI"] = value; }
		}

		public T typedTarget { get { return target as T; } }

		protected GameObject gameObject
		{
			get
			{
				var comp = target as Component;
				return comp == null ? null : comp.gameObject;
			}
		}

		protected static BetterPrefs prefs { get { return BetterPrefs.EditorInstance; } }

		protected virtual string id { get { return RTHelper.GetTargetID(target); } }

		protected bool foldout
		{
			get { return foldouts[id]; }
			set { foldouts[id] = value; }
		}

		protected static Foldouts _foldouts;
		protected static Foldouts foldouts { get { return _foldouts ?? (_foldouts = new Foldouts(prefs)); } }

		private Type mTargetType;
		private bool prevGui;
		public Type targetType
		{
			get { return mTargetType ?? (mTargetType = target.GetType()); }
		}

		/// <summary>
		/// Performs a safe-cast to the editor's target to the specified generic argument and returns the result
		/// </summary>
		public TTarget GetCustomTypedTarget<TTarget>() where TTarget : T
		{
			return target as TTarget;
		}

		private void OnEnable()
		{
			prevGui = useUnityGUI;
			gui = prevGui ? (BaseGUI)new TurtleGUI() : new RabbitGUI();
			OnAwake();
#if DBG
			Log(GetType().Name + " for " + target.GetType().Name + " OnEnable");
#endif
		}

		/// <summary>
		/// Override this instead of OnEnable for your editor initialization
		/// </summary>
		protected virtual void OnAwake()
		{
		}

		/// <summary>
		/// Override this instead of OnInspectorGUI for your gui code
		/// </summary>
		protected virtual void OnGUI()
		{
		}

		public void OnInlinedGUI(BaseGUI otherGui, bool showHeader)
		{
			this.gui = otherGui;
			//this.showHeader = showHeader; TODO: remove base editor and just stick with better editor or something no need for 2 bases
			OnGUI();
		}

		public sealed override void OnInspectorGUI()
		{
			if (prevGui != useUnityGUI)
			{
				prevGui = useUnityGUI;
				gui = prevGui ? (BaseGUI)new TurtleGUI() : new RabbitGUI();
			}

			gui.OnGUI(OnGUI, new Vector2(0f, 35f));
		}

		protected static void Log(string msg, params object[] args)
		{
			Debug.Log(string.Format(msg, args));
		}

		protected static void Log(object msg)
		{
			Debug.Log(msg);
		}
	}

	public static class MenuItems
	{
		[MenuItem("Tools/Vexe/GUI/UseUnityGUI")]
		public static void UseUnityGUI()
		{
			BetterPrefs.EditorInstance.Bools["UnityGUI"] = true;
		}

		[MenuItem("Tools/Vexe/GUI/UseRabbitGUI")]
		public static void UseRabbitGUI()
		{
			BetterPrefs.EditorInstance.Bools["UnityGUI"] = false;
		}
	}
}