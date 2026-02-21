#!/bin/sh

cargo build --release --target x86_64-unknown-linux-gnu
cargo build --release --target x86_64-pc-windows-gnu
#CARGO_TARGET_WASM32_UNKNOWN_EMSCRIPTEN_LINKER=/home/foo/Unity/Hub/Editor/6000.3.4f1/Editor/Data/PlaybackEngines/WebGLSupport/BuildTools/Emscripten/llvm/clang cargo build --release --target wasm32-unknown-emscripten
#CARGO_TARGET_WASM32_UNKNOWN_EMSCRIPTEN_LINKER=/home/foo/Unity/Hub/Editor/6000.3.4f1/Editor/Data/PlaybackEngines/WebGLSupport/BuildTools/Emscripten/llvm/clang DISABLE_EXCEPTION_CATCHING=1 cargo rustc --release --lib -- -C link-args=-fno-exceptions --crate-type staticlib --target wasm32-unknown-emscripten
#DISABLE_EXCEPTION_CATCHING=1 cargo rustc --release --lib -- -C linker=/home/foo/Unity/Hub/Editor/6000.3.4f1/Editor/Data/PlaybackEngines/WebGLSupport/BuildTools/Emscripten/llvm/clang --crate-type staticlib --target wasm32-unknown-emscripten
#CARGO_TARGET_WASM32_UNKNOWN_EMSCRIPTEN_LINKER=/home/foo/Unity/Hub/Editor/6000.3.4f1/Editor/Data/PlaybackEngines/WebGLSupport/BuildTools/Emscripten/llvm/clang cargo rustc --release --lib --crate-type staticlib --target wasm32-unknown-emscripten
#cargo rustc --release --lib --crate-type staticlib --target wasm32-unknown-emscripten
#cargo rustc --release --lib --crate-type staticlib --target wasm32-wasip1 -- -C linker=/home/foo/Unity/Hub/Editor/6000.3.4f1/Editor/Data/PlaybackEngines/WebGLSupport/BuildTools/Emscripten/llvm/clang
cargo rustc --release --lib --crate-type staticlib --target wasm32-wasip1
