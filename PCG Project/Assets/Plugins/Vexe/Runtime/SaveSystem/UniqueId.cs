#define DBG

using System;
using UnityEngine;
using Vexe.Runtime.Types;

[ExecuteInEditMode, MinimalView, DisallowMultipleComponent]
public class UniqueId : BetterBehaviour
{
	[Serialize, Hide]
	private string _id;

	[Show, Readonly]
	public string Id
	{
		set { _id = value; }
		get
		{
			if (string.IsNullOrEmpty(_id))
			{
#if DBG
				var prev = _id;
#endif
				_id = Guid.NewGuid().ToString();
#if DBG
				Log("{0} New id: {1} prev: {2}", name, _id, prev);
#endif
			}
			return _id;
		}
	}
}
