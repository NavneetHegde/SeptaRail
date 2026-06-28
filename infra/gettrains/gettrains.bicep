@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param septa_env_outputs_azure_container_apps_environment_default_domain string

param septa_env_outputs_azure_container_apps_environment_id string

param gettrains_containerimage string

param gettrains_identity_outputs_id string

param funcstorage97a68_outputs_blobendpoint string

param funcstorage97a68_outputs_queueendpoint string

param funcstorage97a68_outputs_tableendpoint string

param funcstorage97a68_outputs_datalakeendpoint string

param gettrains_identity_outputs_clientid string

param septa_env_outputs_azure_container_registry_endpoint string

param septa_env_outputs_azure_container_registry_managed_identity_id string

resource gettrains 'Microsoft.App/containerApps@2025-10-02-preview' = {
  name: 'gettrains'
  location: location
  properties: {
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: 8080
        transport: 'http'
      }
      registries: [
        {
          server: septa_env_outputs_azure_container_registry_endpoint
          identity: septa_env_outputs_azure_container_registry_managed_identity_id
        }
      ]
      runtime: {
        dotnet: {
          autoConfigureDataProtection: true
        }
      }
    }
    environmentId: septa_env_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: gettrains_containerimage
          name: 'gettrains'
          env: [
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY'
              value: 'in_memory'
            }
            {
              name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
              value: 'true'
            }
            {
              name: 'FUNCTIONS_WORKER_RUNTIME'
              value: 'dotnet-isolated'
            }
            {
              name: 'AzureFunctionsJobHost__telemetryMode'
              value: 'OpenTelemetry'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'AzureWebJobsStorage__blobServiceUri'
              value: funcstorage97a68_outputs_blobendpoint
            }
            {
              name: 'AzureWebJobsStorage__queueServiceUri'
              value: funcstorage97a68_outputs_queueendpoint
            }
            {
              name: 'AzureWebJobsStorage__tableServiceUri'
              value: funcstorage97a68_outputs_tableendpoint
            }
            {
              name: 'AzureWebJobsStorage__dataLakeServiceUri'
              value: funcstorage97a68_outputs_datalakeendpoint
            }
            {
              name: 'Aspire__Azure__Storage__Blobs__AzureWebJobsStorage__ServiceUri'
              value: funcstorage97a68_outputs_blobendpoint
            }
            {
              name: 'Aspire__Azure__Storage__Queues__AzureWebJobsStorage__ServiceUri'
              value: funcstorage97a68_outputs_queueendpoint
            }
            {
              name: 'Aspire__Azure__Data__Tables__AzureWebJobsStorage__ServiceUri'
              value: funcstorage97a68_outputs_tableendpoint
            }
            {
              name: 'Aspire__Azure__Storage__Files__DataLake__AzureWebJobsStorage__ServiceUri'
              value: funcstorage97a68_outputs_datalakeendpoint
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: gettrains_identity_outputs_clientid
            }
            {
              name: 'AZURE_TOKEN_CREDENTIALS'
              value: 'ManagedIdentityCredential'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${gettrains_identity_outputs_id}': { }
      '${septa_env_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
  kind: 'functionapp'
}