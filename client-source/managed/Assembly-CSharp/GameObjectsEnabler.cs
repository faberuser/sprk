using NGame2.NUI;
using UnityEngine;

public class GameObjectsEnabler : MonoBehaviour
{
	public GameObject[] GameObjectList;

	public void SetEnableAll(bool enable)
	{
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(GameObjectList); i < num; i++)
		{
			UIHelper.EnableGameObject(GameObjectList[i], enable);
		}
	}
}
