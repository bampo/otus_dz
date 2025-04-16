set IMAGE_NAME=monguard2/otus_dz_7_orders_svc:1.0
set DOCKERFILE_PATH=./Order/Order.Service/Dockerfile
docker build -t %IMAGE_NAME% -f %DOCKERFILE_PATH% .
docker push %IMAGE_NAME%
