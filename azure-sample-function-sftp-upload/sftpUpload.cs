using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Text;

namespace sftpUpload
{
    public static class sftpUpload
    {
        // Http client for key vault
        private static HttpClient kvhttpClient = new HttpClient();

        [FunctionName("sftpUpload")]
        public static async System.Threading.Tasks.Task RunAsync([BlobTrigger("uploads/{name}", Connection = "storageAcct")]Stream myBlob, string name, ILogger log)
        {
            // Sample Logging
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            // Grab Secrets and Keys from Azure KeyVault
            // Reference: https://medium.com/statuscode/getting-key-vault-secrets-in-azure-functions-37620fd20a0b 
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback), kvhttpClient);
            Microsoft.Azure.KeyVault.Models.SecretBundle secretBundle = await kvClient.GetSecretAsync(Environment.GetEnvironmentVariable("sftpkey"));

            // Convert key to stream data type for PrivateKeyFile variable
            byte[] byteArray = Encoding.ASCII.GetBytes(secretBundle.Value);
            MemoryStream sshkey = new MemoryStream(byteArray);

            // Using SSH.Net Library: https://github.com/sshnet/SSH.NET ; Other ssh/sftp libs can be found here: https://www.sftp.net/client-libraries
            // Sample code for SSH.NET: https://ourcodeworld.com/articles/read/369/how-to-access-a-sftp-server-using-ssh-net-sync-and-async-with-c-in-winforms
            PrivateKeyFile keyFile = new PrivateKeyFile(sshkey);
            var keyFiles = new[] { keyFile };

            // These variables can also be stored on key vault instead of local.settings.json. If it is stored in keyvault, you will need to change the ref. to that of the location of the keys.
            string host = Environment.GetEnvironmentVariable("serveraddress");
            string sftpUsername = Environment.GetEnvironmentVariable("sftpusername");
            string password = Environment.GetEnvironmentVariable("password");

            var methods = new List<AuthenticationMethod>();
            methods.Add(new PasswordAuthenticationMethod(sftpUsername, password));
            methods.Add(new PrivateKeyAuthenticationMethod(sftpUsername, keyFiles));

            // Connect to SFTP Server and Upload file 
            ConnectionInfo con = new ConnectionInfo(host, 22, sftpUsername, methods.ToArray());
            using (var client = new SftpClient(con))
            {
                client.Connect();
                client.UploadFile(myBlob, $"/uploads/{name}");

                var files = client.ListDirectory("/uploads");
                foreach (var file in files)
                {
                    log.LogInformation(file.Name);
                }
                client.Disconnect();
            }
        }
    }
}


