using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fasterflect;
using UnityEditor;
using UnityEngine;
using Vexe.Editor.GUIs;
using Vexe.Editor.Helpers;
using Vexe.Runtime.Exceptions;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;

namespace Vexe.Editor.Drawers
{
	public abstract class BasePopupAttributeDrawer<T> : AttributeDrawer<T, PopupAttribute>
	{
		private string[] values;
		private int? currentIndex;
		private MethodInvoker populate;

		protected override void OnSingleInitialization()
		{
			string populateMethod = attribute.PopulateFrom;
			if (populateMethod.IsNullOrEmpty())
			{
				values = attribute.values;
			}
			else
			{
				var all = targetType.GetAllMembers(typeof(object));
				var member = all.FirstOrDefault(x => x.MemberType == MemberTypes.Method && x.Name == populateMethod);
				if (member == null)
					throw new MemberNotFoundException(populateMethod);

				populate = (member as MethodInfo).DelegateForCallMethod();

				var pop = populate(rawTarget);
				if (pop != null)
					values = ProcessPopulation(pop);
			}

			if (values.IsNullOrEmpty())
				values = new[] { "NA" };
		}

		Func<T, string> _getString;
		Func<T, string> GetString()
		{
			return _getString ?? (_getString = new Func<T, string>(x => x.ToString()).Memoize());
		}

		public override void OnGUI()
		{
			if (populate != null && attribute.AlwaysUpdate)
			{
				var pop = populate(rawTarget);
				if (pop != null)
					values = ProcessPopulation(pop);
			}

			if (!currentIndex.HasValue)
			{
				string currentValue = GetString().Invoke(memberValue);
				currentIndex = values.IndexOf(currentValue);
				if (currentIndex == -1)
				{
					currentIndex = 0;
					if (values.Length > 0)
						SetValue(values[0]);
				}
			}

			int x = gui.Popup(niceName, currentIndex.Value, values);
			{
				if (currentIndex != x)
				{
					SetValue(values[x]);
					currentIndex = x;

					var rabbit = gui as RabbitGUI;
					if (rabbit != null)
						rabbit.RequestReset();
				}
			}
		}

		protected abstract string[] ProcessPopulation(object population);
		protected abstract void SetValue(string value);
	}

	public class IntPopupAttributeDrawer : BasePopupAttributeDrawer<int>
	{
		protected override string[] ProcessPopulation(object population)
		{
			return population is int[] ? 
				(population as int[]).Select(x => x.ToString()).ToArray() : 
				(population as List<int>).Select(x => x.ToString()).ToArray();
		}

		protected override void SetValue(string value)
		{
			memberValue = Convert.ToInt32(value);
		}
	}
	public class FloatPopupAttributeDrawer : BasePopupAttributeDrawer<float>
	{
		protected override string[] ProcessPopulation(object population)
		{
			return population is float[] ? 
				(population as float[]).Select(x => x.ToString()).ToArray() : 
				(population as List<float>).Select(x => x.ToString()).ToArray();
		}

		protected override void SetValue(string value)
		{
			memberValue = Convert.ToSingle(value);
		}
	}
	public class StringPopupAttributeDrawer : BasePopupAttributeDrawer<string>
	{
		protected override void OnSingleInitialization()
		{
			base.OnSingleInitialization();
			if (memberValue == null)
				memberValue = string.Empty;
		}

		protected override string[] ProcessPopulation(object population)
		{
			return population is string[] ? 
				(population as string[]).Select(x => x).ToArray() : 
				(population as List<string>).Select(x => x).ToArray();
		}

		protected override void SetValue(string value)
		{
			memberValue = value;
		}
	}
}