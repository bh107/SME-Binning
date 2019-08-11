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

    [InitializedBus]
    public interface BRAMCtrl : IBus
    {
        bool ena { get; set; }
        uint addr { get; set; }
        bool wrena { get; set; }
        uint wrdata { get; set; }
    }

    [InitializedBus]
    public interface BRAMResult : IBus
    {
        uint rddata { get; set; }
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
