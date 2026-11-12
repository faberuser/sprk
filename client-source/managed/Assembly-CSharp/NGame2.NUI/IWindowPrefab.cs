namespace NGame2.NUI;

public interface IWindowPrefab
{
	IWindow WindowRaw { get; }

	bool IsOpen();

	void CloseWindow();

	void ReleaseWindow();
}
