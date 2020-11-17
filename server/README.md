# Tetris
server
 
#build
    > git submodule init
    > git submodule update
    > vcpkg\bootstrap-vcpkg.sh
    > vcpkg\vcpkg.exe install --feature-flags=manifests --triplet=x64-windows
