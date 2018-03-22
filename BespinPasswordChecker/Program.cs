using System;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace BespinPasswordChecker
{
    class Program
    {
        private string keyVaultURL = "https://pwned.vault.azure.net/";
        private string secretName = "SQLreader";
        private string servicesAuthConnectionString = "RunAs=App;AppId=d0335552-748f-4360-9b4d-d25a0478df10;TenantId=9656014e-20b5-4e57-8358-ae4c00dee57d;CertificateThumbprint=c5f575dd891a9af98bb6dfc33a56822a2d79d9ad;CertificateStoreLocation=CurrentUser";
        private string connectionString = "Server=tcp:bespin3.database.windows.net,1433;Initial Catalog=pwned;Persist Security Info=False;User ID=reader;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        private string secret;
        private SqlConnection cnn;

        static void Main(string[] args)
        {
            new Program();
        }

        public Program()
        {
            Flow();
        }

        private async void Flow()
        {
            try { 
                Authenticate();
                ConnectToSQLServer();
                Console.WriteLine("Enter no input to exit. Enter만 누르면 프로그램 끝납니다.");
                CheckPassword();
                DisconnectFromSQLServer();
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void Authenticate()
        {
            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider(servicesAuthConnectionString);

                var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(
                    azureServiceTokenProvider.KeyVaultTokenCallback));

                var secret = kv.GetSecretAsync(keyVaultURL, secretName).Result;

                this.secret = secret.Value;
            } 
            catch (Exception ex)
            {  
                throw new Exception($"Error while authenticating with Azure - please check if certificate is installed. {ex.Message}");
            }
            
        }

        private void ConnectToSQLServer()
        {
            connectionString = connectionString.Replace("{your_password}", secret);      
            cnn = new SqlConnection(connectionString);
            secret = "";
            connectionString = "";
            try
            {
                cnn.Open();
                // Console.WriteLine("Connection Successful!");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while opening connection with DB - please check if internet access is active. {ex.Message}");
            }
        }

        private void DisconnectFromSQLServer()
        {
            try
            {
                cnn.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while closing connection with DB. {ex.Message}");
            }
        }

        private bool CheckHashExists(string hash)
        {
            try
            {     
                var stmt = "select occurence from[dbo].[pwned2017] where hash_sha1 = 0x" + hash;
                var command = new SqlCommand(stmt, cnn);
                var dataReader = command.ExecuteReader();

                var result = dataReader.HasRows;

                dataReader.Close();
                command.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while executing statment on DB: {ex.Message}", ex);   
            }
        }

        private void CheckPassword()
        {
            string hash_sha1 = "";

            while (true)
            { 

                Console.Write("Enter password / 비번 입력하세요:");

                hash_sha1 = Helper.HashFromConsole();

                if (hash_sha1.ToLower().CompareTo("da39a3ee5e6b4b0d3255bfef95601890afd80709") == 0) return;

                var color = Console.ForegroundColor;
                if (CheckHashExists(hash_sha1))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("비밀번호는 데이터베이스안에 있습니다. 얼른 바꾸세요! Your password is within the database. Change it quickly!");  
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("비밀번호는 데이터베이스안에 없습니다. 사용 ㅇㅋ! Your password is not within the database. Use it!");
                }
                Console.ForegroundColor = color;

            }
        }


    }


}
