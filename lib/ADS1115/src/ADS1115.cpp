#include "ADS1115.h"
#include <application.h>
#include <spark_wiring_i2c.h>

int ADS1115::ReadADS1115() {
        // Start I2C Transmission
        Wire.beginTransmission(_i2caddr);
        // Select data register
        Wire.write(ADS1115_POINTER_CONVERT);
        // Stop I2C Transmission
        Wire.endTransmission();
        // Request 2 bytes of data
        Wire.requestFrom(_i2caddr, 2);
        // Read 2 bytes of data
        // raw_adc msb, raw_adc lsb
        unsigned int data[2];
        if (Wire.available() == 2) {
                data[0] = Wire.read();
                data[1] = Wire.read();
        }
        // Convert the data
        int raw_adc = (data[0] << 8) + data[1];
        if (raw_adc > 32767) {
                raw_adc -= 65535;
        }
        return raw_adc;
}
// read the REGISTER and store in the adc[adcIndex]
int ADS1115::ReadADS1115(int adcIndex){
  adc[adcIndex] = ReadADS1115();
  return adc[adcIndex];
}

// congigurate ads1115,write to configuration register
void ADS1115::SendConfig(byte config[]) {
        // Start I2C Transmission
        Wire.beginTransmission(_i2caddr);
        // Select configuration register
        Wire.write(ADS1115_POINTER_CONFIG);
        Wire.write(config[0]);
        Wire.write(config[1]);
        // Stop I2C Transmission
        Wire.endTransmission();
}
void ADS1115::SendConfig(int configIndex){
    SendConfig(configs[configIndex]);
}

//set up the interrupt
void ADS1115::InterruptSetup() {
        // Start I2C Transmission
        Wire.beginTransmission(_i2caddr);
        // Select thresh register
        Wire.write(ADS1115_LO_THRESH);
        Wire.write(0x00);
        Wire.write(0x00);
        // Stop I2C Transmission
        Wire.endTransmission();
        // Start I2C Transmission
        Wire.beginTransmission(_i2caddr);
        // Select thresh register
        Wire.write(ADS1115_HI_THRESH);
        Wire.write(0x80);
        Wire.write(0x00);
        // Stop I2C Transmission
        Wire.endTransmission();
}
// general constructor
ADS1115::ADS1115(int numADC, byte _i2caddr) {
        this->numADC = numADC;
        this->_i2caddr = _i2caddr;
        // configs[4][2] ={{0x00,0x00},{0x00,0x00},{0x00,0x00},{0x00,0x00}}
}
//for single-ended imput
ADS1115::ADS1115(int numADC, byte _i2caddr, byte gain, byte dataRate) {
        this->numADC = numADC;
        this->_i2caddr = _i2caddr;
        byte configADS1115MSB = ADS1115_OS_SINGLE |
                                gain |
                                ADS1115_MODE_CONTIN;
        byte configADS1115LSB = dataRate;

        configs[0][0] = configADS1115MSB | ADS1115_MUX_SINGLE_0;
        configs[0][1] = configADS1115LSB;
        configs[1][0] = configADS1115MSB | ADS1115_MUX_SINGLE_1;
        configs[1][1] = configADS1115LSB;
        configs[2][0] = configADS1115MSB | ADS1115_MUX_SINGLE_2;
        configs[2][1] = configADS1115LSB;
        configs[3][0] = configADS1115MSB | ADS1115_MUX_SINGLE_3;
        configs[3][1] = configADS1115LSB;
}


// /*
//    The helper function for ADS1115::SetConfig(...)
//  */
// void ADS1115:: SetConfigStatic(byte configToSet[], byte gain,byte mux,byte dataRate,
//                                bool isContinous){
//         byte configMode = isContinous ? ADS1115_MODE_CONTIN : ADS1115_MODE_SINGLE;
//         configToSet[0] = ADS1115_OS_SINGLE | gain |configMode|mux;
//         configToSet[1] = dataRate;
// }
void ADS1115::SetConfig(int configNum,byte gain,byte mux,byte dataRate,bool isContinous,bool send){
        // SetConfigStatic(configs[configNum],gain,mux,dataRate,isContinous);
        byte configMode = isContinous ? ADS1115_MODE_CONTIN : ADS1115_MODE_SINGLE;
        configs[configNum][0] = ADS1115_OS_SINGLE | gain |configMode|mux;
        configs[configNum][1] = dataRate;
        if (send) {
          SendConfig(configNum);
        }
}

void ADS1115::TimerCallBack() {
        // Serial.printf("%d\n",adcTimerInt);
        // // the port for conversion is 2-previous from the configuration
        // int conversionPort = adcTimerInt - 2;
        // the port for conversion is 2-previous from the configuration
        // int conversionPort = adcTimerInt - 1;
        // conversionPort = conversionPort < 0 ? conversionPort + numADC : conversionPort;

        // int reading = ReadADS1115();
        // Serial.print((int)read8(ADS1115_POINTER_CONFIG));
        // Serial.print('\t');
        // Serial.print((int)configs[adcTimerInt][0]);
        // Serial.print("\n");

        if((read8(ADS1115_POINTER_CONFIG)|ADS1115_OS_SINGLE) ==configs[adcTimerInt][0]){
          adc[adcTimerInt] = ReadADS1115();
          adcTimerInt = (adcTimerInt + 1) == numADC ? 0 : adcTimerInt + 1;
        }else{
          Serial.printf("NN\n");
        }
        SendConfig(adcTimerInt);
}

/**
 * Read a byte from a given address on the driver
 * @param  addr  The address
 * @return       The value at the given address
 */
uint8_t ADS1115::read8(uint8_t addr) {
        Wire.beginTransmission(_i2caddr);
        Wire.write(addr);
        Wire.endTransmission();
        Wire.requestFrom((uint8_t)_i2caddr, (uint8_t)1);
        return Wire.read();
}

/**
 * Write a byte to a given address on the driver
 * @param addr  The address
 * @param val   The byte to be written
 */
void ADS1115::write8(uint8_t addr, uint8_t val) {
        Wire.beginTransmission(_i2caddr);
        Wire.write(addr);
        Wire.write(val);
        Wire.endTransmission();
}

/////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////
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
WheatstonBridge::WheatstonBridge(float r1,float r3, float r4){
        this->r1 = r1;
        this->r3 = r3;
        this->r4 = r4;
        this->a = r4/(r3+r4);
}
float WheatstonBridge::getR2(float vs,float vg){

        r2 = r1/(1/(a+vg/vs)-1);
        return r2;
}
float WheatstonBridge::getR2(int vs,int vg,float gain){
        // gain is the gain_vg/gain_vs
        r2 = r1/(1/(a+(float)vg/(float)vs/gain)-1);
        return r2;
}
