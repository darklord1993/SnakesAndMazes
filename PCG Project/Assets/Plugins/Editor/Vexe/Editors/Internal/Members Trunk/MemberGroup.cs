using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vexe.Runtime.Extensions;

namespace Vexe.Editor.Internal
{
	public class MemberGroup
	{
		public List<VisibleMember> Members { get; private set; }
		private readonly Func<MemberInfo, VisibleMember> newMember;

		public MemberGroup(List<VisibleMember> members, Func<MemberInfo, VisibleMember> newMember)
		{
			Members = members;
			this.newMember = newMember;
		}

		public MemberGroup(Func<MemberInfo, VisibleMember> newMember)
			: this(new List<VisibleMember>(), newMember)
		{
		}

		public void AddMember(MemberInfo minfo)
		{
			Members.Add(newMember(minfo) as VisibleMember);
		}

		public void AddMember(VisibleMember member)
		{
			Members.Add(member);
		}

		public void UnionMembers(IEnumerable<VisibleMember> members)
		{
			Members = Members.Union(members).ToList();
		}

		public void AddMembers(IEnumerable<VisibleMember> members)
		{
			members.Foreach(AddMember);
		}

		public void AddMembers(MemberGroup input, Func<VisibleMember, bool> predicate)
		{
			input.Members.Where(predicate).Foreach(AddMember);
		}
	}
}