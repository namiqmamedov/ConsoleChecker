using RuriLib.Parallelization.Models;
using RuriLib.Parallelization;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using RuriLib.Proxies;
using RuriLib.Proxies.Clients;
using RuriLib.Http;
using RuriLib.Http.Models;
using Timer = System.Timers.Timer;
using System.Drawing;

namespace ConsoleChecker
{
    class Program
    {

        public static IEnumerable<string> wordlist;
        public static int numberOfThread = 0;
        public static int good = 0;
        public static int bad = 0;
        public static int check = 0;
        public static int cpm = 0;
        public static string remainingTime = "";
        public static Parallelizer<string, Result> parallelizer = null;
        public static readonly Object obj0 = new Object();

        static void Main(string[] args)
        {
            string fancyTitle = @"
                 ▄████▄   ▒█████   ███▄    █   ██████  ▒█████   ██▓    ▓█████     ▄████▄   ██░ ██ ▓█████  ▄████▄   ██ ▄█▀▓█████  ██▀███  
                ▒██▀ ▀█  ▒██▒  ██▒ ██ ▀█   █ ▒██    ▒ ▒██▒  ██▒▓██▒    ▓█   ▀    ▒██▀ ▀█  ▓██░ ██▒▓█   ▀ ▒██▀ ▀█   ██▄█▒ ▓█   ▀ ▓██ ▒ ██▒
                ▒▓█    ▄ ▒██░  ██▒▓██  ▀█ ██▒░ ▓██▄   ▒██░  ██▒▒██░    ▒███      ▒▓█    ▄ ▒██▀▀██░▒███   ▒▓█    ▄ ▓███▄░ ▒███   ▓██ ░▄█ ▒
                ▒▓▓▄ ▄██▒▒██   ██░▓██▒  ▐▌██▒  ▒   ██▒▒██   ██░▒██░    ▒▓█  ▄    ▒▓▓▄ ▄██▒░▓█ ░██ ▒▓█  ▄ ▒▓▓▄ ▄██▒▓██ █▄ ▒▓█  ▄ ▒██▀▀█▄  
                ▒ ▓███▀ ░░ ████▓▒░▒██░   ▓██░▒██████▒▒░ ████▓▒░░██████▒░▒████▒   ▒ ▓███▀ ░░▓█▒░██▓░▒████▒▒ ▓███▀ ░▒██▒ █▄░▒████▒░██▓ ▒██▒
                ░ ░▒ ▒  ░░ ▒░▒░▒░ ░ ▒░   ▒ ▒ ▒ ▒▓▒ ▒ ░░ ▒░▒░▒░ ░ ▒░▓  ░░░ ▒░ ░   ░ ░▒ ▒  ░ ▒ ░░▒░▒░░ ▒░ ░░ ░▒ ▒  ░▒ ▒▒ ▓▒░░ ▒░ ░░ ▒▓ ░▒▓░
                  ░  ▒     ░ ▒ ▒░ ░ ░░   ░ ▒░░ ░▒  ░ ░  ░ ▒ ▒░ ░ ░ ▒  ░ ░ ░  ░     ░  ▒    ▒ ░▒░ ░ ░ ░  ░  ░  ▒   ░ ░▒ ▒░ ░ ░  ░  ░▒ ░ ▒░
                ░        ░ ░ ░ ▒     ░   ░ ░ ░  ░  ░  ░ ░ ░ ▒    ░ ░      ░      ░         ░  ░░ ░   ░   ░        ░ ░░ ░    ░     ░░   ░ 
                ░ ░          ░ ░           ░       ░      ░ ░      ░  ░   ░  ░   ░ ░       ░  ░  ░   ░  ░░ ░      ░  ░      ░  ░   ░     
                ░                                                                ░                       ░                               ";

            Colorful.Console.WriteLine(fancyTitle, System.Drawing.Color.Cyan);
            wordlist = File.ReadLines("test.txt");
            Console.WriteLine($"Total wordlist: {wordlist.Count()}");
            Console.WriteLine("Enter the number of threads");
            numberOfThread = Convert.ToInt32(Console.ReadLine());

            _ = MainAsync(args);

            Timer timer = new Timer()
            {
                AutoReset = true
            };

            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            
            Console.ReadLine();
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            cpm = parallelizer.CPM;
            remainingTime = parallelizer.Remaining.ToString();
            UpdateConsole();
        }

