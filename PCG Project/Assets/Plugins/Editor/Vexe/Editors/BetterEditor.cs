//#define PROFILE
//#define DBG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fasterflect;
using UnityEditor;
using UnityEngine;
using Vexe.Editor.Internal;
using Vexe.Editor.Editors;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Serialization;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;
using Vexe.Runtime.Exceptions;
using Vexe.Runtime.Helpers;

namespace Vexe.Editor.Editors
{
	public abstract class BetterEditor : BaseEditor<UnityObject>
	{
		private List<MemberCategory> baseCategories;
		private List<VisibleMember> visibleMembers;
		private SerializedProperty script;
		private EditorMember serializationData;
		private bool minimalView;
		private int repaintCount;
		private MembersDisplay display;

		private static MembersDisplay DefaultDisplay
		{
			get { return MembersDisplay.GuiBox | MembersDisplay.Splitter; }
		}

		protected abstract bool dbg { get; set; }
		protected abstract void OnAwakeAssertion();

		//static bool clearedCache;
		//static BetterEditor()
		//{
		//	//if (!clearedCache)
		//	{
		//		clearedCache = true;
		//		MemberDrawersHandler.Instance.ClearCache();
		//	}
		//}

		protected override void OnAwake()
		{
			OnAwakeAssertion();

			// Fetch visible members
			{
				Func <MemberInfo, VisibleMember> newMember = m => new VisibleMember(m, target, target, id);
				var allMembers = ReflectionUtil.GetMemoizedMembers(targetType);
				visibleMembers = SerializationLogic.Default.GetVisibleMembers(allMembers).Select(newMember).ToList();

				// Initialize view
				{
					minimalView = targetType.IsDefined<MinimalViewAttribute>(true);
					if (minimalView)
					{ 
						visibleMembers = visibleMembers.OrderBy(x => x.DisplayOrder).ToList();
					}
					else
					{
						baseCategories = new List<MemberCategory>();

						Func<string, float,  MemberCategory> newCategory = (path, order) =>
							new MemberCategory(path, new List<ICanBeDrawn>(), order, id, prefs);

						var basic = targetType.GetCustomAttribute<BasicViewAttribute>(true);
						if (basic != null)
						{
							var c = newCategory(string.Empty, 0f);
							c.Indent = false;
							c.HideHeader = true;
							visibleMembers.OrderBy(x => x.DisplayOrder).Foreach(c.AddMember);
							baseCategories.Add(c);
						}
						else
						{
							// Create the intial input from the target's visible members
							var input = new MemberGroup(visibleMembers, newMember);

							// Create resolvers
							var resolvers = new GroupResolver[]
							{
								new PatternResolver(),		new MemberTypesResolver(),
								new ReturnTypeResolver(),	new ExplicitMemberAddResolver(),
								new CategoryMembersResolver()
							};
							var core = new CoreResolver(resolvers, () => new MemberGroup(newMember));

							Action<DefineCategoryAttribute, MemberCategory> resolve = (def, category) =>
								core.Resolve(input, def).Members.Foreach(category.AddMember);

							var multiple	= targetType.GetCustomAttribute<DefineCategoriesAttribute>(true);
							var ignored		= targetType.GetCustomAttributes<IgnoreCategoriesAttribute>(true);
							var definitions = targetType.GetCustomAttributes<DefineCategoryAttribute>(true);
							if (multiple != null)
								definitions = definitions.Concat(multiple.names.Select(n => new DefineCategoryAttribute(n, 1000)));

							definitions = definitions.Where(d => !ignored.Any(ig => ig.Paths.Contains(d.FullPath)))
								                     .ToList();

							Func<string, string[]> ParseCategoryPath = fullPath =>
							{
								int nPaths = fullPath.Split('/').Length;
								string[] result = new string[nPaths];
								for (int i = 0, index = -1; i < nPaths - 1; i++)
								{
									index = fullPath.IndexOf('/', index + 1);
									result[i] = fullPath.Substring(0, index);
								}
								result[nPaths - 1] = fullPath;
								return result;
							};

							// Order by exclusivity then path lengths
							var defs = from d in definitions
									   let paths = ParseCategoryPath(d.FullPath)
									   orderby !d.Exclusive//, paths.Length
									   select new { def = d, paths };

							Func<MemberCategory, List<MemberCategory>> getParentCats = parent =>
								parent == null ? baseCategories :
								(from m in parent.Members
								 let cat = m as MemberCategory
								 where cat != null
								 select cat).ToList();

							// Parse paths and resolve definitions
							var categories = new Dictionary<string, MemberCategory>();
							foreach (var x in defs)
							{
								var paths = x.paths;
								var d = x.def;

								MemberCategory parent = null;

								for (int i = 0; i < paths.Length; i++)
								{
									var p = paths[i];
									var current = getParentCats(parent).FirstOrDefault(c => c.FullPath == p);
									if (current == null)
									{
										current = newCategory(p, d.DisplayOrder);
										if (i == 0)
											baseCategories.Add(current);
										if (parent != null)
											parent.AddMember(current);
									}
									categories[p] = current;
									parent = current;
								}

								categories[paths.Last()].ForceExpand = d.ForceExpand;
								resolve(d, categories[paths.Last()]);
								categories.Clear();
								parent.Members = parent.Members.OrderBy(m => m.DisplayOrder).ToList();
							}
							baseCategories = baseCategories.OrderBy(x => x.DisplayOrder).ToList();
						}
					}

					script = serializedObject.FindProperty("m_Script");

					var disInt = prefs.Ints.ValueOrDefault(id + "display", -1);
					display = disInt == -1 ? DefaultDisplay : (MembersDisplay)disInt;

					var field = targetType.Field("_serializationData", Flags.InstancePrivate);
					if (field == null)
						throw new MemberNotFoundException("_serializationData");

					serializationData = new EditorMember(field, target, target, id);

					OnInitialized();

#if DBG
					Log(GetType().Name + " for " + target.GetType().Name + " Initialized");
#endif
				}
			}
		}

