using System;
using System.Reflection;
using UnityEditor;
using Vexe.Editor.GUIs;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Internal
{
	public class VisibleMember : ICanBeDrawn, IEquatable<VisibleMember>
	{
		private readonly MemberInfo info;
		private readonly object rawTarget;
		private readonly UnityObject unityTarget;
		private readonly string key;

		public BaseGUI gui        { get; set; }
		public MemberInfo Info    { get { return info; } }
		public Type DataType      { get { return info.GetDataType(); } }
		public string Name		  { get { return info.Name; } }
		public float DisplayOrder
		{
			get
			{
				var attr = Info.GetCustomAttribute<DisplayOrderAttribute>();
				return attr == null ? -1 : attr.displayOrder;
			}
		}

		public VisibleMember(MemberInfo info, object rawTarget, UnityObject unityTarget, string key)
		{
			this.info        = info;
			this.rawTarget   = rawTarget;
			this.unityTarget = unityTarget;
			this.key         = key;
		}

		public void Draw()
		{
			gui.Member(info, rawTarget, unityTarget, key, false);
		}

		public void HeaderSpace()
		{
			gui.Space(10f);
		}

		public bool Equals(VisibleMember other)
		{
			return info.Equals(other.info);
		}
	}
}