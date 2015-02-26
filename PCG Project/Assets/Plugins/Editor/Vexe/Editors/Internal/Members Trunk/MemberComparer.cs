using System.Collections.Generic;

namespace Vexe.Editor.Internal
{
	public class MemberComparer : IEqualityComparer<VisibleMember>
	{
		public bool Equals(VisibleMember x, VisibleMember y)
		{
			return x.Info.Equals(y.Info);
		}

		public int GetHashCode(VisibleMember obj)
		{
			return obj.Info.GetHashCode();
		}
	}
}