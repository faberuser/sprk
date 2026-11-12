namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerHeroFriendlyInfo : IAccountAttrKey<int>
{
	public int[] ConverSelectedHeroindice;

	int IAccountAttrKey<int>.Key => HeroIndex;

	public int HeroIndex { get; set; }

	public int SelectedHeroIndex { get; set; }

	public int FriendlyPoint { get; set; }

	public string LastGreetingTime { get; set; }

	public string LastConversationTime { get; set; }

	public string LastGiftTime { get; set; }

	public string SelectedTime { get; set; }

	public string SelectedHeroIndice { get; set; }

	public string LastRouletteTime { get; set; }
}
