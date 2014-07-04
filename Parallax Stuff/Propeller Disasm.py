import struct

#
#  Operation Code Types
#
#  0: No Arguement
#  1: Has a 'effect' code
#  2: Has a signed offset
#  3: Has a Packed literal
#  4: Has an unsigned offset
#  5: Has an unsigned offset and 'effect' code
#  6: Byte literal
#  7: Word literal
#  8: Near-Long literal
#  9: Long literal
#  10: Object.Call pair
#  11: Memory Operation Code
#

simpleOps = [
	('FRAME_CALL_RETURN', 0),
	('FRAME_CALL_NORETURN', 0),
	('FRAME_CALL_ABORT', 0),
	('FRAME_CALL_TRASHABORT', 0),
	('BRANCH', 2),
	('CALL', 6),
	('OBJCALL', 10),
	('OBJCALL_INDEXED', 10),
	('LOOP_START', 2),
	('LOOP_CONTINUE', 2),
	('JUMP_IF_FALSE', 2),
	('JUMP_IF_TRUE', 2),
	('JUMP_FROM_STACK', 0),
	('COMPARE_CASE', 2),
	('COMPARE_CASE_RANGE', 2),
	('LOOK_ABORT', 0),
	('LOOKUP_COMPARE', 0),
	('LOODOWN_COMPARE', 0),
	('LOOKUPRANGE_COMPARE', 0),
	('LOOKDOWNRANGE_COMPARE', 0),
	('QUIT', 0),
	('MARK_INTERPRETED', 0),
	('STRSIZE', 0),
	('STRCOMP', 0),
	('BYTEFILL', 0),
	('WORDFILL', 0),
	('LONGFILL', 0),
	('WAITPEQ', 0),
	('BYTEMOVE', 0),
	('WORDMOVE', 0),
	('LONGMOVE', 0),
	('WAITPNE', 0),
	('CLKSET', 0),
	('COGSTOP', 0),
	('LOCKRET', 0),
	('WAITCNT', 0),
	('READ_INDEXED_SPR', 0),
	('WRITE_INDEXED_SPR', 0),
	('EFFECT_INDEXED_SPR', 1),
	('WAITVID', 0),
	('COGINIT_RETURNS', 0),
	('LOCKNEW_RETURNS', 0),
	('LOCKSET_RETURNS', 0),
	('LOCKCLR_RETURNS', 0),
	('COGINIT', 0),
	('LOCKNEW', 0),
	('LOCKSET', 0),
	('LOCKCLR', 0),
	('ABORT', 0),
	('ABORT_WITH_RETURN', 0),
	('RETURN', 0),
	('POP_RETURN', 0),
	('PUSH_NEG1', 0),
	('PUSH_0', 0),
	('PUSH_1', 0),
	('PUSH_PACKED_LIT', 3),
	('PUSH_BYTE_LIT', 6),
	('PUSH_WORD_LIT', 7),
	('PUSH_MID_LIT', 8),
	('PUSH_LONG_LIT', 9),
	('UNKNOWN OP $3C', 0),
	('INDEXED_MEM_OP', 11),
	('INDEXED_RANGE_MEM_OP', 11),
	('MEMORY_OP', 11),
	('PUSH_VARMEM_LONG_0', 0),
	('POP_VARMEM_LONG_0', 0),
	('EFFECT_VARMEM_LONG_0', 1),
	('REFERENCE_VARMEM_LONG_0', 0),
	('PUSH_VARMEM_LONG_1', 0),
	('POP_VARMEM_LONG_1', 0),
	('EFFECT_VARMEM_LONG_1', 1),
	('REFERENCE_VARMEM_LONG_1', 0),
	('PUSH_VARMEM_LONG_2', 0),
	('POP_VARMEM_LONG_2', 0),
	('EFFECT_VARMEM_LONG_2', 1),
	('REFERENCE_VARMEM_LONG_2', 0),
	('PUSH_VARMEM_LONG_3', 0),
	('POP_VARMEM_LONG_3', 0),
	('EFFECT_VARMEM_LONG_3', 1),
	('REFERENCE_VARMEM_LONG_3', 0),
	('PUSH_VARMEM_LONG_4', 0),
	('POP_VARMEM_LONG_4', 0),
	('EFFECT_VARMEM_LONG_4', 1),
	('REFERENCE_VARMEM_LONG_4', 0),
	('PUSH_VARMEM_LONG_5', 0),
	('POP_VARMEM_LONG_5', 0),
	('EFFECT_VARMEM_LONG_5', 1),
	('REFERENCE_VARMEM_LONG_5', 0),
	('PUSH_VARMEM_LONG_6', 0),
	('POP_VARMEM_LONG_6', 0),
	('EFFECT_VARMEM_LONG_6', 1),
	('REFERENCE_VARMEM_LONG_6', 0),
	('PUSH_VARMEM_LONG_7', 0),
	('POP_VARMEM_LONG_7', 0),
	('EFFECT_VARMEM_LONG_7', 1),
	('REFERENCE_VARMEM_LONG_7', 0),
	('PUSH_LOCALMEM_LONG_0', 0),
	('POP_LOCALMEM_LONG_0', 0),
	('EFFECT_LOCALMEM_LONG_0', 1),
	('REFERENCE_LOCALMEM_LONG_0', 0),
	('PUSH_LOCALMEM_LONG_1', 0),
	('POP_LOCALMEM_LONG_1', 0),
	('EFFECT_LOCALMEM_LONG_1', 1),
	('REFERENCE_LOCALMEM_LONG_1', 0),
	('PUSH_LOCALMEM_LONG_2', 0),
	('POP_LOCALMEM_LONG_2', 0),
	('EFFECT_LOCALMEM_LONG_2', 1),
	('REFERENCE_LOCALMEM_LONG_2', 0),
	('PUSH_LOCALMEM_LONG_3', 0),
	('POP_LOCALMEM_LONG_3', 0),
	('EFFECT_LOCALMEM_LONG_3', 1),
	('REFERENCE_LOCALMEM_LONG_3', 0),
	('PUSH_LOCALMEM_LONG_4', 0),
	('POP_LOCALMEM_LONG_4', 0),
	('EFFECT_LOCALMEM_LONG_4', 1),
	('REFERENCE_LOCALMEM_LONG_4', 0),
	('PUSH_LOCALMEM_LONG_5', 0),
	('POP_LOCALMEM_LONG_5', 0),
	('EFFECT_LOCALMEM_LONG_5', 1),
	('REFERENCE_LOCALMEM_LONG_5', 0),
	('PUSH_LOCALMEM_LONG_6', 0),
	('POP_LOCALMEM_LONG_6', 0),
	('EFFECT_LOCALMEM_LONG_6', 1),
	('REFERENCE_LOCALMEM_LONG_6', 0),
	('PUSH_LOCALMEM_LONG_7', 0),
	('POP_LOCALMEM_LONG_7', 0),
	('EFFECT_LOCALMEM_LONG_7', 1),
	('REFERENCE_LOCALMEM_LONG_7', 0),
	('PUSH_MAINMEM_BYTE', 0),
	('POP_MAINMEM_BYTE', 0),
	('EFFECT_MAINMEM_BYTE', 1),
	('REFERENCE_MAINMEM_BYTE', 0),
	('PUSH_OBJECTMEM_BYTE', 4),
	('POP_OBJECTMEM_BYTE', 4),
	('EFFECT_OBJECTMEM_BYTE', 5),
	('REFERENCE_OBJECTMEM_BYTE', 4),
	('PUSH_VARIABLEMEM_BYTE', 4),
	('POP_VARIABLEMEM_BYTE', 4),
	('EFFECT_VARIABLEMEM_BYTE', 5),
	('REFERENCE_VARIABLEMEM_BYTE', 4),
	('PUSH_INDEXED_LOCALMEM_BYTE', 4),
	('POP_INDEXED_LOCALMEM_BYTE', 4),
	('EFFECT_INDEXED_LOCALMEM_BYTE', 5),
	('REFERENCE_INDEXED_LOCALMEM_BYTE', 4),
	('PUSH_INDEXED_MAINMEM_BYTE', 0),
	('POP_INDEXED_MAINMEM_BYTE', 0),
	('EFFECT_INDEXED_MAINMEM_BYTE', 1),
	('REFERENCE_INDEXED_MAINMEM_BYTE', 0),
	('PUSH_INDEXED_OBJECTMEM_BYTE', 4),
	('POP_INDEXED_OBJECTMEM_BYTE', 4),
	('EFFECT_INDEXED_OBJECTMEM_BYTE', 5),
	('REFERENCE_INDEXED_OBJECTMEM_BYTE', 4),
	('PUSH_INDEXED_VARIABLEMEM_BYTE', 4),
	('POP_INDEXED_VARIABLEMEM_BYTE', 4),
	('EFFECT_INDEXED_VARIABLEMEM_BYTE', 5),
	('REFERENCE_INDEXED_VARIABLEMEM_BYTE', 4),
	('PUSH_INDEXED_LOCALMEM_BYTE', 4),
	('POP_INDEXED_LOCALMEM_BYTE', 4),
	('EFFECT_INDEXED_LOCALMEM_BYTE', 5),
	('REFERENCE_INDEXED_LOCALMEM_BYTE', 4),
	('PUSH_MAINMEM_WORD', 0),
	('POP_MAINMEM_WORD', 0),
	('EFFECT_MAINMEM_WORD', 1),
	('REFERENCE_MAINMEM_WORD', 0),
	('PUSH_OBJECTMEM_WORD', 4),
	('POP_OBJECTMEM_WORD', 4),
	('EFFECT_OBJECTMEM_WORD', 5),
	('REFERENCE_OBJECTMEM_WORD', 4),
	('PUSH_VARIABLEMEM_WORD', 4),
	('POP_VARIABLEMEM_WORD', 4),
	('EFFECT_VARIABLEMEM_WORD', 5),
	('REFERENCE_VARIABLEMEM_WORD', 4),
	('PUSH_LOCALMEM_WORD', 4),
	('POP_LOCALMEM_WORD', 4),
	('EFFECT_LOCALMEM_WORD', 5),
	('REFERENCE_LOCALMEM_WORD', 4),
	('PUSH_INDEXED_MAINMEM_WORD', 0),
	('POP_INDEXED_MAINMEM_WORD', 0),
	('EFFECT_INDEXED_MAINMEM_WORD', 1),
	('REFERENCE_INDEXED_MAINMEM_WORD', 0),
	('PUSH_INDEXED_OBJECTMEM_WORD', 4),
	('POP_INDEXED_OBJECTMEM_WORD', 4),
	('EFFECT_INDEXED_OBJECTMEM_WORD', 5),
	('REFERENCE_INDEXED_OBJECTMEM_WORD', 4),
	('PUSH_INDEXED_VARIABLEMEM_WORD', 4),
	('POP_INDEXED_VARIABLEMEM_WORD', 4),
	('EFFECT_INDEXED_VARIABLEMEM_WORD', 5),
	('REFERENCE_INDEXED_VARIABLEMEM_WORD', 4),
	('PUSH_INDEXED_LOCALMEM_WORD', 4),
	('POP_INDEXED_LOCALMEM_WORD', 4),
	('EFFECT_INDEXED_LOCALMEM_WORD', 5),
	('REFERENCE_INDEXED_LOCALMEM_WORD', 4),
	('PUSH_MAINMEM_LONG', 0),
	('POP_MAINMEM_LONG', 0),
	('EFFECT_MAINMEM_LONG', 1),
	('REFERENCE_MAINMEM_LONG', 0),
	('PUSH_OBJECTMEM_LONG', 4),
	('POP_OBJECTMEM_LONG', 4),
	('EFFECT_OBJECTMEM_LONG', 5),
	('REFERENCE_OBJECTMEM_LONG', 4),
	('PUSH_VARIABLEMEM_LONG', 4),
	('POP_VARIABLEMEM_LONG', 4),
	('EFFECT_VARIABLEMEM_LONG', 5),
	('REFERENCE_VARIABLEMEM_LONG', 4),
	('PUSH_LOCALMEM_LONG', 4),
	('POP_LOCALMEM_LONG', 4),
	('EFFECT_LOCALMEM_LONG', 5),
	('REFERENCE_LOCALMEM_LONG', 4),
	('PUSH_INDEXED_MAINMEM_LONG', 0),
	('POP_INDEXED_MAINMEM_LONG', 0),
	('EFFECT_INDEXED_MAINMEM_LONG', 1),
	('REFERENCE_INDEXED_MAINMEM_LONG', 0),
	('PUSH_INDEXED_OBJECTMEM_LONG', 4),
	('POP_INDEXED_OBJECTMEM_LONG', 4),
	('EFFECT_INDEXED_OBJECTMEM_LONG', 5),
	('REFERENCE_INDEXED_OBJECTMEM_LONG', 4),
	('PUSH_INDEXED_VARIABLEMEM_LONG', 4),
	('POP_INDEXED_VARIABLEMEM_LONG', 4),
	('EFFECT_INDEXED_VARIABLEMEM_LONG', 5),
	('REFERENCE_INDEXED_VARIABLEMEM_LONG', 4),
	('PUSH_INDEXED_LOCALMEM_LONG', 4),
	('POP_INDEXED_LOCALMEM_LONG', 4),
	('EFFECT_INDEXED_LOCALMEM_LONG', 5),
	('REFERENCE_INDEXED_LOCALMEM_LONG', 4),
	('ROTATE_RIGHT', 0),
	('ROTATE_LEFT', 0),
	('SHIFT_RIGHT', 0),
	('SHIFT_LEFT', 0),
	('LIMIT_MIN', 0),
	('LIMIT_MAX', 0),
	('NEGATE', 0),
	('COMPLEMENT', 0),
	('BIT_AND', 0),
	('ABSOLUTE_VALUE', 0),
	('BIT_OR', 0),
	('BIT_XOR', 0),
	('ADD', 0),
	('SUBTRACT', 0),
	('ARITH_SHIFT_RIGHT', 0),
	('BIT_REVERSE', 0),
	('LOGICAL_AND', 0),
	('ENCODE', 0),
	('LOGICAL_OR', 0),
	('DECODE', 0),
	('MULTIPLY', 0),
	('MULTIPLY_HI', 0),
	('DIVIDE', 0),
	('MODULO', 0),
	('SQUARE_ROOT', 0),
	('LESS', 0),
	('GREATER', 0),
	('NOT_EQUAL', 0),
	('EQUAL', 0),
	('LESS_EQUAL', 0),
	('GREATER_EQUAL', 0),
	('LOGICAL_NOT', 0)
    ]

