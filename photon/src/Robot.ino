/*
 * Project test2
 * Description:
 * Author: bx2150@columbia.edu
 * Date:
 */
#define COMPILE_CYCLIC_EXPERIMENT false
// compile this file if COMPILE_SOFT_MOTOR==true
#if COMPILE_CYCLIC_EXPERIMENT

#include "cwpack.h";
// #include "cwpack_defines.h"
#include "helper.h"
#include <application.h>
#include "ADS1115.h"
#include "Adafruit_PCA9685.h"
#include "TCA9548A-RK.h"
// #include <spark_wiring_i2c.h>


//////////////////////// Networking ////////////////////////////////////////////
// UDP Port used for two way communication
#define REMOTE_PORT 9071 // the port Photon will send message to
#define LOCAL_PORT 8888  // the receiving port on the Photon
#define BUFFER_SIZE 64
UDP udp;// An UDP instance to let us send and receive packets over UDP
IPAddress remoteIP(192, 168, 0, 3);
unsigned char buff[BUFFER_SIZE];
cw_unpack_context uc;
cw_pack_context pc;
#define THERMISTOR_FLAG 6 // identification code for thermistor value message
////////////////////////////////////////////////////////////////////////////////
//////////////////////// Joule Heating /////////////////////////////////////////
#define NUM_RELAY 1
int RELAY[] = {A4};
int RELAY_PWM[] = {0};
PCA9685 ledDriver = PCA9685(0x40, false);  // Use the default address, turn off debugging
// I2c multiplexer
TCA9548A mux(Wire, 0);
////////////////////////////////////////////////////////////////////////////////
/////////////////////// Temperature measuring //////////////////////////////////
#define NUM_ADS1115 6
#define NUM_THERMISTER 5
#define VCC_INDEX NUM_ADS1115-1
ADS1115 ads1115s[NUM_ADS1115] {{1, ADS1115_ADDRESS_ADDR_GND},
                               {1, ADS1115_ADDRESS_ADDR_GND},
                               {1, ADS1115_ADDRESS_ADDR_GND},
                               {1, ADS1115_ADDRESS_ADDR_GND},
                               {1, ADS1115_ADDRESS_ADDR_GND},
                               {1, ADS1115_ADDRESS_ADDR_GND}};
int * voltage_3_3v_adc = &(ads1115s[VCC_INDEX].adc[0]);

void ads1115CallbackHelper() {
        // ads1115s[0].TimerCallBack();
        mux.setChannel(0);
        ads1115s[0].ReadADS1115();
        mux.setChannel(1);
        ads1115s[1].ReadADS1115();
        // ads1115s[1].TimerCallBack();
}

//////////////////////////////////////////////////////////////////////////////
WheatstonBridge wheatstonBridges[NUM_THERMISTER]={
        WheatstonBridge(1500,2000,2000),
        WheatstonBridge(1500,2000,2000),
        WheatstonBridge(1500,2000,2000),
        WheatstonBridge(1500,2000,2000),
        WheatstonBridge(1500,2000,2000)
};
/////////////////////////////////////////

#define send_buffer_len NUM_THERMISTER * 5 + 8

float THERMISTER_WIRE_OHMS[] = {1.2,1.2,1.2,1.2,0.1};
float TEMPERATURE[] = {20,20,20,20,20};

uint8_t send_buffer[send_buffer_len];
#define UPDATE_RATE_TEMPERATURE 1
float KEEP_RATE_TEMPERATURE = 1 - UPDATE_RATE_TEMPERATURE;
////////////////////////////////////////////////////////////////////////////////
/////////////////////// Timmer setup////////////////////////////////////////////
unsigned long timePrevious = 0;
unsigned int callbackInterval = 10;
Timer timer_debug(1000, timerCallBack);
Timer timer2(40, ReportTemperature);
// Timer timerADS1115(21,ads1115CallbackHelper);
////////////////////////////////////////////////////////////////////////////////

// setup() runs once, when the device is first turned on.
void setup() {
        // Initialise I2C communication as MASTER
        Wire.setSpeed(300000); // set clock speed [Hz]
        Wire.begin();
        for (size_t i = 0; i < NUM_THERMISTER; i++) {
                mux.setChannel(i);
                ads1115s[i].SetConfig(0,ADS1115_PGA_2_048V,ADS1115_MUX_DIFF_0_1,ADS1115_DR_128SPS,true,true);//gain=8
        }
        mux.setChannel(VCC_INDEX);
        ads1115s[VCC_INDEX].SetConfig(0,ADS1115_PGA_4_096V,ADS1115_MUX_DIFF_0_1,ADS1115_DR_128SPS,true,true);//gain=8
        ////////////////////////////////////////////////
        ledDriver.begin(); // This calls Wire.begin()
        ledDriver.setPWMFreq(1500); // Maximum PWM frequency is 1520

        /////////////////////////////////////
        cw_unpack_context_init(&uc, buff, BUFFER_SIZE, 0);
        Particle.function("myHandler", myHandler);
        Particle.function("getLocalIP", getLocalIP);
        // Put initialization like pinMode and begin functions here.
        for (int i = 0; i < NUM_RELAY; i++) {
                pinMode(RELAY[i], OUTPUT);
                digitalWrite(RELAY[i], LOW);
        }
        udp.begin(REMOTE_PORT);
        udp.begin(LOCAL_PORT);
        // timer_debug.start();
        timer2.start();
        Serial.begin(9600);
}

