namespace NGame2.NEditor.NViewer;

public interface IViewer
{
	string Name { get; }

	void DrawGUI();

	void Set(object obj);
}
