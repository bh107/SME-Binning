using System;
using SME;
using SME.VHDL;

namespace SME_Binning
{

    class MainClass
    {
        public static void Main(string[] args)
        {
            using(var sim = new Simulation())
            {
                var adder = new Adder();
                var bram = new BRAM();
                var bram_porta = new BRAMPortAPacker();
                var bram_portb = new BRAMPortBPacker();
                var forward = new Forwarder();
                var mux = new AdderMux();
                var input_pipe = new Pipe();
                var intermediate_pipe = new Pipe();
                var tester = new Tester();

                adder.brama = input_pipe_unpacker.output;
                adder.bramb = mux.output;
                bram.ain = bram_porta.output;
                bram.bin = bram_portb.output;
                bram_porta.input = tester.output;
                bram_portb.dtct = intermediate_pipe.output;
                bram_portb.adderout = adder.output;
                forward.input = input_pipe.output;
                forward.intermediate = intermediate_pipe.output;
                mux.brama = bram.aout;
                mux.bramb = //TODO;
                mux.forward = forward.forward;
                input_pipe.input = tester.output;
                intermediate_pipe.input = input_pipe.output;
                tester.bram_result = bram.bout;

                sim
                    .AddTopLevelInput(input_pipe.input, tester.bram_ctrl)
                    .AddTopLevelOutput(bram.bout)
                    //.BuildCSV()
                    //.BuildVHDL()
                    .Run();
            }
        }
    }

}
