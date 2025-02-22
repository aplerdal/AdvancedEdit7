# Tracks
Different track data is stored in several different places throughout the game. The main two are the track definition and the track header.

## Header
Stores a list of the tracks.

Located at `0x258000`, it is a list of 49 track offsets relative to `0x258000`. It is followed by a 16 character cstring.
### Struct
```c
struct track_header {
    uint[49] offsets;
    char[16] date;
};
```

## Track Definition
Stores menu data and functions involved with the track.
### Layout