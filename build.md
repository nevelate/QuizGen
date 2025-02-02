### Clone this repository.

## Building TDLib

* Download and install Microsoft Visual Studio 2015 or later.
* Download and install [CMake](https://cmake.org/download/); choose "Add CMake to the system PATH" option while installing.
* Install `gperf`, `zlib`, and `openssl` using [vcpkg](https://github.com/Microsoft/vcpkg#quick-start):
```
git clone https://github.com/Microsoft/vcpkg.git
cd vcpkg
git checkout 07b30b49e5136a36100a2ce644476e60d7f3ddc1
.\bootstrap-vcpkg.bat
.\vcpkg.exe install gperf:x64-windows gperf:x86-windows openssl:x64-windows openssl:x86-windows zlib:x64-windows zlib:x86-windows
```
* (Optional. For XML documentation generation.) Download [PHP](https://windows.php.net/download). Add the path to php.exe to the PATH environment variable.
* Build `TDLib` with CMake enabling `.NET` support and specifying correct path to `vcpkg` toolchain file:
```
cd <path to QuizGen>/QuizGen
mkdir build
cd build
cmake -A Win32 -DTD_ENABLE_DOTNET=ON -DCMAKE_TOOLCHAIN_FILE=<path to vcpkg>/scripts/buildsystems/vcpkg.cmake ../../..
cmake --build . --config Release
cmake --build . --config Debug
cd ..
mkdir build64
cd build64
cmake -A x64 -DTD_ENABLE_DOTNET=ON -DCMAKE_TOOLCHAIN_FILE=<path to vcpkg>/scripts/buildsystems/vcpkg.cmake ../../..
cmake --build . --config Release
cmake --build . --config Debug
```

### Open solution with Visual Studio.
