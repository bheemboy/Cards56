@echo off
ssh -i C:\Users\surehman\Nextcloud\AppConfigs\OracleCloud\MyInstance-1\ssh\myinstance-1.key ubuntu@129.146.65.5
scp -i C:\Users\surehman\Nextcloud\AppConfigs\OracleCloud\MyInstance-1\ssh\myinstance-1.key C:\Projects\Cards56\oracle.56cards.com\docker-compose.yml ubuntu@129.146.65.5:/home/ubuntu

pause
