#!/bin/bash
sudo /usr/share/dotnet/dotnet publish AquaMonitor/AquaMonitor.csproj -c Release -o /home/pi/Documents/aquamonitor/Publish -r linux-arm
zip -9 -r AquaMonitor32b.zip Publish/*
sudo systemctl stop kestrel-aquamonitor.service
sudo unzip -o AquaMonitor32b.zip -d /usr/local/wwwroot
sudo systemctl start kestrel-aquamonitor.service

## Linux Subsys
## sudo dotnet publish AquaMonitor/AquaMonitor.csproj -c Release -o Publish32 -r linux-arm
## 7z a AquaMonitor32b.zip ./Publish32/{*,.[!.]*}