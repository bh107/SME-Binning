using System;
using SME;
using SME.VHDL;

namespace SME_Binning
{
	
	public class Adder : SimpleProcess
	{
		[InputBus]
		AdderIn adderin;
		[InputBus]
		BRAM0AOutPipelinedOut bram0out;

		[OutputBus]
		AdderOut output;

		protected override void OnTick()
		{
			output.din = bram0out.dout + adderin.dout;
		}
	}

	public class AdderInMux : SimpleProcess
	{
		[InputBus]
		BRAM1BOut bram1bout;
		[InputBus]
		BRAM1AForwarded bram1aout;
		[InputBus]
		Forward forward;

		[OutputBus]
		AdderIn adderin;

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
		BRAM0AIn ain;
		[InputBus]
		BRAM0BIn bin;

		[OutputBus]
		BRAM0AOut aout;
		[OutputBus]
		BRAM0BOut bout;

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
		BRAM1AIn ain;
		[InputBus]
		BRAM1BIn bin;

		[OutputBus]
		BRAM1AOut aout;
		[OutputBus]
		BRAM1BOut bout;

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
		AXIBRAM0In axi;
		[InputBus]
		BRAM0AInIntermediate dist;
		[InputBus]
		AXIInput ctrl;

		[OutputBus]
		BRAM0AIn bram;

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
		BRAM0AOut bramout;
		[InputBus]
		AXIInput ctrl;

		[OutputBus]
		AXIBRAM0Out axibram;
		[OutputBus]
		BRAM0AOutPipelinedIn pipeline;

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
		AXIBRAM1In axi;
		[InputBus]
		AdderOut adder;
		[InputBus]
		BRAM0BOutPipelinedOut pipe;
		[InputBus]
		AXIOutput ctrl;

		[OutputBus]
		BRAM1AIn bram;

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

	[ClockedProcess]
	public class Distributer : SimpleProcess
	{
		[InputBus]
		AXIInput input;

		[OutputBus]
		OutputStep0 output;
		[OutputBus]
		BRAM0AInIntermediate bramain;
		[OutputBus]
		BRAM0BIn brambin;

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
		BRAM0BOut bram0;

		[OutputBus]
		BRAM1BIn bram1;

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
		BRAM1AOut bram;

		[OutputBus]
		AXIBRAM1Out axi;

		protected override void OnTick()
		{
			axi.dout = bram.dout;
		}
	}

	[ClockedProcess]
	public class OutputPipe0 : SimpleProcess
	{
		[InputBus]
		OutputStep0 input;

		[OutputBus]
		OutputStep1 output;

		protected override void OnTick()
		{
			output.outputrdy = input.outputrdy;
		}
	}

	[ClockedProcess]
	public class Pipe : SimpleProcess
	{
		[InputBus]
		BRAM0AOutPipelinedIn bram0ain;
		[InputBus]
		BRAM0BOut bram0bin;
		[InputBus]
		AXIInput axiin;
		[InputBus]
		OutputStep1 axiout;
		[InputBus]
		BRAM1AIn bram1ain;

		[OutputBus]
		BRAM0AOutPipelinedOut bram0aout;
		[OutputBus]
		BRAM0BOutPipelinedOut bram0bout;
		[OutputBus]
		Forward forward;
		[OutputBus]
		BRAM1AForwarded bram1aforwarded;
		[OutputBus]
		AXIOutput output; // TODO that naming though

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
