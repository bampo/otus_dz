kubectl create ns myns

rem helm install pg16 bitnami/postgresql -f pg-values.yml --namespace myns

rem helm install rabbit bitnami/rabbitmq -f rmq-values.yml --namespace myns  

kubectl apply -f init-svcs.yml

kubectl apply -f gate.yml