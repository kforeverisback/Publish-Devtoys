#!/usr/bin/env bash

## Make sure dpkg-dev is installed in the Ubuntu/Debian system
## sudo apt install dpkg-dev debhelper -y

git submodule update --init --recursive
mkdir -p ./publish/x64/DevToys.Linux-linux-x64-portable

dotnet build submodules/DevToys/src/generators/ResxHelperGenerator/ResxHelperGenerator.csproj

#dotnet publish submodules/DevToys/src/app/dev/platforms/desktop/DevToys.CLI/DevToys.CLI.csproj -v minimal -c Release --nologo --sc -o ./publish/x64/DevToys.CLI-linux-x64-portable/ -r linux-x64 -p:DebugType=None -p:DebugSymbols=False -f net8.0 -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true

publish_dir=./publish/x64/DevToys.Linux-linux-x64-portable
dotnet publish submodules/DevToys/src/app/dev/platforms/desktop/DevToys.Linux/DevToys.Linux.csproj -v minimal -c Release --nologo -o "$publish_dir" -r linux-x64 -f net8.0 --sc -p:DebugType=None -p:DebugSymbols=False

cp submodules/DevToys/assets/logo/Icon-Windows-Linux.png "$publish_dir"

dotnet publish submodules/DevToys.Tools/src/DevToys.Tools/DevToys.Tools.csproj -v minimal -c Release --nologo -o "$publish_dir/Plugins/Devtoys.Tools" -r linux-x64 -f net8.0 -p:DebugType=None -p:DebugSymbols=False
