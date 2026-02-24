# Directory Structure

Any documentation goes in the root folder, or a subfolder of related documentation.

Files necessary for the game running belong in the Assets folder this includes:
    - Audio files (mp3,wav) in Assets/Audio
    - Imported packages in Assets/PackageName
    - Scene Configurations in Assets/Scenes
        -> Unity should handle this automatically
    - Any Unity prefabs in Assets/Prefabs
        -> Could be with related Prefabs in a subfolder
    - Scripts in Assets/Scripts
        -> Could be further sorted in Subfolders
    - Any .meta files should be left alone, Unity will update these as necessary.

# Testing

1. Ensure Unity Test Framework package is added to the project

2. Place tests in Packages/Test Framework/Tests/"category" where category is:
    Unit
    Integration
    Validation
    System
3. Tests should be named "Method/Class""test #".cs (or whichever ;anguage you wrote the test in)

4. Go to Window -> General -> Test Runner
5. Select testing category
6. Run desired tests 

