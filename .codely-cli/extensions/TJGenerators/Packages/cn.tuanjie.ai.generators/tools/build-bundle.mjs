#!/usr/bin/env node
/**
 * Assemble a TJGenerators distribution bundle:
 *
 *   build/TJGenerators.zip   (--archive 时仅保留 zip，不保留目录)
 *
 * Usage:
 *   node tools/build-bundle.mjs [--out-dir build/TJGenerators] [--archive]
 *   tools/pack.sh          (Linux / macOS)
 *   tools\pack.bat         (Windows)
 */

import { execSync } from "node:child_process";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const REPO_ROOT = path.resolve(__dirname, "..");

const EXTENSION_COPY_PATHS = ["experience", "skills", "gemini-extension.json"];

const PACKAGE_MANIFEST = {
  dependencies: {
    "cn.tuanjie.ai.generators": "file:cn.tuanjie.ai.generators",
  },
};

function parseArgs(argv) {
  const options = {
    outDir: path.join(REPO_ROOT, "build", "TJGenerators"),
    archive: false,
    keepMeta: false,
  };

  for (let i = 0; i < argv.length; i += 1) {
    const arg = argv[i];
    if (arg === "--archive") {
      options.archive = true;
    } else if (arg === "--keep-meta") {
      options.keepMeta = true;
    } else if (arg === "--out-dir") {
      options.outDir = path.resolve(argv[++i]);
    } else if (arg === "--help" || arg === "-h") {
      console.log(`Usage: node tools/build-bundle.mjs [options]

Options:
  --out-dir <path>   Output root (default: build/TJGenerators)
  --archive          Create build/TJGenerators.zip and remove the output folder
  --keep-meta        Keep .meta under extension assets too
`);
      process.exit(0);
    } else {
      throw new Error(`Unknown argument: ${arg}`);
    }
  }

  return options;
}

function assertExtensionReady(extensionDir) {
  if (!fs.existsSync(extensionDir)) {
    throw new Error(`Extension assets not found at ${extensionDir}.`);
  }

  for (const rel of EXTENSION_COPY_PATHS) {
    const full = path.join(extensionDir, rel);
    if (!fs.existsSync(full)) {
      throw new Error(`Missing extension path: ${rel}`);
    }
  }
}

function copyPath(src, dest, { excludeMeta }) {
  const stat = fs.statSync(src);
  if (stat.isDirectory()) {
    fs.mkdirSync(dest, { recursive: true });
    for (const name of fs.readdirSync(src)) {
      if (excludeMeta && name.endsWith(".meta")) continue;
      copyPath(path.join(src, name), path.join(dest, name), { excludeMeta });
    }
    return;
  }

  fs.mkdirSync(path.dirname(dest), { recursive: true });
  fs.copyFileSync(src, dest);
}

function copyExtensionAssets(extensionDir, outDir, { excludeMeta }) {
  for (const rel of EXTENSION_COPY_PATHS) {
    copyPath(path.join(extensionDir, rel), path.join(outDir, rel), { excludeMeta });
  }

  const manifestPath = path.join(outDir, "manifest.json");
  fs.writeFileSync(manifestPath, `${JSON.stringify(PACKAGE_MANIFEST, null, 2)}\n`, "utf8");
}

function removeMetaFiles(rootDir) {
  if (!fs.existsSync(rootDir)) return 0;

  let removed = 0;
  for (const entry of fs.readdirSync(rootDir, { withFileTypes: true })) {
    const full = path.join(rootDir, entry.name);
    if (entry.isDirectory()) {
      removed += removeMetaFiles(full);
    } else if (entry.name.endsWith(".meta")) {
      fs.unlinkSync(full);
      removed += 1;
    }
  }
  return removed;
}

function copyDirRecursive(src, dest) {
  fs.mkdirSync(dest, { recursive: true });
  for (const entry of fs.readdirSync(src, { withFileTypes: true })) {
    const srcPath = path.join(src, entry.name);
    const destPath = path.join(dest, entry.name);
    if (entry.isDirectory()) {
      copyDirRecursive(srcPath, destPath);
    } else {
      fs.copyFileSync(srcPath, destPath);
    }
  }
}

