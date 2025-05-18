kubectl create ns myns

helm install pg16 bitnami/postgresql -f pg-values.yml --namespace myns

helm install rabbit bitnami/rabbitmq -f rmq-values.yml --namespace myns  

kubectl apply -f init-svcs.yml

kubectl apply -f gate.yml