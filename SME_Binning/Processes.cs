using System;
using SME;
using SME.VHDL;

namespace SME_Binning
{

    [ClockedProcess]
    public class Adder : SimpleProcess
    {
        [InputBus]
        public BRAMResult brama;
        [InputBus]
        public BRAMResult bramb;

        [OutputBus]
        public AdderResult output = Scope.CreateBus<AdderResult>();

        protected override void OnTick()
        {
            output.val = brama.rddata + bramb.rddata;
        }
    }

    public class AdderMux : SimpleProcess
    {
        [InputBus]
        public BRAMResult brama;
        [InputBus]
        public BRAMResult bramb;
        [InputBus]
        public Forward forward;

        [OutputBus]
        public BRAMResult output = Scope.CreateBus<BRAMResult>();

        protected override void OnTick()
        {
            output.rddata = forward.flg ? bramb.rddata : brama.rddata;
        }
    }

    [ClockedProcess]
    public class BRAM : SimpleProcess
    {
        public BRAM(int size)
        {
            data = new uint[size];
        }

        [InputBus]
        public BRAMCtrl ain;
        [InputBus]
        public BRAMCtrl bin;

        [OutputBus]
        public BRAMResult aout = Scope.CreateBus<BRAMResult>();
        [OutputBus]
        public BRAMResult bout = Scope.CreateBus<BRAMResult>();

        uint[] data;

        protected override void OnTick()
        {
            if (ain.ena)
            {
                if (ain.wrena)
                {
                    data[ain.addr >> 2] = ain.wrdata;
                }
                aout.rddata = data[ain.addr >> 2];
            }

            if (bin.ena)
            {
                if (bin.wrena)
                {
                    data[bin.addr >> 2] = bin.wrdata;
                }
                bout.rddata = data[bin.addr >> 2];
            }
        }
    }

    public class BRAMPortAPacker : SimpleProcess
    {
        [InputBus]
        public Detector input;

        [OutputBus]
        public BRAMCtrl output = Scope.CreateBus<BRAMCtrl>();

        protected override void OnTick()
        {
            output.ena = input.valid;
            output.addr = input.idx << 2;
            output.wrena = false;
            output.wrdata = 0;
        }
    }

    public class BRAMPortBPacker : SimpleProcess
    {
        [InputBus]
        public Detector dtct;
        [InputBus]
        public AdderResult adderout;

        [OutputBus]
        public BRAMCtrl output = Scope.CreateBus<BRAMCtrl>();

        protected override void OnTick()
        {
            output.ena = dtct.valid;
            output.addr = dtct.idx << 2;
            output.wrena = dtct.valid;
            output.wrdata = adderout.val;
        }
    }

    public class Forwarder : SimpleProcess
    {
        [InputBus]
        public Detector input;
        [InputBus]
        public Detector intermediate;

        [OutputBus]
        public Forward forward;

        protected override void OnTick()
        {
            forward.flg = intermediate.valid &&
                input.idx == intermediate.idx;
        }
    }

    [ClockedProcess]
    public class Pipe : SimpleProcess
    {
        [InputBus]
        public Detector dtctin;

        [OutputBus]
        public Detector dtctout = Scope.CreateBus<Detector>();

        protected override void OnTick()
        {
            dtctout.valid = dtctin.valid;
            dtctout.idx = dtctin.idx;
            dtctout.data = dtctin.data;
        }
    }

}
