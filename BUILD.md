# Build Instructions

1. Open the project in Unity
2. Go to File -> build profiles
3. Select build profile for your target platform
4. If making a new build profile
    - Configure any platform settings
    - Architecture
    - Development build
    - Ensure all scenes are included in scenes list
    - (HexSandBox,SpellEditor, MainMenu)
5. If building for web
    - Either click publish to play to put the build on unity's servers
    - Or use the WebGL build target
    - then zip the file and upload to your hosting site.
6. If building for desktop
    - Ensure your machine is running the targetted platform (You can use a VM if neccessary)
    - Select your platform from the list
    - Click build