lops = {
    0x00: "COPY",
    0x08: "PRE_RANDOM",
    0x0C: "POST_RANDOM",

    0x10: "PRE_EXTEND_8",
    0x14: "PRE_EXTEND_16",
    0x18: "POST_EXTEND_8",
    0x1C: "POST_EXTEND_16",

    0x20: "PRE_INCREMENT_COGMEM",
    0x22: "PRE_INCREMENT_BYTE",
    0x24: "PRE_INCREMENT_WORD",
    0x26: "PRE_INCREMENT_LONG",
    0x28: "POST_INCREMENT_COGMEM",
    0x2A: "POST_INCREMENT_BYTE",
    0x2C: "POST_INCREMENT_WORD",
    0x2E: "POST_INCREMENT_LONG",

    0x30: "PRE_DECREMENT_COGMEM",
    0x32: "PRE_DECREMENT_BYTE",
    0x34: "PRE_DECREMENT_WORD",
    0x36: "PRE_DECREMENT_LONG",
    0x38: "POST_DECREMENT_COGMEM",
    0x3A: "POST_DECREMENT_BYTE",
    0x3C: "POST_DECREMENT_WORD",
    0x3E: "POST_DECREMENT_LONG",

    0x40: "ROTATE_RIGHT",
    0x41: "ROTATE_LEFT",
    0x42: "SHIFT_RIGHT",
    0x43: "SHIFT_LEFT",
    0x44: "MINIMUM",
    0x45: "MAXIMUM",
    0x46: "NEGATE",
    0x47: "COMPLEMENT",
    0x48: "BIT_AND",
    0x49: "ABSOLUTE_VALUE",
    0x4a: "BIT_OR",
    0x4b: "BIT_XOR",
    0x4c: "ADD",
    0x4d: "SUBTRACT",
    0x4e: "ARITH_SHIFT_RIGHT",
    0x4f: "BIT_REVERSE",

    0x50: "LOGICAL_AND",
    0x51: "ENCODE",
    0x52: "LOGICAL_OR",
    0x53: "DECODE",
    0x54: "MULTIPLY",
    0x55: "MULTIPLY_HI",
    0x56: "DIVIDE",
    0x57: "MODULO",
    0x58: "SQUARE_ROOT",
    0x59: "LESS",
    0x5a: "GREATER",
    0x5b: "NOT_EQUAL",
    0x5c: "EQUAL",
    0x5d: "LESS_EQUAL",
    0x5e: "GREATER_EQUAL",
    0x5F: "NOT",
    }

