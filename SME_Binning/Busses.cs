using System;
using SME;
using SME.VHDL;

namespace SME_Binning
{
	
	[InitializedBus]
	public interface AdderIn : IBus
	{
		uint dout { get; set; }
	}

	[InitializedBus]
	public interface AdderOut : IBus
	{
		uint din { get; set; }
	}

	[InitializedBus, TopLevelInputBus]
	public interface AXIBRAM0In : IBus
	{
		bool ena { get; set; }
		UInt14 addr { get; set; }
		uint din { get; set; }
		UInt4 we { get; set; }
	}

	[InitializedBus, TopLevelOutputBus]
	public interface AXIBRAM0Out : IBus
	{
		uint dout { get; set; }
	}

	[InitializedBus, TopLevelInputBus]
	public interface AXIBRAM1In : IBus
	{
		bool ena { get; set; }
		short addr { get; set; }
		uint din { get; set; }
		UInt4 we { get; set; }
	}

	[InitializedBus, TopLevelOutputBus]
	public interface AXIBRAM1Out : IBus
	{
		uint dout { get; set; }
	}

	[InitializedBus, TopLevelInputBus]
	public interface AXIInput : IBus
	{
		uint inputrdy { get; set; }
		uint size { get; set; }
		uint rst { get; set; }
	}

	[InitializedBus, TopLevelOutputBus]
	public interface AXIOutput : IBus
	{
		uint outputrdy { get; set; }
	}

	[InitializedBus, TopLevelOutputBus]
	public interface BRAM0AIn : IBus
	{
		bool ena { get; set; }
		UInt12 addr { get; set; }
		uint din { get; set; }
		UInt4 we { get; set; }
	}

	[InitializedBus, TopLevelInputBus]
	public interface BRAM0AOut : IBus
	{
		uint dout { get; set; }
	}

	[InitializedBus, TopLevelOutputBus]
	public interface BRAM0BIn : IBus
	{
		bool ena { get; set; }
		UInt12 addr { get; set; }
		uint din { get; set; }
		UInt4 we { get; set; }
	}

	[InitializedBus]
	public interface BRAM0AInIntermediate : IBus
	{
		bool ena { get; set; }
		UInt12 addr { get; set; }
		uint din { get; set; }
		UInt4 we { get; set; }
	}

	[InitializedBus]
	public interface BRAM0AOutIntermediate : IBus
	{
		uint dout { get; set; }
	}

	[InitializedBus]
	public interface BRAM0AOutPipelinedIn : IBus
	{
		uint dout { get; set; }
	}

	[InitializedBus]
	public interface BRAM0AOutPipelinedOut : IBus
	{
		uint dout { get; set; }
	}

	[InitializedBus, TopLevelInputBus]
	public interface BRAM0BOut : IBus
	{
		uint dout { get; set; }
	}

	[InitializedBus]
	public interface BRAM0BOutPipelinedOut : IBus
	{
		uint dout { get; set; }
	}

	[InitializedBus, TopLevelOutputBus]
	public interface BRAM1AIn : IBus
	{
		bool ena { get; set; }
		UInt14 addr { get; set; }
		uint din { get; set; }
		UInt4 we { get; set; }
	}

	[InitializedBus]
	public interface BRAM1AInBroadcasterIn : IBus
	{
		bool ena { get; set; }
		UInt14 addr { get; set; }
		uint din { get; set; }
		UInt4 we { get; set; }
	}

	[InitializedBus]
	public interface BRAM1AInBroadcasterOut : IBus
	{
		bool ena { get; set; }
		UInt14 addr { get; set; }
		uint din { get; set; }
		UInt4 we { get; set; }
	}

	[InitializedBus]
	public interface BRAM1AForwarded : IBus
	{
		uint dout { get; set; }
	}

	[InitializedBus, TopLevelInputBus]
	public interface BRAM1AOut : IBus
	{
		uint dout { get; set; }
	}

	[InitializedBus, TopLevelOutputBus]
	public interface BRAM1BIn : IBus
	{
		bool ena { get; set; }
		UInt14 addr { get; set; }
		uint din { get; set; }
		UInt4 we { get; set; }
	}

	[InitializedBus, TopLevelInputBus]
	public interface BRAM1BOut : IBus
	{
		uint dout { get; set; }
	}

	[InitializedBus]
	public interface Forward : IBus
	{
		bool flg { get; set; }
	}

	[InitializedBus]
	public interface OutputStep0 : IBus
	{
		uint outputrdy { get; set; }
	}

	[InitializedBus]
	public interface OutputStep1 : IBus
	{
		uint outputrdy { get; set; }
	}

	[InitializedBus]
	public interface OutputStep2 : IBus
	{
		uint outputrdy { get; set; }
	}

}
