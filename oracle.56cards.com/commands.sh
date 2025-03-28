sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 80 -j ACCEPT
sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 443 -j ACCEPT
sudo netfilter-persistent save

sudo snap install docker
sudo groupadd docker
sudo usermod -aG docker $USER
sudo systemctl restart snap.docker.dockerd.service

git clone https://github.com/bheemboy/Cards56.git
cd Cards56
docker login
docker build -t bheemboy/cards56web:latest-arm64 -t bheemboy/cards56web:$(date +%Y.%m.%d)-arm64 .
docker push --all-tags bheemboy/cards56web


ssh -i "C:\Downloads\OracleCloud\MyInstance-1.key" ubuntu@129.146.65.5
scp -i C:\Users\surehman\Nextcloud\AppConfigs\OracleCloud\MyInstance-1\ssh\myinstance-1.key C:\Projects\Cards56\oracle.56cards.com\nginx.conf ubuntu@129.146.65.5:/home/ubuntu/
scp -i C:\Users\surehman\Nextcloud\AppConfigs\OracleCloud\MyInstance-1\ssh\myinstance-1.key C:\Projects\Cards56\oracle.56cards.com\docker-compose.yml ubuntu@129.146.65.5:/home/ubuntu
