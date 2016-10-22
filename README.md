# libpd64
A port of LibPD to 64-bit compatibilty with Unity

## Setting up a build environment
LibPD must be built in a UNIX-like environment because the build toolchain
requires a few UNIX system tools. There are many packages that provide this, but
MSYS2 is a particularly good one.

[Download MSYS2 here](http://msys2.github.io/)

MSYS2 doesn't come with a compiler as part of the base package, so you'll need to install gcc next. From an MSYS2 terminal, run:

```
$ pacman -Sy
$ pacman -S mingw-w64-x86_64-gcc
```

Finally, you'll need a [git installation](https://git-scm.com/downloads) if you don't already have one.

## Compiling the libpd DLL
Open up a git terminal and run:

```
$ git clone https://github.com/libpd/libpd
```

The LibPD repository has a few submodules that must be added:

```
$ cd libpd
$ git submodule init
$ git submodule update
```

Back to MSYS2:

```
$ cd libpd
$ ./mingw64_build_csharp.bat
```

This will result in a lot of warnings as libpd has many pointer-to-integer casts. We'll fix those later.

The important things that we're looking for are:

- `libpd/libs/libpdcsharp.dll`
- `libpd/libs/mingw64/libwinpthread-1.dll`

These are what we'll copy into we'll end up copying into our Unity project.

## Setting up a Unity project
We'll be building off of [uPD by Magicolo](https://github.com/Magicolo/uPD). Clone uPD with:

```
$ cd ..
$ git clone https://github.com/Magicolo/uPD
```

Then make a new Unity project that you want to use LibPD with. Go to `Assets > Import Package > Custom Package` and select `uPD/uPD.unitypackage`. Import all assets, then select that you made a backup and are ready to upgrade.

uPD has a few rough edges, ignore the UnauthorizedAccessException about being unable to copy `libpdcsharp.dll` for now.

Example scenes are:

- `Assets/Magicolo/!Examples/PureData/SoundExample.unity`
- `Assets/Magicolo/!Examples/PureData/ContainerExample.unity`
- `Assets/Magicolo/!Examples/PureData/SequenceExample.unity`

The DLLs packaged with uPD are compiled for 32-bit, so we need to inject our new LibPD DLLs into the project now. Copy:

- `libpd/libs/libpdcsharp.dll` → `Assets/Magicolo/AudioTools/PureData/Plugins/libpdcsharp.dll`
- `libpd/libs/mingw64/libwinpthread-1.dll` → `Assets/Magicolo/AudioTools/PureData/Plugins/libwinpthread-1.dll`

After copying these DLLs, we're ready to go. Open up an example scene and hit play.

## State of the project
The third example scene crashes Unity when the sequence is played. This is currently under investigation.

What has been done so far:

- pure-data has been ported to 64 bit to remove warnings
- A system for debugging has been put into place so that the crash can be traced into pure-data
- Traces for the system calls into LibPD have been accrued

## How to debug Unity crashes
The Unity crashes can't be debugged normally, as libpdcsharp.dll doesn't have MSVC debugging symbols. To debug:

- Open the Task Manager
- Right click the Unity process
- Select 'Debug' and choose Visual Studio

When the crash happens, Visual Studio will now be able to retrieve a stack trace. The trace will only be the addresses of the symbols, so we need to be able to map them back to the function calls.
We'll need to modify our makefile to build libpdcsharp.dll with debug symbols, then strip them out into a separate file with objdump.
Once we have those symbols, we can look up the stack trace by working backward from the stack address to the first function symbol before it.
