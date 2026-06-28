@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource septa_env_acr 'Microsoft.ContainerRegistry/registries@2025-04-01' = {
  name: take('septaenvacr${uniqueString(resourceGroup().id)}', 50)
  location: location
  sku: {
    name: 'Basic'
  }
  tags: {
    'aspire-resource-name': 'septa-env-acr'
  }
}

output name string = septa_env_acr.name

output loginServer string = septa_env_acr.properties.loginServer

output id string = septa_env_acr.id