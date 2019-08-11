﻿using System;
using SME;
using SME.VHDL;

namespace SME_Binning
{

    public class Tester : SimulationProcess
    {
        [InputBus]
        public BRAMResult bram_result;

        [OutputBus]
        public BRAMCtrl bram_ctrl = Scope.CreateBus<BRAMCtrl>();
        [OutputBus]
        public Detector output = Scope.CreateBus<Detector>();

        uint[] inputdata = {
            3, 4, 1, 6, 7, 8,
            0, 1, 1, 0, 2, 2,
        };

        uint[] outputdata = {
            9, 5, 15,
        };

        Random rand = new Random();

        bool short_test;

        public Tester(bool short_test)
        {
            this.short_test = short_test;
        }

        public async override System.Threading.Tasks.Task Run()
        {
            return;

            /*****
             *
             * Hard coded test
             *
             *****/
            /* TODO
            // Ensure that the network is waiting for input
            await ClockAsync();
            input.inputrdy = 0;
            input.size = 0;
            input.rst = 0;

            // Transfer inputdata
            for (uint i = 0; i < inputdata.Length; i++)
            {
                bram0in.addr = (UInt14)(i << 2);
                bram0in.din = inputdata[i];
                bram0in.ena = true;
                bram0in.we = 0xF;
                await ClockAsync();
            }

            // Ensure that no write comes from the outside
            bram0in.addr = 0;
            bram0in.ena = false;
            bram0in.din = 0;
            bram0in.we = 0;
            await ClockAsync();

            // Start the network
            input.inputrdy = 1;
            input.size = (uint)(inputdata.Length >> 1);
            input.rst = 1;
            await ClockAsync();
            await ClockAsync();
            await ClockAsync();

            // Wait for the network to finish
            while (output.outputrdy != 3) { Console.WriteLine("{0}", output.outputrdy); await ClockAsync(); }
            input.inputrdy = 0;

            // Verify that the output matches the precomputed output
            for (uint i = 0; i < outputdata.Length; i++)
            {
                bram1in.addr = (short)(i << 2);
                bram1in.ena = true;
                bram1in.we = 0;
                bram1in.din = 0;
                await ClockAsync();
                await ClockAsync();
                System.Diagnostics.Debug.Assert(bram1out.dout == outputdata[i], bram1out.dout + " != " + outputdata[i]);
            }
            return;
            /*****
             *
             * Continueous test, i.e. whether on not multiple inputs into same bins will work.
             *
             *****/
            /* TODO
            // Ensure that the network is waiting for input
            await ClockAsync();
            input.inputrdy = 0;
            input.size = 0;
            input.rst = 0;

            // Define additional input data
            inputdata = new uint[] {
                12, 15, 3, 5, 1,
                0,   2, 1, 0, 2,
            };
            outputdata[0] += 12 + 5;
            outputdata[1] += 3;
            outputdata[2] += 15 + 1;

            // Transfer the input data
            for (uint i = 0; i < inputdata.Length; i++)
            {
                bram0in.addr = (UInt14)(i << 2);
                bram0in.din = inputdata[i];
                bram0in.ena = true;
                bram0in.we = 0xF;
                await ClockAsync();
            }

            // Ensure that no write comes from the outside
            bram0in.addr = 0;
            bram0in.ena = false;
            bram0in.din = 0;
            bram0in.we = 0;
            await ClockAsync();

            // Signal to start the network
            input.inputrdy = 1;
            input.size = (uint)(inputdata.Length >> 1);
            input.rst = 1;
            await ClockAsync();
            await ClockAsync(); // TODO same as above
            await ClockAsync();

            // Wait for the network to finish
            while (output.outputrdy != 3) await ClockAsync();
            input.inputrdy = 0;

            // Verify that the output matches the precomputed output
            for (uint i = 0; i < outputdata.Length; i++)
            {
                bram1in.addr = (short)(i << 2);
                bram1in.ena = true;
                bram1in.we = 0;
                bram1in.din = 0;
                await ClockAsync();
                await ClockAsync();
                System.Diagnostics.Debug.Assert(bram1out.dout == outputdata[i], bram1out.dout + " != " + outputdata[i]);
            }

            if (m_shortTest)
            {
                await ClockAsync();
                Completed = true;
                return;
            }

            /*****
             *
             * Generated test
             *
             *****/
            /* TODO
            // Ensure that the network is waiting for input
            await ClockAsync();
            input.inputrdy = 0;
            input.size = 0;
            input.rst = 0;

            // Set the sizes used by the generated test
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
                bram0in.addr = (UInt14)(i << 2);
                bram0in.din = inputdata[i];
                bram0in.ena = true;
                bram0in.we = 0xF;
                await ClockAsync();
            }

            // Zero initialize output memory
            for (uint i = 0; i < outputsize; i++)
            {
                bram1in.addr = (short)(i << 2);
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
            await ClockAsync();
            await ClockAsync(); // TODO as above

            int clocks = 3;
            // Wait for network to finish
            while (output.outputrdy != 3)
            {
                await ClockAsync();
                clocks++;
            }
            input.inputrdy = 0;
            Console.WriteLine("Generated binning of {0} random values took {1} clock ticks", inputdata.Length >> 1, clocks);

            // Verify that the data in the output memory matches the precomputed results
            int errors = 0;
            for (uint i = 0; i < outputsize; i++)
            {
                bram1in.addr = (short)(i << 2);
                bram1in.ena = true;
                bram1in.din = 0;
                bram1in.we = 0;
                await ClockAsync();
                await ClockAsync();
                bool equal = bram1out.dout == outputdata[i];
                System.Diagnostics.Debug.Assert(equal, bram1out.dout + " != " + outputdata[i]);
                errors += equal ? 0 : 1;
            }

            await ClockAsync();
            Completed = true;
            */
        }
    }

}
