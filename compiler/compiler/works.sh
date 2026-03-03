#!/bin/bash

RUSTFLAGS=-Ctarget-cpu=mvp cargo build --lib --release -Zbuild-std=panic_abort,std --target wasm32-unknown-emscripten
