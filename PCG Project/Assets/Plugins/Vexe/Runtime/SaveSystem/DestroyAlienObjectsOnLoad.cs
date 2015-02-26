using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;

[DisallowMultipleComponent, ExecuteInEditMode, RequireComponent(typeof(SaveMarker))]
public abstract class DestroyAlienObjectOnLoad : MonoBehaviour
{
	[SerializeField]
	protected List<string> snapshot = new List<string>();

	void OnEnable()
	{
		EventManager.Subscribe<BeganSavingEvent>(TakeSnapshot);
		EventManager.Subscribe<FinishedLoadingEvent>(LoadSnapshot);
	}

	void OnDisable()
	{
		EventManager.Unsubscribe<BeganSavingEvent>(TakeSnapshot);
		EventManager.Unsubscribe<FinishedLoadingEvent>(LoadSnapshot);
	}

	protected abstract void LoadSnapshot(FinishedLoadingEvent e);
	protected abstract void TakeSnapshot(BeganSavingEvent e);
}
