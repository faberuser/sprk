namespace NGame2.NUI.NHelper;

public class RunePriceInfo
{
	public bool IsDiscountKeepRuneEvent { get; private set; }

	public bool IsFreeKeepRune { get; private set; }

	public int Price { get; private set; }

	public RunePriceInfo(bool isDiscountEvent, bool isFreeKeepRune, int price)
	{
		IsDiscountKeepRuneEvent = isDiscountEvent;
		IsFreeKeepRune = isFreeKeepRune;
		Price = price;
	}
}
