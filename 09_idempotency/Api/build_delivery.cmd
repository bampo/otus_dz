set IMAGE_TAG=monguard2/otus_dz_8_delivery_api:1.0
set DIR=Delivery
docker build -t %IMAGE_TAG% -f %DIR%/Dockerfile .
docker push %IMAGE_TAG%