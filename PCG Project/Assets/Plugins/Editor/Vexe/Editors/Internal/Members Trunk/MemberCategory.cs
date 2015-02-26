//#define PROFILE

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Vexe.Editor.GUIs;
using Vexe.Editor.Helpers;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;

namespace Vexe.Editor.Internal
{
	public class MemberCategory : ICanBeDrawn
	{
		private readonly string fullPath;
		private readonly string name;
		private readonly string id;
		private readonly BetterPrefs prefs;
		private MembersDisplay display;

		public BaseGUI gui               { get; set; }
		public List<ICanBeDrawn> Members { get; set; }
		public float DisplayOrder        { get; set; }
		public string Name               { get { return name; } }
		public string FullPath           { get { return fullPath; } }
		public bool ForceExpand          { get; set; }
		public bool HideHeader           { get; set; }
		public bool IsExpanded           { get; private set; }
		public bool Indent               { get; set; }
		public MembersDisplay Display
		{
			get { return display; }
			set
			{
				if (display != value)
				{
					display = value;
					Members.OfType<MemberCategory>().Foreach(c => c.Display = display);
				}
			}
		}

		public MemberCategory(string fullPath, List<ICanBeDrawn> members, float displayOrder, string id, BetterPrefs prefs)
		{
			Members       = members;
			DisplayOrder  = displayOrder;
			this.prefs    = prefs;
			this.fullPath = fullPath;
			this.name     = FullPath.Substring(FullPath.LastIndexOf('/') + 1);
			this.id       = id + fullPath;
			Indent        = true;
		}

		public void AddMember(ICanBeDrawn member)
		{
			Members.Add(member);
		}

		// Keys & Foldouts
		#region
		private bool DoHeader()
		{
			bool foldout = false;
			using (gui.Horizontal(EditorStyles.toolbarButton))
			{
				gui.Space(10f);
				foldout = gui.Foldout(name, prefs.Bools.ValueOrDefault(id), Layout.sExpandWidth());
				prefs.Bools[id] = foldout;
			}

			return foldout;
		}
		#endregion


		public void Draw()
		{
			int count = Members.Count;
			if (count == 0)
				return;

			IsExpanded = HideHeader || DoHeader();
			if (!(IsExpanded || ForceExpand))
				return;

			gui.Space(1f);

			bool showGuiBox   = (Display & MembersDisplay.GuiBox) > 0;
			bool showSplitter = (Display & MembersDisplay.Splitter) > 0;

			using (gui.Indent(showGuiBox ? EditorStyles.textArea : GUIStyle.none, Indent))
			{
				gui.Space(5f);
#if PROFILE
				Profiler.BeginSample(name + " Members");
#endif
				for (int i = 0; i < count; i++)
				{
					var member = Members[i];

					using (gui.Horizontal())
					{
						member.gui = this.gui;
						member.HeaderSpace();
						using (gui.Vertical())
						{
							member.Draw();
						}
					}

					if (showSplitter && i < count - 1)
						gui.Splitter();
					gui.Space(2f);
				}
#if PROFILE
				Profiler.EndSample();
#endif
			}
		}

		public void HeaderSpace()
		{
			if (IsExpanded)
				gui.Space(4f);
		}
	}
}
