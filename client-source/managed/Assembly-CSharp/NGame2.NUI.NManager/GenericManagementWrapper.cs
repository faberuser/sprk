using System;
using NVespa;

namespace NGame2.NUI.NManager;

public class GenericManagementWrapper<T> : IManagementWrapper where T : BaseManagement<T>
{
	public BaseManagement<T> Management;

	public GenericManagementWrapper(BaseManagement<T> management)
	{
		Management = management;
	}

	void IManagementWrapper.CloseAll(bool immediately)
	{
		if (Management != null)
		{
			Management.CloseAllActiveWindow(immediately);
		}
	}

	void IManagementWrapper.ResetDepth()
	{
		if (Management != null)
		{
			Management.ResetDepth();
		}
	}

	void IManagementWrapper.ReorderDepth(EDepthOrder depthOrder, ref int baseDepth)
	{
		if (Management != null)
		{
			Management.ReorderDepth(depthOrder, ref baseDepth);
		}
	}

	PoolingList<PrefabWrapper> IManagementWrapper.GetOpenedList()
	{
		if (Management != null)
		{
			return Management.GetOpenedList();
		}
		return default(PoolingList<PrefabWrapper>);
	}

	bool IManagementWrapper.IsOpenedAny()
	{
		if (Management != null)
		{
			return Management.IsOpenedAny();
		}
		return false;
	}

	bool IManagementWrapper.IsPersistent()
	{
		if (Management != null)
		{
			return Management.IsPersistent();
		}
		return false;
	}

	void IManagementWrapper.OnWindowOpened(IWindow window)
	{
		if (Management != null)
		{
			Management.OnWindowOpened(window);
		}
	}

	void IManagementWrapper.MakeDirty()
	{
		if (Management != null)
		{
			Management.MakeDirty();
		}
	}

	IWindow IManagementWrapper.GetInstance(Type windowType)
	{
		if (Management != null)
		{
			return Management.GetInstanceWindow(windowType);
		}
		return null;
	}

	bool IManagementWrapper.ShowLobbyMenu()
	{
		if (Management != null)
		{
			return Management.ShowLobbyMenu();
		}
		return false;
	}
}
