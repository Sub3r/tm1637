# C# tm1637 Driver for Windows 10 IoT
Driver for a TM1637 Display written in C# on the Windows 10 IoT platform

This driver works under Windows 10 IoT with any Universal Windows application. It is tested with the Windows IoT Extension for the UWP Version 10.0.14393.0

### How to get started

1. Make sure to add to your C# UWP Project the Reference "Windows IoT Extension for the UWP".
2. Create the tm1637 Object with your GPIO Pin for the Clock, the GPIO Pin for the DIO and the desired brightness (0-7)
3. Write any text with write(). - You can only write a maximum of 4 characters at once.

### Wiring 

  Vcc to the 3.3V Output
  GND to GND
  DIO to any GPIO Pin
  CLK to any GPIO Pin

### How it works

To communicate with the TM1637 bit banging is used. For more information:
https://en.wikipedia.org/wiki/Bit_banging

