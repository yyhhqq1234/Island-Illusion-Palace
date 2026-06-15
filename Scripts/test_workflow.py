import requests, json, time

SERVER = 'http://10.150.164.64:8189'

print("=== game-character-design (NPU修复验证) ===")
r = requests.post(f'{SERVER}/workflows/execute', json={
    'workflow_id': 'game-character-design'
}, timeout=60)
print(f'Status: {r.status_code}')
data = r.json()
print(json.dumps(data, ensure_ascii=False)[:400])

pid = data.get('prompt_id')
if not pid:
    print('No prompt_id'); exit(1)
print(f'\nprompt_id={pid}, waiting...')

for i in range(50):
    time.sleep(5)
    try:
        q = requests.get(f'{SERVER}/comfy/queue', timeout=5).json()
    except Exception as e:
        continue
    in_q = any(item[1] == pid for lst in [q.get('queue_running',[]), q.get('queue_pending',[])] for item in lst)
    if not in_q:
        hist = requests.get(f'{SERVER}/comfy/history/{pid}', timeout=5).json()
        if pid in hist:
            entry = hist[pid]
            si = entry.get('status', {})
            sstr = si.get('status_str','?')
            print(f'status_str: {sstr}')
            if sstr == 'error':
                msgs = si.get('messages',[])
                for mt, md in reversed(msgs):
                    if mt == 'execution_error':
                        print(f'NODE: {md.get("node_type","?")}')
                        print(f'EXCEPTION: {md.get("exception_message","?")[:500]}')
                        break
                    elif mt == 'binary_error':
                        print(f'BINARY ERROR: {md.get("message","?")[:300]}')
            else:
                outputs = entry.get('outputs',{})
                for nid, ndata in outputs.items():
                    imgs = ndata.get('images',[])
                    if imgs:
                        fn = imgs[0]['filename']
                        print(f'IMAGE: {fn}')
                        dl = requests.get(f"{SERVER}/comfy/view?filename={fn}",timeout=30)
                        p = "d:/Program Files/Unity/U3Dproject/Island-Illusion-Palace/Assets/ArtMaterials/Boss/时空守护者/Concept/test_gcd_npu.png"
                        open(p,'wb').write(dl.content); print(f'Saved: {p}')
            break
    print(f'  wait {(i+1)*5}s...')
else: print('TIMEOUT')
