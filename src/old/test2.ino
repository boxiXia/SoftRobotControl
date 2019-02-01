// /*
//  * Project test2
//  * Description:
//  * Author: bx2150@columbia.edu
//  * Date:
//  */
// #define COMPILE_SOFT_MOTOR true
// //compile this file if COMPILE_SOFT_MOTOR==1
// #if COMPILE_SOFT_MOTOR==true
//
// #include "cwpack.h";
// // #include "cwpack_defines.h"
// #include "helper.h"
//
// #define THERMISTOR_FLAG 6 // identification code for thermistor value message
//
// // UDP Port used for two way communication
// #define REMOTE_PORT 9071 // the port Photon will send message to
// #define LOCAL_PORT 8888   // the receiving port on the Photon
// #define BUFFER_SIZE 64
// UDP udp; // An UDP instance to let us send and receive packets over UDP
// IPAddress remoteIP(192, 168, 0, 3);
// unsigned char buff[BUFFER_SIZE];
// cw_unpack_context uc;
// cw_pack_context pc;
//
// #define NUM_RELAY 4
// int RELAY[] = {D0, D1, D2, D3};
// int RELAY_PWM[] = {0, 0, 0, 0};
// /////////////////////////////////////////
// #define NUM_THERMISTER 5
// #define UPDATE_RATE_TEMPERATURE 1
// #define send_buffer_len NUM_THERMISTER * 5 + 8
//
// float KEEP_RATE_TEMPERATURE = 1 - UPDATE_RATE_TEMPERATURE;
// int THERMISTERS[] = {A1, A2, A3, A4, A5};
// // float PULL_DOWN_RESISTORS[] = {4695,4671,4681,4691,4693};
//
// float PULL_DOWN_RESISTORS[] = {4684, 4683, 4684, 4695, 4693};
// float THERMISTER_WIRE_OHMS[NUM_THERMISTER] = {0};
// // float THERMISTER_WIRE_OHMS[]={0.8,0.8,0.8,0.8,0};
//
// float TEMPERATURE[NUM_THERMISTER] = {20};
//
// uint8_t send_buffer[send_buffer_len];
// ////////////////////////////////////////////////
// void udpSend(uint8_t *outbuff, int size) {
//   if (udp.sendPacket(outbuff, size, remoteIP, REMOTE_PORT) < 0) {
//     Particle.publish("Error");
//   }
// }
// uint8_t out_buff[] = {0x11, 0x12, 0x13, 0x14};
// void timerCallBack() { udpSend(out_buff, sizeof(out_buff)); }
// Timer timer(1000, timerCallBack);
//
// void ReportTemperature() {
//   for (int i = 0; i < NUM_THERMISTER; i++) {
//     int thermister_analog = analogRead(THERMISTERS[i]);
//     float thermister_ohm =
//         (4096.0 / thermister_analog - 1) * PULL_DOWN_RESISTORS[i] -
//         THERMISTER_WIRE_OHMS[i];
//     TEMPERATURE[i] =
//         KEEP_RATE_TEMPERATURE * TEMPERATURE[i] +
//         UPDATE_RATE_TEMPERATURE * multiMap(thermister_ohm, ohms, tempCs, 166);
//     Serial.printf("%.2f\t", TEMPERATURE[i]);
//   }
//   Serial.printf("\n");
//
//   cw_pack_context_init(&pc, send_buffer, send_buffer_len, 0);
//   cw_pack_array_size(&pc, 3);
//   // 3 element: [message_type],[command],[unsigned long millis()]
//   // command for temperature:float[]:[t1,t2,t3,...]
//   // cw_pack_float(&pc,((float)millis())/1000);
//   cw_pack_unsigned(&pc, THERMISTOR_FLAG);
//   cw_pack_array_size(&pc, NUM_THERMISTER);
//   for (int i = 0; i < NUM_THERMISTER; i++) {
//     cw_pack_float(&pc, TEMPERATURE[i]);
//   }
//   cw_pack_unsigned(&pc, millis());
//   udpSend(send_buffer, pc.current - pc.start);
// }
//
// Timer timer2(10, ReportTemperature);
//
// //////////////////////////////////////////////////////////////////////////////
//
// //////////////////////////////////////////////////////////////////////////////
//
// // setup() runs once, when the device is first turned on.
// void setup() {
//   cw_unpack_context_init(&uc, buff, BUFFER_SIZE, 0);
//   Particle.function("myHandler", myHandler);
//   Particle.function("getLocalIP", getLocalIP);
//   // Put initialization like pinMode and begin functions here.
//   for (int i = 0; i < NUM_RELAY; i++) {
//     pinMode(RELAY[i], OUTPUT);
//     digitalWrite(RELAY[i], LOW);
//   }
//
//   udp.begin(REMOTE_PORT);
//   udp.begin(LOCAL_PORT);
//   // timer.start();
//   timer2.start();
//   Serial.begin(9600);
// }
//
// // loop() runs over and over again, as quickly as it can execute.
// void loop() {
//   int packetSize = udp.parsePacket();
//   // Check if data has been received
//   if (packetSize > 0) {
//     // // While we have characters to read read them
//     udp.read(buff, BUFFER_SIZE);
//     // Null terminate (needed for atoi)
//     // buff[packetSize] = '\0';
//     // Serial.printlnf((const char*)buff);
//     cw_unpack_context_init(&uc, buff, packetSize, 0);
//     // cw_unpack_next(&uc);
//     // if (uc.item.type == CWP_ITEM_ARRAY && uc.item.as.array.size == 2){
//     if (UnpackCheckArray(&uc, 2)) {
//       if (UnpackCheckU8(&uc, 7)) {
//         // printHex(&uc.item.as.u8,1);
//         cw_unpack_next(&uc);
//         if (uc.item.type == CWP_ITEM_ARRAY) {
//             int num = uc.item.as.array.size;
//           for (int relayIndex = 0; relayIndex < num; relayIndex++) {
//             cw_unpack_next(&uc);
//             // int value = (int)uc.item.as.u8;
//             // at 1024 Hz
//             analogWrite(RELAY[relayIndex], uc.item.as.u8, 1024);
//           }
//         }
//       }
//     }
//     // printHex(buff,packetSize);
//
//     // Ignore other chars
//     udp.flush();
//     // Reset out buffer
//   }
// }
//
// int myHandler(String command) {
//   int commaIndex = command.indexOf(",");
//   Serial.println(WiFi.localIP());
//   int relayIndex = command.substring(0, commaIndex + 1).toInt();
//   int value = command.substring(commaIndex + 1).toInt();
//   if ((relayIndex < NUM_RELAY) && relayIndex >= 0) {
//     analogWrite(RELAY[relayIndex], value, 256);
//   }
//   return 0;
// }
//
// // #include <vector>
// // std::vector<int> v;
// //
// // void setup()
// // {
// //
// //     for(int i=10; i>=0; --i){
// //         v.push_back(i);
// //     }
// //     Serial.begin(9600);
// // }
// // void loop(){
// //     for(int i=10; i>=0; --i){
// //         Serial.println(v[i]);
// //         max(2,4);
// //     }
// //
// // }
// #endif