CogReg = [
    "MEM_0",
    "MEM_1",
    "MEM_2",
    "MEM_3",
    "MEM_4",
    "MEM_5",
    "MEM_6",
    "MEM_7",
    "MEM_8",
    "MEM_9",
    "MEM_A",
    "MEM_B",
    "MEM_C",
    "MEM_D",
    "MEM_E",
    "MEM_F",
    "PAR",
    "CNT",
    "INA",
    "INB",
    "OUTA",
    "OUTB",
    "DIRA",
    "DIRB",
    "CTRA",
    "CTRB",
    "FRQA",
    "FRQB",
    "PHSA",
    "PHSB",
    "VCFG",
    "VSCL"
    ]

v1 = 0x100 - (len(simpleOps))
v2 = 0x80 - len(lops)

print v1, "voids remaining in main instruction set"
print v2, "voids remaining in the arithmatic set"
print "%2.2f%% Completed" % (100.0 * (0x180-v1-v2) / 0x180)

def PackedSigned( fo, addresses ):
    code = ord(fo.read(1))

    if code & 0x80 != 0:
        code = ord(fo.read(1)) | (code<<8)

        if code & 0x4000:
            code -= 0x10000
        else:
            code &= 0x3FFF
    else:
        if code & 0x40:
            code -= 0x80

    if code >= 0:
        addresses += [fo.tell()+code]

    return code

