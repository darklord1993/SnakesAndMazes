using UnityEngine;
using Vexe.Runtime.Extensions;

/// <summary>
/// Attach this script to a gameObject if you want to destroy any newly added components to that gameObject
/// since the last save when loading it back.
/// Ex: 1- GO1 has Components X, Y.
///		2- Save game.
///		3- Add component Z to GO1.
///		4- Load game.
///		5- If GO1 had this script on it, Z would get destroyed otherwise not
/// </summary>
public class DestroyAlienComponentsOnLoad : DestroyAlienObjectOnLoad
{
	protected override void LoadSnapshot(FinishedLoadingEvent e)
	{
		var all = gameObject.GetAllComponents();
		for (int i = 0; i < all.Length; i++)
		{
			var comp = all[i];
			if (!snapshot.Contains(comp.GetType().Name))
				comp.Destroy();
		}
	}

	protected override void TakeSnapshot(BeganSavingEvent e)
	{
		snapshot.Clear();
		var all = gameObject.GetAllComponents();
		for (int i = 0; i < all.Length; i++)
		{
			snapshot.Add(all[i].GetType().Name);
		}
	}
}