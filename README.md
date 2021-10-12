# AssemblyReflector
Copies a .net framework assembly to a new assembly that only contains the types and members without including any of the original IL code. Probably not compatible with .net core or .net 5.0

### Commandline arguments
```
-i, --input     Required. Path to input assembly.

-o, --output    Required. Path to output file or directory.

-f, --force     (Default: false) If set then output files will be overwritten if they exist.
```
