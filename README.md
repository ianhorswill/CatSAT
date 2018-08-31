# CatSAT
CatSAT is a declarative language embedded in C# that makes it easy to write simple procedural content generators for games, such as
character generators, enemies, etc.  Being declarative means you tell it the space of possibilities for the generated objects, 
along with whatever constraints (requirements) you want to apply to them, and the system will automatically find random instances
that satisfy the constraints without you having to write a specialized generator algorithm.

This repo contains the following projects and subfolders:
* CatSAT: the main DLL for the system
* Documentation: tutorial and AIIDE paper on the system, as well as alpha docs for PCGToy
* PCGToy: a GUI to let non-programmers build generators.  Built for PROCJAM
* PCGToyLoader: a DLL to load PCGToy files so you can use them in your games
* PCGToyUnity: The minimum possible Unity project that uses a PCGToy file to randomize objects
* Tests: just what it says on the tin

To get started, grab the current release, drop the DLL in your game, and read the Tutorial.
