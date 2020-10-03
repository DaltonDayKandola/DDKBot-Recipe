az login

az account set --subscription "a9d17eebe-012f-4dac-a186-3154db2c619d"

az storage account create --name ddkstorageaccount01 --resource-group ddkbot-group-1 --location uksouth --sku Standard_LRS --encryption-services blob

az ad signed-in-user show --query objectId -o tsv | az role assignment create --role "Storage Blob Data Contributor" --assignee "william.dalton@ddklimited.com" --scope "/subscriptions/9d17eebe-012f-4dac-a186-3154db2c619d/resourceGroups/ddkbot-group-1/providers/Microsoft.Storage/storageAccounts/ddkstorageaccount01"

az storage container create --account-name ddkstorageaccount01 --name ddkcontainer01 --auth-mode login