		protected sealed override void OnGUI()
		{
			#if PROFILE
			Profiler.BeginSample(targetType.Name + " OnInspectorGUI");
			Profiler.BeginSample(targetType.Name + " Header");
			#endif

			// Header
			{
				string scriptKey = id + "script";
				gui.Space(3f);
				using (gui.Horizontal(EditorStyles.toolbarButton))
				{
					gui.Space(10f);
					foldouts[scriptKey] = gui.Foldout(foldouts[scriptKey]);
					gui.Space(-12f);
					gui.Object("Script", script.objectReferenceValue);
				}

				if (foldouts[scriptKey])
				{
					gui.Space(2f);

					using (gui.Indent(GUI.skin.textField))
					{
						gui.Space(2f);
						dbg = gui.Toggle("Debug", dbg);
						var mask = gui.BunnyMask("Display", display);
						{
							var newValue = (MembersDisplay)mask;
							if (display != newValue)
							{
								display = newValue;
								prefs.Ints[id + "display"] = mask;
							}
						}

						gui.Member(serializationData, true);
					}
				}
			}

			#if PROFILE
			Profiler.EndSample();
			#endif

			gui.BeginCheck();

			#if PROFILE
			Profiler.BeginSample(targetType.Name + " Members");
			#endif

			if (minimalView)
			{
				gui.Space(4f);
				for (int i = 0; i < visibleMembers.Count; i++)
				{ 
					var vim = visibleMembers[i];
					vim.gui = gui;
					vim.Draw();
				}
			}
			else
			{
				for (int i = 0; i < baseCategories.Count; i++)
				{
					var cat     = baseCategories[i];
					cat.Display = display;
					cat.gui     = gui;
					cat.Draw();
				}
			}

			OnBetterGUI();

			#if PROFILE
			Profiler.EndSample();
			#endif

			if (gui.HasChanged())
			{
				EditorUtility.SetDirty(target);
			}

			#if PROFILE
			Profiler.EndSample();
			#endif

			// Fixes somes cases of editor slugishness
			if (repaintCount < 2)
			{
				repaintCount++;
				Repaint();
			}
		}

		/// <summary>
		/// override this when writing gui code for your custom BetterEditors instead of using OnInspectorGUI or OnGUI
		/// </summary>
		protected virtual void OnBetterGUI()
		{
		}

		/// <summary>
		/// override this when initializing your custom BetterEditors instead of using OnEnable or OnAwake
		/// </summary>
		protected virtual void OnInitialized()
		{
		}
	}
}
