namespace NShared;

public class UserTutorialManager : AccountAttrManager<int, TutorialInfo>
{
	public bool IsComplete(int tutorialIndex)
	{
		return Get(tutorialIndex) != null;
	}
}
