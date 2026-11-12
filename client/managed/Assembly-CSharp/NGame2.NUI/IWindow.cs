using NGame2.NScene.NPreloader;

namespace NGame2.NUI;

public interface IWindow
{
	EDepthOrder DepthOrder { get; }

	void ReserveCache(UiPreloader preloader);

	void SetBasePanelDepth(int depth);

	bool IsOpen();

	void Close();
}