        static async Task MainAsync(string[] args)
        {
            // This func takes an input type of 'int', a cancellation token, and an output type of `Task` of `bool`
            Func<string, CancellationToken, Task<Result>> parityCheck = new(async (number, token) =>
            {
                var settings = new ProxySettings();
                var proxyClient = new NoProxyClient(settings);
                var email = number.Split(":")[0];
                var password = number.Split(":")[1];
                var result = new Result();

                // Create the custom proxied client
                using var client = new RLHttpClient(proxyClient);

                // Create the request
                using var request = new HttpRequest
                {
                    Uri = new Uri("https://reqres.in/api/login"),
                    Method = HttpMethod.Post,
                    Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer ey..." }
                },
                    Cookies = new Dictionary<string, string>
                {
                    { "PHPSESSID", "12345" }
                },

                    // Content a.k.a. the "post data"
                    Content = new StringContent($"{{\"email\":\"{email}\",\"password\":\"{password}\"}}", Encoding.UTF8, "application/json")
                };

                // Send the request and get the response (this can fail so make sure to wrap it in a try/catch block)
                using var response = await client.SendAsync(request);

                // Read and print the content of the response
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);

                if(content.Contains("token"))
                {
                    result.Email = email;
                    result.Password = password;
                    result.Status = Status.Good;
                }
                else if(content.Contains("error"))
                {
                    result.Email = email;
                    result.Password = password;
                    result.Status = Status.Bad;
                }

                return result;
            });


             parallelizer = ParallelizerFactory<string, Result>.Create(
                type: ParallelizerType.TaskBased, // Use task-based (it's better)
                workItems: wordlist, // The work items are all integers from 1 to 100
                workFunction: parityCheck, // Use the work function we defined above
                degreeOfParallelism: 5, // Use 5 concurrent tasks at most
                totalAmount: wordlist.Count(), // The total amount of tasks you expect to have, used for calculating progress
                skip: 0); // How many items to skip from the start of the provided enumerable

            // Hook the events
            parallelizer.NewResult += OnResult;
            parallelizer.Completed += OnCompleted;
            parallelizer.Error += OnException;
            parallelizer.TaskError += OnTaskError;

            await parallelizer.Start();

            // It's important to always pass a cancellation token to avoid waiting forever if something goes wrong!
            var cts = new CancellationTokenSource();
            cts.CancelAfter(10000);

            await parallelizer.WaitCompletion(cts.Token);
        }

        private static void OnResult(object sender, ResultDetails<string, Result> value)
        {
            if(value.Result.Status == Status.Good)
            {
                Interlocked.Increment(ref good);
                Interlocked.Increment(ref check);

            }
            else if(value.Result.Status == Status.Bad)
            {
                Interlocked.Increment(ref bad);
                Interlocked.Increment(ref check);
            }
            UpdateConsole();
        }
        private static void OnCompleted(object sender, EventArgs e) => Console.WriteLine("All work completed!");
        private static void OnTaskError(object sender, ErrorDetails<string> details)
            => Console.WriteLine($"Got error {details.Exception.Message} while processing the item {details.Item}");
        private static void OnException(object sender, Exception ex) => Console.WriteLine($"Exception: {ex.Message}");

        private static void UpdateConsole()
        {
            var obj = obj0;
            lock(obj)
            {
                Colorful.Console.SetCursorPosition(0, 11);
                Colorful.Console.Title = string.Format("Console Base Checker - [Progress: {0}/{1}] [Good: {2}] [Bad: {3}] [CPM: {4}] [Remaining Time {5}]", new Object[] {
                wordlist.Count(),
                check,
                good,
                bad,
                cpm,
                remainingTime
            });
                Colorful.Console.Write(string.Format("                      Total Combo             -[{0}]", wordlist.Count(), Color.BlueViolet));
                Colorful.Console.SetCursorPosition(0, 11);
                Colorful.Console.Write(string.Format("                      Total Thread            -[{0}]", numberOfThread, Color.BlueViolet));
                Colorful.Console.SetCursorPosition(0, 12);
                Colorful.Console.Write(string.Format("                      Progress                -[{0}/{1}]", wordlist.Count(), Color.GreenYellow));
                Colorful.Console.SetCursorPosition(0, 13);
                Colorful.Console.Write(string.Format("                      Good                    -[{0}]", good, Color.Green));
                Colorful.Console.SetCursorPosition(0, 14);
                Colorful.Console.Write(string.Format("                      Bad                     -[{0}]", bad, Color.Red));
                Colorful.Console.Write(string.Format("                      CPM                     -[{0}]", cpm, System.Drawing.Color.Brown));
                Colorful.Console.SetCursorPosition(0, 16);
            }
        }
    }
}

