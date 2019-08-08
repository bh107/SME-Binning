using System;
using SME;
using SME.VHDL;

namespace SME_Binning
{
	
	public class Adder : SimpleProcess
	{
		[InputBus]
		private readonly AdderIn adderin = Scope.CreateOrLoadBus<AdderIn>();
		[InputBus]
		private BRAM0AOutPipelinedOut bram0out = Scope.CreateOrLoadBus<BRAM0AOutPipelinedOut>();

		[OutputBus]
		private readonly AdderOut output = Scope.CreateOrLoadBus<AdderOut>();

		protected override void OnTick()
		{
			output.din = bram0out.dout + adderin.dout;
		}
	}

	public class AdderInMux : SimpleProcess
	{
		[InputBus]
        private readonly BRAM1BOut bram1bout = Scope.CreateOrLoadBus<BRAM1BOut>();
		[InputBus]
        private readonly BRAM1AForwarded bram1aout = Scope.CreateOrLoadBus<BRAM1AForwarded>();
		[InputBus]
        private readonly Forward forward = Scope.CreateOrLoadBus<Forward>();

		[OutputBus]
        private readonly AdderIn adderin = Scope.CreateOrLoadBus<AdderIn>();

		protected override void OnTick()
		{
			if (forward.flg)
			{
				adderin.dout = bram1aout.dout;
			}
			else
			{
				adderin.dout = bram1bout.dout;
			}
		}
	}

	public class BRAM0 : SimulationProcess
	{
		[InputBus]
        private readonly BRAM0AIn ain = Scope.CreateOrLoadBus<BRAM0AIn>();
		[InputBus]
        private readonly BRAM0BIn bin = Scope.CreateOrLoadBus<BRAM0BIn>();

		[OutputBus]
        private readonly BRAM0AOut aout = Scope.CreateOrLoadBus<BRAM0AOut>();
		[OutputBus]
        private readonly BRAM0BOut bout = Scope.CreateOrLoadBus<BRAM0BOut>();

        [OutputBus]
        private readonly AXIBRAM0In axi0 = Scope.CreateOrLoadBus<AXIBRAM0In>();
        [OutputBus]
        private readonly AXIInput ctrl = Scope.CreateOrLoadBus<AXIInput>();
        [OutputBus]
        private readonly AXIBRAM1In axi1 = Scope.CreateOrLoadBus<AXIBRAM1In>();


        uint[] data = new uint[4096];

		public async override System.Threading.Tasks.Task Run()
		{
			while (true)
			{
				await ClockAsync();
				if (ain.ena)
				{
					if (ain.we == 0xF)
					{
						data[ain.addr] = ain.din;
					}
					aout.dout = data[ain.addr];
				}

				if (bin.ena)
				{
					if (bin.we == 0xF)
					{
						data[bin.addr] = bin.din;
					}
					bout.dout = data[bin.addr];
				}
			}
		}
	}

	public class BRAM1 : SimulationProcess
	{
		[InputBus]
        private readonly BRAM1AIn ain = Scope.CreateOrLoadBus<BRAM1AIn>();
		[InputBus]
        private readonly BRAM1BIn bin = Scope.CreateOrLoadBus<BRAM1BIn>();

		[OutputBus]
        private readonly BRAM1AOut aout = Scope.CreateOrLoadBus<BRAM1AOut>();
		[OutputBus]
        private readonly BRAM1BOut bout = Scope.CreateOrLoadBus<BRAM1BOut>();

		uint[] data = new uint[16384];

		public async override System.Threading.Tasks.Task Run()
		{
            while (true)
            {
				await ClockAsync();
				if (ain.ena)
				{
					if (ain.we == 0xF)
					{
						data[ain.addr] = ain.din;
					}
					aout.dout = data[ain.addr];
				}

				if (bin.ena)
				{
					if (bin.we == 0xF)
					{
						data[bin.addr] = bin.din;
					}
					bout.dout = data[bin.addr];
				}
			}
		}
	}

	public class BRAM0AInMux : SimpleProcess
	{
		[InputBus]
        private readonly AXIBRAM0In axi = Scope.CreateOrLoadBus<AXIBRAM0In>();
		[InputBus]
        private readonly BRAM0AInIntermediate dist = Scope.CreateOrLoadBus<BRAM0AInIntermediate>();
		[InputBus]
        private readonly AXIInput ctrl = Scope.CreateOrLoadBus<AXIInput>();

		[OutputBus]
        private readonly BRAM0AIn bram = Scope.CreateOrLoadBus<BRAM0AIn>();

		protected override void OnTick()
		{
			if (ctrl.inputrdy == 1)
			{
				bram.addr = dist.addr;
				bram.din = dist.din;
				bram.ena = dist.ena;
				bram.we = dist.we;
			}
			else
			{
				bram.addr = (UInt12)(axi.addr >> 2);
				bram.din = axi.din;
				bram.ena = axi.ena;
				bram.we = axi.we;
			}
		}
	}

	public class BRAM0AOutDecoder : SimpleProcess
	{
		[InputBus]
        private readonly BRAM0AOut bramout = Scope.CreateOrLoadBus<BRAM0AOut>();
		[InputBus]
        private readonly AXIInput ctrl = Scope.CreateOrLoadBus<AXIInput>();

		[OutputBus]
        private readonly AXIBRAM0Out axibram = Scope.CreateOrLoadBus<AXIBRAM0Out>();
		[OutputBus]
        private readonly BRAM0AOutPipelinedIn pipeline = Scope.CreateOrLoadBus<BRAM0AOutPipelinedIn>();

