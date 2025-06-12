set IMAGE_TAG=monguard2/otus_kurs_stubs_api:1.0
set DIR=Services/Stubs
docker build -t %IMAGE_TAG% -f %DIR%/Dockerfile .
docker push %IMAGE_TAG%