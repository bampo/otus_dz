set IMAGE_TAG=monguard2/otus_kurs_notification_api:1.0
set DIR=Services/Notification
docker build -t %IMAGE_TAG% -f %DIR%/Dockerfile .
docker push %IMAGE_TAG%