		protected override void OnTick()
		{
			if (ctrl.inputrdy == 1)
			{
				axibram.dout = 0;
				pipeline.dout = bramout.dout;
			}
			else
			{
				axibram.dout = bramout.dout;
				pipeline.dout = 0;
			}
		}
	}

	public class BRAM1AInMux : SimpleProcess
	{
		[InputBus]
        private readonly AXIBRAM1In axi = Scope.CreateOrLoadBus<AXIBRAM1In>();
		[InputBus]
        private readonly AdderOut adder = Scope.CreateOrLoadBus<AdderOut>();
		[InputBus]
        private readonly BRAM0BOutPipelinedOut pipe = Scope.CreateOrLoadBus<BRAM0BOutPipelinedOut>();
		[InputBus]
        private readonly OutputStep2 ctrl = Scope.CreateOrLoadBus<OutputStep2>();

		[OutputBus]
        private readonly BRAM1AInBroadcasterIn bram = Scope.CreateOrLoadBus<BRAM1AInBroadcasterIn>();

		protected override void OnTick()
		{
			if (ctrl.outputrdy == 1)
			{
				bram.addr = (UInt14)(axi.addr >> 2);
				bram.din = axi.din;
				bram.ena = axi.ena;
				bram.we = axi.we;
			}
			else
			{
				bram.addr = (UInt14)pipe.dout;
				bram.din = adder.din;
				bram.ena = true;
				bram.we = 0xF;
			}
		}
	}

	public class BRAM1AInBroadcaster : SimpleProcess
	{
		[InputBus]
        private readonly BRAM1AInBroadcasterIn input = Scope.CreateOrLoadBus<BRAM1AInBroadcasterIn>();

		[OutputBus]
        private readonly BRAM1AInBroadcasterOut output0 = Scope.CreateOrLoadBus<BRAM1AInBroadcasterOut>();
		[OutputBus]
        private readonly BRAM1AIn output1 = Scope.CreateOrLoadBus<BRAM1AIn>();

		protected override void OnTick()
		{
			output0.ena = input.ena;
			output0.addr = input.addr;
			output0.din = input.din;
			output0.we = input.we;

			output1.ena = input.ena;
			output1.addr = input.addr;
			output1.din = input.din;
			output1.we = input.we;
		}
	}

	[ClockedProcess]
	public class Distributer : SimpleProcess
	{
		[InputBus]
        private readonly AXIInput input = Scope.CreateOrLoadBus<AXIInput>();

		[OutputBus]
        private readonly OutputStep0 output = Scope.CreateOrLoadBus<OutputStep0>();
		[OutputBus]
        private readonly BRAM0AInIntermediate bramain = Scope.CreateOrLoadBus<BRAM0AInIntermediate>();
		[OutputBus]
        private readonly BRAM0BIn brambin = Scope.CreateOrLoadBus<BRAM0BIn>();

		bool active = false;
		UInt12 offset = 0;
		uint size = 0;
		byte countdown = 0;
		bool can_reset = true;

		protected override void OnTick()
		{
			output.outputrdy = (uint)(offset >= size ? 1 : 0);
			if (active)
			{
				if (offset >= size)
				{
					active = false;
					bramain.ena = false;
					brambin.ena = false;
				}
				else
				{
					bramain.addr = offset;
					bramain.ena = true;
					bramain.we = 0;
					bramain.din = 0;

					brambin.addr = (UInt12)(size + offset);
					brambin.ena = true;
					brambin.we = 0;
					brambin.din = 0;

					offset++;
				}
			}
			else
			{
				if (can_reset && input.rst == 1)
				{
					offset = 0;
					size = input.size;
					active = true;
					countdown = 1;
				}
			}
			can_reset = input.rst == 0;
		}
	}

	public class BRAM0BOutForwarder : SimpleProcess
	{
		[InputBus]
        private readonly BRAM0BOut bram0 = Scope.CreateOrLoadBus<BRAM0BOut>();

		[OutputBus]
        private readonly BRAM1BIn bram1 = Scope.CreateOrLoadBus<BRAM1BIn>();

		protected override void OnTick()
		{
			bram1.addr = (UInt14)bram0.dout;
			bram1.ena = true;
			bram1.we = 0;
			bram1.din = 0;
		}
	}

	public class BRAM1AOutForwarder : SimpleProcess
	{
		[InputBus]
        private readonly BRAM1AOut bram = Scope.CreateOrLoadBus<BRAM1AOut>();

		[OutputBus]
        private readonly AXIBRAM1Out axi = Scope.CreateOrLoadBus<AXIBRAM1Out>();

		protected override void OnTick()
		{
			axi.dout = bram.dout;
		}
	}

	[ClockedProcess]
	public class OutputPipe0 : SimpleProcess
	{
		[InputBus]
        private readonly OutputStep0 input = Scope.CreateOrLoadBus<OutputStep0>();

		[OutputBus]
        private readonly OutputStep1 output = Scope.CreateOrLoadBus<OutputStep1>();

		protected override void OnTick()
		{
			output.outputrdy = input.outputrdy;
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

	public class SignalConcat : SimpleProcess
	{
		[InputBus]
        private readonly OutputStep0 dist = Scope.CreateOrLoadBus<OutputStep0>();
		[InputBus]
        private readonly OutputStep2 pipe = Scope.CreateOrLoadBus<OutputStep2>();

		[OutputBus]
        private readonly AXIOutput axi = Scope.CreateOrLoadBus<AXIOutput>();

		protected override void OnTick()
		{
			axi.outputrdy = (dist.outputrdy << 1) | pipe.outputrdy;
		}
	}

}
