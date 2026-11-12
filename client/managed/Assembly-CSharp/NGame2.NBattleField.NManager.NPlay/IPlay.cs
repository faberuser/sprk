using System;

namespace NGame2.NBattleField.NManager.NPlay;

public interface IPlay
{
	void Setup(IDataProvider dataProvider, Action onReadyBattle);
}
