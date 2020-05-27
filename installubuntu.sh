#!/bin/bash

sudo apt -y install net-tools
sudo apt -y install wpasupplicant
sudo apt -y install wireless-tools
sudo ifconfig wlan0 up
wget https://raw.githubusercontent.com/hargrave81/Aquamonitor/master/wifi.sh
sudo cp wifi.sh /etc/wifi.sh
sudo chmod u+x wifi.sh
sudo timedatectl set-timezone America/New_York
sudo apt -y install vsftpd
sudo systemctl start vsftpd
sudo systemctl enable vsftpd
sudo useradd -p $(openssl passwd -1 "RemoteAccess1") "remote"
sudo ufw allow 20/tcp
sudo ufw allow 21/tcp
wget https://raw.githubusercontent.com/hargrave81/Aquamonitor/master/vsftpd.conf
sudo cp vsftp.conf /etc/vsftpd.conf
sudo systemctl restart vsftpd.service
wget https://download.visualstudio.microsoft.com/download/pr/f2e1cb4a-0c70-49b6-871c-ebdea5ebf09d/acb1ea0c0dbaface9e19796083fe1a6b/dotnet-sdk-3.1.300-linux-arm.tar.gz
wget https://download.visualstudio.microsoft.com/download/pr/06f9feeb-cd19-49e9-a5cd-a230e1d8c52f/a232fbb4a6e6a90bbe624225e180308a/aspnetcore-runtime-3.1.4-linux-arm.tar.gz
sudo mkdir /usr/share/dotnet
sudo tar zxf dotnet-sdk-3.1.300-linux-arm.tar.gz -C /usr/share/dotnet
sudo tar zxf aspnetcore-runtime-3.1.4-linux-arm.tar.gz -C /usr/share/dotnet
wget https://raw.githubusercontent.com/hargrave81/Aquamonitor/master/profile
sudo cp profile ~/.profile
sudo apt-get -y install nginx
wget https://raw.githubusercontent.com/hargrave81/Aquamonitor/master/default.conf
sudo cp default.conf /etc/nginx/sites-available/default
sudo nginx -t
sudo nginx -s reload
sudo ufw allow 80/tcp
sudo apt-get -y install unzip
wget https://raw.githubusercontent.com/hargrave81/Aquamonitor/master/Aquadeployservice
wget https://raw.githubusercontent.com/hargrave81/Aquamonitor/master/Aquamonitorservice
sudo cp Aquadeployservice /etc/systemd/system/kestrel-aquadeploy.service
sudo cp Aquamonitorservice /etc/systemd/system/kestrel-aquamonitor.service
sudo mkdir /usr/local/wwwroot
wget https://github.com/hargrave81/Aquamonitor/raw/master/AquaMonitor32.zip
wget https://github.com/hargrave81/Aquamonitor/raw/master/AquaDeploy32.zip
sudo unzip -o AquaMonitor32.zip -d /usr/local/wwwroot/Publish
sudo unzip -o AquaDeploy32.zip -d /usr/local/aquadeploy
sudo systemctl enable kestrel-aquamonitor.service
sudo systemctl start kestrel-aquamonitor.service
sudo systemctl enable kestrel-aquadeploy.service
sudo systemctl start kestrel-aquadeploy.service
sudo iwlist wlan0 scan | grep ESSID
echo "To enable wifi please do the following ..."
echo "Locate your wifi (case sensative) and enter it into the following command"
echo "wpa_passphrase MyRouterSSID \"myssidcasesensative\" | sudo tee /etc/wpa_supplicant.conf"
echo "then run crontab -e (select nano or your preferred editor) and add this line at the very end"
echo "@reboot /etc/wifi.sh"
echo "if your wifi doesnt work on reboot, simply uncomment the DCHP lines in the /etc/wifi.sh file"