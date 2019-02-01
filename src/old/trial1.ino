// #include "cwpack.h";
// // #include "cwpack_defines.h"
// #include "helper.h"
// #include <application.h>
// #include "ADS1115.h"
// #include "Adafruit_PCA9685.h"
// #include "TCA9548A-RK.h"
//
// #define NUM_ADS1115 3
// #define NUM_THERMISTER 2
// #define VCC_INDEX NUM_ADS1115-1
// ADS1115 ads1115s[NUM_ADS1115] {{1, ADS1115_ADDRESS_ADDR_SDA},
//                                {1, ADS1115_ADDRESS_ADDR_VDD},
//                                {1, ADS1115_ADDRESS_ADDR_GND}};
//
//
// unsigned long timePrevious = 0;
// unsigned int callbackInterval = 100;
//
// void setup() {
//         Wire.setSpeed(200000); // set clock speed [Hz]
//         Wire.begin();
//         // preset
//         //int configNum,byte gain,byte mux,byte dataRate
//
//
//         ads1115s[0].SetConfig(0,ADS1115_PGA_2_048V,ADS1115_MUX_DIFF_2_3,ADS1115_DR_128SPS,true,true);
//         ads1115s[1].SetConfig(0,ADS1115_PGA_2_048V,ADS1115_MUX_DIFF_2_3,ADS1115_DR_128SPS,true,true);
//         ads1115s[2].SetConfig(0,ADS1115_PGA_4_096V,ADS1115_MUX_DIFF_2_3,ADS1115_DR_128SPS,true,true);
// }
//
// void loop() {
//         if (millis()>callbackInterval+timePrevious) {
//                 timePrevious = millis();
//                 // // ads1115CallbackHelper();
//                 // // ads1115s[0].SendConfig(0);
//                 // // mux.setChannel(0);
//                 // tcaselect(0);
//                 // // ads1115s[0].SendConfig(0);
//                 // ads1115s[0].adc[0] = ads1115s[0].ReadADS1115();
//                 // // ads1115s[1].SendConfig(0);
//                 // // mux.setChannel(1);
//                 // tcaselect(1);
//                 // // ads1115s[1].SendConfig(0);
//                 // ads1115s[1].adc[0] = ads1115s[1].ReadADS1115();
//                 for (size_t i = 0; i < NUM_ADS1115; i++) {
//                         ads1115s[i].ReadADS1115(0);
//                         // ads1115s[i].adc[0] = ads1115s[i].ReadADS1115();
//                 }
//                 // mux.setChannel(7);
//                 // ads1115s[5].ReadADS1115(0);
//                 Serial.printf("%5d\t%5d\t%5d\n",
//                               ads1115s[0].adc[0],
//                               ads1115s[1].adc[0],
//                               ads1115s[2].adc[0]);
//
//         }
//
// }
