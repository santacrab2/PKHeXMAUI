@echo off
rem Windows 桌面调试版启动器：不需要打包 APK，改完代码重新 build 后双击即可
rem
rem 用法：
rem   1) 先 build：
rem      dotnet build PKHeX.MAUI.csproj -p:TargetFrameworks=net10.0-windows10.0.26100.0 -p:WindowsPackageType=None -c Debug
rem   2) 双击本文件启动
rem
rem DOTNET_ROOT 默认指向用户目录下的 .dotnet（便携安装）。
rem 若本机没装在默认位置，或装在别处，改下面这行填自己的路径。
set "DOTNET_ROOT=%USERPROFILE%\.dotnet"
set "EXE=%~dp0bin\Debug\net10.0-windows10.0.26100.0\win-x64\PKHeX.MAUI.exe"
if not exist "%EXE%" (
  echo 未找到 Windows 编译产物，请先执行：
  echo   dotnet build PKHeX.MAUI.csproj -p:TargetFrameworks=net10.0-windows10.0.26100.0 -p:WindowsPackageType=None -c Debug
  pause
  exit /b 1
)
start "" "%EXE%"
