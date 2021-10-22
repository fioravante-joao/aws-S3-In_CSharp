using Amazon.Runtime;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace s3_project
{
    class Program
    {
        public static void Main(string[] args)
        {
            MinhaAws minhaAws = new MinhaAws();
            minhaAws.MetodosManipulacaoAws();


            Console.ReadLine();
        }
    }
}
