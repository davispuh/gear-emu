import struct
fo = file("c:\\documents and settings\\bvandiver\\desktop\\Stack Length Demo.binary","rb")
data = fo.read()
fo.close()

data = struct.unpack(str(len(data)/4)+"I",data)

conditions = [
"IF_NEVER",
"IF_NZ_AND_NC",
"IF_NC_AND_Z",
"IF_NC\t",
"IF_C_AND_NZ",
"IF_NZ\t",
"IF_C_NE_Z",
"IF_NC_OR_NZ",
"IF_C_AND_Z",
"IF_C_EQ_Z",
"IF_Z\t",
"IF_NC_OR_Z",
"IF_C\t",
"IF_C_OR_NZ",
"IF_Z_OR_C",
"\t"]

inst = ["RWBYTE\t",
	"RWWORD\t",
	"RWLONG\t",
	"HUBOP\t",
	"MUL\t",
	"MULS\t",
	"ENC\t",
	"ONES\t",
	"ROR\t",
	"ROL\t",
	"SHR\t",
	"SHL\t",
	"RCR\t",
	"RCL\t",
	"SAR\t",
	"REV\t",
	"MINS\t",
	"MAXS\t",
	"MIN\t",
	"MAX\t",
	"MOVS\t",
	"MOVD\t",
	"MOVI\t",
	"JMPRET\t",
	"AND\t",
	"ANDN\t",
	"OR\t",
	"XOR\t",
	"MUXC\t",
	"MUXNC\t",
	"MUXZ\t",
	"MUXNZ\t",
	"ADD\t",
	"SUB\t",
	"ADDABS\t",
	"SUBABS\t",
	"SUMC\t",
	"SUMNC\t",
	"SUMZ\t",
	"SUMNZ\t",
	"MOV\t",
	"NEG\t",
	"ABS\t",
	"ABSNEG\t",
	"NEGC\t",
	"NEGNC\t",
	"NEGZ\t",
	"NEGNZ\t",
	"CMPS\t",
	"CMPSX\t",
	"ADDX\t",
	"SUBX\t",
	"ADDS\t",
	"SUBS\t",
	"ADDSX\t",
	"SUBSX\t",
	"CMPSUB\t",
	"DJNZ\t",
	"TJNZ\t",
	"TJZ\t",
	"WAITPEQ\t",
	"WAITPNE\t",
	"WAITCNT\t",
	"WAITVID\t" ]

def HEX(x):
    return "$"+hex(x)[2:-1]

for i in range(len(data)):
    op = data[i]
    inst_code   = (op >> 26) & 0x3F
    zcri        = (op >> 22) & 0xF
    cond        = (op >> 18) & 0xF
    dest        = (op >>  9) & 0x1FF
    src         = (op >>  0) & 0x1FF

    flags = ""
    if zcri & 0x08:
        flags += ",WZ"
    if zcri & 0x04:
        flags += ",WC"
    if zcri & 0x02:
        flags += ",WR"

    if zcri & 0x01:
        WriteI = ""
    else:
        WriteI = "#"

    if cond == 0x00:
        print "%x:\t\t\tNOP" % (i*4+0x8000)
    else:
        print "%x:\t%s\t%s\t%s,%s\t%s" % (i*4+0x8000,conditions[cond], inst[inst_code], HEX(dest), WriteI+HEX(src), flags[1:])
