// // ADS1115 I2C address is 0x48(72)
// #define Addr 0x48
//
// int raw_adc = 0;
// void setup()
// {
//     Serial.begin(9600);
//   // Set variable
//   Particle.variable("i2cdevice", "ADS1115");
//   Particle.variable("raw_adc", raw_adc);
//
//   // Initialise I2C communication as MASTER
//   Wire.begin();
//   // Initialise Serial Communication, set baud rate = 9600
//   // Start I2C Transmission
//   Wire.beginTransmission(Addr);
//   // Select configuration register
//   Wire.write(0x01);
//   // AINP = AIN0 and AINN = AIN1, +/- 2.048V
//   Wire.write(0x84);
//   // Continuous conversion mode, 128 SPS
//   Wire.write(0x83);
//   // Stop I2C Transmission
//   Wire.endTransmission();
//   delay(300);
// }
//
// void loop()
// {
//   unsigned int data[2];
//
//   // Start I2C Transmission
//   Wire.beginTransmission(Addr);
//   // Select data register
//   Wire.write(0x00);
//   // Stop I2C Transmission
//   Wire.endTransmission();
//
//   // Request 2 bytes of data
//   Wire.requestFrom(Addr, 2);
//
//   // Read 2 bytes of data
//   // raw_adc msb, raw_adc lsb
//   if (Wire.available() == 2)
//   {
//     data[0] = Wire.read();
//     data[1] = Wire.read();
//   }
//
//   // Convert the data
//   int raw_adc = (data[0] * 256.0) + data[1];
//   if (raw_adc > 32767)
//   {
//     raw_adc -= 65535;
//   }
//
//   // Output data to dashboard
//   Particle.publish("Digital Value of Analog Input :", String(raw_adc));
//
//   delay(500);
// }
