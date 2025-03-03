# crud-api/templates/_helpers.tpl
{{- define "crud-api.name" -}}
    {{- if .Values.nameOverride }}
    {{- .Values.nameOverride | trunc 63 | trimSuffix "-" }}
    {{- else if contains .Chart.Name .Release.Name }}
    {{- .Release.Name | trunc 63 | trimSuffix "-" }}
  {{- else }}
    {{- printf "%s-%s" .Release.Name .Chart.Name | trunc 63 | trimSuffix "-" }}
  {{- end }}
{{- end }}

{{- define "crud-api.fullname" -}}
  {{- printf "%s-%s" .Release.Namespace (include "crud-api.name" .) | trunc 63 | trimSuffix "-" }}
{{- end }}