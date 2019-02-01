// Distributed with a free-will license.
// Use it any way you want, profit or free, provided it fits in the licenses of
// its associated works. ADS1115 This code is designed to work with the
// ADS1115_I2CADC I2C Mini Module available from ControlEverything.com.
// https://www.controleverything.com/content/Analog-Digital-Converters?sku=ADS1115_I2CADC#tabs-0-product_tabset-2

#include "ADS1115.h"
// #include <application.h>
#include <spark_wiring_i2c.h>

#define adcInterruptPin A3


ADS1115 ads1115(4,ADS1115_ADDRESS_ADDR_GND,
    ADS1115_PGA_6_144V,ADS1115_DR_250SPS);
void ads1115CallbackHelper(){
    ads1115.TimerCallBack();
}

Timer timer(1000, TimerCallBack);
void setup() {
  // Initialise I2C communication as MASTER
  Wire.setSpeed(400000); // set clock speed [Hz]
  Wire.begin();
  ads1115.InterruptSetup();
  attachInterrupt(adcInterruptPin, ads1115CallbackHelper, FALLING);
  ads1115.SendConfig(ads1115.configs[0]);

  // Initialise Serial Communication, set baud rate = 9600
  Serial.begin(9600);

  timer.start();
}

void loop() {
    // Particle.variable("adcTimerInt", &adcTimerInt, INT);
}

void TimerCallBack() {
  // Serial.println("ADC:%4d %4d",adc[0],adc[1]);
  char str[25];
  sprintf(str, "%5d %5d %5d %5d", ads1115.adc[0], ads1115.adc[1], ads1115.adc[2], ads1115.adc[3]);
  Particle.publish("ADC", str);
}
