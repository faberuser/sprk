using NGameAsset.NCreature;
using NVespa;

namespace NGame2.NRender.NRenderFacade;

public class OnEnableVisitor : IGenericVisitor<SelfEffectInstance>, IVisitor
{
	public static OnEnableVisitor Default = new OnEnableVisitor();

	void IGenericVisitor<SelfEffectInstance>.Visit(SelfEffectInstance selfEffect)
	{
		selfEffect.Close();
	}
}
