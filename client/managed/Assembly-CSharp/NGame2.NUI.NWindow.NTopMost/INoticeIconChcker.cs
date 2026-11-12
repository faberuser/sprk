using NVespa.NNGUIExtension;

namespace NGame2.NUI.NWindow.NTopMost;

public interface INoticeIconChcker
{
	void Init(IGrid grid);

	bool Check(bool canShow);

	void Close();

	void CheckContentsStatus();
}
