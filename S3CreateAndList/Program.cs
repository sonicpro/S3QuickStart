using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3CreateAndList
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var s3Client = new AmazonS3Client();
            if (GetBucketName(args, out string bucketName))
            {
                // If a bucket name was supplied, create the bucket.
                // Call the API method directly.
                try
                {
                    Console.WriteLine($"\nCreating bucket {bucketName}...");
                    var createResponse = await s3Client.PutBucketAsync(bucketName);
                    Console.WriteLine($"Result: {createResponse.HttpStatusCode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Caught exception when creating a bucket:");
                    Console.WriteLine(ex.Message);
                }
            }

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
    }
}
