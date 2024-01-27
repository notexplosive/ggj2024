:: Delete Output Directory
del /Q /S .\.build\*
rd /Q /S .\.build

:: Build GGJ2024
neato monogame-release-build .\GGJ2024\GGJ2024.csproj ".build"

:: Delete pdb files
powershell -command "ls .build | where name -like *.pdb | remove-item"

:: Publish
neato publish-itch ".build" "notexplosive" "ggj2024" "windows"

:: Show build output
explorer ".build"