namespace NShared;

public class BattleAreaElementData : DateElementField
{
	public int BattleAreaGroupIndex { get; set; }

	public int AreaBottomM { get; set; }

	public int AreaTopM { get; set; }

	public int AreaLeftM { get; set; }

	public int AreaRightM { get; set; }

	public int CenterBiasM { get; set; }

	public int Angle { get; set; }

	public BattleAreaElementData()
	{
	}

	public BattleAreaElementData(int groupIndex, int bottom, int top, int left, int right, int center, int angle)
	{
		BattleAreaGroupIndex = groupIndex;
		AreaBottomM = bottom;
		AreaTopM = top;
		AreaLeftM = left;
		AreaRightM = right;
		CenterBiasM = center;
		Angle = angle;
	}

	bool DateElementField.IsValid()
	{
		return BattleAreaGroupIndex > -1;
	}
}
