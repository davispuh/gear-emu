dat
entry1  byte 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5
        byte 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5
        byte 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5
        byte 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5

entry2  byte 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5
        byte 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5
        byte 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5
        byte 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5

entry3  byte 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5
        byte 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5
        byte 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5
        byte 0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5

string1 byte "string",0
string2 byte "string1",0
string3 byte "string2",0

pub start | x
  x := @entry1
  x := @entry2
  x := @entry3
  
  BYTEMOVE( 0, @string1, 16 )
  WORDMOVE( 0, @string2, 8 )
  LONGMOVE( 0, @string3, 4 )



pub start2(x) | y  