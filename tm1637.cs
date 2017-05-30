//  Author: sub3rX@gmail.com
//
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA


using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace driver
{
    class tm1637
    {
        private GpioPin clkPin;
        private GpioPin dataPin;

        private byte brightness;
        private byte[] digits = { 0x00, 0x00, 0x00, 0x00 };

        private static readonly byte[] brackets = { 0x39, 0x0f };
        private static readonly byte[] numbers = {0x3f, 0x06, 0x5b, 0x4f, 0x66,
                            0x6d, 0x7d, 0x07, 0x7f, 0x6f};

        private static readonly byte[] characters = {0x77, 0x7c, 0x39, 0x5e, 0x79,
                            0x71, 0x6f, 0x76, 0x30, 0x1e,
                            0x00, 0x38, 0x00, 0x00, 0x5c,
                            0x73, 0x67, 0x50, 0x5b, 0x78,
                            0x3e, 0x1c, 0x00, 0x00, 0x6e,
                            0x5b};


        public tm1637(int pinClock, int pinData, int brightness)
        {
            this.brightness = (byte)brightness;

            var gpio = GpioController.GetDefault();

            clkPin = gpio.OpenPin(pinClock);
            clkPin.SetDriveMode(GpioPinDriveMode.Output);

            dataPin = gpio.OpenPin(pinData);
            dataPin.SetDriveMode(GpioPinDriveMode.Output);

            clkPin.Write(GpioPinValue.Low);
            dataPin.Write(GpioPinValue.Low);
            //Init Display 
            for(int i=0; i < 4; i++)
            {
                this.digits[i] = 0x00;
            }
            //Set Brightness
            setBrightness(brightness);
            //Display blinks during Startup
            startupShow();
        }
        ~tm1637()
        {
            write("    ");
        }

        private async void startupShow()
        {
            for (int i = 0; i < 10; i++)
            {
                write("----");
                await Task.Delay(200);
                write("    ");
                await Task.Delay(200);
            }
        }
        public void setBrightness(int bright)
        {
            this.brightness = (byte)(bright & 0x07);
            update();
        }
        private void update()
        {
            //Start transmission
            startDisp();
            writeByte(0x40);
            stopDisp();

            //Set curser to first position
            startDisp();
            writeByte(0xC0);
            //Write text
            for (int i = 0; i < digits.Length; i++)
            {
                writeByte(digits[i]);
            }
            stopDisp();

            startDisp();
            writeByte((byte)(0x88 | this.brightness));
            stopDisp();
        }
        public void write(string w)
        {
            //Convert from String to Byte
            char c;
            for(int i=0; i < digits.Length; i++)
            {
                if (w.Length - 1 >= i)
                    c = w[i];
                else
                    c = ' ';
                this.digits[i] = encode(c);
            }
            update();
        }

        private void startDisp()
        {
            clkPin.Write(GpioPinValue.High);
            dataPin.Write(GpioPinValue.High);
            dataPin.Write(GpioPinValue.Low);
        }
        private void stopDisp()
        {
            clkPin.Write(GpioPinValue.Low);
            dataPin.Write(GpioPinValue.Low);
            clkPin.Write(GpioPinValue.High);
            dataPin.Write(GpioPinValue.High);
        }
        private void writeByte(byte input)
        {
            //Bit Banging magic is here
            for (int i = 0; i < 8; i++)
            {
                clkPin.Write(GpioPinValue.Low);

                if((input & 0x01) == 1)
                {
                    dataPin.Write(GpioPinValue.High);
                }
                else
                {
                    dataPin.Write(GpioPinValue.Low);
                }
                input >>= 1;

                clkPin.Write(GpioPinValue.High);
            }

            //Suppress Answere
            clkPin.Write(GpioPinValue.Low);
            clkPin.Write(GpioPinValue.High);
            clkPin.Write(GpioPinValue.Low);
        }
        private byte encode(char ch)
        {
            if (ch >= '0' && ch <= '9')
                return numbers[(int)ch - 48];
            if (ch >= 'a' && ch <= 'z')
                return characters[(int)ch - 97];
            if (ch >= 'A' && ch <= 'Z')
                return characters[(int)ch - 65];
            if (ch == '[')
                return brackets[0];
            if (ch == ']')
                return brackets[1];
            if (ch == '(' || ch == ')')
                return brackets[(int)ch - 40];
            if (ch == '-')
                return 0x40;
            if (ch == '_')
                return 0x08;
            if (ch == '}')
                return 0x70;
            if (ch == '{')
                return 0x46;
            return 0x00;
        }
    }
}
