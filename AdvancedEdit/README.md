# Folder structure
```
Serialization/   Handles all ROM loading and saving as well as some of the GBA types.
UI/              Contains all the code for rendering UI and handling input.
    Editors/     Contains all the different track editors. This includes things like the AI editor and the tilemap editor.
    Tools/       Contains all the tools. This ranges from broad tools like view (used for zoom pan etc.) to more specifc ones like the bucket tool.
    Undo/        Contains all the basic code for the undo system. This centers around the UndoManager.
    Windows/     Contains all the window drawing code. Contains things like the track selector or the track window.
```
