using System;
using UnityEngine;

namespace NVespa.NGui.NBlockManager;

public class LayoutSpace : IDisposable
{
	public LayoutSpace Open(float space)
	{
		GUILayout.Space(space);
		return this;
	}

	public virtual void Dispose()
	{
	}
}
