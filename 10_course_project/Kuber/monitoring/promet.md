## Установите Prometheus в Minikube с помощью Helm
Для минимальной настройки используем kube-prometheus-stack, который включает Prometheus, Grafana и необходимые компоненты.

- Добавьте Helm-репозиторий:

  ```
  helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
  helm repo update
  ```

- Создайте namespace для мониторинга:

  `kubectl create namespace monitoring`

- Установите kube-prometheus-stack в минимальной конфигурации:
Создайте файл prometheus-values.yaml для минимальной настройки:

  ```
  prometheus:
    prometheusSpec:
      resources:
        requests:
          memory: "512Mi"
          cpu: "250m"
        limits:
          memory: "1Gi"
          cpu: "500m"
      serviceMonitorSelectorNilUsesHelmValues: false
  grafana:
    enabled: false # Отключаем Grafana для минимализма
  alertmanager:
    enabled: false # Отключаем Alertmanager
  kubeStateMetrics:
    enabled: false # Отключаем kube-state-metrics
  prometheusOperator:
    enabled: true
  prometheus-node-exporter:
    enabled: false # Отключаем node-exporter
  ```
- Установите чарт:

  `helm install prometheus prometheus-community/kube-prometheus-stack -n monitoring -f prometheus-values.yaml`

- Проверьте установку:
  Убедитесь, что поды Prometheus запущены:

`kubectl get pods -n monitoring`

Вы должны увидеть поды, например `prometheus-kube-prometheus-prometheus-0.`

## Настройте ServiceMonitor для сбора метрик:
  Создайте файл service-monitor.yaml для Prometheus, чтобы он собирал метрики с вашего сервиса:
  yaml

```
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: dotnet-app-monitor
  namespace: monitoring
  labels:
    release: prometheus
spec:
  selector:
    matchLabels:
      app: dotnet-app
  endpoints:
  - port: http
    path: /metrics
    interval: 15s
  namespaceSelector:
    matchNames:
    - default
```

- Примените манифест:

  `kubectl apply -f service-monitor.yaml`

## Проверьте сбор метрик
  Порт-форвардинг для доступа к Prometheus:

`kubectl port-forward -n monitoring svc/prometheus-kube-prometheus-prometheus 9090:9090`

Откройте http://localhost:9090 в браузере.

- роверьте цели (targets):
  В веб-интерфейсе Prometheus перейдите в Status > Targets. Вы должны увидеть цель default/dotnet-app-service:80 в состоянии UP.

- Выполните запрос PromQL:
  В интерфейсе Prometheus введите запрос, например:

`rate(requests_total[5m])`

Это покажет скорость запросов к вашему приложению за последние 5 минут.

## (Опционально) Настройка Grafana для визуализации
  Если хотите визуализировать метрики, включите Grafana в prometheus-values.yaml:
  yaml

```
grafana:
  enabled: true
  resources:
    requests:
      memory: "256Mi"
      cpu: "100m"
    limits:
      memory: "512Mi"
      cpu: "200m"
```

- Переустановите чарт:

  `helm upgrade prometheus prometheus-community/kube-prometheus-stack -n monitoring -f prometheus-values.yaml`

- Получите доступ к Grafana:

  `kubectl port-forward -n monitoring svc/prometheus-grafana 3000:80`

  Откройте http://localhost:3000 (логин: admin, пароль: найдите через `kubectl get secret -n monitoring prometheus-grafana -o jsonpath='{.data.admin-password}' | base64 --decode)`.

  Добавьте Prometheus как источник данных (URL: http://prometheus-kube-prometheus-prometheus:9090) и создайте дашборд для метрики requests_total.




