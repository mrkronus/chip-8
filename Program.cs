using Chip_8;
using SDL2;

Emulator Emu = new Emulator();
SDLWrapper Sdl = new SDLWrapper();

void UpdateKeys()
{
    var SDLKeys = Sdl.Keys;
    var Chip8Keys = Emu.Chip8.Keys;

    Chip8Keys[0x1] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_1]);
    Chip8Keys[0x2] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_2]);
    Chip8Keys[0x3] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_3]);
    Chip8Keys[0xC] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_4]);

    Chip8Keys[0x4] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_q]);
    Chip8Keys[0x5] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_w]);
    Chip8Keys[0x6] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_e]);
    Chip8Keys[0xD] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_r]);

    Chip8Keys[0x7] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_a]);
    Chip8Keys[0x8] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_s]);
    Chip8Keys[0x9] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_d]);
    Chip8Keys[0xE] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_f]);

    Chip8Keys[0xA] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_z]);
    Chip8Keys[0x0] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_x]);
    Chip8Keys[0xB] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_c]);
    Chip8Keys[0xF] = Convert.ToByte(SDLKeys[(int)SDL.SDL_Keycode.SDLK_v]);
}

Emu.Setup();
Sdl.Setup();

while (Sdl.IsRunning)
{
    Sdl.PollEvents();
    UpdateKeys();
    Emu.Update(Sdl);
}

Sdl.CleanUp();