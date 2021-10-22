using Amazon.Runtime;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using System.IO; //declarando a biblioteca de entrada e saída de arquivos
                 //a biblioteca IO

namespace s3_project
{
    public class MinhaAws
    {
        private static string DIRETORIO_ARQUIVOS =
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            + @"C:\Users\JVFS\Desktop";   //Insira o caminho do seu diretório para criação dos arquivos de Exemplo


        //Definindo sua chave de acesso AWS
        private static string accesskey = "Inserir aqui sua AccessKey";
        private static string secretkey = "Inserir aqui sua secretKey";

        private static RegionEndpoint Region = RegionEndpoint.SAEast1;  //SETANDO A REGIÃO
        private List<string> Arquivos { get; set; }
        private AmazonS3Client Client { get; set; }
        private string Bucket { get; set; }

        /// <summary>
        /// Criando 3 arquivos no diretório C:\Users\JVFS\Desktop
        /// </summary>
        private void CriarArquivos()
        {
            Console.WriteLine("Criando os arquivos abaixo no diretório " + DIRETORIO_ARQUIVOS + "...");
            string[] rand = { "arquivo1", "arquivo2", "arquivo3" };
            string arquivo = null;
            this.Arquivos = new List<string>();


            for (int i = 0; i < 5; i++)
            {
                arquivo = rand[i] + "teste-amazon-s3.txt";
                Console.Write(arquivo);
                File.WriteAllText(DIRETORIO_ARQUIVOS + arquivo, "Exemplo de arquivo armazenado na Amazon S3");
                Arquivos.Add(arquivo);
                Console.WriteLine("\t\tSucesso!");
            }
        }

        /// <summary>
        /// Faz Login na Aws
        /// </summary>
        private void LoginAws()
        {
            Console.WriteLine("Logando na AWS - Serviço S3");
            Thread.Sleep(2000);

            try
            {
                BasicAWSCredentials basicAWSCredentials = new BasicAWSCredentials(accesskey, secretkey);
                this.Client = new AmazonS3Client(basicAWSCredentials, Region);
                Console.WriteLine($"Login successfully!");
                Thread.Sleep(3000);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}, \nLocal: {e.StackTrace}");
            }            
        }

        /// <summary>
        /// Lista todos os buckets
        /// </summary>
        private void ListaBuckets()
        {
            BasicAWSCredentials basicAWSCredentials = new BasicAWSCredentials(accesskey, secretkey);
            AmazonS3Client s3 = new AmazonS3Client(basicAWSCredentials, Region);

            Console.WriteLine("\n\nRecuperando lista de buckets....");
            ListBucketsResponse response = null;
            response = Client.ListBuckets();

            int i = 1;
            foreach (S3Bucket bucket in response.Buckets)
            {
                Console.WriteLine("# " + i + "\t" + bucket.BucketName);

                //Chama o método ListaArquivos passando o nome do Bucket desejado por parametro
                this.ListaArquivos(bucket.BucketName); 
                i++;
            }
            Console.WriteLine("\nFim...");
            Console.ReadLine();
        }

        /// <summary>
        /// Listas todos os objetos do bucket passado como parametro
        /// </summary>
        /// <param name="bucket"></param>
        private void ListaArquivos(string bucket)
        {
            Console.WriteLine($"\n\nListando arquivos do bucket {bucket} ...\n");
            Thread.Sleep(2000);

            ListObjectsRequest request = new ListObjectsRequest();
            request.BucketName = bucket;

            AmazonS3Client amazonS3Client = new AmazonS3Client();

            ListObjectsResponse response = amazonS3Client.ListObjects(request);

            int i = 1;
            foreach (S3Object obj in response.S3Objects)
            {
                Console.WriteLine("===================================================================");
                Console.WriteLine("# " + i + "\t Name: " + obj.Key + ", " + "ClassType: " + obj.StorageClass + ", " + "Size: " + obj.Size);
                Console.WriteLine("===================================================================");

                i++;
            }
        }

        /// <summary>
        /// Criação de um novo Bucket
        /// </summary>
        private void CriaNovoBucket()
        {
            //Faça Login antes de usar os métodos.
            //this.LoginAws();
            this.Bucket = "meu-bucket" + DateTime.Now.ToString("yyyyMMddHHmmss");
            Console.WriteLine($"Criando novo Bucket {this.Bucket}...");
            Thread.Sleep(2000);

            PutBucketRequest putBucketRequest = new PutBucketRequest();
            putBucketRequest.CannedACL = S3CannedACL.BucketOwnerFullControl;
            PutBucketResponse putBucketResponse = new PutBucketResponse();
            putBucketRequest.BucketName = this.Bucket;

            putBucketResponse = Client.PutBucket(putBucketRequest);

            Console.WriteLine($"Bucket {this.Bucket}, criado com sucesso!");
        }

        /// <summary>
        /// Download dos objetos
        /// </summary>
        /// <param name="obj"></param>
        private void Download(S3Object obj)
        {
            Console.WriteLine($"Fazendo o Download do arquivo  {obj.Key} \n");

            GetObjectRequest getObjectRequest = new GetObjectRequest();
            getObjectRequest.BucketName = obj.BucketName;
            getObjectRequest.Key = obj.Key;

            GetObjectResponse getObjectResponse = new GetObjectResponse();
            getObjectResponse.WriteResponseStreamToFile(DIRETORIO_ARQUIVOS + @"download-" + obj.Key);
            Process.Start(DIRETORIO_ARQUIVOS + @"download-" + obj.Key);
            this.ListaBuckets();
        }

        /// <summary>
        /// Faz upload dos arquivos para o bucket no S3
        /// </summary>
        private void Upload()
        {
            Console.WriteLine("\n\nEfetuando o upload dos arquivos abaixo...");
            Thread.Sleep(2000);


            for (int i = 0; i < Arquivos.Count; i++)
            {
                UploadPartRequest request = new UploadPartRequest
                {
                    BucketName = this.Bucket,
                    FilePath = DIRETORIO_ARQUIVOS + Arquivos[i],
                    Key = Arquivos[i].Substring(0, 1) + "/" + Arquivos[i]
                };


                Console.Write(Arquivos[i]);
                UploadPartResponse response = this.Client.UploadPart(request);


                //Habilita o acesso do arquivo via url
                this.Client.PutACL(new PutACLRequest
                {
                    BucketName = this.Bucket,
                    Key = Arquivos[i].Substring(0, 1) + "/" + Arquivos[i],
                    CannedACL = S3CannedACL.PublicRead
                });


                Console.WriteLine("\t\tSucesso!");
                Thread.Sleep(500);
            }
            Thread.Sleep(2500);
        }

        /// <summary>
        /// Método public que possibilita chamar os métodos private dessa classe e utiliza-los fora da classe principal.
        /// </summary>
        public void MetodosManipulacaoAws()
        {
            this.CriarArquivos();
            this.LoginAws();
            this.CriaNovoBucket();
            this.Upload();
            this.ListaBuckets();            
        }
    }
}
