set IMAGE_TAG=monguard2/otus_dz_8_api_gateway:1.0
set DIR=ApiGateway
docker build -t %IMAGE_TAG% -f %DIR%/Dockerfile .
docker push %IMAGE_TAG%