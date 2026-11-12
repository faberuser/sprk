namespace NShared.SetReviewPopup;

[JsonSerializable(Javascript = true)]
public enum AppReviewPopupType
{
	None,
	PopupOff,
	AppReview,
	End
}