// loop() runs over and over again, as quickly as it can execute.
void loop() {

        if (millis()>callbackInterval+timePrevious) {
          timePrevious = millis();
          // ads1115CallbackHelper();
          // ads1115s[0].SendConfig(0);
          for (size_t i = 0; i < NUM_ADS1115; i++) {
                  mux.setChannel(i);
                  ads1115s[i].ReadADS1115(0);

          }

        }



        int packetSize = udp.parsePacket();
        // Check if data has been received
        if (packetSize > 0) {
                // // While we have characters to read read them
                udp.read(buff, BUFFER_SIZE);
                // Null terminate (needed for atoi)
                // buff[packetSize] = '\0';
                // Serial.printlnf((const char*)buff);
                //printHex(buff,BUFFER_SIZE);
                cw_unpack_context_init(&uc, buff, packetSize, 0);
                // cw_unpack_next(&uc);
                // if (uc.item.type == CWP_ITEM_ARRAY && uc.item.as.array.size == 2){
                if (UnpackCheckArray(&uc, 2)) {
                        if (UnpackCheckU8(&uc, 7)) {
                                // printHex(&uc.item.as.u8,1);
                                cw_unpack_next(&uc);
                                if (uc.item.type == CWP_ITEM_ARRAY) {
                                        int num = uc.item.as.array.size;
                                        for (int relayIndex = 0; relayIndex < num; relayIndex++) {
                                                cw_unpack_next(&uc);
                                                // int value = (int)uc.item.as.u8;
                                                // at 2048 Hz
                                                // analogWrite(RELAY[relayIndex], uc.item.as.u8, 2048);

                                                //ledDriver.setVal(relayIndex, uc.item.as.u8*4);
                                                ledDriver.setVal(relayIndex, uc.item.as.u64);


                                                //Serial.println((int)uc.item.as.u8);
                                        }
                                }
                        }
                }
                // printHex(buff,packetSize);

                // Ignore other chars
                udp.flush();
                // Reset out buffer
        }
}

int myHandler(String command) {
        int commaIndex = command.indexOf(",");
        Serial.println(WiFi.localIP());
        int relayIndex = command.substring(0, commaIndex + 1).toInt();
        int value = command.substring(commaIndex + 1).toInt();
        if ((relayIndex < NUM_RELAY) && relayIndex >= 0) {
                analogWrite(RELAY[relayIndex], value, 256);
        }
        return 0;
}

void udpSend(uint8_t * outbuff, int size) {
        if (udp.sendPacket(outbuff, size, remoteIP, REMOTE_PORT) < 0) {
                Particle.publish("Error");
        }
}
uint8_t out_buff[] = {0x11, 0x12, 0x13, 0x14};
void timerCallBack() {
        udpSend(out_buff, sizeof(out_buff));
        Serial.println("packet sent");
}


void ReportTemperature() {
        for (int i = 0; i < NUM_THERMISTER; i++) {
                //   int thermister_analog = ads1115s[0].adc[i];
                float thermistor_ohm = wheatstonBridges[i].getR2(*voltage_3_3v_adc,ads1115s[i].adc[0],2) - THERMISTER_WIRE_OHMS[i];
                TEMPERATURE[i] =
                    KEEP_RATE_TEMPERATURE * TEMPERATURE[i] +
                    UPDATE_RATE_TEMPERATURE * multiMap(thermistor_ohm, ohms, tempCs, 4122);


                // float temperature = (thermistor_ohm/thermistors_ohm_at_0[i]-1)/0.00385;
                // if(temperature<400) {
                //         TEMPERATURE[i] = temperature;
                // }else{
                //         // Serial.printlnf("Bad reading:%.2f\n",temperature);
                // }

                Serial.print(TEMPERATURE[i]);

                // // Serial.printf("%.2f\t", TEMPERATURE[i]);
                // Serial.print(thermistor_ohm);
                Serial.print('\t');
        }
        Serial.print('\n');
        // Serial.printf("%5f\t%5f\t%5f\t%5f\t%5f\n",
        //               TEMPERATURE[0],
        //               TEMPERATURE[1],
        //               TEMPERATURE[2],
        //               TEMPERATURE[3],
        //               TEMPERATURE[4]);
        // Serial.printf("\t\t");
        // Serial.printf("%5d\t%5d\t%5d\t%5d\t%5d\t%5d\n",
        //               ads1115s[0].adc[0],
        //               ads1115s[1].adc[0],
        //               ads1115s[2].adc[0],
        //               ads1115s[3].adc[0],
        //               ads1115s[4].adc[0],
        //               *voltage_3_3v_adc);

        cw_pack_context_init(&pc, send_buffer, send_buffer_len, 0);
        cw_pack_array_size(&pc, 3);
        // 3 element: [message_type],[command],[unsigned long millis()]
        // command for temperature:float[]:[t1,t2,t3,...]
        // cw_pack_float(&pc,((float)millis())/1000);
        cw_pack_unsigned(&pc, THERMISTOR_FLAG);
        cw_pack_array_size(&pc, NUM_THERMISTER);
        for (int i = 0; i < NUM_THERMISTER; i++) {
                cw_pack_float(&pc, TEMPERATURE[i]);
        }
        cw_pack_unsigned(&pc, millis());
        udpSend(send_buffer, pc.current - pc.start);
}

#endif
