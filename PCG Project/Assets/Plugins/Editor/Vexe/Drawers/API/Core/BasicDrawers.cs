using System;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Vexe.Editor.Drawers
{
	public abstract class BasicDrawer<T> : ObjectDrawer<T>
	{
		protected virtual Func<string, T, T> GetField()
		{
			throw new NotImplementedException();
		}

		public override void OnGUI()
		{
			memberValue = GetField().Invoke(niceName, memberValue);
		}
	}

	public class IntDrawer : BasicDrawer<int>
	{
		protected override Func<string, int, int> GetField()
		{
			return gui.Int;
		}
	}

	public class FloatDrawer : BasicDrawer<float>
	{
		protected override Func<string, float, float> GetField()
		{
			return gui.Float;
		}
	}

	public class StringDrawer : BasicDrawer<string>
	{
		protected override Func<string, string, string> GetField()
		{
			return gui.Text;
		}
	}

	public class Vector2Drawer : BasicDrawer<Vector2>
	{
		protected override Func<string, Vector2, Vector2> GetField()
		{
			return gui.Vector2;
		}
	}

	public class Vector3Drawer : BasicDrawer<Vector3>
	{
		protected override Func<string, Vector3, Vector3> GetField()
		{
		  return gui.Vector3;
		}
	}

	public class BoolDrawer : BasicDrawer<bool>
	{
		protected override Func<string, bool, bool> GetField()
		{
			return gui.Toggle;
		}
	}

	public class ColorDrawer : BasicDrawer<Color>
	{
		protected override Func<string, Color, Color> GetField()
		{
			return gui.Color;
		}
	}

	public class BoundsDrawer : BasicDrawer<Bounds>
	{
		protected override Func<string, Bounds, Bounds> GetField()
		{
			return gui.BoundsField;
		}
	}

	public class RectDrawer : BasicDrawer<Rect>
	{
		protected override Func<string, Rect, Rect> GetField()
		{
			return gui.Rect;
		}
	}

	public class QuaternionDrawer : BasicDrawer<Quaternion>
	{
		protected override Func<string, Quaternion, Quaternion> GetField()
		{
			return gui.Quaternion;
		}
	}

	public class UnityObjectDrawer : BasicDrawer<UnityObject>
	{
		public override void OnGUI()
		{
			memberValue = gui.Object(niceName, memberValue, memberType, !AssetDatabase.Contains(unityTarget));
		}
	}

	public class LayerMaskDrawer : BasicDrawer<LayerMask>
	{
		protected override Func<string, LayerMask, LayerMask> GetField()
		{
			return gui.Layer;
		}
	}

	public class EnumDrawer : BasicDrawer<Enum>
	{
		protected override Func<string, Enum, Enum> GetField()
		{
			return gui.EnumPopup;
		}
	}
}
