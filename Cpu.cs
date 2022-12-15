namespace Chip_8
{
    internal class Cpu
    {
        // The Chip 8 has 35 opcodes which are all two bytes long.
        // We need two bytes to store the current opcode.
        // https://en.wikipedia.org/wiki/CHIP-8#Opcode_table
        public ushort OpCode = 0;

        // The Chip 8 has 4K memory in total, so we just allocate
        // a buffer for it
        public const uint TotalMemoryBytes = 4096;
        public byte[] Memory = new byte[TotalMemoryBytes];

        // The Chip 8 has 15 8-bit general purpose registers
        // named V0, V1 up to VE. The 16th register is also
        // used, but is special cased.
        // https://en.wikipedia.org/wiki/CHIP-8#Registers
        public const uint TotalRegisters = 16;
        public byte[] V = new byte[TotalRegisters];

        // There is an Index register I and a program counter (ProgramCounter)
        // which can have a value from 0x000 to 0xFFF
        public ushort IndexRegister = 0;
        public ushort ProgramCounter = 0;

        // 0x000-0x1FF - Chip 8 interpreter (contains font set in emu)
        // 0x050-0x0A0 - Used for the built in 4x5 pixel font set(0-F)
        // 0x200-0xFFF - Program ROM and work RAM

        // The graphics system: The chip 8 has one instruction that
        // draws sprite to the screen.Drawing is done in XOR mode and
        // if a pixel is turned off as a result of drawing, the VF
        // register is set.This is used for collision detection.

        // The graphics of the Chip 8 are black and white and the screen
        // has a total of 2048 pixels (64 x 32). This can easily be
        // implemented using an array that hold the pixel state(1 or 0):
        public const uint ScreenPixelsX = 64;
        public const uint ScreenPixelsY = 32;
        public byte[] GfxBuffer = new byte[ScreenPixelsX * ScreenPixelsY];

        // Interupts and hardware registers. The Chip 8 has none, but
        // there are two timer registers that count at 60 Hz. When set
        // above zero they will count down to zero. The system’s buzzer
        // sounds whenever the sound timer reaches zero.
        public byte DelayTimer = 0;
        public byte SoundTimer = 0;

        // Anytime there is a jump or a call to a subrutine, we need to 
        // store the current program counter, there by creating a stack.
        // There are 16 levels to this stack, and we also need a pointer
        // to keep track of where we are.
        // https://en.wikipedia.org/wiki/CHIP-8#The_stack
        public const uint StackCount = 16;
        public ushort[] Stack = new ushort[StackCount];
        public ushort StackPointer = 0;

        // The Chip 8 has a HEX based keypad(0x0-0xF),
        // we will store this in an array of 16 bytes
        // https://en.wikipedia.org/wiki/CHIP-8#Input
        public const uint KeyCount = 16;
        public byte[] Keys = new byte[KeyCount];

        // Start this as true so we clear the screen at least once
        public bool DrawFlag = true;

        // This is the default font table. for info on
        // how it is used, refer to the font section of
        // https://multigesture.net/articles/how-to-write-an-emulator-chip-8-interpreter/
        private readonly byte[] Chip8Fontset =
        {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        private Random random = new Random();

        internal void Initialize()
        {
            //
            // Program counter starts at 0x200
            ProgramCounter = 0x200;

            Array.Clear(GfxBuffer);
            Array.Clear(Stack);
            Array.Clear(Keys);
            Array.Clear(Memory);

            //
            // Load fontset to the first 80 bytes of memory
            Array.Copy(Chip8Fontset, 0, Memory, 0, Chip8Fontset.Length);
        }

        internal void LoadGame(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            if (fileInfo.Exists && fileInfo.Length <= (4096 - 512))
            {
                var buffer = File.ReadAllBytes(fileName);

                // Copy to memory starting at 0x200 (512)
                Array.Copy(buffer, 0, Memory, 512, buffer.Length);
            }
        }

        internal void EmulateCycle()
        {
            //
            // Get the current OpCode
            OpCode = (ushort)(Memory[ProgramCounter] << 8 | Memory[ProgramCounter + 1]);

            //
            // Decode OpCode (https://en.wikipedia.org/wiki/CHIP-8#Opcode_table)            
            // System.Diagnostics.Debug.WriteLine($"Processing OpCode 0x{OpCode.ToString("X4")}");

            switch (OpCode & 0xF000)
            {
                case 0x0000:
                    {
                        switch (OpCode & 0x000F)
                        {
                            case 0x0000:
                                {
                                    // 0x00E0: Clears the screen
                                    Array.Clear(GfxBuffer);

                                    DrawFlag = true;
                                    ProgramCounter += 2;
                                }
                                break;

                            case 0x000E:
                                {
                                    // 0x00EE: Returns from subroutine
                                    // i.e. whatever was in the stack before us
                                    // is now the program counter
                                    --StackPointer;
                                    ProgramCounter = Stack[StackPointer];
                                    ProgramCounter += 2;
                                }
                                break;

                            default:
                                {
                                    System.Diagnostics.Debug.WriteLine($"Unknown OpCode [0x0000]: 0x{OpCode.ToString("X4")}");
                                }
                                break;
                        }
                    }
                    break;

                case 0x1000:
                    {
                        // 0x1NNN: Jumps to address NNN
                        // i.e set the program counter to NNN
                        ProgramCounter = (ushort)(OpCode & 0x0FFF);
                    }
                    break;

                case 0x2000:
                    {
                        // 0x2NNN: Calls subroutine at NNN.
                        // i.e put the current program counter in the stack
                        // and set the program counter to NNN
                        Stack[StackPointer] = ProgramCounter;
                        ++StackPointer;
                        ProgramCounter = (ushort)(OpCode & 0x0FFF);
                    }
                    break;

                case 0x3000:
                    {
                        // 0x3XNN: Skips the next instruction if VX equals NN
                        // i.e. jump 4 bytes instead of the usual 2 if
                        // the condidition is met
                        if (V[(OpCode & 0x0F00) >> 8] == (OpCode & 0x00FF))
                        {
                            ProgramCounter += 4;
                        }
                        else
                        {
                            ProgramCounter += 2;
                        }
                    }
                    break;

                case 0x4000:
                    {
                        // 0x4XNN: Skips the next instruction if VX doesn't equal NN
                        // i.e. jump 4 bytes instead of the usual 2 if
                        // the condidition is met
                        if (V[(OpCode & 0x0F00) >> 8] != (OpCode & 0x00FF))
                        {
                            ProgramCounter += 4;
                        }
                        else
                        {
                            ProgramCounter += 2;
                        }
                    }
                    break;

                case 0x5000:
                    {
                        // 0x5XY0: Skips the next instruction if VX equals VY.
                        // i.e. jump 4 bytes instead of the usual 2 if
                        // the condidition is met
                        if (V[(OpCode & 0x0F00) >> 8] == V[(OpCode & 0x00F0) >> 4])
                        {
                            ProgramCounter += 4;
                        }
                        else
                        {
                            ProgramCounter += 2;
                        }
                    } 
                    break;

                case 0x6000:
                    {
                        // 0x6XNN: Sets VX to NN.
                        V[(OpCode & 0x0F00) >> 8] = (byte)(OpCode & 0x00FF);
                        ProgramCounter += 2;
                    }
                    break;

                case 0x7000:
                    {
                        // 0x7XNN: Adds NN to VX.
                        V[(OpCode & 0x0F00) >> 8] += (byte)(OpCode & 0x00FF);
                        ProgramCounter += 2;
                    }
                    break;

                case 0x8000:
                    switch (OpCode & 0x000F)
                    {
                        case 0x0000:
                            {
                                // 0x8XY0: Sets VX to the value of VY
                                V[(OpCode & 0x0F00) >> 8] = V[(OpCode & 0x00F0) >> 4];
                                ProgramCounter += 2;
                            }
                            break;

                        case 0x0001:
                            {
                                // 0x8XY1: Sets VX to "VX OR VY"
                                V[(OpCode & 0x0F00) >> 8] |= V[(OpCode & 0x00F0) >> 4];
                                ProgramCounter += 2;
                            }
                            break;

                        case 0x0002:
                            {
                                // 0x8XY2: Sets VX to "VX AND VY"
                                V[(OpCode & 0x0F00) >> 8] &= V[(OpCode & 0x00F0) >> 4];
                                ProgramCounter += 2;
                            }
                            break;

                        case 0x0003:
                            {
                                // 0x8XY3: Sets VX to "VX XOR VY"
                                V[(OpCode & 0x0F00) >> 8] ^= V[(OpCode & 0x00F0) >> 4];
                                ProgramCounter += 2;
                            }
                            break;

                        case 0x0004:
                            {
                                // 0x8XY4: Adds VY to VX. VF is set to 1 when
                                // there's a carry, and to 0 when there isn't					
                                if (V[(OpCode & 0x00F0) >> 4] > (0xFF - V[(OpCode & 0x0F00) >> 8]))
                                {
                                    V[0xF] = 1;
                                }
                                else
                                {
                                    V[0xF] = 0;
                                }

                                V[(OpCode & 0x0F00) >> 8] += V[(OpCode & 0x00F0) >> 4];
                                ProgramCounter += 2;
                            }
                            break;

                        case 0x0005:
                            {
                                // 0x8XY5: VY is subtracted from VX. VF is set to 0
                                // when there's a borrow, and 1 when there isn't
                                if (V[(OpCode & 0x00F0) >> 4] > V[(OpCode & 0x0F00) >> 8])
                                {
                                    V[0xF] = 0;
                                }
                                else
                                {
                                    V[0xF] = 1;
                                }

                                V[(OpCode & 0x0F00) >> 8] -= V[(OpCode & 0x00F0) >> 4];
                                ProgramCounter += 2;
                            }
                            break;

                        case 0x0006:
                            {
                                // 0x8XY6: Shifts VX right by one. VF is set to
                                // the value of the least significant bit of VX
                                // before the shift
                                V[0xF] = (byte)(V[(OpCode & 0x0F00) >> 8] & 0x1);
                                V[(OpCode & 0x0F00) >> 8] >>= 1;
                                ProgramCounter += 2;
                            }
                            break;

                        case 0x0007:
                            {
                                // 0x8XY7: Sets VX to VY minus VX. VF is set to
                                // 0 when there's a borrow, and 1 when there isn't
                                if (V[(OpCode & 0x0F00) >> 8] > V[(OpCode & 0x00F0) >> 4])
                                {
                                    V[0xF] = 0;
                                }
                                else
                                {
                                    V[0xF] = 1;
                                }

                                V[(OpCode & 0x0F00) >> 8] = (byte)(V[(OpCode & 0x00F0) >> 4] - V[(OpCode & 0x0F00) >> 8]);
                                ProgramCounter += 2;
                            }
                            break;

                        case 0x000E:
                            {
                                // 0x8XYE: Shifts VX left by one. VF is set to the value
                                // of the most significant bit of VX before the shift
                                V[0xF] = (byte)(V[(OpCode & 0x0F00) >> 8] >> 7);
                                V[(OpCode & 0x0F00) >> 8] <<= 1;
                                ProgramCounter += 2;
                            } break;

                        default:
                            {
                                System.Diagnostics.Debug.WriteLine($"Unknown OpCode [0x8000]: 0x{OpCode.ToString("X4")}");
                            }
                            break;
                    }
                    break;

                case 0x9000:
                    {
                        // 0x9XY0: Skips the next instruction if VX doesn't equal VY
                        if (V[(OpCode & 0x0F00) >> 8] != V[(OpCode & 0x00F0) >> 4])
                        {
                            ProgramCounter += 4;
                        }
                        else
                        {
                            ProgramCounter += 2;
                        }
                    }
                    break;

                case 0xA000:
                    {
                        // ANNN: Sets I to the address NNN
                        IndexRegister = (ushort)(OpCode & 0x0FFF);
                        ProgramCounter += 2;
                    }
                    break;

                case 0xB000:
                    {
                        // BNNN: Jumps to the address NNN plus V0
                        ProgramCounter = (ushort)((OpCode & 0x0FFF) + V[0]);
                    }
                    break;

                case 0xC000:
                    {
                        // CXNN: Sets VX to a random number and NN
                        V[(OpCode & 0x0F00) >> 8] = (byte)((random.Next() % 0xFF) & (OpCode & 0x00FF));
                        ProgramCounter += 2;
                    }
                    break;

                case 0xD000:
                    {
                        // DXYN: Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
                        // Each row of 8 pixels is read as bit-coded starting from memory location I; 
                        // I value doesn't change after the execution of this instruction. 
                        // VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, 
                        // and to 0 if that doesn't happen
                        ushort x = V[(OpCode & 0x0F00) >> 8];
                        ushort y = V[(OpCode & 0x00F0) >> 4];
                        ushort height = (ushort)(OpCode & 0x000F);
                        ushort pixel;

                        V[0xF] = 0;
                        for (int yLine = 0; yLine < height; yLine++)
                        {
                            pixel = Memory[IndexRegister + yLine];
                            for (int xLine = 0; xLine < 8; xLine++)
                            {
                                if ((pixel & (0x80 >> xLine)) != 0)
                                {
                                    if (GfxBuffer[(x + xLine + ((y + yLine) * ScreenPixelsX))] == 1)
                                    {
                                        V[0xF] = 1;
                                    }

                                    GfxBuffer[x + xLine + ((y + yLine) * ScreenPixelsX)] ^= 1;
                                }
                            }
                        }

                        DrawFlag = true;
                        ProgramCounter += 2;
                    }
                    break;

                case 0xE000:
                    {
                        switch (OpCode & 0x00FF)
                        {
                            case 0x009E:
                                {
                                    // EX9E: Skips the next instruction if the key stored in VX is pressed
                                    if (Keys[V[(OpCode & 0x0F00) >> 8]] != 0)
                                    {
                                        ProgramCounter += 4;
                                    }
                                    else
                                    {
                                        ProgramCounter += 2;
                                    }
                                }
                                break;

                            case 0x00A1:
                                {
                                    // EXA1: Skips the next instruction if the key stored in VX isn't pressed
                                    if (Keys[V[(OpCode & 0x0F00) >> 8]] == 0)
                                    {
                                        ProgramCounter += 4;
                                    }
                                    else
                                    {
                                        ProgramCounter += 2;
                                    }
                                }
                                break;

                            default:
                                {
                                    System.Diagnostics.Debug.WriteLine($"Unknown OpCode [0xE000]: 0x{OpCode.ToString("X4")}");
                                }
                                break;
                        }
                    }
                    break;

                case 0xF000:
                    {
                        switch (OpCode & 0x00FF)
                        {
                            case 0x0007:
                                {
                                    // FX07: Sets VX to the value of the delay timer
                                    V[(OpCode & 0x0F00) >> 8] = DelayTimer;
                                    ProgramCounter += 2;
                                }
                                break;

                            case 0x000A:
                                {
                                    // FX0A: A key press is awaited, and then stored in VX
                                    bool keyPress = false;

                                    for (byte i = 0; i < 16; ++i)
                                    {
                                        if (Keys[i] != 0)
                                        {
                                            V[(OpCode & 0x0F00) >> 8] = i;
                                            keyPress = true;
                                        }
                                    }

                                    if (!keyPress)
                                    {
                                        // If we didn't received a keypress,
                                        // skip this cycle and try again.
                                        return;
                                    }

                                    ProgramCounter += 2;
                                }
                                break;

                            case 0x0015:
                                {
                                    // FX15: Sets the delay timer to VX
                                    DelayTimer = V[(OpCode & 0x0F00) >> 8];
                                    ProgramCounter += 2;
                                }
                                break;

                            case 0x0018:
                                {
                                    // FX18: Sets the sound timer to VX
                                    SoundTimer = V[(OpCode & 0x0F00) >> 8];
                                    ProgramCounter += 2;
                                }
                                break;

                            case 0x001E:
                                {
                                    // FX1E: Adds VX to I
                                    // VF is set to 1 when range overflow (I+VX>0xFFF),
                                    // and 0 when there isn't.
                                    if (IndexRegister + V[(OpCode & 0x0F00) >> 8] > 0xFFF)
                                    {
                                        V[0xF] = 1;
                                    }
                                    else
                                    {
                                        V[0xF] = 0;
                                    }

                                    IndexRegister += V[(OpCode & 0x0F00) >> 8];
                                    ProgramCounter += 2;
                                }
                                break;

                            case 0x0029:
                                {
                                    // FX29: Sets I to the location of the sprite for the character in VX.
                                    // Characters 0-F (in hexadecimal) are represented by a 4x5 font
                                    IndexRegister = (ushort)(V[(OpCode & 0x0F00) >> 8] * 0x5);
                                    ProgramCounter += 2;
                                }
                                break;

                            case 0x0033:
                                {
                                    // FX33: Stores the Binary-coded decimal representation
                                    // of VX at the addresses I, I plus 1, and I plus 2
                                    Memory[IndexRegister] = (byte)(V[(OpCode & 0x0F00) >> 8] / 100);
                                    Memory[IndexRegister + 1] = (byte)(V[(OpCode & 0x0F00) >> 8] / 10 % 10);
                                    Memory[IndexRegister + 2] = (byte)(V[(OpCode & 0x0F00) >> 8] % 100 % 10);
                                    ProgramCounter += 2;
                                }
                                break;

                            case 0x0055:
                                {
                                    // FX55: Stores V0 to VX in memory starting at address I					
                                    for (int i = 0; i <= ((OpCode & 0x0F00) >> 8); ++i)
                                    {
                                        Memory[IndexRegister + i] = V[i];
                                    }

                                    // On the original interpreter, when the operation is done, I = I + X + 1.
                                    IndexRegister += (ushort)(((OpCode & 0x0F00) >> 8) + 1);
                                    ProgramCounter += 2;
                                }
                                break;

                            case 0x0065:
                                {
                                    // FX65: Fills V0 to VX with values from memory starting at address I					
                                    for (int i = 0; i <= ((OpCode & 0x0F00) >> 8); ++i)
                                    {
                                        V[i] = Memory[IndexRegister + i];
                                    }

                                    // On the original interpreter, when the operation is done, I = I + X + 1.
                                    IndexRegister += (ushort)(((OpCode & 0x0F00) >> 8) + 1);
                                    ProgramCounter += 2;
                                }
                                break;

                            default:
                                {
                                    System.Diagnostics.Debug.WriteLine($"Unknown OpCode [0xF000]: 0x{OpCode.ToString("X4")}");
                                }
                                break;
                        }
                    }
                    break;

                default:
                    {
                        System.Diagnostics.Debug.WriteLine($"Unknown OpCode: 0x{OpCode.ToString("X4")}");
                    }
                    break;
            }

            //
            // Update timers
            if (DelayTimer > 0)
            {
                --DelayTimer;
            }

            if (SoundTimer > 0)
            {
                if (SoundTimer == 1)
                {
                    // TODO: make some actual noise
                    System.Diagnostics.Debug.WriteLine($"BEEP!\n");
                }
                --SoundTimer;
            }
        }

        public void RenderToDebugOutput()
        {
            // This will really slow down the emulation,
            // since writing to debug output is slow. It 
            // will also get a little messy with the normal
            // output getting mixed in mid line, etc.
            for (int y = 0; y < ScreenPixelsY; ++y)
            {
                for (int x = 0; x < ScreenPixelsX; ++x)
                {
                    if (GfxBuffer[(y * ScreenPixelsX) + x] == 0)
                    {
                        System.Diagnostics.Debug.Write("O");
                    }
                    else
                    {
                        System.Diagnostics.Debug.Write(" ");
                    }
                }

                System.Diagnostics.Debug.Write("\n");
            }

            System.Diagnostics.Debug.Write("\n");
        }
    }
}
