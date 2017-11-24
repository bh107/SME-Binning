# SME Binning

This project aims to construct binning hardware.

It is given an array of values, which it puts into bins according to given
corresponding indices. Input comes from ethernet, which is why the input buffer (``BRAM0``) is set to be of size ``4096`` (2^(ceil(log_2((9*1024)/4)))). The target output buffer (``BRAM1``) ranges from 10000 to 100000. Converted to logarithmic, it becomes ``16384`` (2^(ceil(log_2(10000)))) to ``131072`` (2^(ceil(log_2(100000)))).

E.g.

    vals = [2, 6, 7, 4, 2]
    inds = [0, 0, 1, 2, 0]
    results = [10, 7, 4]

This project have succesfully been placed and routed on two FPGAs. Results are with an output buffer of 10000. To get the results with an output buffer of 100000, the utilization is 10 times bigger, as the biggest resource consumption is Block RAM.
* Xilinx Zynq Z7020 at 100 MHz at ~10 % utilization, resulting in an inner processing speed of 64 x 100 = ~6400 Mbps per unit. I.e. 64000 x 10 = ~64000 Mbps = ~64Gbps
* Xilinx Kintex Ultrascale+ xcku5p at 590 MHz at ~3 % utilization, resulting in an inner processing speed of 590 x 64 = ~37760 Mbps. I.e. 377760 x 33 = ~1246080 Mbps = ~1246 Gbps
