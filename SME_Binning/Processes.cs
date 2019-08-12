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
        public Detector input;

        [OutputBus]
        public AdderResult output = Scope.CreateBus<AdderResult>();

        protected override void OnTick()
        {
            output.val = input.data + brama.rddata;
        }
    }

    public class AdderMux : SimpleProcess
    {
        [InputBus]
        public BRAMResult brama;
        [InputBus]
        public AdderResult adder;
        [InputBus]
        public Forward forward;

        [OutputBus]
        public BRAMResult output = Scope.CreateBus<BRAMResult>();

        protected override void OnTick()
        {
            output.rddata = forward.flg ? adder.val : brama.rddata;
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
            }

            if (bin.ena)
            {
                if (bin.wrena)
                {
                    data[bin.addr >> 2] = bin.wrdata;
                }
            }

            if (ain.ena)
                aout.rddata = data[ain.addr >> 2];
            if (bin.ena)
                bout.rddata = data[bin.addr >> 2];
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
        [InputBus]
        public BRAMCtrl external;

        [OutputBus]
        public BRAMCtrl output = Scope.CreateBus<BRAMCtrl>();

        protected override void OnTick()
        {
            if (dtct.valid)
            {
                output.ena = true;
                output.addr = dtct.idx << 2;
                output.wrena = true;
                output.wrdata = adderout.val;
            }
            else
            {
                output.ena = external.ena;
                output.addr = external.addr;
                output.wrena = external.wrena;
                output.wrdata = external.wrdata;
            }
        }
    }

    public class Forwarder : SimpleProcess
    {
        [InputBus]
        public Detector input;
        [InputBus]
        public Detector intermediate;

        [OutputBus]
        public Forward forward = Scope.CreateBus<Forward>();

        protected override void OnTick()
        {
            forward.flg = intermediate.valid &&
                input.idx == intermediate.idx;
        }
    }

    public class IdleChecker : SimpleProcess
    {
        [InputBus]
        public Detector input;

        [OutputBus]
        public Idle output = Scope.CreateBus<Idle>();

        protected override void OnTick() 
        {
            output.flg = !input.valid;
        }
    }

    [ClockedProcess]
    public class Pipe : SimpleProcess
    {
        [InputBus]
        public Detector input;

        [OutputBus]
        public Detector output = Scope.CreateBus<Detector>();

        protected override void OnTick()
        {
            output.valid = input.valid;
            output.idx   = input.idx;
            output.data  = input.data;
        }
    }

}
