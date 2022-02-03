using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3CreateAndList
{
    class Program
    {
        private const string PublicUrlFormat = "https://{0}.s3-{1}.amazonaws.com/{2}";

        private readonly Settings settings = new Settings
        {
            S3Bucket = "sonicpro-bucket",
            S3Folders = "itext-generated/",
            AWS_REGION = "us-west-2"
        };

        static async Task Main(string[] args)
        {
            var s3Client = new AmazonS3Client();

            //if (GetBucketName(args, out string bucketName))
            //{
            //    // If a bucket name was supplied, create the bucket.
            //    // Call the API method directly.
            //    try
            //    {
            //        Console.WriteLine($"\nCreating bucket {bucketName}...");
            //        var createResponse = await s3Client.PutBucketAsync(bucketName);
            //        Console.WriteLine($"Result: {createResponse.HttpStatusCode}");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("Caught exception when creating a bucket:");
            //        Console.WriteLine(ex.Message);
            //    }
            //}

            var file1 = @"C:\Users\binque.timok\Downloads\test.pdf";
            Console.WriteLine((await new Program().UploadFileToS3("testkey2.pdf", file1, s3Client)).FileName);

            var file2 = @"C:\asd.pdf";
            using var fileStream = new FileStream(file2, FileMode.Open);
            Console.WriteLine((await new Program().UploadStreamToS3("asdkey.pdf", fileStream, s3Client)).FileName);

            // List the buckets owned by the user.
            // Call a class method that calls the API method.
            Console.WriteLine("\nGetting a list of your buckets...");
            var listResponse = await MyListBucketsAsync(s3Client);
            Console.WriteLine($"Number of buckets {listResponse.Buckets.Count}.");
            foreach (var b in listResponse.Buckets)
            {
                Console.WriteLine(b.BucketName);
            }
        }

        private static bool GetBucketName(string[] args, out string bucketName)
        {
            var retval = false;
            bucketName = string.Empty;
            if (args.Length == 0)
            {
                Console.WriteLine(@"
No arguments specified. Will simply the list of your Amazon S3 buckets.
If you wish to create a bucket, supply a valid, globally unique bucket name.");
                retval = false;
            }
            else if (args.Length == 1)
            {
                bucketName = args[0];
                retval = true;
            }
            else
            {
                Console.WriteLine("\nToo many arguments specified." +
                    "\n\ndotnet_tutorials - A utility to list your Amazon S3 buckets and optionally create a new one." +
                    "\n\nUsage: S3CreateAndList [bucket_name]" +
                    "\n - bucket_name: A valid, globally unique bucket name." +
                    "\n - If bucket_name isn't supplied, this utility simply lists your buckets.");
                Environment.Exit(1);
            }

            return retval;
        }

        private static async Task<ListBucketsResponse> MyListBucketsAsync(IAmazonS3 s3Client)
        {
            return await s3Client.ListBucketsAsync();
        }

        public async Task<S3Result> UploadFileToS3(string name, string fileFullName, IAmazonS3 s3Client)
        {
            var key = GetKey(name);
            var request = new PutObjectRequest
            {
                FilePath = fileFullName,
                Key = key,
                BucketName = this.settings.S3Bucket,
                CannedACL = S3CannedACL.PublicRead
            };
            return await this.Put(request, s3Client);
        }

        public async Task<S3Result> UploadStreamToS3(string name, Stream stream, IAmazonS3 s3Client)
        {
            string key = GetKey(name);
            var request = new PutObjectRequest
            {
                AutoCloseStream = true,
                Key = key,
                BucketName = this.settings.S3Bucket,
                InputStream = stream,
                CannedACL = S3CannedACL.PublicRead
            };
            return await this.Put(request, s3Client);
        }

        public async Task<Stream> GetFromS3(string name, IAmazonS3 s3Client)
        {
            var key = GetKey(name);
            var request = new GetObjectRequest
            {
                BucketName = this.settings.S3Bucket,
                Key = key
            };
            var result = await s3Client.GetObjectAsync(request);
            return result.ResponseStream;
        }

        private string GetKey(string name)
        {
            var folders = this.settings.S3Folders;
            if (folders[^1] != '/')
            {
                folders += "/";
            }
            return Path.Combine(folders, name);
        }

        private async Task<S3Result> Put(PutObjectRequest request, IAmazonS3 s3Client)
        {
            await s3Client.PutObjectAsync(request);
            return new S3Result
            {
                FileName = string.Format(PublicUrlFormat, this.settings.S3Bucket, this.settings.AWS_REGION, request.Key)
            };
        }


    }

    public class Settings
    {
        public string LicenseKeyFileName { get; set; }

        public string FileSinkPath { get; set; }

        public string S3Bucket { get; set; }

        public string S3Folders { get; set; } = "/";

        public string AWS_REGION { get; set; }
    }

    public class S3Result
    {
        public string FileName { get; set; }
    }
}
