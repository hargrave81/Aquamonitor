## Aquaponics System
### Raspberry Pi Setup (3 B+)
##### Quick install on a 32bit OS
sudo wget https://raw.githubusercontent.com/hargrave81/Aquamonitor/master/installubuntu.sh -v -O install.sh && sudo chmod +777 install.sh && sudo ./install.sh; sudo rm -rf install.sh
##### Initial Setup
- Download 20.04 x64 Server https://ubuntu.com/download/raspberry-pi/thank-you?version=20.04&architecture=arm64+raspi
  - (you may need to unzip the image if its in .xz format with 7Zip https://www.7-zip.org/download.html)
- Use balenaEtcher https://www.balena.io/etcher/ to burn the image to a microSD
- Boot your Raspberry Pi up with your freshly minted microSD card
- connect your device to LAN based internet
- login with ubuntu / ubuntu and give it a new password
- run the following commands to get things "setup" for server headless wireless mode
- sudo bash
- apt install net-tools
- apt install wpasupplicant
- apt install wireless-tools
- ifconfig wlan0 up
- iwlist wlan0 scan | grep ESSID
  locate the SSID that you wish to connect to and perform the following
- wpa_passphrase MyRouterSSID "myssidcasesensative" | tee /etc/wpa_supplicant.conf
- wpa_supplicant -i wlan0 -c /etc/wpa_supplicant.conf
  Ensure this command works before proceeding
- cd /etc
- nano wifi.sh        (place the contents of wifi.sh inside that file and save it using CTRL+O [enter], CTRL+X)
- chmod u+x wifi.sh
- crontab -e
  - here select nano (or your preferred editor)
- add the following command at the end of the file
  - @reboot /etc/wifi.sh    (CTRL+O [enter], CTRL+X to save an exit NANO)
- timedatectl set-timezone America/New_York
- reboot
  - if after a reboot you do not have network access do the following
  - edit your wifi.sh file and uncomment the two lines for DHCP

##### Install VSFTP
- sudo apt install vsftpd
- sudo systemctl start vsftpd
- sudo systemctl enable vsftpd
- sudo useradd –m remote
- sudo passwd remote
- sudo ufw allow 20/tcp
- sudo ufw allow 21/tcp
- sudo nano /etc/vsftpd.conf
  - Find the entry labeled write_enable=NO, and change the value to “YES.”
- sudo systemctl restart vsftpd.service 

##### Install ASPNET CORE 3.1 x64
- cd /home/ubuntu
- wget https://download.visualstudio.microsoft.com/download/pr/2ea7ea69-6110-4c39-a07c-bd4df663e49b/5d60f17a167a5696e63904f7a586d072/dotnet-sdk-3.1.102-linux-arm64.tar.gz
- wget https://download.visualstudio.microsoft.com/download/pr/ec985ae1-e15c-4858-b586-de5f78956573/f585f8ffc303bbca6a711ecd61417a40/aspnetcore-runtime-3.1.2-linux-arm64.tar.gz
- mkdir /home/ubuntu/dotnet-arm64
- tar zxf dotnet-sdk-3.1.102-linux-arm64.tar.gz -C /home/ubuntu/dotnet-arm64
- tar zxf aspnetcore-runtime-3.1.2-linux-arm64.tar.gz -C /home/ubuntu/dotnet-arm64
- sudo nano .profile
  - add the following lines to the end of the document and save it using CTRL+O [enter], CTRL+X
    # set .NET Core SDK and Runtime path
    export DOTNET_ROOT=$HOME/dotnet-arm64
    export PATH=$PATH:$HOME/dotnet-arm64

##### Install NGINX
- sudo apt-get install nginx
- sudo /etc/init.d/nginx start
- sudo nano /etc/nginx/sites-available/default
  - copy contents of default.conf, then restart nginx with following commands
- sudo nginx -t
- sudo nginx -s reload
- sudo ufw allow 80/tcp

##### Install AquaMonitor
- sudo apt-get install unzip
- sudo nano /etc/systemd/system/kestrel-aquamonitor.service
  - copy contents of Aquamonitorservice
- install the aquamonitor
  - sudo mkdir /usr/local/wwwroot
  - sudo mkdir /usr/local/wwwroot/Publish
  - wget https://github.com/hargrave81/Aquamonitor/raw/master/AquaMonitor.zip
  - wget https://github.com/hargrave81/Aquamonitor/raw/master/AquaDeploy.zip
  - sudo unzip -o AquaMonitor.zip -d /usr/local/wwwroot/Publish
  - sudo unzip -o AquaDeploy.zip -d /usr/local/aquadeploy
- sudo systemctl enable kestrel-aquamonitor.service
- sudo systemctl start kestrel-aquamonitor.service
- sudo systemctl status kestrel-aquamonitor.service
- sudo nano /etc/systemd/system/kestrel-aquadeploy.service
  - copy contents of Aquadeployservice
- sudo systemctl enable kestrel-aquadeploy.service
- sudo systemctl start kestrel-aquadeploy.service
- sudo systemctl status kestrel-aquadeploy.service


##### HTU21D support
Requires Raspbian OS
Requires linux-arm (not 64 builds)


##### Building source code - Aquamonitor
- On Build Box
dotnet publish ./AquaMonitor/AquaMonitor.csproj -c Release -o ./Publish -r linux-arm64
Compress-Archive -Path Publish/* -DestinationPath AquaMonitor.zip -CompressionLevel "Optimal" -Force
- on Raspberry Pi
sudo systemctl stop kestrel-aquamonitor.service
sudo unzip -o /home/remote/AquaMonitor.zip -d /usr/local/wwwroot/Publish
sudo systemctl start kestrel-aquamonitor.service

##### WebDeploy Helper
- On Build Box
dotnet publish ./WebDeploy/WebDeploy.csproj -c Release -o ./DeployLinux -r linux-arm64
Compress-Archive -Path DeployLinux/* -DestinationPath AquaDeploy.zip -CompressionLevel "Optimal" -Force
- On Raspberry Pi
  - transfer zip file to raspberry pi  
  - sudo systemctl stop kestrel-aquadeploy.service
  - sudo unzip -o /home/remote/AquaDeploy.zip -d /usr/local/aquadeploy
  - sudo systemctl start kestrel-aquadeploy.service    



a1B2c3d4e5f^
