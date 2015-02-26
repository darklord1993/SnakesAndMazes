using System.Text.RegularExpressions;
using Fasterflect;
using UnityEngine;
using Vexe.Editor.Helpers;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;

namespace Vexe.Editor.Drawers
{
	public class AnimatorVariableAttributeDrawer : AttributeDrawer<string, AnimVarAttribute>
	{
		private string[] variables;
		private int current;

		private Animator animator;
		private Animator Animator
		{
			get
			{
				if (animator == null)
				{
					string getter = attribute.GetAnimator;
					if (getter.IsNullOrEmpty())
					{
						animator = gameObject.GetComponent<Animator>();
					}
					else
					{
						animator = targetType.GetMethod(getter, Flags.InstanceAnyVisibility)
										      .Invoke(rawTarget, null) as Animator;
					}
				}
				return animator;
			}
		}

		protected override void OnSingleInitialization()
		{
			if (memberValue == null)
				memberValue = "";

			if (Animator != null && Animator.runtimeAnimatorController != null)
				FetchVariables();
		}

		private void FetchVariables()
		{
			variables = EditorHelper.GetAnimatorVariableNames(Animator);
			if (variables.IsEmpty())
				variables = new[] { "N/A" };
			else
			{
				if (!attribute.AutoMatch.IsNullOrEmpty())
				{
					string match = niceName.Remove(niceName.IndexOf(attribute.AutoMatch));
					match = Regex.Replace(match, @"\s+", "");
					if (variables.ContainsValue(match))
						memberValue = match;
				}
				current = variables.IndexOfZeroIfNotFound(memberValue);
			}
		}

		public override void OnGUI()
		{
			if (Animator == null || Animator.runtimeAnimatorController == null)
			{
				memberValue = gui.Text(niceName, memberValue);
			}
			else
			{
				if (variables.IsNullOrEmpty())
				{
					FetchVariables();
				}

				var x = gui.Popup(niceName, current, variables);
				{
					if (current != x || memberValue != variables[x])
						memberValue = variables[current = x];
				}
			}
		}
	}
}