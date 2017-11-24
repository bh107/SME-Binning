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
				.Run(typeof(MainClass).Assembly);
		}
	}

}