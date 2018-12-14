# Azure Sample - Azure Function Blob File to SFTP 
The following repo contains an example of how to use a blob trigger to pick up a file from blob storage and upload to an SFTP Server using c#. 

Setup Steps: 

1. Setup or use and existing linux-based SFTP Server. 
    1. Launch an Azure Linux VM: https://docs.microsoft.com/en-us/azure/virtual-machines/linux/quick-create-portal
    2. Set up SSH with no shell access: https://www.digitalocean.com/community/tutorials/how-to-enable-sftp-without-shell-access-on-ubuntu-18-04

2. Create an Azure Key Vault to store ssh key
    1. Create an Azure Key Vault: https://docs.microsoft.com/en-us/azure/key-vault/quick-create-portal
    2. Upload your SSH Key to key vault: https://serverfault.com/questions/848168/putting-rsa-keys-into-azure-key-vault 
        1. Note that you can not upload a pem file via portal, you need to use the azure cli
    3. Copy your ssh keyvault location for use in local.settings.json - you can find this in the azure portal under secrets
    4. Copy your sftp server address, username and password for use in local.settings.json

3. Create a blob storage account
    1. Ex. https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-portal 
    2. Create a container called "upload"
    3. Copy your storage account connection string for use in local.settings.json
    
4. Launch the solution that you cloned, rename the local.settings.example.json file to local.settings.json and copy the appropriate credentials to the right corresponding variables. 

5. Build and test the solution. When you upload a file onto the blob storage container called upload, the corresponding file will be uploaded to the SFTP server you have setup. 
