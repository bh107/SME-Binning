/******************************************************************************
*
* Copyright (C) 2009 - 2014 Xilinx, Inc.  All rights reserved.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* Use of the Software is limited solely to applications:
* (a) running on a Xilinx device, or
* (b) that interact with a Xilinx device through a bus or interconnect.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
* XILINX  BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
* WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF
* OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Except as contained in this notice, the name of the Xilinx shall not be used
* in advertising or otherwise to promote the sale, use or other dealings in
* this Software without prior written authorization from Xilinx.
*
******************************************************************************/

/*
 * helloworld.c: simple test application
 *
 * This application configures UART 16550 to baud rate 9600.
 * PS7 UART (Zynq) is not initialized by this application, since
 * bootrom/bsp configures it to baud rate 115200
 *
 * ------------------------------------------------
 * | UART TYPE   BAUD RATE                        |
 * ------------------------------------------------
 *   uartns550   9600
 *   uartlite    Configurable only in HW design
 *   ps7_uart    115200 (configured by bootrom/bsp)
 */

#include <stdio.h>
#include "platform.h"
#include "xil_printf.h"
#include "xparameters.h"
#include "AXIForwarder.h"
#include "xil_io.h"

int *bram0 = (int *) XPAR_BRAM_0_BASEADDR;
int *bram1 = (int *) XPAR_BRAM_1_BASEADDR;

int ctrl 	   = XPAR_AXIFORWARDER_0_S00_AXI_BASEADDR;
int input_rdy  = AXIFORWARDER_S00_AXI_SLV_REG0_OFFSET;
int input_rst  = AXIFORWARDER_S00_AXI_SLV_REG1_OFFSET;
int input_size = AXIFORWARDER_S00_AXI_SLV_REG2_OFFSET;
int output_rdy = AXIFORWARDER_S00_AXI_SLV_REG3_OFFSET;

void print_regs() {
	printf("-----\r\n");
	printf("%d\n", AXIFORWARDER_mReadReg(ctrl, input_rdy));
	printf("%d\n", AXIFORWARDER_mReadReg(ctrl, input_rst));
	printf("%d\n", AXIFORWARDER_mReadReg(ctrl, input_size));
	printf("%d\n", AXIFORWARDER_mReadReg(ctrl, output_rdy));
	printf("-----\r\n");
}

void print_bram0() {
	printf("BRAM0: [");
	for (int i = 0; i < 20; i++)
		printf("%d ", bram0[i]);
	printf("]\r\n");
}

void print_bram1() {
	printf("BRAM1: [");
	for (int i = 0; i < 20; i++)
		printf("%d ", bram1[i]);
	printf("]\r\n");
}

int bram_tests() {
	printf("Starting bram test... ");
	for (int i = 0; i < 100; i++) {
		bram0[i] = i << 1;
		bram1[i] = i << 2;
	}
	int valid = 1;
	for (int i = 0; i < 100; i++) {
		valid = valid && bram0[i] == i << 1;
		valid = valid && bram1[i] == i << 2;
	}
	printf("%d\r\n", valid);
	return valid;
}

void zero_mem() {
	for (int i = 0; i < 100; i++) {
		bram0[i] = 0;
		bram1[i] = 0;
	}
}

int main()
{
    init_platform();

    AXIFORWARDER_mWriteReg(ctrl, input_rdy, 0);
	AXIFORWARDER_mWriteReg(ctrl, input_size, 0);
	AXIFORWARDER_mWriteReg(ctrl, input_rst, 0);
    bram_tests();
    zero_mem();

    printf("Starting!\r\n");
    AXIFORWARDER_mWriteReg(ctrl, input_rdy, 0);
    for (int i = 0; i < 4; i++) {
    	bram1[i] = 0;
    }
    bram0[0] = 5;
    bram0[1] = 2;
    bram0[2] = 3;
    bram0[3] = 4;
    bram0[4] = 0;
    bram0[5] = 1;
    bram0[6] = 2;
    bram0[7] = 0;
    print_bram0();
	print_bram1();
	print_regs();
    AXIFORWARDER_mWriteReg(ctrl, input_rdy, 1);
    AXIFORWARDER_mWriteReg(ctrl, input_size, 4);
    AXIFORWARDER_mWriteReg(ctrl, input_rst, 1);
    print_regs();
    printf("Waiting for hardware... ");
    sleep(1);
    while (AXIFORWARDER_mReadReg(ctrl, output_rdy) == 0) {
    	printf("aoeu\r\n");
    }

    AXIFORWARDER_mWriteReg(ctrl, input_rdy, 0);
    AXIFORWARDER_mWriteReg(ctrl, input_size, 0);
    AXIFORWARDER_mWriteReg(ctrl, input_rst, 0);
    printf("Done!\r\n");
    print_regs();
    print_bram0();
    print_bram1();

    cleanup_platform();
    return 0;
}
