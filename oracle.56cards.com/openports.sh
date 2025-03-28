sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 80 -j ACCEPT
sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 443 -j ACCEPT
sudo netfilter-persistent save

sudo snap install docker
sudo groupadd docker
sudo usermod -aG docker $USER
sudo systemctl restart snap.docker.dockerd.service

ssh -i "C:\Downloads\OracleCloud\MyInstance-1.key" ubuntu@129.146.65.5