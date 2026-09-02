const fs = require('fs');
const https = require('https');
const http = require('http');

// 从 package.json 读取版本号
const packageJson = JSON.parse(fs.readFileSync('./package.json', 'utf8'));
const version = packageJson.version;

// 配置
const API_BASE_URL = process.env.API_BASE_URL || 'https://codely.tuanjie.cn';
const PLUGIN_NAME = process.env.PLUGIN_NAME || 'unity-mcp-server';
const RELEASE_NOTE = process.env.RELEASE_NOTE || `Auto-registered from CI/CD pipeline - Version ${version}`;
// A canary release is any version like #.#.#-exp.# (e.g. 1.0.67-exp.3).
const IS_CANARY_RELEASE = /^\d+\.\d+\.\d+-exp\.\d+$/.test(version);

// 构建请求数据
const data = JSON.stringify({
  plugin_name: PLUGIN_NAME,
  version: version,
  release_note: RELEASE_NOTE,
  active: true,
  is_canary_version: IS_CANARY_RELEASE
});

// 解析 URL
const url = new URL(`${API_BASE_URL}/api/plugin/register`);
const client = url.protocol === 'https:' ? https : http;

const options = {
  hostname: url.hostname,
  port: url.port || (url.protocol === 'https:' ? 443 : 80),
  path: url.pathname,
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Content-Length': data.length
  }
};

console.log(`Registering ${PLUGIN_NAME} version ${version}...`);
console.log(`API URL: ${API_BASE_URL}/api/plugin/register`);
console.log(`Canary release: ${IS_CANARY_RELEASE}`);

const req = client.request(options, (res) => {
  let responseData = '';

  res.on('data', (chunk) => {
    responseData += chunk;
  });

  res.on('end', () => {
    if (res.statusCode >= 200 && res.statusCode < 300) {
      console.log('✅ Successfully registered plugin version');
      console.log('Response:', responseData);
      try {
        const parsed = JSON.parse(responseData);
        console.log(`Protocol Version: ${parsed.protocol_version}`);
      } catch (e) {
        // Ignore parse errors
      }
      process.exit(0);
    } else {
      console.error('❌ Failed to register plugin version');
      console.error(`Status: ${res.statusCode}`);
      console.error('Response:', responseData);
      process.exit(1);
    }
  });
});

req.on('error', (error) => {
  console.error('❌ Request error:', error);
  process.exit(1);
});

req.write(data);
req.end();

