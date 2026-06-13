# ComfyUI 美术资产生成脚本 v2
# 用法: .\comfyui_generate.ps1 "时空守护者" concept
#       .\comfyui_generate.ps1 "时空守护者" sprite idle

param(
    [string]$AssetType = "",
    [string]$Stage = "concept",
    [string]$State = "idle"
)

$ErrorActionPreference = "Stop"
$ServerUrl = "http://10.150.164.64:8188"
$OutputBase = "d:\Program Files\Unity\U3Dproject\Island-Illusion-Palace\Assets\ArtMaterials\Boss"

# ==========================================
# 提示词 (使用 here-string 避免解析问题)
# ==========================================
$ConceptBasePositive = @'
(dark fantasy concept art:1.3), (digital painting:1.2), dramatic lighting, (soul glow: blue-purple luminescence #4A3A8C), (crystal erosion: geometric cyan crystals #00D4FF), rim light, high contrast, detailed texture, atmospheric perspective
'@

$ConceptNegative = @'
photorealistic, 3D render, CGI, smooth vector art, flat design, bright sunny, cartoon, anime, chibi, pixel art, low resolution, blurry, watermark, text, signature, ugly, distorted, bad anatomy, extra limbs, fused fingers, low quality, jpeg artifacts
'@

$SpriteBasePositive = @'
(pixel art:1.4), (2D top-down game sprite:1.3), dark fantasy, (soul glow: blue-purple luminescence #4A3A8C), (crystal erosion: geometric cyan crystals #00D4FF), cel-shading, 1px outline, transparent background, dark atmospheric, high contrast, limited color palette
'@

$SpriteNegative = @'
photorealistic, 3D render, CGI, smooth vector art, flat design, bright sunny, cartoon, anime, chibi, painterly texture, visible brushstrokes, blurry, watermark, text, signature, ugly, distorted, bad anatomy, extra limbs, fused fingers, low quality, jpeg artifacts, anti-aliasing
'@

# ==========================================
# 时空守护者专属提示词 (here-string)
# ==========================================
$GuardianPrompt = @'
(time guardian boss:1.4), massive armored entity, clockwork mechanisms embedded in armor, (golden #C8A848 sacred markings:1.2), floating time gears, hourglass core, imposing stance, ancient guardian, 4-heads-tall
'@

# ==========================================
# 状态提示词
# ==========================================
$StatePrompts = @{
    "idle"   = "standing idle pose, neutral stance, breathing slightly"
    "walk"   = "walking forward, stepping motion, dynamic movement"
    "attack" = "mid-swing attack pose, arm raised, energy flowing, action pose"
    "skill"  = "casting spell, energy swirling, dramatic power pose"
    "death"  = "falling down, collapsing, dissolving into particles"
}

# ==========================================
# 组合提示词
# ==========================================
$statePromptText = $StatePrompts[$State]
if (-not $statePromptText) { $statePromptText = "idle pose" }

if ($Stage -eq "concept") {
    $positivePrompt = "$ConceptBasePositive, $GuardianPrompt"
    $negativePrompt = $ConceptNegative
    $genWidth = 1024
    $genHeight = 1024
    $outputDir = "$OutputBase\$AssetType\Concept"
} else {
    $positivePrompt = "$SpriteBasePositive, $GuardianPrompt, $statePromptText, single frame, pixel art character"
    $negativePrompt = $SpriteNegative
    $scale = 8
    $genWidth = 512
    $genHeight = 512
    $outputDir = "$OutputBase\$AssetType\Frames\$State"
}

# 确保输出目录存在
$null = New-Item -ItemType Directory -Path $outputDir -Force

# ==========================================
# 显示信息
# ==========================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " ComfyUI 资产生成" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "资产: $AssetType | 阶段: $Stage | 状态: $State" -ForegroundColor Yellow
Write-Host "输出: $outputDir" -ForegroundColor Yellow
Write-Host ""
Write-Host "正向提示词:" -ForegroundColor Green
Write-Host "  $positivePrompt" -ForegroundColor Gray
Write-Host "反向提示词:" -ForegroundColor Red
Write-Host "  $negativePrompt" -ForegroundColor Gray
Write-Host "尺寸: ${genWidth}x${genHeight}" -ForegroundColor Gray

# ==========================================
# 构建 Workflow JSON
# ==========================================
$seed = Get-Random -Maximum 2147483647

$model = "动漫 primemix_v21.safetensors"
$sampler = "euler_ancestral"
$scheduler = "normal"
$steps = 20
$cfg = 7.0

$workflow = @{
    "1" = @{
        class_type = "CheckpointLoaderSimple"
        inputs = @{ ckpt_name = $model }
    }
    "2" = @{
        class_type = "CLIPTextEncode"
        inputs = @{ text = $positivePrompt; clip = @("1", 1) }
    }
    "3" = @{
        class_type = "CLIPTextEncode"
        inputs = @{ text = $negativePrompt; clip = @("1", 1) }
    }
    "4" = @{
        class_type = "EmptyLatentImage"
        inputs = @{ width = $genWidth; height = $genHeight; batch_size = 1 }
    }
    "5" = @{
        class_type = "KSampler"
        inputs = @{
            seed = $seed
            steps = $steps
            cfg = $cfg
            sampler_name = $sampler
            scheduler = $scheduler
            denoise = 1
            model = @("1", 0)
            positive = @("2", 0)
            negative = @("3", 0)
            latent_image = @("4", 0)
        }
    }
    "6" = @{
        class_type = "VAEDecode"
        inputs = @{ samples = @("5", 0); vae = @("1", 2) }
    }
    "7" = @{
        class_type = "SaveImageWebsocket"
        inputs = @{ images = @("6", 0) }
    }
}

$workflowJson = $workflow | ConvertTo-Json -Depth 10 -Compress

# ==========================================
# 提交任务
# ==========================================
Write-Host ""
Write-Host "提交任务到 ComfyUI ($ServerUrl)..." -ForegroundColor Cyan

$requestBody = @{
    prompt = ($workflowJson | ConvertFrom-Json)
    client_id = "cli-generator"
} | ConvertTo-Json -Depth 20 -Compress

$response = Invoke-RestMethod -Uri "$ServerUrl/api/prompt" -Method POST -Body $requestBody -ContentType "application/json" -TimeoutSec 30
$promptId = $response.prompt_id

if (-not $promptId) {
    Write-Error "提交失败!"
    exit 1
}

Write-Host "任务已提交! prompt_id: $promptId" -ForegroundColor Green

# ==========================================
# 轮询等待完成
# ==========================================
Write-Host "等待生成完成..." -ForegroundColor Cyan
$maxWait = 600
$completed = $false

for ($i = 0; $i -lt $maxWait; $i++) {
    Start-Sleep -Seconds 2

    try {
        $history = Invoke-RestMethod -Uri "$ServerUrl/api/history/$promptId" -TimeoutSec 10
        $historyJson = ($history | ConvertTo-Json -Depth 5 -Compress)

        if ($historyJson.Length -gt 10 -and $historyJson -ne "{}") {
            $completed = $true
            break
        }
    } catch {
        # 忽略轮询错误
    }

    if (($i + 1) % 15 -eq 0) {
        Write-Host "  已等待 $(($i + 1) * 2)秒..." -ForegroundColor Gray
    }
}

if (-not $completed) {
    Write-Error "生成超时 (20分钟)"
    exit 1
}

Write-Host "生成完成!" -ForegroundColor Green

# ==========================================
# 解析和下载图片
# ==========================================
$historyObj = $history | ConvertTo-Json -Depth 10 | ConvertFrom-Json
$images = @()

foreach ($pk in $historyObj.PSObject.Properties) {
    $outputs = $historyObj.($pk.Name).outputs
    if ($outputs) {
        foreach ($nk in $outputs.PSObject.Properties) {
            $imgs = $outputs.($nk.Name).images
            if ($imgs) {
                foreach ($img in $imgs) {
                    $images += $img
                }
            }
        }
    }
}

if ($images.Count -eq 0) {
    Write-Error "没有输出图片"
    exit 1
}

Write-Host "获取到 $($images.Count) 张图片" -ForegroundColor Green

# 下载
$ts = Get-Date -Format "yyyyMMdd_HHmmss"
$saved = @()

for ($i = 0; $i -lt $images.Count; $i++) {
    $img = $images[$i]

    # 使用 UriBuilder 构建 URL，避免 & 在字符串中的解析问题
    $builder = New-Object System.UriBuilder("$ServerUrl/api/view")
    $query = [System.Web.HttpUtility]::ParseQueryString("")
    $query["filename"] = $img.filename
    $query["subfolder"] = $img.subfolder
    $query["type"] = $img.type
    $builder.Query = $query.ToString()
    $dlUri = $builder.Uri

    if ($Stage -eq "concept") {
        $saveName = $AssetType + "_Concept_" + $ts + ".png"
    } else {
        $saveName = $AssetType + "_" + $State + "_" + ($i + 1).ToString() + ".png"
    }

    $savePath = Join-Path $outputDir $saveName
    Write-Host "Download: $($img.filename) -> $savePath"

    Invoke-WebRequest -Uri $dlUri -OutFile $savePath -TimeoutSec 60
    $saved += $savePath
    Write-Host "  Saved!" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " 完成!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "输出: $outputDir" -ForegroundColor Yellow
foreach ($f in $saved) {
    $msg = "  " + $f
    Write-Host $msg -ForegroundColor White
}