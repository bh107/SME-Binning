﻿using System;
using SME;
using SME.VHDL;

namespace SME_Binning
{

    public class Tester : SimulationProcess
    {
        [InputBus]
        public BRAMResult bram_result;
        [InputBus]
        public Idle idle;

        [OutputBus]
        public BRAMCtrl bram_ctrl = Scope.CreateBus<BRAMCtrl>();
        [OutputBus]
        public Detector output = Scope.CreateBus<Detector>();

        int mem_size;
        Random rand = new Random();
        bool short_test;

        public Tester(bool short_test, int mem_size)
        {
            this.short_test = short_test;
            this.mem_size = mem_size;
        }

        private async System.Threading.Tasks.Task MemRead(uint addr)
        {
            bram_ctrl.ena = true;
            bram_ctrl.addr = addr << 2;
            bram_ctrl.wrena = false;
            bram_ctrl.wrdata = 0;
            await ClockAsync();
            // Ensure no latching
            bram_ctrl.ena = false;
            bram_ctrl.addr = 0;
            bram_ctrl.wrena = false;
            bram_ctrl.wrdata = 0;
        }

        private async System.Threading.Tasks.Task MemWrite(uint addr, uint data)
        {
            bram_ctrl.ena = true;
            bram_ctrl.addr = addr << 2;
            bram_ctrl.wrena = true;
            bram_ctrl.wrdata = data;
            await ClockAsync();
            // Ensure no latching
            bram_ctrl.ena = false;
            bram_ctrl.addr = 0;
            bram_ctrl.wrena = false;
            bram_ctrl.wrdata = 0;
        }

        private async System.Threading.Tasks.Task Test(bool reset, uint[] input_idxs, uint[] input_data, uint[] output_data)
        {
            // Ensure that memory is initialised as 0
            if (reset)
                for (uint i = 0; i < mem_size; i++)
                    await MemWrite(i, 0);

            // Transfer inputdata
            for (uint i = 0; i < input_idxs.Length; i++)
                await ToBinning(input_idxs[i], input_data[i]);

            // Wait until binning is idle
            while (!idle.flg)
                await ClockAsync();

            // Verify that the result matches the expected output
            await MemRead(0);
            for (uint i = 1; i <= output_data.Length; i++)
            {
                await MemRead((uint)(i % mem_size));
                System.Diagnostics.Debug.Assert(
                    bram_result.rddata == output_data[i-1],
                    $"Error on index {i-1}: Expected {output_data[i-1]}, got {bram_result.rddata}");
            }
        }

        private async System.Threading.Tasks.Task ToBinning(uint idx, uint data)
        {
            output.valid = true;
            output.idx = idx;
            output.data = data;
            await ClockAsync();
            // Ensure no latching
            output.valid = false;
            output.idx = 0;
            output.data = 0;
        }

        public async override System.Threading.Tasks.Task Run()
        {
            // Ensure that the network is waiting for input
            await ClockAsync();

            /*****
             *
             * Hard coded test
             *
             *****/
            uint[] input_idxs  = { 0, 1, 1, 0, 2, 2 };
            uint[] input_data  = { 3, 4, 1, 6, 7, 8 };
            uint[] output_data = { 9, 5, 15 };
            await Test(true, input_idxs, input_data, output_data);

            /*****
             *
             * Continueous test
             * Tests whether on not multiple inputs into same bins without resetting will work.
             *
             *****/
            input_idxs = new uint[] {  0,  2, 1, 0, 2 };
            input_data = new uint[] { 12, 15, 3, 5, 1 };
            for (int i = 0; i < input_data.Length; i++)
                output_data[input_idxs[i]] += input_data[i];
            await Test(false, input_idxs, input_data, output_data);

            /*****
             *
             * Test to capture error, which occurs when there is a gap between two
             * of the same indices
             *
             */
            input_idxs = new uint[] { 0, 1, 0, 1 };
            input_data = new uint[] { 1, 2, 3, 4 };
            output_data = new uint[] { 4, 6 };
            await Test(true, input_idxs, input_data, output_data);

            /*****
             *
             * Generated test - short
             *
             *****/
            int short_test_length = 1000;
            input_idxs  = new uint[short_test_length];
            input_data  = new uint[short_test_length];
            output_data = new uint[mem_size];
            for (int i = 0; i < short_test_length; i++)
            {
                input_idxs[i] = (uint)rand.Next(mem_size);
                input_data[i] = (uint)rand.Next(10);
                output_data[input_idxs[i]] += input_data[i];
            }
            await Test(true, input_idxs, input_data, output_data);

            if (short_test)
                return;
            /*****
             *
             * Generated test - long
             *
             */
            int long_test_length = 100 * mem_size;
            input_idxs  = new uint[long_test_length];
            input_data  = new uint[long_test_length];
            output_data = new uint[mem_size];
            for (int i = 0; i < long_test_length; i++)
            {
                input_idxs[i] = (uint)rand.Next(mem_size);
                input_data[i] = (uint)rand.Next();
                output_data[input_idxs[i]] += input_data[i];
            }
            await Test(true, input_idxs, input_data, output_data);
        }
    }

}
