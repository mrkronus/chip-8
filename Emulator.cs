using SDL2;

namespace Chip_8
{
    internal class Emulator
    {
        public Cpu Chip8 = new Cpu();
        private uint previousTicks = 0;

        public void Setup()
        {
            // Initialize the Chip8 system and load the game into the memory  
            Chip8.Initialize();
            Chip8.LoadGame("test_opcode.ch8");
        }

        public void Update(SDLWrapper sdl)
        {
            var ticks = SDL.SDL_GetTicks();
            float delta = ticks - previousTicks;

            if (delta > (1000.0f / 120.0f))
            {
                previousTicks = ticks;

                // Emulate one cycle
                Chip8.EmulateCycle();

                // If the draw flag is set, update the screen
                if (Chip8.DrawFlag)
                {
                    sdl.PreRender();

                    //Chip8.RenderToDebugOutput();
                    DrawGraphics(sdl);
                    Chip8.DrawFlag = false;

                    sdl.PostRender();
                }
            }
        }

        private void DrawGraphics(SDLWrapper sdl)
        {
            var rect = new SDL.SDL_Rect { x = 0, y = 0, w = 10, h = 10 };

            for (int y = 0; y < Cpu.ScreenPixelsY; ++y)
            {
                for (int x = 0; x < Cpu.ScreenPixelsX; ++x)
                {
                    if (Chip8.GfxBuffer[(y * Cpu.ScreenPixelsX) + x] == 0)
                    {
                        rect.x = 10 * x;
                        rect.y = 10 * y;

                        SDL.SDL_RenderFillRect(sdl.Renderer, ref rect);
                    }
                }
            }
        }
    }
}
