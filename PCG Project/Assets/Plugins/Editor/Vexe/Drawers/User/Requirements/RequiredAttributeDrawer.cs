using System;
using Fasterflect;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Types;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Drawers
{
	public class RequiredAttributeDrawer : BaseRequirementAttributeDrawer<RequiredAttribute>
	{
		protected override Component GetComponent(GameObject from, Type componentType)
		{
			Component result = null;
			if (member.IsNull())
			{
				result = UnityObject.FindObjectOfType(memberType) as Component;
				if (result == null)
				{
					if (attribute.Add && !memberType.IsAbstract)
					{
						var go = new GameObject("[Required] " + memberTypeName);
						result = go.AddComponent(memberType);
					}
				}

				if (result == null)
					gui.HelpBox("Reference is required but is yet to be assigned...", MessageType.Warning);
			}

			return result;
		}

		protected override GameObject GetGO(GameObject from)
		{
			GameObject result = null;
			if (member.IsNull())
			{
				if (attribute.Add)
					result = new GameObject("[Required] " + member.Name);
				else result = GameObject.Find(member.Name);

				if (result == null)
					gui.HelpBox("Reference is required but is yet to be assigned...", MessageType.Warning);
			}

			return result;
		}
	}
}