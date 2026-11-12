using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree;

public interface IRuntime
{
	bool IsPlaying { get; }

	ERuntimeState RuntimeState { get; }

	IRuntime Parent { get; set; }

	TreeData Tree { get; }

	LocalVariable Variable { get; }

	bool Start();

	void Update();

	bool IsEnd();

	void RequestStop(ERequestStop request);

	void Close();

	IRuntime CaptureSelf();

	void ReleaseSelf();
}
