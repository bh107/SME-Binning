using System;
using SME;
using SME.VHDL;

namespace SME_Binning
{
	
	public class FullTester : SimulationProcess
	{
		[InputBus]
		AXIOutput output;
		[InputBus]
		AXIBRAM0Out bram0out;
		[InputBus]
		AXIBRAM1Out bram1out;

		[OutputBus]
		AXIInput input;
		[OutputBus]
		AXIBRAM0In bram0in;
		[OutputBus]
		AXIBRAM1In bram1in;

		uint inputrdy = 0;
		uint outputrdy = 0;
		uint size = 0;
		uint rst = 0;

		uint[] inputdata = {
			3, 4, 1, 6, 7, 8,
			0, 1, 1, 0, 2, 2,
		};

		uint[] outputdata = {
			9, 5, 15,
		};

		Random rand = new Random();

		public async override System.Threading.Tasks.Task Run()
		{
			// Hard coded test
			await ClockAsync();
			input.inputrdy = inputrdy;
			input.size = size;
			input.rst = size;

			for (uint i = 0; i < inputdata.Length; i++)
			{
				bram0in.addr = (UInt12)i;
				bram0in.din = inputdata[i];
				bram0in.ena = true;
				bram0in.we = 0xF;
				await ClockAsync();
			}

			bram0in.addr = 0;
			bram0in.ena = false;
			bram0in.din = 0;
			bram0in.we = 0;
			await ClockAsync();
			input.inputrdy = 1;
			input.size = (uint)(inputdata.Length >> 1);
			input.rst = 1;
			await ClockAsync();
			input.rst = 0;
			await ClockAsync();
			await ClockAsync();

			while (output.outputrdy == 0) await ClockAsync();

			for (uint i = 0; i < outputdata.Length; i++)
			{
				bram1in.addr = (UInt14)i;
				bram1in.ena = true;
				bram1in.we = 0;
				bram1in.din = 0;
				await ClockAsync();
				await ClockAsync();
				System.Diagnostics.Debug.Assert(bram1out.dout == outputdata[i], bram1out.dout + " != " + outputdata[i]);
			}

			// Generated test
			int inputsize = 2304; // 9kb / 4
			int inputmid = inputsize >> 1;
			int outputsize = 10000;
			inputdata = new uint[inputsize];
			outputdata = new uint[outputsize];

			// Generate input
			for (int i = 0; i < inputsize; i++)
			{
				if (i < inputmid)
					inputdata[i] = (uint)rand.Next(1000);
				else
					inputdata[i] = (uint)rand.Next(outputsize);
			}

			// Generate output
			for (int i = 0; i < inputmid; i++)
			{
				outputdata[inputdata[i + inputmid]] += inputdata[i];
			}

			// Ensure network is not running
			input.inputrdy = 0;
			input.rst = 0;
			input.size = 0;
			await ClockAsync();

			// Transfer inputdata to input memory
			for (uint i = 0; i < inputsize; i++)
			{
				bram0in.addr = (UInt12)i;
				bram0in.din = inputdata[i];
				bram0in.ena = true;
				bram0in.we = 0xF;
				await ClockAsync();
			}

			// Zero initialize output memory
			for (uint i = 0; i < outputsize; i++)
			{
				bram1in.addr = (UInt14)i;
				bram1in.din = 0;
				bram1in.ena = true;
				bram1in.we = 0xF;
				await ClockAsync();
			}
			bram1in.ena = false;
			bram1in.we = 0;

			// Initialize and start network
			bram0in.addr = 0;
			bram0in.ena = false;
			bram0in.din = 0;
			bram0in.we = 0;
			await ClockAsync();
			input.inputrdy = 1;
			input.size = (uint)inputmid;
			input.rst = 1;
			await ClockAsync();
			input.rst = 0;
			await ClockAsync();
			await ClockAsync();

			int clocks = 3;
			// Wait for network to finish
			while (output.outputrdy == 0)
			{
				await ClockAsync();
				clocks++;
			}
			Console.WriteLine("Binning took {0} clock ticks", clocks);

			// Verify that the data in the output memory matches the precomputed results
			int errors = 0;
			for (uint i = 0; i < outputsize; i++)
			{
				bram1in.addr = (UInt14)i;
				bram1in.ena = true;
				bram1in.din = 0;
				bram1in.we = 0;
				await ClockAsync();
				await ClockAsync();
				bool equal = bram1out.dout == outputdata[i];
				System.Diagnostics.Debug.Assert(equal, bram1out.dout + " != " + outputdata[i]);
				errors += equal ? 0 : 1;
			}
		}
	}

}
