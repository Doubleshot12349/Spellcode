use std::{
    env, fs,
    path::{Path, PathBuf},
    process::Command,
};

type DynError = Box<dyn std::error::Error>;

fn main() -> Result<(), DynError> {
    let task = env::args().nth(1);
    match task.as_deref() {
        Some("dist") => dist()?,
        Some("dist-all") => dist_all()?,
        _ => return Err("Command not found (try `cargo xtask dist` or `dist-all`)".into())
    }
    Ok(())
}

struct Target {
    triple: Option<&'static str>,
    crate_type: Option<&'static str>,
    output: Option<&'static str>,
    unity_path: &'static str
}

const LINUX: Target = Target {
    triple: Some("x86_64-unknown-linux-gnu"),
    crate_type: None,
    output: Some("target/x86_64-unknown-linux-gnu/release/libcompiler.so"),
    unity_path: "linux"
};
const WINDOWS_MINGW: Target = Target {
    triple: Some("x86_64-pc-windows-gnu"),
    crate_type: None,
    output: Some("target/x86_64-pc-windows-gnu/release/compiler.dll"),
    unity_path: "windows"
};
const WINDOWS_MSVC: Target = Target {
    triple: Some("x86_64-pc-windows-msvc"),
    crate_type: None,
    output: Some("target/x86_64-pc-windows-msvc/release/compiler.dll"),
    unity_path: "windows"
};
const WEB: Target = Target {
    triple: Some("wasm32-wasip1"),
    crate_type: Some("staticlib"),
    output: Some("target/wasm32-wasip1/release/libcompiler.a"),
    unity_path: "web"
};

#[cfg(unix)]
const WINDOWS: Target = WINDOWS_MINGW;
#[cfg(windows)]
const WINDOWS: Target = WINDOWS_MSVC;

fn dist_all() -> Result<(), DynError> {
    clean()?;

    for target in [LINUX, WINDOWS, WEB] {
        build_binary(&target)?;
        copy_binary(&target)?;
    }

    Ok(())
}

fn clean() -> Result<(), DynError> {
    let _ = fs::remove_dir_all(&dist_dir());
    fs::create_dir_all(&dist_dir())?;
    fs::create_dir_all(&dist_dir().join("linux"))?;
    fs::create_dir_all(&dist_dir().join("windows"))?;
    fs::create_dir_all(&dist_dir().join("web"))?;
    fs::create_dir_all(&dist_dir().join("macos"))?;
    Ok(())
}

fn dist() -> Result<(), DynError> {
    clean()?;

    dist_binary()?;

    Ok(())
}

fn build_binary(Target { triple, crate_type, .. }: &Target) -> Result<(), DynError> {
    let cargo = env::var("CARGO").unwrap_or_else(|_| "cargo".to_string());
    let mut args = vec![];
    if let Some(v) = crate_type {
        args.push("rustc");
        args.push("-p");
        args.push("compiler");
        args.push("--lib");
        args.push("--crate-type");
        args.push(v);
    } else {
        args.push("build");
    }
    args.push("--release");
    if let Some(v) = triple {
        args.push("--target");
        args.push(v);
    }
    let status = Command::new(cargo)
        .current_dir(project_root())
        .args(args).status()?;

    if !status.success() {
        Err("cargo build failed")?;
    }

    Ok(())
}

fn dist_binary() -> Result<(), DynError> {
    let target = if cfg!(target_os = "linux") {
        LINUX
    } else if cfg!(target_os = "windows") {
        WINDOWS_MSVC
    } else {
        panic!()
    };

    build_binary(&target)?;
    copy_binary(&target)?;

    build_binary(&WEB)?;
    copy_binary(&WEB)?;

    Ok(())
}

fn copy_binary(target: &Target) -> Result<(), DynError> {
    let produced_file = match target.output {
        Some(v) => PathBuf::from(v),
        None => todo!()
    };
    fs::copy(&produced_file, dist_dir().join(target.unity_path).join(produced_file.file_name().unwrap()))?;

    Ok(())
}


fn project_root() -> PathBuf {
    Path::new(&env!("CARGO_MANIFEST_DIR"))
        .ancestors()
        .nth(1)
        .unwrap()
        .to_path_buf()
}

fn dist_dir() -> PathBuf {
    project_root().parent().unwrap().join("Assets/Plugins")
}

