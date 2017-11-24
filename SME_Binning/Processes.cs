using System;
using SME;
using SME.VHDL;

namespace SME_Binning
{
	
	public class Adder : SimpleProcess
	{
		[InputBus]
		BRAM1BOut bram1out;
		[InputBus]
		BRAM0AOutPipelinedOut bram0out;

		[OutputBus]
		AdderOut output;

		protected override void OnTick()
		{
			output.din = bram0out.dout + bram1out.dout;
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
				else
				{
					aout.dout = 0;
				}

				if (bin.ena)
				{
					if (bin.we == 0xF)
					{
						data[bin.addr] = bin.din;
					}
					bout.dout = data[bin.addr];
				}
				else
				{
					bout.dout = 0;
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
				else
				{
					aout.dout = 0;
				}

				if (bin.ena)
				{
					if (bin.we == 0xF)
					{
						data[bin.addr] = bin.din;
					}
					bout.dout = data[bin.addr];
				}
				else
				{
					bout.dout = 0;
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
				bram.addr = axi.addr;
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
				bram.addr = axi.addr;
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
		AXIOutput output;
		[OutputBus]
		BRAM0AInIntermediate bramain;
		[OutputBus]
		BRAM0BIn brambin;

		bool active = false;
		UInt12 offset = 0;
		uint size = 0;
		byte countdown = 0;

		protected override void OnTick()
		{
			if (active)
			{
				if (offset == size)
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
				if (input.rst == 1)
				{
					offset = 0;
					size = input.size;
					active = true;
					countdown = 1;
					output.outputrdy = 0;
				}
				else if (countdown == 0)
				{
					output.outputrdy = 1;
				}
				else
				{
					output.outputrdy = 0;
					countdown--;
				}
			}
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
	public class Pipe : SimpleProcess
	{
		[InputBus]
		BRAM0AOutPipelinedIn bram0ain;
		[InputBus]
		BRAM0BOut bram0bin;
		[InputBus]
		AXIInput axiin;
		[InputBus]
		AXIOutput axiout;

		[OutputBus]
		BRAM0AOutPipelinedOut bram0aout;
		[OutputBus]
		BRAM0BOutPipelinedOut bram0bout;

		protected override void OnTick()
		{
			if (axiin.inputrdy == 1 || axiout.outputrdy == 0)
			{
				bram0aout.dout = bram0ain.dout;
				bram0bout.dout = bram0bin.dout;
			}
			else
			{
				bram0aout.dout = 0;
				bram0bout.dout = 0;
			}
		}
	}

}
