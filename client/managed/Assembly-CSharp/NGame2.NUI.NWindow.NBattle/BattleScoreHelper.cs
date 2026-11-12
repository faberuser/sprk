namespace NGame2.NUI.NWindow.NBattle;

public class BattleScoreHelper
{
	public static void UpdateScore(UILabel scoreLabel, BattleScoreContext scoreContext)
	{
		if (scoreContext != null)
		{
			scoreContext.Update();
			long score = scoreContext.GetScore();
			UpdateLabel(scoreLabel, Vespa_Long_Extension_Method.WithDamageUnit(score));
		}
	}

	private static void UpdateLabel(UILabel label, string labelText)
	{
		UIHelper.SetLabelText(label, labelText);
	}
}
