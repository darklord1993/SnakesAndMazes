using UnityEditor;
using Vexe.Runtime.Types;

namespace Vexe.Editor.Drawers
{
	public class ParagraphAttributeDrawer : AttributeDrawer<string, ParagraphAttribute>
	{
		protected override void OnSingleInitialization()
		{
			if (memberValue == null)
				memberValue = string.Empty;
		}

		public override void OnGUI()
		{
			gui.Label(niceName);
			memberValue = gui.TextArea(memberValue);
		}
	}
}