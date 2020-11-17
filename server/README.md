# Tetris
- simple ranking server using REST API 
- implemented by cpprestsdk, sqlite
 
## build

    > git submodule update
    > vcpkg\bootstrap-vcpkg.sh
    > vcpkg\vcpkg.exe install --feature-flags=manifests --triplet=x64-windows

## test
    > http://localhost:34568/tetris/get_rank
    > http://localhost:34568/tetris/put_rank?name=ehei&score=42

* put uri on browser