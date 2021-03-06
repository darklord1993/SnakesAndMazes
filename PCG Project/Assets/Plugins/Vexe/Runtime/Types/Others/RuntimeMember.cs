﻿#if UNITY_EDITOR || UNITY_STANDALONE
#define FASTERFLECT
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fasterflect;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Helpers;

namespace Vexe.Runtime.Types
{
	public class RuntimeMember
	{
		private MemberSetter setter;
		private MemberGetter getter;

		static Attribute[] Empty = new Attribute[0];

		private object target;
		public object Target
		{
			get { return target; }
			set
			{
				if (!IsStatic && target != value)
					target = value.WrapIfValueType();
			}
		}

		public Type Type { get; protected set; }

		public Type TryRuntimeType
		{
			get
			{
				var x = Value;
				if (x != null)
					return x.GetType();
				return Type;
			}
		}

		public MemberInfo Info { get; private set; }

		public bool IsStatic { get; private set; }

		public MemberTypes MemberType { get { return Info == null ? MemberTypes.Custom : Info.MemberType; } }

		public object Value
		{
			get { return Get(); }
			set { Set(value); }
		}

		protected Attribute[] attributes;
		public Attribute[] Attributes
		{
			get { return attributes ?? (attributes = Info == null ? Empty : Info.GetCustomAttributes<Attribute>().ToArray()); }
		}

		private string typeNiceName;
		public string TypeNiceName
		{
			get { return typeNiceName ?? (typeNiceName = Type.GetNiceName()); }
		}

		public string Name { get; protected set; }

		private string niceName;
		public string NiceName
		{
			get { return niceName ?? (niceName = Name.Replace("_", "").SplitPascalCase()); }
		}

		public RuntimeMember(MemberInfo member, object target)
		{
			if (member == null) return;

			Name     = member.Name;
			Info     = member;
			IsStatic = Info.IsStatic();
			Target   = target;

			var field = member as FieldInfo;
			if (field != null)
			{
				if (field.IsLiteral)
					throw new InvalidOperationException("Can't wrap const fields " + field.Name);

#if FASTERFLECT
				setter = field.DelegateForSetFieldValue();
				getter = field.DelegateForGetFieldValue();
#else
				setter = field.SetValue;
				getter = field.GetValue;
#endif
				Type = field.FieldType;
			}
			else
			{
				var property = member as PropertyInfo;
				if (property == null)
					throw new InvalidOperationException("MemberInfo {0} is not supported. Only fields and readable properties are".FormatWith(member));

				if (property.IsIndexer())
					throw new InvalidOperationException("Can't wrap member {0} because it is an indexer.".FormatWith(member));

				if (!property.CanRead)
					throw new InvalidOperationException("Can only wrap readable properties " + member.Name);

				if (property.CanWrite)
				{
#if FASTERFLECT
					setter = property.DelegateForSetPropertyValue();
#else
					setter = (x, y) => property.SetValue(x, y, null);
#endif
				}
				else setter = (obj, value) => { };

#if FASTERFLECT
				getter = property.DelegateForGetPropertyValue();
#else
				getter = x => property.GetValue(x, null);
#endif

				Type = property.PropertyType;
			}
		}

		public object Get(object target)
		{
			Target = target;
			return Get();
		}

		public virtual object Get()
		{
			return getter(Target);
		}

		public TCast As<TCast>() where TCast : class
		{
			return Get() as TCast;
		}

		public void Set(object target, object value)
		{
			Target = target;
			Set(value);
		}

		public virtual void Set(object value)
		{
			setter(Target, value);
		}

		public override string ToString()
		{
			return TypeNiceName + " " + Name;
		}

		public override int GetHashCode()
		{
			return Info.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var member = obj as RuntimeMember;
			return member != null && member.Info == Info;
		}

		public static explicit operator FieldInfo(RuntimeMember member)
		{
			return member.Info as FieldInfo;
		}

		public static explicit operator PropertyInfo(RuntimeMember member)
		{
			return member.Info as PropertyInfo;
		}

		/// <summary>
		/// Returns a lazy enumerable DataMember representation of the specified member infos
		/// </summary>
		public static IEnumerable<RuntimeMember> Enumerate(IEnumerable<MemberInfo> members, object target)
		{
			foreach (var member in members)
			{
				var field = member as FieldInfo;
				if (field != null)
				{
					if (field.IsLiteral)
						continue;
				}
				else
				{
					var prop = member as PropertyInfo;
					if (prop == null || !prop.CanRead || prop.IsIndexer())
						continue;
				}

				yield return new RuntimeMember(member, target);
			}
		}

		public static IEnumerable<RuntimeMember> Enumerate(Type type, object target, BindingFlags flags)
		{
			Assert.ArgumentNotNull(type, "type");
			return Enumerate(type.GetMembers(flags), target);
		}

		public static IEnumerable<RuntimeMember> Enumerate(Type type, object target)
		{
			return Enumerate(type, target, Flags.InstanceAnyVisibility);
		}

		public static IEnumerable<RuntimeMember> Enumerate(Type type)
		{
			return Enumerate(type, null);
		}

		public static IEnumerable<RuntimeMember> Enumerate(object target, BindingFlags flags)
		{
			Assert.ArgumentNotNull(target, "target");
			return Enumerate(target.GetType(), target, flags);
		}

		private static Func<Type, List<RuntimeMember>> enumerateMemoized;
		public static Func<Type, List<RuntimeMember>> EnumerateMemoized
		{
			get {
				return enumerateMemoized ?? (enumerateMemoized = new Func<Type, List<RuntimeMember>>(type =>
					RuntimeMember.Enumerate(type).ToList()).Memoize());
			}
		}
	}
}
