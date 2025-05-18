set IMAGE_TAG=monguard2/otus_dz_8_payments_api:1.0
set DIR=Payments
docker build -t %IMAGE_TAG% -f %DIR%/Dockerfile .
docker push %IMAGE_TAG%