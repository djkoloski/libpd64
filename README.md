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

Unity will now be able to use these DLLs. However, the interface for LibPD has changed since the last update to uPD and we need to change a few things.

- In `Assets/Magicolo/AudioTools/PureData/LibPD/LibPDNativeMethods.cs`
	- On line 88, change `EntryPoint="libpd_safe_init"` to `EntryPoint="libpd_init"`

That's all of the changes we need to make to get uPD working again. Open up the first example scene and try it out!

## Fixing uPD
uPD has a few persistent warnings and errors. Let's get ride of those.

- In `Assets/Magicolo/AudioTools/PureData/PureData.cs`
	- Add `using UnityEngine.SceneManagement;` to the top
	- Change `OnLevelWasLoaded(int level)` to `OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)`
	- Add:
        ```
		void OnEnable()
		{
			SceneManager.sceneLoaded += OnLevelFinishedLoading;
		}
		void OnDisable()
		{
			SceneManager.sceneLoaded -= OnLevelFinishedLoading;
		}
		```
- In `Assets/Magicolo/GeneralTools/Extensions/AudioClipExtensions.cs`
    - Remove the `true` argument from the call to `AudioClip.Create` on line 27
- In `Assets/Magicolo/Extensions/StringExtensions.cs1
    - Change `charInfo.width` to `charInfo.advance` on lines `142` and `166`

## Fixing LibPD
LibPD comes with a lot of warnings when we compile it, and we'd like to be able to see if there are any legitimately worrying ones. We can dump the build log to a file using:

```
$ ./mingw64_build_csharp.bat &> out.txt
```

Looking at the build log, there appear to be a lot of warnings about casting pointers to integers of a different size. Most of these are because of the type of pure-data's casting between integers and pointers of different sizes now that it's being compiled for 64-bit. We can fix them by changing the type of t_int to be 64-bit.

On line 86 of `m_pd.h`, change:

```
#define PD_LONGINTTYPE long
```

to

```
#define PD_LONGINTTYPE long long int
```

After recompiling, that's the vast majority of our warnings fixed. The rest of the warnings are pretty benign, so we'll leave them alone for now.

## State of the project
Prior to October 2016, there were bugs in pure-data that caused the third scene (SequenceExample) to crash when the sequence was played. After updating, the third scene works properly.
