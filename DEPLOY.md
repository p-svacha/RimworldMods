# How to deploy to the workshop

## Update

1. Build the solution in <mod>/<version>/Source/<mod>/<mod>.sln
2. Make sure that the resulting build is placed in <mod>/<version>/Assemblies
    * There should be the .dll of the mod with the modification date of now
    * If using harmony (probably yes) make sure the harmony dll is also included
3. Make sure <mod>/About/About.xml is all correct
4. Make sure <mod>/About/PublishedFileId.txt exists (makes sure that the mod is updated and not a new one created)
5. Temporarily move away the following files and directories, so they are not uploaded
    * <mod>/<version>/Source (FOR EACH VERSION)
    * <mod>/<version>/Assemblies/0Harmony.* (FOR EACH VERSION)
    * <mod>/About/*.psd files
6. Open Rimworld
7. Publish in Mod Manager (in-Game > Select Mod > Advanced... > Update on Steam Workshop)
8. Readd removed files