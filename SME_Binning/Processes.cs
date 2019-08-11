using System;
using SME;
using SME.VHDL;

namespace SME_Binning
{

    [ClockedProcess]
    public class Adder : SimpleProcess
    {
        [InputBus]
        public BRAMResult bram0;
        [InputBus]
        public BRAMResult bram1;

        [OutputBus]
        public AdderResult output = Scope.CreateBus<AdderResult>();

        protected override void OnTick()
        {
            output.val = bram0.rddata + bram1.rddata;
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
        public BRAMResult adderin = Scope.CreateBus<BRAMResult>();

        protected override void OnTick()
        {
            adderin.rddata = forward.flg ? bramb.rddata : brama.rddata;
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

        public async override System.Threading.Tasks.Task Run()
        {
            if (ain.ena)
            {
                if (ain.we)
                {
                    data[ain.addr >> 2] = ain.din;
                }
                aout.dout = data[ain.addr >> 2];
            }

            if (bin.ena)
            {
                if (bin.we)
                {
                    data[bin.addr >> 2] = bin.din;
                }
                bout.dout = data[bin.addr >> 2];
            }
        }
    }

    public class Forwarder : SimpleProcess
    {
        [InputBus]
        public BRAMCtrl brama;
        [InputBus]
        public BRAMCtrl bramb;

        [OutputBus]
        public Forward forward;

        protected override void OnTick()
        {
            forward.flg = brama.ena &&
                brama.addr == bramb.addr &&
                bramb.ena &&
                bramb.wrena;
        }
    }

    [ClockedProcess]
    public class Pipe : SimpleProcess
    {
        [InputBus]
        private readonly BRAM0AOutPipelinedIn bram0ain = Scope.CreateOrLoadBus<BRAM0AOutPipelinedIn>();
        [InputBus]
        private readonly BRAM0BOut bram0bin = Scope.CreateOrLoadBus<BRAM0BOut>();
        [InputBus]
        private readonly AXIInput axiin = Scope.CreateOrLoadBus<AXIInput>();
        [InputBus]
        private readonly OutputStep1 axiout = Scope.CreateOrLoadBus<OutputStep1>();
        [InputBus]
        private readonly BRAM1AInBroadcasterOut bram1ain = Scope.CreateOrLoadBus<BRAM1AInBroadcasterOut>();

        [OutputBus]
        private readonly BRAM0AOutPipelinedOut bram0aout = Scope.CreateOrLoadBus<BRAM0AOutPipelinedOut>();
        [OutputBus]
        private readonly BRAM0BOutPipelinedOut bram0bout = Scope.CreateOrLoadBus<BRAM0BOutPipelinedOut>();
        [OutputBus]
        private readonly Forward forward = Scope.CreateOrLoadBus<Forward>();
        [OutputBus]
        private readonly BRAM1AForwarded bram1aforwarded = Scope.CreateOrLoadBus<BRAM1AForwarded>();
        [OutputBus]
        private readonly OutputStep2 output = Scope.CreateOrLoadBus<OutputStep2>(); // TODO that naming though

        protected override void OnTick()
        {
            if (axiout.outputrdy == 0) // TODO vivado/MAXIVBinningAXIZynq/bd4 simulation forces.txt
            {
                bram0aout.dout = bram0ain.dout;
                bram0bout.dout = bram0bin.dout;
                forward.flg = bram1ain.we == 0xF && bram1ain.addr == bram0bin.dout;
                bram1aforwarded.dout = bram1ain.din;
            }
            else
            {
                bram0aout.dout = 0;
                bram0bout.dout = 0;
                forward.flg = true;
                bram1aforwarded.dout = 0;
            }
            output.outputrdy = axiout.outputrdy;
        }
    }

}
