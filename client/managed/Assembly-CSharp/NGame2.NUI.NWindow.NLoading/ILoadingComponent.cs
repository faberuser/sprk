using NShared;

namespace NGame2.NUI.NWindow.NLoading;

public interface ILoadingComponent
{
	void Open(LoadingData loadingData);

	bool IsCompleted();

	UIWidget GetAnchorWidget(EAnchorPivot pivot);
}
