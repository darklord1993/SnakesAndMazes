using System;
using Vexe.Editor.GUIs;

namespace Vexe.Editor.Internal
{
	public interface ICanBeDrawn
	{
		BaseGUI gui        { get; set; }
		float DisplayOrder { get; }
		string Name        { get; }
		void Draw();
		void HeaderSpace();
	}
}