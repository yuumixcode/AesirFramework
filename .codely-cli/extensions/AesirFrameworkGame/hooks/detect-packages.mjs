#!/usr/bin/env node
// Aesir 依赖门禁 hook —— SessionStart 时查明工程装没装 RAA/RAM，把结论注入上下文。
//
// 为什么存在：技能正文的依赖门禁第①步是"先探测工程实际装了什么"，写在文档里靠 AI 自觉执行，
// 可能被整段跳过直接硬写模块代码。本 hook 把那一步变成机械事实——AI 拿到的是结论而不是待办。
//
// 边界（刻意不做）：
//   - 不替代门禁②③：读工程记忆、询问用户是否安装，仍由 AI 在会话中执行。装包会改用户工程的
//     Packages/manifest.json 并联网下载，属需用户授权的动作，hook 绝不代劳。
//   - 不写任何文件（无状态缓存、无心跳日志）、不 spawn 子进程、不联网、不碰 Unity 编辑器。
//   - 只在 SessionStart 注入一次；会话中途装包由执行安装的 AI 自己知道，故文案声明快照时效。
//   - 非 Unity 工程（无 Assets/ 与 Packages/）零注入，不污染无关会话的上下文。
//
// 契约：stdin 收 {hook_event_name, cwd, ...}；stdout 只出 JSON
// （{"hookSpecificOutput":{"hookEventName","additionalContext"}} 或空对象表示放行）；
// 诊断走 stderr；任何异常一律静默放行——本 hook 绝不影响会话启动。
import fs from 'node:fs';
import path from 'node:path';

const PACKAGES = [
  {
    key: 'RAA',
    id: 'cn.runestone.aesir.architecture',
    inline: 'AesirArchitecture',
    branch: 'AesirArchitecture-latest',
    cnb: 'https://cnb.cool/runestone-game/AesirFramework.git#AesirArchitecture-latest',
    github: 'https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest',
  },
  {
    key: 'RAM',
    id: 'cn.runestone.aesir.modules',
    inline: 'AesirModules',
    branch: 'AesirModules-latest',
    cnb: 'https://cnb.cool/runestone-game/AesirFramework.git#AesirModules-latest',
    github: 'https://github.com/yuumixcode/AesirFramework.git#AesirModules-latest',
  },
];

const MODULE_SKILLS = 'aesir-ui / aesir-events / aesir-audio / aesir-scene';
const SNAPSHOT_NOTE = '本结论为会话启动快照；会话中途装过包后按 aesir-router 的探测方法复验。';

function emit(obj) {
  process.stdout.write(JSON.stringify(obj));
  process.exit(0);
}
function pass() {
  emit({});
}

// 三形态判定，命中即止：内联（unitypackage 装进 Assets）→ UPM 落地 → 仅声明。
// 声明用文本包含而非 JSON 解析：manifest 损坏时仍能认出安装意图，且天然免除解析异常。
function detect(root, pkg) {
  if (fs.existsSync(path.join(root, 'Assets', 'Runestone', pkg.inline))) return '已装(内联)';
  if (fs.existsSync(path.join(root, 'Packages', pkg.id, 'package.json'))) return '已装(UPM)';
  try {
    const cache = path.join(root, 'Library', 'PackageCache');
    if (fs.readdirSync(cache).some((n) => n.startsWith(pkg.id + '@'))) return '已装(UPM)';
  } catch {}
  for (const file of ['manifest.json', 'packages-lock.json']) {
    try {
      const text = fs.readFileSync(path.join(root, 'Packages', file), 'utf8');
      if (text.includes(pkg.id) || text.includes(pkg.branch)) return '已装(声明)';
    } catch {}
  }
  return null;
}

function scopeText(raa, ram) {
  if (raa && ram) return '12 个技能全部可用。';
  if (raa) {
    return `架构与工程初始化技能可用；${MODULE_SKILLS} 四个模块技能失效——缺 RAM，一个都编不过。`;
  }
  if (ram) {
    return '仅 aesir-project-structure 可用——RAM 缺 RAA 连编译都过不了。只缺 RAA 时不要手写 manifest 依赖（RAM 的 package.json 不声明它），走菜单 Tools/Aesir/Modules/Install Dependencies。';
  }
  return '仅 aesir-project-structure 可用（纯目录结构与文档知识，不依赖包）；其余技能全部失效——先装包。';
}

function installText(missing) {
  const lines = missing.map((p) => `${p.key}：CNB ${p.cnb} ｜ GitHub 兜底 ${p.github}`);
  return (
    '缺包处置（顺序不可颠倒）：先 read_memory（scope=all）看有无"不要安装"类明确禁止——有则不装，只用已装的那一面；' +
    '没有则必须 ask_user 询问，问句说清装哪个包、从哪装、装完会写 Packages/manifest.json 并从网络下载。' +
    '不得静默安装，也不得改用原生 SceneManager / 自建 Canvas 栈 / PlayOneShot / static event 总线顶替。' +
    '线路 CNB 优先、GitHub 兜底；两个都缺且用户同意时先 RAA 后 RAM。' +
    lines.join(' ')
  );
}

function run(cwd) {
  const root = path.resolve(cwd || '.');
  // 守卫：不是 Unity 工程就零注入
  if (!fs.existsSync(path.join(root, 'Assets')) || !fs.existsSync(path.join(root, 'Packages'))) return pass();

  const states = PACKAGES.map((p) => detect(root, p));
  const [raa, ram] = states;
  const missing = PACKAGES.filter((_, i) => !states[i]);
  const facts = PACKAGES.map((p, i) => `${p.key}=${states[i] || '未装'}`).join(' ');

  let ctx = `[Aesir 依赖门禁] ${facts} → ${scopeText(raa, ram)}`;
  if (missing.length) ctx += ' ' + installText(missing);
  ctx += ' ' + SNAPSHOT_NOTE;

  emit({ hookSpecificOutput: { hookEventName: 'SessionStart', additionalContext: ctx } });
}

let raw = '';
process.stdin.setEncoding('utf8');
process.stdin.on('data', (chunk) => {
  raw += chunk;
});
process.stdin.on('end', () => {
  try {
    const input = JSON.parse(raw.replace(/^\uFEFF/, ''));
    if (input.hook_event_name !== 'SessionStart') return pass();
    run(input.cwd);
  } catch (e) {
    console.error('aesir-package-gate error:', e && e.stack ? e.stack : String(e));
    pass();
  }
});