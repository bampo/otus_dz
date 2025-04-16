set IMAGE_NAME=monguard2/otus_dz_7_apigateway_svc:1.0
set DOCKERFILE_PATH=./ApiGateway/Dockerfile
docker build -t %IMAGE_NAME% -f %DOCKERFILE_PATH% .
docker push %IMAGE_NAME%
