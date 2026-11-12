using UnityEngine;

namespace NGame2.NUI.NWindow;

public class AlignerPrice : MonoBehaviour
{
	public enum PriceType
	{
		Gold,
		Gem
	}

	public UILabel BeforePriceLabel;

	public UILabel AfterPriceLabel;

	public UILabelAligner PriceAlignerLabel;

	public Transform GoldSprTransform;

	public Transform GemSprTransform;

	public void SetPrice(PriceType type, int totalPrice, int totalBonusPrice)
	{
		SetPriceType(type);
		bool num = totalPrice < totalBonusPrice;
		EnableAfterPriceLabel(enable: true);
		if (num)
		{
			EnableBeforePriceLabel(enable: true);
			SetBeforePrice(totalPrice);
			SetAfterPrice(totalBonusPrice);
		}
		else
		{
			SetAfterPrice(totalPrice);
			EnableBeforePriceLabel(enable: false);
		}
		Aligner();
	}

	private void SetPriceType(PriceType type)
	{
		AllUnalbePriceType();
		switch (type)
		{
		case PriceType.Gold:
			EnableGoldPriceType(enable: true);
			break;
		case PriceType.Gem:
			EnableGemPriceType(enable: true);
			break;
		}
	}

	private void AllUnalbePriceType()
	{
		EnableGoldPriceType(enable: false);
		EnableGemPriceType(enable: false);
	}

	private void EnableGoldPriceType(bool enable)
	{
		if (!(GoldSprTransform == null))
		{
			UIHelper.EnableGameObject(GoldSprTransform, enable);
		}
	}

	private void EnableGemPriceType(bool enable)
	{
		if (!(GemSprTransform == null))
		{
			UIHelper.EnableGameObject(GemSprTransform, enable);
		}
	}

	private void SetBeforePrice(int price)
	{
		if (!(BeforePriceLabel == null))
		{
			UIHelper.SetLabelText(BeforePriceLabel, Vespa_Int_Extension_Method.ToCommaString(price));
		}
	}

	private void SetAfterPrice(int price)
	{
		if (!(AfterPriceLabel == null))
		{
			UIHelper.SetLabelText(AfterPriceLabel, Vespa_Int_Extension_Method.ToCommaString(price));
		}
	}

	private void EnableBeforePriceLabel(bool enable)
	{
		if (!(BeforePriceLabel == null))
		{
			UIHelper.EnableGameObject(BeforePriceLabel, enable);
		}
	}

	private void EnableAfterPriceLabel(bool enable)
	{
		if (!(AfterPriceLabel == null))
		{
			UIHelper.EnableGameObject(AfterPriceLabel, enable);
		}
	}

	private void Aligner()
	{
		if (PriceAlignerLabel != null)
		{
			PriceAlignerLabel.Align();
		}
	}
}
