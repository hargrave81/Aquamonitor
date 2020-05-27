#!/bin/bash
sudo ifconfig wlan0 up
sleep 5
sudo killall wpa_supplicant
sleep 3
sudo wpa_supplicant -B -i wlan0 -c /etc/wpa_supplicant.conf
## this may be needed on raspberry pi 4 or other versions of raspberry pi if your networkcard does not get an IP automatically
## sleep 2
## sudo dhclient wlan0