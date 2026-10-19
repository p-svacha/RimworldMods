# How to deploy to the workshop

1. Build the solution in <mod>/<version>/Source/<mod>/<mod>.sln
2. Make sure that the resulting build is placed in <mod>/<version>/Assemblies
    * There should be the .dll of the mod with the modification date of now
    * If using harmony (probably yes) make sure the harmony dll is also included
3. Make sure <mod>/About/About.xml is all correct
4. If updating an existing mod, make sure <mod>/About/PublishedFileId.txt exists (makes sure that the mod is updated and not a new one created)
    * If creating a new mod, make sure the file DOES NOT exist.
5. Temporarily move away the following files and directories, so they are not uploaded
    * <mod>/<version>/Source (FOR EACH VERSION)
    * <mod>/<version>/Assemblies/0Harmony.* (FOR EACH VERSION)
    * <mod>/<version>/Assemblies/<dependency>.dll (FOR EACH VERSION, FOR EACH DEPENDENCY)
    * <mod>/About/*.psd files
6. Open Rimworld
7. Publish in Mod Manager (in-Game > Select Mod > Advanced... > Update on Steam Workshop)
8. Readd removed files
9. Update changelog on Workshop page.
10. Optional: If updating an existing mod, make a comment with the most important changes.