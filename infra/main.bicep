targetScope = 'subscription'

param resourceGroupName string

param location string

param principalId string

resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
}

module septa_env_acr 'septa-env-acr/septa-env-acr.bicep' = {
  name: 'septa-env-acr'
  scope: rg
  params: {
    location: location
  }
}

module septa_env 'septa-env/septa-env.bicep' = {
  name: 'septa-env'
  scope: rg
  params: {
    location: location
    septa_env_acr_outputs_name: septa_env_acr.outputs.name
    userPrincipalId: principalId
  }
}

module funcstorage97a68 'funcstorage97a68/funcstorage97a68.bicep' = {
  name: 'funcstorage97a68'
  scope: rg
  params: {
    location: location
  }
}

module gettrains_identity 'gettrains-identity/gettrains-identity.bicep' = {
  name: 'gettrains-identity'
  scope: rg
  params: {
    location: location
  }
}

module gettrains_roles_funcstorage97a68 'gettrains-roles-funcstorage97a68/gettrains-roles-funcstorage97a68.bicep' = {
  name: 'gettrains-roles-funcstorage97a68'
  scope: rg
  params: {
    location: location
    funcstorage97a68_outputs_name: funcstorage97a68.outputs.name
    principalId: gettrains_identity.outputs.principalId
  }
}

output septa_env_AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = septa_env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN

output septa_env_AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = septa_env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID

output septa_env_AZURE_CONTAINER_REGISTRY_ENDPOINT string = septa_env.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT

output septa_env_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = septa_env.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID

output gettrains_identity_id string = gettrains_identity.outputs.id

output funcstorage97a68_blobEndpoint string = funcstorage97a68.outputs.blobEndpoint

output funcstorage97a68_queueEndpoint string = funcstorage97a68.outputs.queueEndpoint

output funcstorage97a68_tableEndpoint string = funcstorage97a68.outputs.tableEndpoint

output funcstorage97a68_dataLakeEndpoint string = funcstorage97a68.outputs.dataLakeEndpoint

output gettrains_identity_clientId string = gettrains_identity.outputs.clientId