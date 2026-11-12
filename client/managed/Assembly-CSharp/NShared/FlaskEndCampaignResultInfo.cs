namespace NShared;

[JsonSerializable(Javascript = true)]
public class FlaskEndCampaignResultInfo
{
	public int FilledFlaskIndex { get; set; }

	public int FilledFlaskCount { get; set; }

	public FlaskResultInfo Info { get; set; }
}
