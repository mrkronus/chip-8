<h1>mrkronus/chip-8</h1>

This project is a Chip-8 emulator written in C# for windows. It works well enough to play a number of games but is not complete. This project was made primarily as a learning exercise for myself to get familiar with concepts used in emulation. Maybe I will come back to this at a later date and finish it.

![tetris](https://github.com/mrkronus/Chip-8/blob/main/tetris.gif)

To compile, just open the sln in Visual Studio and build/run.

The current status is: 

<h2>Complete</h2>

- [x] Basic emulation with most opcodes implemented
- [x] A way to render (using [SDL](https://www.libsdl.org/) in this case)
- [x] Input support
- [x] Fix enough bugs to play a game
- [x] Pass the tests in the [chip8-test-rom](https://github.com/corax89/chip8-test-rom)

<h2>Incomplete</h2>

- [ ] Sound
- [ ] Functional UX/settings
- [ ] Settings
- [ ] Some way to load roms at runtime instead of hard coding them 😅
- [ ] Accurate timing
- [ ] Support for Super Chip-8 and other extensions (https://chip-8.github.io/extensions/)

<h1>Resources</h1>

The following is a list of the resources I used when developing this emulator. The first link explains a lot of the concepts of emulation in an easy to digest form, so if you are working on your emulator, I would suggest starting there. The rest are handy references I found along the way.

- [How to write an emulator (CHIP-8 interpreter)](https://multigesture.net/articles/how-to-write-an-emulator-chip-8-interpreter/)
- [CHIP-8 on Wikipedia](https://en.wikipedia.org/wiki/CHIP-8)
- [Cowgod's Chip-8 Technical Reference v1.0](http://devernay.free.fr/hacks/chip8/C8TECH10.HTM)
- [Columbia University Chip-8 Design Specification](http://www.cs.columbia.edu/~sedwards/classes/2016/4840-spring/designs/Chip8.pdf)
- [TJA's mirror of the goldroad.co.uk doc](https://multigesture.net/wp-content/uploads/mirror/goldroad/chip8.shtml)
- [Awesome CHIP-8 (collection of links)](https://chip-8.github.io/links/)

For roms, there's quite a few with these links:

- [CHIP-8 Archive (public domain programs)](https://johnearnest.github.io/chip8Archive/)
- [chip8-test-rom](https://github.com/corax89/chip8-test-rom)

I also used the following to get an idea of how to use SDL in C#:

- [C# SDL Tutorial – Part 1 Setup](https://jsayers.dev/c-sharp-sdl-tutorial-part-1-setup/)
