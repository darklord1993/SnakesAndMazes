using Vexe.Runtime.Types.GUI;

namespace Vexe.Editor.Drawers
{
	public class ColorDuoDrawer : ObjectDrawer<ColorDuo>
	{
		public override void OnGUI()
		{
			foldout = gui.Foldout(niceName, foldout);

			if (!foldout) return;

			using (gui.Vertical())
			{
				memberValue.FirstColor = gui.Color("First", memberValue.FirstColor);
				memberValue.SecondColor = gui.Color("Second", memberValue.SecondColor);
			}
			
		}
	}
}