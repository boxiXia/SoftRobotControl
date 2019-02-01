#include "application.h"
#include "cwpack.h"
const char * hex = "0123456789ABCDEF";
//refer to: https://stackoverflow.com/questions/6357031/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-in-c
void printHex(unsigned char* buff,int buff_length){
  char str[buff_length*3];
  unsigned char * pin = buff;
  char * pout = str;
  for(int i=0;i < buff_length-1; ++i){
    *pout++ = hex[(*pin>>4)&0xF];
    *pout++ = hex[(*pin++)&0xF];
    *pout++ = ':';
  }
  *pout++ = hex[(*pin>>4)&0xF];
  *pout++ = hex[(*pin)&0xF];
  *pout = 0;
  Serial.printlnf("%s", str);
}
///////////////////////////////////////////////////////////////////////////////////////////



///////////////////////////////////////////////////////////////////////////////////////////

//Cloud function: get local IP address
// command can be empty
int getLocalIP(String command)
{
  Serial.println(WiFi.localIP());
  Particle.publish("LocalIP", String(WiFi.localIP()), PRIVATE);
  return 0;
}

//////////////////////////////////////////////////////////////////////////////////////////
// Temperature sensor
//reference:http://playground.arduino.cc/Main/MultiMap
// note: the _in array should have increasing values
float multiMap(float val, float* _in, float* _out, int size)
{
	// take care the value is within range
	// val = constrain(val, _in[0], _in[size-1]);
	if (val <= _in[0]) return _out[0];
	if (val >= _in[size - 1]) return _out[size - 1];

	int pos;
	int l = 1;
	int r = size-2;
	while (l <= r)
	{
		pos = (l + r) / 2;
		// Check if val is present at mid
		if (_in[pos] == val) {
			// this will handle all exact "points" in the _in array
			return _out[pos];
		}
		else if ((r - l == 1)) {
			break;
		}
		// If val greater, ignore left half
		if (_in[pos] < val)
			l = pos + 1;
		// If val is smaller, ignore right half
		else
			r = pos - 1;
	}
	// interpolate in the right segment for the rest
	return (val - _in[pos]) * (_out[pos+1] - _out[pos]) / (_in[pos+1] - _in[pos]) + _out[pos];
}
