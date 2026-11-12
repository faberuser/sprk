using System.Collections;
using System.Linq;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class CloseAllWindow : IActivity
{
	private string[] _managements;

	public static IActivity Create(string[] values)
	{
		return new CloseAllWindow(values);
	}

	public CloseAllWindow(string[] managements)
	{
		_managements = managements;
	}

	IEnumerator IActivity.coStart()
	{
		for (int index = 0; index < 3; index++)
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.ForEach(delegate(IManagementWrapper managementWrapper)
			{
				if (!managementWrapper.IsPersistent())
				{
					string name = managementWrapper.GetType().Name;
					if (_managements == null || _managements.Length == 0 || _managements.Contains(name))
					{
						managementWrapper.CloseAll(immediately: false);
					}
				}
			});
			yield return null;
		}
	}

	void IActivity.OnRequestStop()
	{
	}
}