def PackedUnsigned( fo ):
    data = ord(fo.read(1))

    if data & 0x80:
        data = ord(fo.read(1)) | (data << 8) & 0x7FFF

    return data

def PrintLOP( fo, addresses ):
    op = ord(fo.read(1))

    if (op & 0x80) == 0:
        print "POP",

    op &= 0x7F

    if op == 0x02:
        print "REPEAT_COMPARE", PackedSigned(fo, addresses)
    elif op == 0x06:
        print "REPEAT_COMPARE_STEP", PackedSigned(fo, addresses)
    elif op in lops:
        print lops[op]
    else:
        raise "UNKNOWN", hex(op)


def DoFunction( fo, base, end, obj_base, assemblies ):
    fo.seek(base)

    guess = False
    addresses = []
    while True:
        # Clear bypassed operations
        naddr = []
        for a in addresses:
            if a > fo.tell():
                naddr += [a]
            if a >= end:
                raise "BRANCH OUT OF RANGE"
        addresses = naddr

        x = ord(fo.read(1))

        print "\t%x:\t%x\t" % (fo.tell()-1,x),

        head, uop = simpleOps[x]
        print head,

        if uop == 1:
            PrintLOP(fo,addresses)

        elif uop == 2:
            print PackedSigned(fo,addresses)

        elif uop == 3:
            data = ord(fo.read(1))

            if data < 0x20:
                print "%x -> %x" % (data, (2 << (data & 0x1F)))
            elif data < 0x40:
                print "%x -> %x" % (data, (2 << (data & 0x1F))-1)
            elif data < 0x60:
                print "%x -> %x" % (data, ~((2 << (data & 0x1F))))
            elif data < 0x80:
                print "%x -> %x" % (data, ~((2 << (data & 0x1F))-1))
            else:
                raise "Unknown packed literal", hex(data)

        elif uop == 4:
            print "[%x]" % PackedUnsigned( fo )
        elif uop == 5:
            print "[%x]" % PackedUnsigned( fo ), PrintLOP(fo,addresses)
        elif uop == 6:
            print "%i" % struct.unpack("B",fo.read(1))
        elif uop == 7:
            print "%i" % struct.unpack(">H",fo.read(2))
        elif uop == 8:
            print "%i" % struct.unpack(">I","\x00"+fo.read(3))
        elif uop == 9:
            print "%i" % struct.unpack(">I",fo.read(4))
        elif uop == 10:
            print "%i.%i" % struct.unpack("BB",fo.read(2))
        elif uop == 11:
            op = ord(fo.read(1))
            style = op & 0xE0

            print CogReg[op & 0x1F],

            if style == 0x80:
                print "PUSH"
            elif style == 0xA0:
                print "POP"
            elif style == 0xC0:
                print "EFFECTED",
                PrintLOP(fo,addresses)
            else:
                raise "UNKNOWN REG STYLE", hex(op)
        else:
            print

        if x == 0x87 or x == 0xA7 or x == 0xC7:
            g = fo.tell()
            guess = obj_base + PackedUnsigned(fo) + 2
            fo.seek(g)
        if x == 0x15:
            guess = False
        if (x == 0x2C or x == 0x28) and guess:
            assemblies += [guess]

        if len(addresses) == 0 and x == 0x32:
            break

    DoDataChunk( fo, fo.tell(), end, [] )

