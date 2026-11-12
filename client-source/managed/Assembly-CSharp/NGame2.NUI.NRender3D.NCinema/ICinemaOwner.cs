using System;

namespace NGame2.NUI.NRender3D.NCinema;

public interface ICinemaOwner
{
	ECinemaOwnerType OwnerType { get; set; }

	IDataProvider DataProvider { get; }

	bool HasNextNodeTree { get; set; }

	bool IsNextOncePlay { get; }

	Cinema Cinema { get; set; }

	Action OnStart { get; }

	Action OnEnd { get; }

	void SetState(ICinemaState state);

	void SetRenderTexture();

	void NoticeError(string errorMessage);

	void SetNextNodeTree(bool isOncePlay, string nodeTreePack, string nodeTreeName, Action onStart, Action onEnd);

	void SetAction(Action onStart, Action onEnd);
}
