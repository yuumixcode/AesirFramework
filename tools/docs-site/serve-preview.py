#!/usr/bin/env python3
"""本地预览服务器:禁用缓存,避免改样式后浏览器仍用旧 CSS。"""

import functools
import http.server
import socketserver
import sys

PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 8900
ROOT = sys.argv[2] if len(sys.argv) > 2 else "site"


class NoCacheHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0")
        self.send_header("Pragma", "no-cache")
        self.send_header("Expires", "0")
        super().end_headers()

    def log_message(self, fmt, *args):  # 静默
        pass


socketserver.TCPServer.allow_reuse_address = True
with socketserver.TCPServer(
    ("127.0.0.1", PORT),
    functools.partial(NoCacheHandler, directory=ROOT),
) as httpd:
    print(f"serving {ROOT} on http://127.0.0.1:{PORT} (no-cache)")
    httpd.serve_forever()
