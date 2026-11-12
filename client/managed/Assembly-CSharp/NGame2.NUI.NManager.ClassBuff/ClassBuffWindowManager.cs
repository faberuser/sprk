using NGame2.NUI.NWindow.ClassBuff;
using NShared;

namespace NGame2.NUI.NManager.ClassBuff;

public class ClassBuffWindowManager : ClassBuffMainInfo.IListener, ClassBuffMainInfoList.IListener, ClassBuffLevelUpPopup.IListener, IClassBuffListener
{
	private ClassBuffManagement _management;

	public void Init(ClassBuffManagement management)
	{
		_management = management;
	}

	public void OpenClassBuffMain(CreatureTagType creatureType)
	{
		if (!(_management == null))
		{
			ClassBuffMain instance = _management.GetInstance<ClassBuffMain>();
			if (!(instance == null))
			{
				VespaCoroutine.Start(instance.Open(creatureType));
			}
		}
	}

	private void OpenClassBuffLevelUpPopup(ClassBuffData data)
	{
		if (!(_management == null))
		{
			ClassBuffLevelUpPopup instance = _management.GetInstance<ClassBuffLevelUpPopup>();
			if (!(instance == null))
			{
				instance.Open(this, data);
			}
		}
	}

	public void PlayLevelUpEffect(int levelUpIndex)
	{
		if (_management == null)
		{
			return;
		}
		ClassBuffMain instance = _management.GetInstance<ClassBuffMain>();
		if (!(instance == null))
		{
			ClassBuffMainInfoList mainInfoList = instance.MainInfoList;
			if (!(mainInfoList == null))
			{
				mainInfoList.PlayLevelUpEffect(levelUpIndex);
			}
		}
	}

	public void ResetClassBuffMain()
	{
		if (!(_management == null))
		{
			ClassBuffMain instance = _management.GetInstance<ClassBuffMain>();
			if (!(instance == null))
			{
				instance.SetClassBuffMainInfo();
			}
		}
	}

	void ClassBuffMainInfo.IListener.Reset(CreatureTagType typeCategory)
	{
		_management.ResetClassBuff(typeCategory);
	}

	void ClassBuffMainInfoList.IListener.OpenLevelUpPopup(ClassBuffData data)
	{
		OpenClassBuffLevelUpPopup(data);
	}

	void ClassBuffLevelUpPopup.IListener.LevelUp(ClassBuffData data)
	{
		_management.LevelUpClassBuff(data);
	}

	void IClassBuffListener.AddPoint()
	{
		_management.AddClassBuffPoint();
	}

	void IClassBuffListener.AddPoint(ItemData itemData)
	{
		_management.AddClassBuffPoint(itemData);
	}

	void IClassBuffListener.AddPoint(HeroExpResultInfo info)
	{
		_management.AddClassBuffPoint(info);
	}
}
