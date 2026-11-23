#!/usr/bin/env node
/**
 * 多断点视觉回归截图:桌面 / 平板 / 手机 × 浅色 / 暗色。
 * 用法:node tools/docs-site/shoot.mjs [outDir] [baseUrl]（主仓根运行;baseUrl 默认 serve-preview.py 的 8900）
 *
 * Playwright 可能只装在全局,这里做一次回退解析。
 */
import { createRequire } from "node:module";
import { execSync } from "node:child_process";
import { mkdir } from "node:fs/promises";

async function loadPlaywright() {
  try {
    return await import("playwright");
  } catch {
    const root = execSync("npm root -g", { encoding: "utf8" }).trim();
    const require = createRequire(`${root}/`);
    return require("playwright");
  }
}

const { chromium } = await loadPlaywright();

const OUT = process.argv[2] ?? "/tmp/aesir-shots";
const BASE = process.argv[3] ?? "http://127.0.0.1:8900";

const PAGES = [
  { name: "home", url: "/" },
  { name: "doc", url: "/product/features/" },
  {
    name: "api",
    url: "/architecture/scripting-api/Runestone/AesirArchitecture/ObservableValue%7BT%7D/",
  },
];

const VIEWPORTS = [
  { name: "desktop", width: 1440, height: 900 },
  { name: "tablet", width: 834, height: 1000 },
  { name: "mobile", width: 390, height: 844 },
];

const SCHEMES = ["light", "dark"];

await mkdir(OUT, { recursive: true });
const browser = await chromium.launch();

for (const vp of VIEWPORTS) {
  for (const scheme of SCHEMES) {
    const ctx = await browser.newContext({
      viewport: { width: vp.width, height: vp.height },
      deviceScaleFactor: 2,
      colorScheme: scheme,
    });
    for (const p of PAGES) {
      const page = await ctx.newPage();
      await page.goto(BASE + p.url, { waitUntil: "networkidle" });
      await page.evaluate(() => document.fonts.ready);
      await page.waitForTimeout(250);
      const file = `${OUT}/${p.name}-${vp.name}-${scheme}.png`;
      await page.screenshot({ path: file, fullPage: false });
      console.log("saved", file);
      await page.close();
    }
    await ctx.close();
  }
}

await browser.close();
