using System;
using SME;
using SME.VHDL;

namespace SME_Binning
{
	
	class MainClass
	{
		public static void Main(string[] args)
		{
			new Simulation()
				.BuildCSVFile()
				.BuildVHDL()
				.Run(
					new Adder(),
					new AdderInMux(),
					new BRAM0(),
					new BRAM1(),
					new BRAM0AInMux(),
					new BRAM0AOutDecoder(),
					new BRAM1AInMux(),
					new BRAM1AInBroadcaster(),
					new Distributer(),
					new BRAM0BOutForwarder(),
					new BRAM1AOutForwarder(),
					new OutputPipe0(),
					new Pipe(),
					new SignalConcat(),
					new FullTester()
				);
		}
	}

}