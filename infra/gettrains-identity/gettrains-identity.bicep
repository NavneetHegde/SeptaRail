@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource gettrains_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: take('gettrains_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = gettrains_identity.id

output clientId string = gettrains_identity.properties.clientId

output principalId string = gettrains_identity.properties.principalId

output principalName string = gettrains_identity.name

output name string = gettrains_identity.name