def DoDataChunk( fo, base, end, assemblies ):
    if base >= end:
        return

    fo.seek(base)
    print "Data chunk %x" % base
    print "Assembly code at: ", assemblies

    print "\tbyte ",
    i = 16

    for c in fo.read(end - base):
        a = ord(c)

        if a <= 0x7E and a >= 0x20:
            print "'%s'," % c,
        elif a < 0x10:
            print "$0%x," % a,
        else:
            print "$%x," % a,

        i = i - 1
        if i == 0:
            i = 16
            print
            print "\tbyte ",
    print

def DoChunk( fo, base = 0, varOffset = 0 ):
    print "Object: %x\nVariable Block Offset: %x" % (base, varOffset)

    fo.seek(base)
    size, longs, objects = struct.unpack("HBB",fo.read(4))

    funct_off = []
    object_off = []
    assemblies = []

    for i in range( 1, longs ):
        funct_off += [struct.unpack("HH",fo.read(4))]

    for i in range( objects ):
        object_off += [struct.unpack("HH",fo.read(4))]

    dataBase = fo.tell()

    for i in range(len(funct_off)):
        offset, stackSize = funct_off[i]
        print "Function: %x ( %i bytes of stack )" % (offset+base,stackSize)

        if i+1 == len(funct_off):
            end = base+size
        else:
            end = funct_off[i+1][0]+base

        DoFunction( fo, offset+base, end, base, assemblies )

    print

    DoDataChunk(fo, dataBase, funct_off[0][0] + base, assemblies )

    for offset, vo in object_off:
        DoChunk( fo, offset+base, vo+varOffset )

