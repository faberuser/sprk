using LZ4s;

namespace LZ4.Services;

internal class Safe32LZ4Service : LZ4.ILZ4Service
{
	public string CodecName => "Safe 32";

	public int Encode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
	{
		return LZ4s.LZ4Codec.Encode32(input, inputOffset, inputLength, output, outputOffset, outputLength);
	}

	public int Decode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength, bool knownOutputLength)
	{
		return LZ4s.LZ4Codec.Decode32(input, inputOffset, inputLength, output, outputOffset, outputLength, knownOutputLength);
	}

	public int EncodeHC(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
	{
		return LZ4s.LZ4Codec.Encode32HC(input, inputOffset, inputLength, output, outputOffset, outputLength);
	}
}
