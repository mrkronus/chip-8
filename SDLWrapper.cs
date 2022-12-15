using SDL2;

namespace Chip_8
{
    internal class SDLWrapper
    {
        IntPtr Window;
        public IntPtr Renderer;
        public bool IsRunning = true;

        // 322 is the number of SDLK_DOWN events
        public bool[] Keys = new bool[322]; 

        public void Setup()
        {
            // Initilizes SDL.
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine($"There was an issue initializing SDL. {SDL.SDL_GetError()}");
            }

            // Create a new window given a title, size, and passes it a flag indicating it should be shown.
            Window = SDL.SDL_CreateWindow(
                "Chip-8",
                SDL.SDL_WINDOWPOS_UNDEFINED,
                SDL.SDL_WINDOWPOS_UNDEFINED,
                640,
                320,
                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

            if (Window == IntPtr.Zero)
            {
                Console.WriteLine($"There was an issue creating the window. {SDL.SDL_GetError()}");
            }

            // Creates a new SDL hardware renderer using the default graphics device with VSYNC enabled.
            Renderer = SDL.SDL_CreateRenderer(
                Window,
                -1,
                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
                SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            if (Renderer == IntPtr.Zero)
            {
                Console.WriteLine($"There was an issue creating the renderer. {SDL.SDL_GetError()}");
            }

            SDL.SDL_SetRenderDrawBlendMode(Renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

            Array.Clear(Keys);
        }

        public void PollEvents()
        {
            // Check to see if there are any events and continue to do so until the queue is empty.
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
            {
                switch (e.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                        {
                            IsRunning = false;
                        }
                        break;

                    case SDL.SDL_EventType.SDL_KEYDOWN:
                        {
                            Keys[(int)e.key.keysym.sym] = true;
                        }
                        break;

                    case SDL.SDL_EventType.SDL_KEYUP:
                        {
                            Keys[(int)e.key.keysym.sym] = false;
                        }
                        break;
                }
            }
        }

        public void PreRender()
        {
            // Sets the color that the screen will be cleared with.
            SDL.SDL_SetRenderDrawColor(Renderer, 0, 0, 0, 255);

            // Clears the current render surface.
            SDL.SDL_RenderClear(Renderer);

            // Set the color to black for drawing our shapes
            SDL.SDL_SetRenderDrawColor(Renderer, 255, 255, 255, 255);
        }

        public void PostRender()
        {
            // Switches out the currently presented render surface with the one we just did work on.
            SDL.SDL_RenderPresent(Renderer);
        }

        public void CleanUp()
        {
            SDL.SDL_DestroyRenderer(Renderer);
            SDL.SDL_DestroyWindow(Window);
            SDL.SDL_Quit();
        }
    }
}
