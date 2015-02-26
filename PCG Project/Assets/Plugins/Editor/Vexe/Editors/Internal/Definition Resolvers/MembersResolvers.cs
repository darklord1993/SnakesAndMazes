using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;

namespace Vexe.Editor.Internal
{
	public class CategoryMembersResolver : MembersResolver
	{
		public override MemberGroup Resolve(MemberGroup input, DefineCategoryAttribute definition)
		{
			var output = newGroup();
			output.AddMembers(input, m =>
			{
				var memberDef = m.Info.GetCustomAttribute<CategoryAttribute>();
				return memberDef != null && memberDef.name == definition.FullPath;
			});
			return output;
		}
	}
}