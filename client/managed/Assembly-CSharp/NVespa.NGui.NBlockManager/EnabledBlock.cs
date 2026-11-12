using System;
using UnityEngine;

namespace NVespa.NGui.NBlockManager;

public class EnabledBlock : IDisposable
{
	private bool _backup;

	public bool enable
	{
		get
		{
			return GUI.enabled;
		}
		set
		{
			GUI.enabled = value;
		}
	}

	public EnabledBlock Open()
	{
		_backup = GUI.enabled;
		return this;
	}

	public EnabledBlock Open(bool i_enable)
	{
		_backup = GUI.enabled;
		GUI.enabled = i_enable;
		return this;
	}

	public virtual void Dispose()
	{
		GUI.enabled = _backup;
	}
}
