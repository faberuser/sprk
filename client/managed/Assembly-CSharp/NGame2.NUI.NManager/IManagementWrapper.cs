using System;
using NVespa;

namespace NGame2.NUI.NManager;

public interface IManagementWrapper
{
	void CloseAll(bool immediately);

	void ResetDepth();

	void ReorderDepth(EDepthOrder depthOrder, ref int baseDepth);

	PoolingList<PrefabWrapper> GetOpenedList();

	bool IsOpenedAny();

	bool IsPersistent();

	void OnWindowOpened(IWindow window);

	IWindow GetInstance(Type windowType);

	void MakeDirty();

	bool ShowLobbyMenu();
}
