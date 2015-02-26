//#define DBG

using System;
using System.Linq;
using System.Reflection;
using Fasterflect;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Drawers
{
	public class DelegateDrawer : ObjectDrawer<BaseDelegate>
	{
		private AddingData adding;
		private EditorMember[] argMembers;
		private object[] argValues;
		private string[] ignored;
		private string headerString;
		private string kAdvanced, kAdd, kInvoke;

		protected override void OnSingleInitialization()
		{
			ignored = DelegateSettings.IgnoredMethods;

			// initialize adding area and foldout keys
			adding       = new AddingData();
			kAdvanced    = id + "advanced";
			kAdd         = id + "add";
			kInvoke      = id + "invoke";
			headerString = string.Format("{0} ({1})", niceName, memberTypeName);

			// make sure delegate is not null
			if (memberValue == null)
				 memberValue = memberType.Instance<BaseDelegate>();

			// initialize arguments data (used when invoking the delegate from the editor)
			int len      = memberValue.ParamTypes.Length;
			argValues    = new object[len];
			argMembers   = new EditorMember[len];

			for (int iLoop = 0; iLoop < len; iLoop++)
			{
				int i = iLoop;
				var paramType = memberValue.ParamTypes[i];

				argValues[i] = paramType.GetDefaultValueEmptyIfString();

				var argMember = new ArgMember(
						@getter     : () => argValues[i],
						@setter     : x => argValues[i] = x,
						@target     : argValues,
						@unityTarget: unityTarget,
						@name       : string.Format("({0})", paramType.GetNiceName()),
						@id         : id + i,
						@dataType   : paramType,
						@attributes : null
					);

				argMember.Target = rawTarget;

				argMembers[i] = argMember;
			}

#if DBG
			Log("Delegate drawer init. " + niceName);
#endif
		}

		public override void OnGUI()
		{
			foldout = gui.Foldout(headerString, foldout, Layout.sExpandWidth());
			if (!foldout) return;

			if (memberValue == null)
			{
#if DBG
				Log("Creating instance " + niceName);
#endif
				memberValue = memberType.Instance<BaseDelegate>();
			}

			// read
			var handlers = memberValue.handlers;

			using (gui.Vertical(GUI.skin.box))
			{
				if (handlers.Count == 0)
				{
					gui.HelpBox("There are no handlers for this delegate");
					goto FOOTER;
				}

				// header
				{
					gui.Space(1.5f);
					using (gui.Horizontal())
					{
						gui.BoldLabel("Target :: Handler");
						using (gui.State(foldout))
						{
							foldouts[kAdvanced] = gui.CheckButton(foldouts[kAdvanced], "advanced mode", MiniButtonStyle.Right);
						}
					}
				}

				gui.Splitter();

				// body
				{
					// target : handler
					for (int iLoop = 0; iLoop < handlers.Count; )
					{
						var i = iLoop;
						var handler = handlers[i];
						var removed = false;
						using (gui.Horizontal())
						{
							gui.Object(handler.target);
							gui.Text(handler.method);
							if (foldouts[kAdvanced])
							{
								if (gui.MoveDownButton())
									handlers.MoveElementDown(i);
								if (gui.MoveUpButton())
									handlers.MoveElementUp(i);
								if (gui.RemoveButton("handler", MiniButtonStyle.ModRight))
								{
									handlers.RemoveAt(i);
									removed = true;
								}
							}
						}

						if (!removed) iLoop++;
					}
				}

				FOOTER:
				gui.Space(3f);
				gui.Splitter();
				{
					// add> source :: invoke>
					using (gui.Horizontal())
					{
						using (gui.State(foldouts[kAdd] && adding.source != null && adding.target != null && adding.method != null))
						{
							if (gui.Button("Add", "Add new handler", EditorStyles.miniButton, Layout.sWidth(40f)))
							{
								handlers.Add(new BaseDelegate.Handler
								{
									target = adding.target,
									method = adding.method.Name
								});
							}
						}

						gui.Space(10f);

						// add foldout
						if (adding.source != null)
							foldouts[kAdd] = gui.Foldout(foldouts[kAdd]);

						// source
						var obj = gui.Object(adding.source);
						{
							if (adding.source != obj)
							{
								adding.source = obj;
								foldouts[kAdd] = true;
							}
						}

						bool paramless = argMembers.IsEmpty();
						using (gui.State(handlers.Count > 0 && (foldouts[kInvoke] || paramless)))
						{
							if (gui.Button("Invoke", EditorStyles.miniButtonRight, Layout.sWidth(50f)))
							{
								for (int i = 0; i < handlers.Count; i++)
								{
									var handler = handlers[i];
									var method = handler.target.GetType().DelegateForCallMethod(handler.method, memberValue.ParamTypes);
									method.Invoke(handler.target, argValues);
								}
							}
						}

						if (!paramless && handlers.Count > 0)
						{
							gui.Space(8f);
							foldouts[kInvoke] = gui.Foldout(foldouts[kInvoke]);
						}
					}


					// adding area: target -- method
					if (foldouts[kAdd] && adding.source != null)
					{
						gui.Space(5f);
						gui.Label("Add handler:");
						using (gui.Indent(GUI.skin.textArea))
						{
							// target
							var gameObject = adding.gameObject;
							if (gameObject == null)
							{
								gui.Popup("Target", 0, new string[] { adding.source.GetType().Name });
								{
									adding.target = adding.source;
								}
							}
							else
							{
								if (adding.source is Component)
									adding.target = adding.source;

								var components      = gameObject.GetAllComponents();
								int cIndex          = components.IndexOfZeroIfNotFound(adding.target);
								var uniqueNames     = components.Select(c => c.GetType().Name).ToList().Uniqify();
								var targetSelection = gui.Popup("Target", cIndex, uniqueNames.ToArray());
								{
									if (cIndex != targetSelection || adding.target != components[targetSelection])
										adding.target = components[targetSelection];
								}
							}

							// method
							if (adding.target != null)
							{
								var methods = adding.target.GetType()
													.GetMethods(memberValue.ReturnType, memberValue.ParamTypes, Flags.InstancePublic, false)
													.Where(m => !m.IsDefined<HideAttribute>())
													.Where(m => !ignored.Contains(m.Name))
													.ToList();
								int mIndex  = methods.IndexOfZeroIfNotFound(adding.method);
								int methodSelection = gui.Popup("Handler", mIndex, methods.Select(m => m.GetFullName()).ToArray());
								{
									if (!methods.IsEmpty() && (mIndex != methodSelection || adding.method != methods[methodSelection]))
										adding.method = methods[methodSelection];
								}
							}
						}
					}

					// invocation args
					if (foldouts[kInvoke])
					{
						gui.Label("Invoke with arguments:");
						using (gui.Indent())
						{
							for (int i = 0; i < argMembers.Length; i++)
								//gui.Member(argMembers[i], unityTarget, id, false);
								gui.Member(argMembers[i], false);
						}
					}
				}
			}

			// write
			memberValue.handlers = handlers;
		}

		private class AddingData
		{
			public UnityObject source;
			public UnityObject target;
			public MethodInfo method;

			public GameObject gameObject
			{
				get
				{
					var component = source as Component;
					return component == null ? source as GameObject : component.gameObject;
				}
			}
		}
	}
}
