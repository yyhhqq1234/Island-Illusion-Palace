"""
CommsClient - ComfyUI Game Art Production Server Client SDK

Copy this file to your client project directory.
Usage:
    from comms_client import CommsClient
    client = CommsClient()  # 自动从 config.yaml 读取地址

Dependencies (install once):
    pip install requests websocket-client
"""

import json
import os
import time
import urllib.request
import urllib.error
from typing import Optional, Dict, Any, List, Callable


# 从 config_manager 读取配置，不可用时回退到默认值
try:
    from config_manager import get_config
    _cfg = get_config()
    DEFAULT_SERVER = _cfg.comms_url
    DEFAULT_WS = _cfg.comms_ws_url
except Exception:
    DEFAULT_SERVER = "http://127.0.0.1:8189"
    DEFAULT_WS = "ws://127.0.0.1:8189"


class CommsClient:
    """Client for ComfyUI Game Art Production Server.

    Supports two modes:
      - HTTP: Simple, single-call, ideal for scripts/batch jobs
      - WebSocket: Real-time, long-running, with event callbacks
    """

    def __init__(self, server_url: str = DEFAULT_SERVER,
                 ws_url: str = DEFAULT_WS,
                 auto_reconnect: bool = True,
                 ping_interval: int = 30,
                 ping_timeout: int = 10):
        self.server_url = server_url.rstrip("/")
        self.ws_url = ws_url
        self.auto_reconnect = auto_reconnect
        self.ping_interval = ping_interval
        self.ping_timeout = ping_timeout
        self.client_id: Optional[str] = None
        self.last_status: Dict[str, Any] = {}
        self._connected = False
        self._handlers: Dict[str, List[Callable]] = {}
        self._ws = None

    # ===================================================================
    #  HTTP API (static methods — no WebSocket needed)
    # ===================================================================

    @staticmethod
    def _http_request(method: str, path: str, server: str = DEFAULT_SERVER,
                      data: dict = None) -> dict:
        url = f"{server}{path}"
        body = json.dumps(data).encode() if data else None
        req = urllib.request.Request(url, data=body, method=method)
        if body:
            req.add_header("Content-Type", "application/json")
        try:
            with urllib.request.urlopen(req, timeout=30) as resp:
                return json.loads(resp.read().decode())
        except urllib.error.HTTPError as e:
            return {"error": f"HTTP {e.code}: {e.reason}", "status_code": e.code}
        except Exception as e:
            return {"error": str(e)}

    @staticmethod
    def http_status(server: str = DEFAULT_SERVER) -> dict:
        """Get server status (GPU/queue/clients)."""
        return CommsClient._http_request("GET", "/status", server)

    @staticmethod
    def http_health(server: str = DEFAULT_SERVER) -> dict:
        """Health check."""
        return CommsClient._http_request("GET", "/health", server)

    @staticmethod
    def http_list_workflows(server: str = DEFAULT_SERVER) -> dict:
        """List all workflows + blueprints (8 total)."""
        return CommsClient._http_request("GET", "/workflows", server)

    @staticmethod
    def http_list_registered(server: str = DEFAULT_SERVER) -> dict:
        """List registered workflows only (4)."""
        return CommsClient._http_request("GET", "/workflows/registered", server)

    @staticmethod
    def http_list_all(server: str = DEFAULT_SERVER) -> dict:
        """List all (alias for /workflows)."""
        return CommsClient._http_request("GET", "/workflows/all", server)

    @staticmethod
    def http_get_workflow(workflow_id: str, server: str = DEFAULT_SERVER) -> dict:
        """Get workflow detail by ID."""
        return CommsClient._http_request("GET", f"/workflows/{workflow_id}", server)

    @staticmethod
    def http_execute_workflow(workflow_id: str, params: dict = None,
                                server: str = DEFAULT_SERVER) -> dict:
        """Execute a workflow or blueprint (unified entry).

        Args:
            workflow_id: ID from /workflows list (works for both workflows & blueprints)
            params: Parameters depending on the workflow/blueprint
                    Common params: prompt, negative_prompt, seed, steps, cfg, width, height
                    Blueprint params vary: check /blueprints/{id} first
            server: Server URL override

        Returns:
            {"prompt_id": "...", "workflow_name": "..."} on success
            {"error": "..."} on failure
        """
        payload = {"workflow_id": workflow_id}
        if params:
            payload["params"] = params
        return CommsClient._http_request("POST", "/workflows/execute", server, payload)

    @staticmethod
    def http_list_blueprints(server: str = DEFAULT_SERVER) -> dict:
        """List all blueprints (30 total)."""
        return CommsClient._http_request("GET", "/blueprints", server)

    @staticmethod
    def http_get_blueprint(blueprint_id: str, server: str = DEFAULT_SERVER) -> dict:
        """Get blueprint detail including params and blueprint_json."""
        return CommsClient._http_request("GET", f"/blueprints/{blueprint_id}", server)

    @staticmethod
    def http_execute_blueprint(blueprint_id: str, params: dict = None,
                                 server: str = DEFAULT_SERVER) -> dict:
        """Execute a blueprint directly."""
        payload = {"blueprint_id": blueprint_id}
        if params:
            payload["params"] = params
        return CommsClient._http_request("POST", "/blueprints/execute", server, payload)

    @staticmethod
    def http_queue(server: str = DEFAULT_SERVER) -> dict:
        """Get current queue status."""
        return CommsClient._http_request("GET", "/comfy/queue", server)

    @staticmethod
    def http_history(prompt_id: str, server: str = DEFAULT_SERVER) -> dict:
        """Get task result by prompt_id."""
        return CommsClient._http_request("GET", f"/comfy/history/{prompt_id}", server)

    @staticmethod
    def http_view(filename: str, subfolder: str = "", file_type: str = "output",
                   server: str = DEFAULT_SERVER) -> bytes:
        """Download generated file. Returns raw bytes."""
        params = urllib.parse.urlencode({"filename": filename, "subfolder": subfolder,
                                          "type": file_type})
        url = f"{server}/comfy/view?{params}"
        with urllib.request.urlopen(url, timeout=120) as resp:
            return resp.read()

    @staticmethod
    def http_interrupt(server: str = DEFAULT_SERVER) -> dict:
        """Interrupt current running task."""
        return CommsClient._http_request("POST", "/comfy/interrupt", server)

    @staticmethod
    def http_free(server: str = DEFAULT_SERVER) -> dict:
        """Free models and release VRAM."""
        return CommsClient._http_request("POST", "/comfy/free", server)

    # ===================================================================
    #  Result waiting helpers
    # ===================================================================

    @staticmethod
    def wait_for_result(prompt_id: str, server: str = DEFAULT_SERVER,
                          timeout: int = 300, poll_interval: int = 3) -> dict:
        """Poll queue until task completes, then fetch outputs.

        Returns:
            {
                "status": "done" | "timeout" | "error",
                "outputs": [
                    {"filename": "ComfyUI_00001_.png", "url": "http://...", "type": "output"},
                    ...
                ]
            }
        """
        deadline = time.time() + timeout
        while time.time() < deadline:
            q = CommsClient.http_queue(server)
            in_queue = False
            for item in q.get("queue_running", []):
                if len(item) > 1 and item[1] == prompt_id:
                    in_queue = True; break
            for item in q.get("queue_pending", []):
                if len(item) > 1 and item[1] == prompt_id:
                    in_queue = True; break

            if not in_queue:
                hist = CommsClient.http_history(prompt_id, server)
                if prompt_id in hist:
                    outputs = []
                    for node_id, node_out in hist[prompt_id].get("outputs", {}).items():
                        for img in node_out.get("images", []):
                            fname = img["filename"]
                            sf = img.get("subfolder", "")
                            ft = img.get("type", "output")
                            url = f"{server}/comfy/view?filename={fname}&subfolder={sf}&type={ft}"
                            outputs.append({"filename": fname, "url": url, "type": ft})
                        for vid in node_out.get("gifs", []):
                            url = f"{server}/comfy/view?filename={vid['filename']}&type=output"
                            outputs.append({"filename": vid["filename"], "url": url, "type": "video"})
                    return {"status": "done", "outputs": outputs}
                elif "error" not in hist:
                    return {"status": "not_found", "outputs": []}

            time.sleep(poll_interval)

        return {"status": "timeout", "outputs": []}

    # ===================================================================
    #  Quick execute + download (one-shot convenience)
    # ===================================================================

    @staticmethod
    def quick_generate(workflow_id: str, params: dict, save_dir: str = ".",
                         server: str = DEFAULT_SERVER, timeout: int = 300) -> list:
        """Execute workflow and download all output files.

        Returns list of saved file paths.
        """
        os_path = __import__("os").path
        result = CommsClient.http_execute_workflow(workflow_id, params, server)
        if "error" in result:
            raise RuntimeError(f"Execute failed: {result['error']}")

        prompt_id = result["prompt_id"]
        wf_name = result.get("workflow_name") or result.get("blueprint_name", workflow_id)
        print(f"[ComfyUI] Submitted: {wf_name} (prompt_id: {prompt_id})")

        out = CommsClient.wait_for_result(prompt_id, server, timeout)
        if out["status"] != "done":
            raise RuntimeError(f"Task {out['status']}")

        saved = []
        for f in out["outputs"]:
            data = CommsClient.http_view(f["filename"], f.get("subfolder", ""),
                                          f.get("type", "output"), server)
            path = os_path.join(save_dir, f["filename"])
            with open(path, "wb") as fp:
                fp.write(data)
            saved.append(path)
            print(f"[ComfyUI] Downloaded: {f['filename']}")
        return saved

    # ===================================================================
    #  WebSocket mode (optional, for real-time apps)
    # ===================================================================

    def on(self, event_type: str):
        """Register event callback decorator.
        
        Events: connected, disconnected, error, welcome,
                 server_status, workflow_list, blueprint_list,
                 workflow_detail, blueprint_detail,
                 workflow_executed, blueprint_executed,
                 hot_reload, chat, ack
        """
        def decorator(func):
            self._handlers.setdefault(event_type, []).append(func)
            return func
        return decorator

    def _emit(self, event_type: str, data: dict):
        for handler in self._handlers.get(event_type, []):
            try:
                handler(data)
            except Exception:
                pass

    def connect(self, blocking: bool = True):
        """Connect via WebSocket. Requires: pip install websocket-client

        When auto_reconnect=True (default), the connection will automatically
        reconnect after disconnection. ping_interval (30s default) keeps the
        connection alive by sending WebSocket protocol-level ping frames.
        """
        self._connect_ws(blocking)

    def _connect_ws(self, blocking: bool):
        if self.auto_reconnect:
            reconnect_delay = max(1, self.ping_interval // 3)
        else:
            reconnect_delay = 0
        import websocket
        self._ws = websocket.WebSocketApp(
            self.ws_url,
            on_open=self._on_ws_open,
            on_message=self._on_ws_message,
            on_error=self._on_ws_error,
            on_close=self._on_ws_close,
        )
        if blocking:
            self._ws.run_forever(
                ping_interval=self.ping_interval,
                ping_timeout=self.ping_timeout,
                reconnect=reconnect_delay,
            )
        else:
            import threading
            t = threading.Thread(
                target=self._ws.run_forever,
                kwargs={
                    "ping_interval": self.ping_interval,
                    "ping_timeout": self.ping_timeout,
                    "reconnect": reconnect_delay,
                },
                daemon=True,
            )
            t.start()

    def disconnect(self):
        """Disconnect and stop auto-reconnect if enabled."""
        if self._ws:
            self.auto_reconnect = False
            self._ws.keep_running = False
            self._ws.close()
            self._ws = None
            self._connected = False

    def wait_connected(self, timeout: int = 10):
        deadline = time.time() + timeout
        while not self._connected and time.time() < deadline:
            time.sleep(0.1)
        return self._connected

    @property
    def is_connected(self):
        return self._connected

    # -- WS send methods --

    def request_status(self):
        self._send({"type": "request_status", "data": {}})

    def get_history(self, count: int = 50):
        self._send({"type": "get_history", "data": {"count": count}})

    def register(self, metadata: dict = None):
        self._send({"type": "register", "data": {"metadata": metadata or {}}})

    def chat(self, message: str):
        self._send({"type": "chat", "data": {"message": message}})

    def ping(self):
        """Send application-level ping (for latency measurement).

        Note: WebSocket protocol-level keepalive pings are sent automatically
        every ping_interval seconds via run_forever() — no manual call needed.
        """
        self._send({"type": "ping", "data": {}})

    def request_priority(self, reason: str = ""):
        self._send({"type": "submit_priority", "data": {"reason": reason}})

    def list_workflows(self, category: str = None):
        d = {} if category is None else {"category": category}
        self._send({"type": "list_workflows", "data": d})

    def get_workflow(self, workflow_id: str):
        self._send({"type": "get_workflow", "data": {"workflow_id": workflow_id}})

    def execute_workflow(self, workflow_id: str, params: dict = None):
        d = {"workflow_id": workflow_id}
        if params:
            d["params"] = params
        self._send({"type": "execute_workflow", "data": d})

    def list_blueprints(self, category: str = None):
        d = {} if category is None else {"category": category}
        self._send({"type": "list_blueprints", "data": d})

    def get_blueprint(self, blueprint_id: str):
        self._send({"type": "get_blueprint", "data": {"blueprint_id": blueprint_id}})

    def execute_blueprint(self, blueprint_id: str, params: dict = None):
        d = {"blueprint_id": blueprint_id}
        if params:
            d["params"] = params
        self._send({"type": "execute_blueprint", "data": d})

    # -- WS internals --

    def _send(self, msg: dict):
        if self._ws and self._ws.sock and self._ws.sock.connected:
            self._ws.send(json.dumps(msg))

    def _on_ws_open(self, ws):
        was_connected = self._connected
        self._connected = True
        if was_connected:
            # Reconnection — re-register with server
            self.register()
            self._emit("reconnected", {})
        else:
            self._emit("connected", {})

    def _on_ws_message(self, ws, message):
        try:
            msg = json.loads(message)
            event_type = msg.get("type", "unknown")
            data = msg.get("data", {})
            if event_type == "welcome":
                self.client_id = data.get("client_id")
                self.last_status = data.get("initial_status", {})
            elif event_type == "server_status":
                self.last_status = data
            self._emit(event_type, data)
        except (json.JSONDecodeError, KeyError):
            pass

    def _on_ws_error(self, ws, error):
        self._emit("error", {"message": str(error)})

    def _on_ws_close(self, ws, close_status_code, close_msg):
        self._connected = False
        self._emit("disconnected", {
            "code": close_status_code,
            "reason": str(close_msg) if close_msg else "",
            "auto_reconnect": self.auto_reconnect,
        })


# ===================================================================
#  Convenience: module-level quick functions (no class needed)
# ===================================================================

def list_workflows(server: str = DEFAULT_SERVER) -> dict:
    """Quick: list all 8 workflows+blueprints."""
    return CommsClient.http_list_workflows(server)


def generate(workflow_id: str, params: dict = None,
             server: str = DEFAULT_SERVER, wait: bool = True,
             save_dir: str = ".") -> dict:
    """One-line generate. Returns prompt_id or file paths if wait=True.

    Usage:
        from comms_client import generate
        # Submit only
        r = generate("game-item-icon", {"prompt": "golden sword"})
        # Submit + wait + download
        files = generate("image-blur", {}, save_dir="output")
    """
    result = CommsClient.http_execute_workflow(workflow_id, params, server)
    if "error" in result:
        return result
    pid = result["prompt_id"]
    name = result.get("workflow_name") or result.get("blueprint_name", workflow_id)
    print(f"[ComfyUI] Submitted: {name} (pid: {pid})")
    if not wait:
        return {"prompt_id": pid, "name": name}

    out = CommsClient.wait_for_result(pid, server)
    if out["status"] != "done":
        return {"prompt_id": pid, "status": out["status"]}

    os_path = __import__("os").path
    files = []
    for f in out["outputs"]:
        data = CommsClient.http_view(f["filename"], f.get("subfolder", ""),
                                      f.get("type", "output"), server)
        path = os_path.join(save_dir, f["filename"])
        with open(path, "wb") as fp:
            fp.write(data)
        files.append(path)
    return {"prompt_id": pid, "files": files}


if __name__ == "__main__":
    # Demo: list all workflows
    print("=" * 60)
    print("ComfyUI Client Demo - Connect to server")
    print("=" * 60)

    s = CommsClient.http_status()
    gpu = s.get("gpu", {})
    print(f"Server:   {DEFAULT_SERVER}")
    print(f"GPU:      {gpu.get('name', '?')}")
    print(f"VRAM:     {gpu.get('vram_used', '?')}MB / {gpu.get('vram_total', '?')}MB")
    print(f"Clients:  {s.get('connected_clients', 0)}")
    print()

    data = CommsClient.http_list_workflows()
    wfs = data.get("workflows", {})
    bps = data.get("blueprints", {})
    print(f"Workflows: {len(wfs)}")
    for wid, w in wfs.items():
        print(f"  {wid}: {w.get('name', '?')} [{w.get('speed', '?')}]")
    print()
    print(f"Blueprints: {len(bps)}")
    cats = {}
    for bid, b in bps.items():
        cats.setdefault(b.get("category", "other"), []).append(bid)
    for cat, ids in sorted(cats.items()):
        print(f"  [{cat}] {len(ids)}: {ids[:3]}...")
