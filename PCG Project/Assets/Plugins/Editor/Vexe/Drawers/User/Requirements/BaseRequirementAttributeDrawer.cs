using System;
using Fasterflect;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Drawers
{
	public abstract class BaseRequirementAttributeDrawer<T> : CompositeDrawer<UnityObject, T> where T : RequiredAttribute
	{
		private bool isMemberGo;
		private Func<GameObject> mGetSource;

		protected Func<GameObject> getSource
		{
			get
			{
				if (mGetSource == null)
				{
					var goProp = targetType.GetProperty("gameObject");
					mGetSource = new Func<GameObject>(() => goProp.GetGetMethod().Invoke(rawTarget, null) as GameObject).Memoize();
				}
				return mGetSource;
			}
		}

		protected override void OnSingleInitialization()
		{
			isMemberGo = memberType.IsA<GameObject>();
			if (!isMemberGo && targetType.GetProperty("gameObject") == null)
				throw new InvalidOperationException("Member `" + member.Name + "` is not a gameObject nor does the target it's in have a gameObject property");
		}

		public override void OnUpperGUI()
		{
			if (member.IsNull())
			{
				var from = getSource();
				memberValue = isMemberGo ? (UnityObject)GetGO(from) : GetComponent(from, memberType);
			}
		}

		protected abstract Component GetComponent(GameObject from, Type componentType);
		protected abstract GameObject GetGO(GameObject from);
	}
}