set IMAGE_NAME=monguard2/otus_dz_7_users_svc:1.0
set DOCKERFILE_PATH=./User/User.Service/Dockerfile
docker build -t %IMAGE_NAME% -f %DOCKERFILE_PATH% .
docker push %IMAGE_NAME%
