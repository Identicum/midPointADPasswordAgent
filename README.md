# Agent

## Build Requirements

- msvc
- dotnet

## Build

### Build Filter DLL
Open Visual Studio x64 Command Prompt and run:
```batch
cl.exe /LD ADPasswordFilter.c /O2 /DEBUG:none /nologo /DWIN64 /link /DLL /DEFAULTLIB:advapi32.lib
```

### Build Agent
```batch
dotnet publish -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true
```

## Installation

- Copy ~ADPasswordFilter.dll~ into %WinDir%\System32\ADPasswordFilter.dll
- Copy ~Agent.exe~ into %ProgramFiles%\ADPasswordFilter\Agent.exe
- Adjust and copy ~config.json~ into %ProgramFiles%\ADPasswordFilter\config.json
- Install and start the BackgroundAgentService running the following commands:
```
SC CREATE BackgroundAgentService binpath="%ProgramFiles%\ADPasswordFilter\Agent.exe"
SC CONFIG BackgroundAgentService start=auto
SC START BackgroundAgentService
```
- Install the ADPasswordFilter.dll:
    1. Run the following command:
        ```
        REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter" /v "Agent" /t REG_SZ /d "%ProgramFiles%\ADPasswordFilter\Agent.exe"
        ```
    2. Manually append "ADPasswordFilter" to "Notification Packages" in "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa"

- Restart the Domain Controller.

## Windows Synchronization Service

Install Windows Service
```batch
SC CREATE BackgroundAgentService binpath="%ProgramFiles%\ADPasswordFilter\Agent.exe"
```

Start Windows Service
```batch
SC START BackgroundAgentService
```

Stop Windows Service
```batch
SC STOP BackgroundAgentService
```

Uninstall Windows Service
```batch
SC DELETE BackgroundAgentService
```

