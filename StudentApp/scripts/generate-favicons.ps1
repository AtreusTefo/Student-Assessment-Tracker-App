[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

function Get-NonTransparentBounds {
  param(
    [Parameter(Mandatory = $true)]
    [System.Drawing.Bitmap]$Bitmap
  )

  $minX = $Bitmap.Width
  $minY = $Bitmap.Height
  $maxX = -1
  $maxY = -1

  for ($y = 0; $y -lt $Bitmap.Height; $y++) {
    for ($x = 0; $x -lt $Bitmap.Width; $x++) {
      if ($Bitmap.GetPixel($x, $y).A -gt 0) {
        if ($x -lt $minX) { $minX = $x }
        if ($y -lt $minY) { $minY = $y }
        if ($x -gt $maxX) { $maxX = $x }
        if ($y -gt $maxY) { $maxY = $y }
      }
    }
  }

  if ($maxX -lt 0) {
    return $null
  }

  return [PSCustomObject]@{
    MinX = $minX
    MinY = $minY
    MaxX = $maxX
    MaxY = $maxY
  }
}

function New-CroppedBitmap {
  param(
    [Parameter(Mandatory = $true)]
    [System.Drawing.Bitmap]$Source,
    [Parameter(Mandatory = $true)]
    [int]$Left,
    [Parameter(Mandatory = $true)]
    [int]$Top,
    [Parameter(Mandatory = $true)]
    [int]$Width,
    [Parameter(Mandatory = $true)]
    [int]$Height
  )

  $crop = New-Object System.Drawing.Bitmap($Width, $Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $graphics = [System.Drawing.Graphics]::FromImage($crop)

  try {
    $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
    $graphics.DrawImage(
      $Source,
      (New-Object System.Drawing.Rectangle(0, 0, $Width, $Height)),
      (New-Object System.Drawing.Rectangle($Left, $Top, $Width, $Height)),
      [System.Drawing.GraphicsUnit]::Pixel
    )
  } finally {
    $graphics.Dispose()
  }

  return $crop
}

function Save-ScaledPng {
  param(
    [Parameter(Mandatory = $true)]
    [System.Drawing.Bitmap]$Source,
    [Parameter(Mandatory = $true)]
    [int]$TargetSize,
    [Parameter(Mandatory = $true)]
    [string]$OutputPath
  )

  $icon = New-Object System.Drawing.Bitmap($TargetSize, $TargetSize, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
  $graphics = [System.Drawing.Graphics]::FromImage($icon)

  try {
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceOver
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

    $usablePixels = [Math]::Max(1, [int][Math]::Floor($TargetSize * 0.94))
    $sourceLongestSide = [Math]::Max($Source.Width, $Source.Height)
    $scale = [double]$usablePixels / [double]$sourceLongestSide

    $drawWidth = [Math]::Max(1, [int][Math]::Round($Source.Width * $scale))
    $drawHeight = [Math]::Max(1, [int][Math]::Round($Source.Height * $scale))
    $drawX = [int][Math]::Floor(($TargetSize - $drawWidth) / 2)
    $drawY = [int][Math]::Floor(($TargetSize - $drawHeight) / 2)

    $graphics.DrawImage(
      $Source,
      (New-Object System.Drawing.Rectangle($drawX, $drawY, $drawWidth, $drawHeight)),
      (New-Object System.Drawing.Rectangle(0, 0, $Source.Width, $Source.Height)),
      [System.Drawing.GraphicsUnit]::Pixel
    )

    $icon.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
  } finally {
    $graphics.Dispose()
    $icon.Dispose()
  }
}

function Write-IcoFile {
  param(
    [Parameter(Mandatory = $true)]
    [string]$OutputPath,
    [Parameter(Mandatory = $true)]
    [object[]]$Frames
  )

  $stream = [System.IO.File]::Open($OutputPath, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write)
  $writer = New-Object System.IO.BinaryWriter($stream)

  try {
    $count = [uint16]$Frames.Count
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write($count)

    $offset = 6 + (16 * $Frames.Count)

    foreach ($frame in $Frames) {
      $size = [int]$frame.Size
      $bytes = [byte[]]$frame.Bytes
      $widthByte = if ($size -ge 256) { [byte]0 } else { [byte]$size }
      $heightByte = if ($size -ge 256) { [byte]0 } else { [byte]$size }

      $writer.Write($widthByte)
      $writer.Write($heightByte)
      $writer.Write([byte]0)
      $writer.Write([byte]0)
      $writer.Write([uint16]1)
      $writer.Write([uint16]32)
      $writer.Write([uint32]$bytes.Length)
      $writer.Write([uint32]$offset)

      $offset += $bytes.Length
    }

    foreach ($frame in $Frames) {
      $writer.Write([byte[]]$frame.Bytes)
    }
  } finally {
    $writer.Dispose()
    $stream.Dispose()
  }
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$publicDirPath = Join-Path $scriptDir '..\public'
if (-not (Test-Path -Path $publicDirPath -PathType Container)) {
  throw "Missing public directory at '$publicDirPath'."
}
$publicDir = (Resolve-Path $publicDirPath).Path

$sourcePath = Join-Path $publicDir 'favicon-64x64.png'
if (-not (Test-Path -Path $sourcePath -PathType Leaf)) {
  throw "Missing source favicon at '$sourcePath'."
}

$loadedSourceBitmap = [System.Drawing.Bitmap]::FromFile($sourcePath)
$sourceBitmap = New-Object System.Drawing.Bitmap($loadedSourceBitmap)
$loadedSourceBitmap.Dispose()

try {
  $bounds = Get-NonTransparentBounds -Bitmap $sourceBitmap
  if ($null -eq $bounds) {
    throw "Source favicon has no visible (non-transparent) pixels."
  }

  $left = [Math]::Max(0, $bounds.MinX - 1)
  $top = [Math]::Max(0, $bounds.MinY - 1)
  $right = [Math]::Min($sourceBitmap.Width - 1, $bounds.MaxX + 1)
  $bottom = [Math]::Min($sourceBitmap.Height - 1, $bounds.MaxY + 1)

  $cropWidth = $right - $left + 1
  $cropHeight = $bottom - $top + 1
  $cropped = New-CroppedBitmap -Source $sourceBitmap -Left $left -Top $top -Width $cropWidth -Height $cropHeight

  try {
    $targets = @(
      @{ Size = 16; Name = 'favicon-16x16.png' },
      @{ Size = 32; Name = 'favicon-32x32.png' },
      @{ Size = 64; Name = 'favicon-64x64.png' },
      @{ Size = 180; Name = 'apple-touch-icon.png' }
    )

    foreach ($target in $targets) {
      $outputPath = Join-Path $publicDir $target.Name
      Save-ScaledPng -Source $cropped -TargetSize $target.Size -OutputPath $outputPath
      Write-Host "Generated $($target.Name)"
    }
  } finally {
    $cropped.Dispose()
  }
} finally {
  $sourceBitmap.Dispose()
}

$favicon64Path = Join-Path $publicDir 'favicon-64x64.png'
$faviconFallbackPath = Join-Path $publicDir 'favicon.png'
Copy-Item -Path $favicon64Path -Destination $faviconFallbackPath -Force
Write-Host "Generated favicon.png (64x64 fallback)"

$icoFrames = @()
foreach ($size in 16, 32, 64) {
  $framePath = Join-Path $publicDir ("favicon-{0}x{0}.png" -f $size)
  $icoFrames += [PSCustomObject]@{
    Size = $size
    Bytes = [System.IO.File]::ReadAllBytes($framePath)
  }
}

$icoPath = Join-Path $publicDir 'favicon.ico'
Write-IcoFile -OutputPath $icoPath -Frames $icoFrames
Write-Host "Generated favicon.ico"

Write-Host "Favicon generation complete."
