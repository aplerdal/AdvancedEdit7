# Super Circuit docs
Mario Kart: Super Circuit Technical documentation. All information is regarding the US version, but may be aplicable to other versions as well. This document assume you have a decent understanding of C and hex editing.

## Basics
Most numbers in this document will be in [hexadecimal](https://en.wikipedia.org/wiki/Hexadecimal), or base 16. Keep this in mind while reading.

All values are stored little endian. In practice, this means that when reading a value you read the bytes from right to left. For example, the address for the track table `0x08258000` would be stored in the rom as `00 80 25 08` This applies to all values.

All compressed data will use LZ77 compression unless explicitly stated otherwise.