using UnityEngine;
using Vexe.Runtime.Types;

[MinimalView, DisallowMultipleComponent, ExecuteInEditMode]
public class PrefabMarker : BetterBehaviour
{
	[Show, Readonly] private bool isAlive;

	public bool IsAlive
	{
		get { return isAlive; }
	}

	void Awake()
	{
		isAlive = true;
	}
}
