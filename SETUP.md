# Build Instructions

## Setup
1. Install Unity
1. Install [rustup](https://rustup.rs)
1. Clone this repository
1. In the `compiler/` directory, run `cargo xtask dist` (to build for all platforms, use `cargo xtask dist-all`)
1. If you already have the Unity editor open, restart it

## Build
1. Open the project in Unity
1. Go to File -> build profiles
1. Select build profile for your target platform
1. If making a new build profile
    - Configure any platform settings
    - Architecture
    - Development build
    - Ensure all scenes are included in scenes list
    - (HexSandBox,SpellEditor, MainMenu)
1. If building for web
    - Either click publish to play to put the build on unity's servers
    - Or use the WebGL build target
    - then zip the file and upload to your hosting site.
1. If building for desktop
    - Ensure your machine is running the targetted platform (You can use a VM if neccessary)
    - Select your platform from the list
    - Click build
  
# For Developers:
  1. Install Unity hub and login/create an account
  2. Clone the repositiory to it's own directory
  3. In Unity Hub click "Add Project From Disk" and select the directory you cloned earlier
  4. Open the project and follow the instructions to install the correct version of the editor
  5. Download [6000.3.4f1] version in Unity Hub for the project
  6. You may need to install Web,Mac,linux and/or Windows development support

