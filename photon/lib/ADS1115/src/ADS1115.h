#ifndef ADS1115_H
#define ADS1115_H

#include <application.h>
#define ADS1115_CONVERSIONDELAY         (8)
//https://github.com/terryjmyers/ADS1115-Lite/blob/master/src/ADS1115_lite.h
/*=========================================================================
   I2C ADDRESS/BITS
   -----------------------------------------------------------------------*/
#define ADS1115_ADDRESS_ADDR_GND    0x48// address pin low (GND)
#define ADS1115_ADDRESS_ADDR_VDD    0x49 // address pin high (VCC)
#define ADS1115_ADDRESS_ADDR_SDA    0x4A // address pin tied to SDA pin
#define ADS1115_ADDRESS_ADDR_SCL    0x4B // address pin tied to SCL pin
#define ADS1115_DEFAULT_ADDRESS     ADS1115_ADDRESS_ADDR_GND
/*=========================================================================*/

/*=========================================================================
   POINTER REGISTERS
   -----------------------------------------------------------------------*/
#define ADS1115_POINTER_CONVERT     (0x00)
#define ADS1115_POINTER_CONFIG      (0x01)
#define ADS1115_LO_THRESH               (0x02)
#define ADS1115_HI_THRESH               (0x03)
/*=========================================================================*/

/*=========================================================================
   CONFIG REGISTER
   -----------------------------------------------------------------------*/
#define ADS1115_OS_SINGLE    (0x80)  // Write: Set to start a single-conversion

//Mux Parameters.  Used as input to setMux()
#define ADS1115_MUX_DIFF_0_1 (0x00)  // Differential P = AIN0, N = AIN1 (default)
#define ADS1115_MUX_DIFF_0_3 (0x10) // Differential P = AIN0, N = AIN3
#define ADS1115_MUX_DIFF_1_3 (0x20)  // Differential P = AIN1, N = AIN3
#define ADS1115_MUX_DIFF_2_3 (0x30)  // Differential P = AIN2, N = AIN3
#define ADS1115_MUX_SINGLE_0 (0x40)  // Single-ended AIN0
#define ADS1115_MUX_SINGLE_1 (0x50)  // Single-ended AIN1
#define ADS1115_MUX_SINGLE_2 (0x60)  // Single-ended AIN2
#define ADS1115_MUX_SINGLE_3 (0x70)  // Single-ended AIN3

//Gain parameter.  Used as input to setGain()
#define ADS1115_PGA_6_144V   (0x00)  // +/-6.144V range = Gain 2/3
#define ADS1115_PGA_4_096V   (0x02)  // +/-4.096V range = Gain 1
#define ADS1115_PGA_2_048V   (0x04)  // +/-2.048V range = Gain 2 (default)
#define ADS1115_PGA_1_024V   (0x06)  // +/-1.024V range = Gain 4
#define ADS1115_PGA_0_512V   (0x08)  // +/-0.512V range = Gain 8
#define ADS1115_PGA_0_256V   (0x0A)  // +/-0.256V range = Gain 16

//NSingle Shot or Continuous Mode.
#define ADS1115_MODE_CONTIN  (0x00)  // Continuous conversion mode
#define ADS1115_MODE_SINGLE  (0x01)  // Power-down single-shot mode (default)

//Sample Rate or Data Rate.  Used as input to setSampleRate().  Note
//Note that there is some over is slightly less
#define ADS1115_DR_8SPS     (0x00)  // 8 SPS(Sample per Second), or a sample every 125ms
#define ADS1115_DR_16SPS    (0x20)  // 16 SPS, or every 62.5ms
#define ADS1115_DR_32SPS    (0x40)  // 32 SPS, or every 31.3ms
#define ADS1115_DR_64SPS    (0x60)  // 64 SPS, or every 15.6ms
#define ADS1115_DR_128SPS   (0x80)  // 128 SPS, or every 7.8ms  (default)
#define ADS1115_DR_250SPS   (0xA0)  // 250 SPS, or every 4ms, note that noise free resolution is reduced to ~14.75-16bits, see table 2 in datasheet
#define ADS1115_DR_475SPS   (0xC0)  // 475 SPS, or every 2.1ms, note that noise free resolution is reduced to ~14.3-15.5bits, see table 2 in datasheet
#define ADS1115_DR_860SPS   (0xE0)  // 860 SPS, or every 1.16ms, note that noise free resolution is reduced to ~13.8-15bits, see table 2 in datasheet

/*=========================================================================*/

//functions:
// int ReadADS1115(const byte &address);
// void SendConfig(byte config[], const byte &address) ;
// void InterruptSetup(const byte &address);

/*=========================================================================*/

class ADS1115 {
public:
int numADC = 4;
int adc[4];         // ads1115 conversion result
byte _i2caddr = ADS1115_ADDRESS_ADDR_GND;
byte configs[4][2];

ADS1115(int numADC, byte _i2caddr);
ADS1115(int numADC, byte _i2caddr, byte gain, byte dataRate);

uint8_t read8(uint8_t addr);
void write8(uint8_t addr, uint8_t val);

void TimerCallBack();
void InterruptSetup();
void SendConfig(byte config[]);
void SendConfig(int configIndex);
int ReadADS1115();
int ReadADS1115(int adcIndex);
// static void SetConfigStatic(byte[],byte,byte,byte,bool=true);
void SetConfig(int configNum,byte gain,byte mux,byte dataRate,bool isContinous,bool send=false);

private:
int adcTimerInt = 0;
int counter = 0;
};

/*
      __________________
     |       |          |
     |       R1         R3
     +       |___+G-____|
     Vs      |  (Vg)    |
     -       |          |
     |       R2 = ?     R4
     |       |          |
     |_______|__________|


*/

class WheatstonBridge {
public:
WheatstonBridge(float,float,float);
float getR2(float,float);
float getR2(int,int,float);
private:
float r1;
float r2;    //Rx
float r3;
float r4;
float a;
};
#endif
