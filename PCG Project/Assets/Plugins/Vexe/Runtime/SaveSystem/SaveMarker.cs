using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Vexe.Runtime.Types;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Attach this script to gameObjects that you want to save in order to save them.
/// You can then specify which components from within that gameObjects you want to save.
/// By default all components are saved.
/// You can use the methods visible in the inspector to mark/unmark children, 
/// apply the current saved components to children and select the store object
/// You can also press Alt-Shift-M to quickly add a SaveMarker to a selection of gameObjects
/// The file also contains the code responsible for showing the "Marked" label on marked gameObjects
/// </summary>
[ExecuteInEditMode, DisallowMultipleComponent, RequireComponent(typeof(UniqueId))]
public class SaveMarker : BetterBehaviour
{
	[Show, PerItem, Uncategorized,
	ShowType(typeof(Component), FromThisGo = true),
	Seq(SeqOpt.PerItemRemove | SeqOpt.LineNumbers | SeqOpt.UniqueItems)]
	public List<Type> ComponentsToSave
	{
		set { toSave = value; }
		get
		{
			if (toSave == null)
			{
				toSave = new List<Type>();
				MarkAllComponentsForSave();
			}
			return toSave;
		}
	}

	[Serialize, Hide]
	private List<Type> toSave;

	[Show, DisplayOrder(0)] void MarkAllComponentsForSave()
	{
		ComponentsToSave.Clear();
		ComponentsToSave.AddRange(gameObject.GetAllComponents().Select(x => x.GetType()));
	}

#if UNITY_EDITOR
	/// <summary>
	/// Recurisvely adds a SaveMarker to children
	/// </summary>
	[Show, DisplayOrder(1)] void MarkChildren()
	{
		var children = gameObject.GetComponentsInChildren<Transform>();
		for (int i = 0; i < children.Length; i++)
			children[i].gameObject.GetOrAddComponent<SaveMarker>();
	}

	/// <summary>
	/// Recurisvely removes SaveMarkers from children
	/// </summary>
	[Show, DisplayOrder(2)] void UnmarkChildren()
	{
		var children = gameObject.GetComponentsInChildren<Transform>();
		for (int i = 0; i < children.Length; i++)
		{
			var child = children[i];
			if (child == transform) continue;
			var marker = child.GetComponent<SaveMarker>();
			if (marker != null)
				marker.Destroy();
		}
	}

	/// <summary>
	/// Recurisvely removes SaveMarkers from children
	/// </summary>
	[Show, DisplayOrder(2.5f)] void UnmarkThisAndChildren()
	{
		UnmarkChildren();
		EditorApplication.delayCall += this.Destroy;
	}

	/// <summary>
	/// Recurisvely adds a SaveMarker to this gameObject's children
	/// applying the same ComponentToSave types that are already defined in this marker
	/// </summary>
	[Show, DisplayOrder(3)] void ApplyToChildren()
	{
		var children = gameObject.GetComponentsInChildren<Transform>();
		for (int i = 0; i < children.Length; i++)
		{
			var child = children[i];
			if (child == transform) continue;
			var marker = child.gameObject.GetOrAddComponent<SaveMarker>();
			marker.ComponentsToSave.Clear();
			for (int j = 0; j < ComponentsToSave.Count; j++)
			{
				var toSave = ComponentsToSave[j];
				if (child.HasComponent(toSave))
				{
					marker.ComponentsToSave.Add(toSave);
				}
			}
		}
	}

	[Show, DisplayOrder(4)] void GoToStoreManager()
	{
		Selection.activeObject = StoreManager.Current;
	}

	[MenuItem("Tools/Vexe/SaveSystem/MarkSelection &#m")]
	public static void MarkSelection()
	{
		bool markChildren = EditorUtility.DisplayDialog("Mark children", "Do you want to mark the selected object(s) children as well?", "Yes", "No");

		var selection = Selection.gameObjects;
		foreach(var go in selection)
		{
			if (go.name == typeof(Store).Name) continue;
			go.GetOrAddComponent<SaveMarker>();
			if (markChildren)
			{
				var children = go.GetComponentsInChildren<Transform>();
				for (int i = 0; i < children.Length; i++)
					children[i].GetOrAddComponent<SaveMarker>();
			}
		}
	}

	[MenuItem("Tools/Vexe/SaveSystem/UnmarkSelection &#u")]
	public static void UnmarkSelection()
	{
		bool unmarkChildren = EditorUtility.DisplayDialog("Unmark children", "Do you want to unmark the selected object(s) children as well?", "Yes", "No");

		var selection = Selection.gameObjects;
		foreach(var go in selection)
		{
			var marker = go.GetComponent<SaveMarker>();
			if (marker != null)
				marker.Destroy();

			if (unmarkChildren)
			{
				var children = go.GetComponentsInChildren<SaveMarker>();
				for (int i = 0; i < children.Length; i++)
					children[i].Destroy();
			}
		}
	}

	static SaveMarker()
	{
		EditorApplication.hierarchyWindowItemOnGUI += MarkHierarchy;
	}

	static void MarkHierarchy(int id, Rect rect)
	{
		var go = EditorUtility.InstanceIDToObject(id) as GameObject;
		if (go == null)
			return;

		var marker = go.GetComponent<SaveMarker>();
		if (marker == null)
			return;

		rect.x += rect.width;
		rect.x -= 60f;

		var color = Color.green;
		color.a *= 0.5f;

		var prev = GUI.contentColor;
		GUI.contentColor = color;
		GUI.Label(rect, "(Marked)");
		GUI.contentColor = prev;
	}
#endif
}
