using System;
using SME;
using SME.VHDL;

namespace SME_Binning
{

    [InitializedBus]
    public interface AdderResult : IBus
    {
        uint val { get; set; }
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

    [InitializedBus]
    public interface Detector : IBus
    {
        bool valid { get; set; }
        uint idx { get; set; }
        uint data { get; set; }
    }

    [InitializedBus]
    public interface Forward : IBus
    {
        bool flg { get; set; }
    }

    public interface Idle : IBus
    {
        [InitialValue(true)]
        bool flg { get; set; }
    }

}
