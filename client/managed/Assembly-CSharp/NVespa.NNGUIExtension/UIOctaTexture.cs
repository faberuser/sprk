namespace NVespa.NNGUIExtension;

public class UIOctaTexture : UITexture
{
	protected override void Awake()
	{
		base.Awake();
		onPostFill = UIOctaSprite.ConvertToOcta;
	}
}
