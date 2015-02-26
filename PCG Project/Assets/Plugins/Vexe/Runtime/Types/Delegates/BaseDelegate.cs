﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Vexe.Runtime.Types
{
	public abstract class BaseDelegate
	{
		[Serialize]
		public List<Handler> handlers = new List<Handler>();

		public abstract Type[] ParamTypes { get; }
		public abstract Type ReturnType { get; }

		public class Handler
		{
			public UnityObject target;
			public string method;
		}
	}
}