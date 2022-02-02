import-module au

$releases = 'https://github.com/veler/DevToys/releases'

function global:au_GetLatest {
     $download_page = Invoke-WebRequest -Uri $releases -UseBasicParsing
     $regex   = '.msixbundle$'
     $url     = $download_page.links | ? href -match $regex | select -First 1 -expand href
     $url     = 'https://github.com' + $url
     $version = $url -split '/' | select -Last 1 -Skip 1
     return @{ Version = $version; URL32 = $url }
}

function global:au_SearchReplace {
    @{
        "tools\chocolateyInstall.ps1" = @{
            "(^[$]url32\s*=\s*)('.*')"      = "`$1'$($Latest.URL32)'"
            "(^[$]checksum32\s*=\s*)('.*')" = "`$1'$($Latest.Checksum32)'"
        }
    }
}

update