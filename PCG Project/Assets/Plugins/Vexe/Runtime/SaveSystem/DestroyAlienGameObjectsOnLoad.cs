using UnityEngine;
using Vexe.Runtime.Extensions;

/// <summary>
/// Use this component to destroy newly created gameObjects since last save on load
/// Ex: 1- We have a scene with X, Y and Z gameObjects
///		2- We save the game
///		3- Instantiate gameObject W
///		4- Load game
///		5- If this script was available, W would get destroyed otherwise not
/// </summary>
public class DestroyAlienGameObjectsOnLoad : DestroyAlienObjectOnLoad
{
	protected override void LoadSnapshot(FinishedLoadingEvent e)
	{
		var all = FindObjectsOfType<GameObject>();
		for (int i = 0; i < all.Length; i++)
		{
			var go = all[i];
			if (!snapshot.Contains(go.GetId()))
				go.Destroy();
		}
	}

	protected override void TakeSnapshot(BeganSavingEvent e)
	{
		snapshot.Clear();
		var all = FindObjectsOfType<GameObject>();
		for (int i = 0; i < all.Length; i++)
		{
			snapshot.Add(all[i].GetId());
		}
	}
}