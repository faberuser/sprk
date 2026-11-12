namespace NVespa.NNGUIExtension;

public interface IUIAlphaGradation
{
	float AllClipAmount { get; set; }

	float TopClipAmount { get; set; }

	float BottomClipAmount { get; set; }

	float LeftClipAmount { get; set; }

	float RightClipAmount { get; set; }
}
