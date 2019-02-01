#include "cwpack.h"
#include "cwpack_defines.h"
#include <stdio.h>
#include <string.h>

void setup()
{
  Serial.begin(9600);
  while(!Serial.isConnected()) // wait for Host to open serial port
    Particle.process();
  cw_pack_context pc;
	#define buffer_length 21

	char buffer[buffer_length];
	cw_pack_context_init(&pc, buffer, buffer_length, 0);

	cw_pack_map_size(&pc, 2);
	cw_pack_str(&pc, "compact\0", 8);
	cw_pack_boolean(&pc, true);
	cw_pack_str(&pc, "schema\0", 7);
	cw_pack_unsigned(&pc, 0);

	int length = pc.current - pc.start;
	// if (length > buffer_length-1) {Serial.printlnf("error!\n");}
	// else {Serial.printlnf("success\n");};

  cw_unpack_context uc;
  cw_unpack_context_init(&uc, pc.start, length, 0);
	cw_unpack_next(&uc);
	if (uc.item.type != CWP_ITEM_MAP || uc.item.as.map.size != 2) { Serial.printlnf("error:not map!\n"); }
	else { Serial.printlnf("item type = map\n"); };

  cw_unpack_next(&uc);
	if (uc.item.type != CWP_ITEM_STR || uc.item.as.str.length != 8) { Serial.printf("string size not 8,error!\n"); }
	else { Serial.printf("string size = %d\n", uc.item.as.str.length); };

	if (strncmp("compact", (char*)uc.item.as.str.start, 7)) { Serial.printf("value is not compact,error!\n"); }
	else { Serial.printf("value = %s\n", uc.item.as.str.start); };

	cw_unpack_next(&uc);
	if (uc.item.type != CWP_ITEM_BOOLEAN || uc.item.as.boolean != true) { Serial.printf("boolean error!\n"); }
	else { Serial.printf("%s", uc.item.as.boolean ? "true" : "false"); }
	//cw_skip_items(&uc, 1); /* skip this */

	cw_unpack_next(&uc);
	if (uc.item.type != CWP_ITEM_STR || uc.item.as.str.length != 7) { Serial.printf("string len!=7,error!\n"); }
	else{Serial.printf("item is:%s\n", uc.item.as.str.start); }
	if (strncmp("schema", (char*)uc.item.as.str.start, 6)) { Serial.printf("string len!=\"schema\",error!\n"); }

	cw_unpack_next(&uc);
	if (uc.item.type != CWP_ITEM_POSITIVE_INTEGER || uc.item.as.u64 != 0) { Serial.printf("error!\n"); }
	else { Serial.printf("item is:%d\n", uc.item.as.u64); }

	cw_unpack_next(&uc);
	if (uc.return_code != CWP_RC_END_OF_INPUT) { Serial.printf("error!\n"); }

}
void loop(){



}
