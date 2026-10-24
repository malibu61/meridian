param([string]$OutputDirectory=(Join-Path (Split-Path -Parent $PSScriptRoot) 'Assets\Resources\Audio\Radio'),[switch]$FleetOnly,[switch]$RescueOnly)
$ErrorActionPreference='Stop'
# Only uses the locally installed Turkish voice. No registry changes, installs or network calls.
$radioLines=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RadioLines.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$radioMobLines=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'RescueRadioLines.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$radioLines=@($radioLines)+@($radioMobLines)
if($FleetOnly){$radioLines=@($radioLines | Where-Object {$_.id -match '^(barbaros_|anadolu_|ruzgar_|fleet_)'})}
if($RescueOnly){$radioLines=@($radioLines | Where-Object {$_.id -match '^mob_'})}
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$radioVoice=New-Object -ComObject SAPI.SpVoice
$radioToken=New-Object -ComObject SAPI.SpObjectToken
$radioToken.SetId('HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens\MSTTS_V110_trTR_Tolga')
$radioVoice.Voice=$radioToken
$radioVoice.Rate=0
$radioVoice.Volume=95
try {
    foreach($radioLine in $radioLines) {
        if($radioLine.id -notmatch '^[a-z0-9_]+$'){throw 'Invalid clip ID'}
        $radioStream=New-Object -ComObject SAPI.SpFileStream
        try {
            $radioStream.Format.Type=22
            $radioStream.Open((Join-Path $OutputDirectory ($radioLine.id+'.wav')),3,$false)
            $radioVoice.AudioOutputStream=$radioStream
            [void]$radioVoice.Speak([string]$radioLine.text,0)
            $radioStream.Close()
        } finally { [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($radioStream) }
    }
    Write-Output ('RADIO_GENERATION_PASS clips='+$radioLines.Count)
} finally {
    [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($radioToken)
    [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($radioVoice)
}
