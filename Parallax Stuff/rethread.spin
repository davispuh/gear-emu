CON
  _clkmode = RCFAST

DAT
              byte "------------"

{{
        Interpreter Core
        Based on research located at http://www.sublab.net/spin.html
}}

              byte "-$F004----------"
intepreter    ORG       0

              FIT
              

{{
        Boot-loader Core        
}}

              byte "-$F800----------"        
boot_loader   ORG       0
        
boot_entry              CLKSET  boot_clock                      ' We want to boot with fast internal
                        MOV     address_ptr, end_of_ram         ' end of ram preset
zero_loop               WRBYTE  zero, address_ptr               ' zero ram (cheat using instruction)
                        DJNZ    address_ptr, #zero_loop         ' loop until clear
zero                    WRBYTE  zero, #0                        ' Clear byte 0 (also doubles byte constant zero)                                                                                            

                        CALL    #serial_comm                    ' Check for serial comms...

                        {{ ---------- EEPROM CODE ---------- }}

                        MOV     OUTA, #0                        ' Pre-clear output
                        CALL    #send_estart
                        MOV     byte_buffer, #%0000_0101        ' Reverse order, load address
                        CALL    #send_ebyte                     ' Send control code
                        CALL    #send_ebyte                     ' Send zeroed address
                        CALL    #send_ebyte
                        CALL    #send_estart                    ' Send start command (reset state machine)
                        MOV     byte_buffer, #%1000_0101        ' Reverse order, start read                        
                        CALL    #send_ebyte

                        MOV     address_ptr, #0                 ' Set to start of ram
eeprom_recv_loop        CALL    #recv_ebyte                     ' Receive a byte from eeprom (with ack)
                        WRBYTE  byte_buffer, address_ptr        ' Write to main ram 
                        ADD     address_ptr, #1                 ' Increment our address pointer
                        CMP     address_ptr, end_of_ram WZ      ' Check to see if we are at the end of ram
              IF_NE     JMP     #eeprom_recv_loop               ' Continue for 32k x 8                         
                        CALL    #send_eend                      ' Send STOP

                        {{ -------- START BOOT ENTRY ------- }} 

sum_check               MOV     address_ptr, end_of_ram         ' Start check sum at the end of ram
                        MOV     byte_total, #0                  ' Our byte total starts at zero
sum_loop                SUB     address_ptr, #1  WZ             ' Decrement address pointer
                        RDBYTE  byte_buffer, address_ptr                       
                        XOR     byte_buffer, #$FF               ' Sum -byte - 1
                        ADD     byte_total, byte_buffer
              IF_NZ     JMP     #sum_loop                       ' Repeat for all of ram
                        AND     byte_total, #$FF WZ             ' Restart if              
              IF_NZ     JMP     #boot_entry

                        {{ ------- INTERPRETER START ------- }}
              
                        RDBYTE  byte_buffer, #4                 ' Set to appropriate clock mode
                        CLKSET  byte_buffer

                        ' TODO: PREINIT INTERPRETER
                        
                        COGINIT boot_cog                        ' Start the interpreter  
clobber_wait            JMP     #clobber_wait                   ' Incase of a slow restart

' --------- SERIAL CODE ----------

serial_comm             MOV     OUTA, #0                        ' Set port for serial load
                        MOV     DIRA, serial_Tx

                        ' TODO: DETECT SERIAL COMM, AND CONTINUE
                        
serial_comm_ret         RET

' --------- EEPROM CODE ----------

{ Send START command }

send_estart             MOV     DIRA, eeprom_out                ' We will be driving both pins
                        MOV     OUTA, eeprom_sda                ' SDA HI, SCK LO
                        MOV     OUTA, eeprom_out                ' SDA HI, SCK HI
                        MOV     OUTA, eeprom_sck                ' SDA LO, SCK HI
                        MOV     OUTA, #0                        ' SDA LO, SCK LO              
send_estart_ret         RET

{ Send END command }

send_eend               MOV     DIRA, eeprom_out                ' We will be driving both pins
                        MOV     OUTA, #0                        ' SDA LO, SCK LO
                        MOV     OUTA, eeprom_sck                ' SDA LO, SCK HI
                        MOV     OUTA, eeprom_out                ' SDA HI, SCK HI
                        MOV     OUTA, eeprom_sda                ' SDA HI, SCK LO              
                        MOV     OUTA, #0                        ' SDA LO, SCK LO              
send_eend_ret           RET

{ Send a byte }

send_ebyte              MOV     DIRA, eeprom_out                ' We will be driving both pins
                        MOV     byte_total, #8                  ' We have 8 bits to send                        
send_ebyte_loop         SHR     byte_buffer, #1 WC              ' Shift out bit to send
              IF_C      MOV     OUTA, eeprom_sda                ' Clock out our data
              IF_NC     MOV     OUTA, #0              
              IF_C      MOV     OUTA, eeprom_out
              IF_NC     MOV     OUTA, eeprom_sck
              IF_C      MOV     OUTA, eeprom_sda
                        MOV     OUTA, #0
                        DJNZ    byte_total, #send_ebyte_loop    ' Repeat for 8 bits

                        MOV     DIRA, eeprom_sck                ' Read our ACK bit
                        TEST    INA, eeprom_sda WZ
              IF_NZ     JMP     #boot_entry                     ' Fail on no ack
send_ebyte_ret          RET    

{ Receive a byte }

recv_ebyte              MOV     DIRA, eeprom_sck                ' Input mode
                        MOV     byte_total, #8                  ' Load 8 bits
                        MOV     byte_buffer, #0
recv_ebyte_loop         MOV     OUTA, eeprom_sck                ' Clock in bit
                        SHL     byte_buffer, #1                 ' Preshift a bit
                        TEST    INA, eeprom_sda  WZ             ' Check if the data bit is high
              IF_NZ     OR      byte_buffer, #1
                        MOV     OUTA, #0
                        DJNZ    byte_total, #recv_ebyte_loop
                        MOV     DIRA, eeprom_out                ' Output mode
                        MOV     OUTA, eeprom_sck                ' Clock an ack (shouldn't do this on last byte)
                        MOV     OUTA, #0                        
recv_ebyte_ret          RET                                              
                        

                        {{ -------- CONSTANTS ------- }}
                        
end_of_ram              LONG    $8000           ' 
par_param               LONG    $7FFC           ' We will boot with the last byte in ram as the param                 

                        ' Constant for restarting COG 0 with the interpreter
                        ' It assumes that everything is aligned in rom properly,
                        ' and that the last byte of RAM is useful for storing boot parameters
                         
boot_cog                LONG    (%0_000 | ($7FFC << 16) | ($FC04 << 2))
boot_clock              LONG    RCFAST

serial_Rx               LONG    %1000 << 28    
serial_Tx               LONG    %0100 << 28
eeprom_out              LONG    %0011 << 28    
eeprom_sda              LONG    %0010 << 28
eeprom_sck              LONG    %0001 << 28

' Runtime Variables
address_ptr             LONG    $0
byte_total              LONG    $0
byte_buffer             LONG    $0
eeprom_ack              LONG    $0
      
              FIT
              byte "----------------"        
                              