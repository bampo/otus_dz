set IMAGE_TAG=monguard2/otus_dz_9_orders_api:1.0
set DIR=Orders
docker build -t %IMAGE_TAG% -f %DIR%/Dockerfile .
docker push %IMAGE_TAG%