OSCM = { 0x00: "XINPUT", 0x08: "XTAL1", 0x10: "XTAL2", 0x18: "XTAL3" }
CLKSEL = {
        0: "RCFAST",
        1: "RCSLOW",
        2: "XINPUT",
        3: "PLL1X",
        4: "PLL2X",
        5: "PLL4X",
        6: "PLL8X",
        7: "PLL16X"
        }

def DumpClockMode(clk):
    print "Clock Mode:",
    if clk & 0x80:
        print "RESET",
    if clk & 0x40:
        print "PLL",
    if clk & 0x20:
        print OSCM[ clk & 0x18 ],

    print CLKSEL[ clk & 0x7 ]


def DumpChecksum(f,checksum_target):
    size = checksum_target

    f.seek(0)
    d = f.read(size)
    if len(d) < 0x8000:
        cs = len(d) - 0x7FEC
    else:
        cs = 0

    for c in d:
        cs += ~ord(c)
        cs &= 0xFF

    if cs:
        print "- Invalid %x" % cs
    else:
        print "- Valid"

def DumpHeader(f):
    mhz,clk,checksum,first_object,var,free,entry_function,checksum_target = struct.unpack("IBBHHHHH",f.read(16))

    print "System Clock Rate: %imhz" %(mhz)
    DumpClockMode(clk)
    print
    print "Start of Variable Space: %x" % var
    print "Start of call frame: %x" % free
    print "Start of stack: %x" % checksum_target
    print "Entry Function: %x" % entry_function
    print "First Object: %x" % first_object
    print "Checksum: %x" % (checksum),

    DumpChecksum(f,var)
    print
    DoChunk( f, 0x10 )


def Disassemble(fn):
    print "--- STARTING OBJECT DUMP FOR %s ---" % fn

    f = file(fn,"rb")
    try:
        DumpHeader(f)
    finally:
        f.close()

Disassemble("testing.binary")
