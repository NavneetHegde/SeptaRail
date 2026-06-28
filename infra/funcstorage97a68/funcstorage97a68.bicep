@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource funcstorage97a68 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: take('funcstorage97a68${uniqueString(resourceGroup().id)}', 24)
  kind: 'StorageV2'
  location: location
  sku: {
    name: 'Standard_GRS'
  }
  properties: {
    accessTier: 'Hot'
    allowSharedKeyAccess: false
    isHnsEnabled: false
    minimumTlsVersion: 'TLS1_2'
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
  tags: {
    'aspire-resource-name': 'funcstorage97a68'
  }
}

output blobEndpoint string = funcstorage97a68.properties.primaryEndpoints.blob

output dataLakeEndpoint string = funcstorage97a68.properties.primaryEndpoints.dfs

output queueEndpoint string = funcstorage97a68.properties.primaryEndpoints.queue

output tableEndpoint string = funcstorage97a68.properties.primaryEndpoints.table

output name string = funcstorage97a68.name

output id string = funcstorage97a68.id