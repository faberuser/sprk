using System;

namespace NGame2.NGuild;

public interface IGuildPenalty
{
	bool OnPenalty();

	DateTime EndTime();

	GuildPenaltyType GetPenaltyType();
}
