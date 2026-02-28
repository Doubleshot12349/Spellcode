#!/bin/sh

cargo build --release --target x86_64-unknown-linux-gnu
cargo build --release --target x86_64-pc-windows-gnu
#cargo build --release --target aarch64-apple-darwin
#cargo build --release --target x86_64-apple-darwin
cargo rustc --release --lib --crate-type staticlib --target wasm32-wasip1