function packUnityPackage() {
  const tmpRoot = fs.mkdtempSync(path.join(os.tmpdir(), "tjgenerators-pack-"));
  const extractDir = path.join(tmpRoot, "extract");

  fs.mkdirSync(extractDir, { recursive: true });

  const tgzName = execSync("npm pack --pack-destination .", {
    cwd: REPO_ROOT,
    stdio: ["ignore", "pipe", "inherit"],
    encoding: "utf8",
  }).trim();

  if (!tgzName.endsWith(".tgz")) {
    throw new Error(`npm pack returned unexpected output: ${JSON.stringify(tgzName)}`);
  }

  const tgzPath = path.join(REPO_ROOT, tgzName);

  try {
    execSync(`tar -xzf "${tgzPath}" -C "${extractDir}"`, {
      cwd: REPO_ROOT,
      stdio: "inherit",
    });

    const extractedPackage = path.join(extractDir, "package");
    if (!fs.existsSync(extractedPackage)) {
      throw new Error(`Expected "package" directory inside ${tgzName}`);
    }

    return { extractedPackage, tmpRoot };
  } finally {
    fs.rmSync(tgzPath, { force: true });
  }
}

function installUnityPackage(outDir, extractedPackage) {
  const packageDest = path.join(outDir, "Packages", "cn.tuanjie.ai.generators");
  fs.rmSync(packageDest, { recursive: true, force: true });
  fs.mkdirSync(path.dirname(packageDest), { recursive: true });
  copyDirRecursive(extractedPackage, packageDest);
}

function createArchive(outDir) {
  const parent = path.dirname(outDir);
  const base = path.basename(outDir);
  const zipPath = path.join(parent, `${base}.zip`);

  fs.rmSync(zipPath, { force: true });

  try {
    execSync(`tar -a -c -f "${zipPath}" -C "${parent}" "${base}"`, {
      stdio: "inherit",
    });
  } catch {
    execSync(`zip -r "${zipPath}" "${base}"`, {
      cwd: parent,
      stdio: "inherit",
    });
  }

  return zipPath;
}

function countFiles(dir) {
  let count = 0;
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    if (entry.isDirectory()) count += countFiles(path.join(dir, entry.name));
    else count += 1;
  }
  return count;
}

function cleanBuildOutput() {
  const buildDir = path.join(REPO_ROOT, "build");
  const buildMeta = path.join(REPO_ROOT, "build.meta");

  fs.rmSync(buildDir, { recursive: true, force: true });
  fs.rmSync(buildMeta, { force: true });
}

function main() {
  const options = parseArgs(process.argv.slice(2));
  const extensionDir = path.join(REPO_ROOT, "extension");

  console.log("TJGenerators bundle build");
  console.log(`  repo:      ${REPO_ROOT}`);
  console.log(`  extension: ${extensionDir}`);
  console.log(`  output:    ${options.outDir}`);

  console.log("Cleaning previous build output...");
  cleanBuildOutput();

  assertExtensionReady(extensionDir);

  console.log("Packing Unity package (npm pack + .npmignore)...");
  const { extractedPackage, tmpRoot } = packUnityPackage();

  try {
    fs.rmSync(options.outDir, { recursive: true, force: true });
    fs.mkdirSync(options.outDir, { recursive: true });

    console.log("Copying extension assets...");
    copyExtensionAssets(extensionDir, options.outDir, { excludeMeta: true });

    installUnityPackage(options.outDir, extractedPackage);
  } finally {
    fs.rmSync(tmpRoot, { recursive: true, force: true });
  }

  if (!options.keepMeta) {
    let removed = 0;
    for (const rel of EXTENSION_COPY_PATHS) {
      if (rel.endsWith(".json")) continue;
      removed += removeMetaFiles(path.join(options.outDir, rel));
    }
    console.log(`Removed ${removed} .meta file(s) from extension assets`);
  }

  if (options.archive) {
    const zipPath = createArchive(options.outDir);
    fs.rmSync(options.outDir, { recursive: true, force: true });
    const sizeMb = (fs.statSync(zipPath).size / (1024 * 1024)).toFixed(2);
    console.log(`Archive: ${zipPath} (${sizeMb} MB)`);
  } else {
    const fileCount = countFiles(options.outDir);
    console.log(`Bundle ready: ${options.outDir} (${fileCount} files)`);
  }
}

main();
