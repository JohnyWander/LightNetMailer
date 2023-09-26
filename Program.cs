using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;

namespace LightNetMailer
{
    enum LogType
    {
        SUCCESS,
        ERROR,
    }
    internal class Program
    {
        static List<cmdarg> AvaiableCmdAargs = new List<cmdarg>()
        {
            new cmdarg("-f","-f specify from name like \"name <email@mail.com>\"",(string from) => mailer.From = from).SetRequired(),
            new cmdarg("-t","-t email@mail.com / email@mail.com;email1@mail.com - to mail address use \";\" for multiple recipents",(string to) => mailer.To = to).SetRequired(),
            new cmdarg("-cc","-cc email@mail.com / email@mail.com;email1@email.com - carbon copy adress \";\" for multiple recipents",(string cc) => mailer.cc = cc),
            new cmdarg("-bcc","-bcc email@mail.com - mail@mail.com;email1@email.com - carbon copy adress \";\" for multiple recipents ",(string bcc) => mailer.bcc = bcc),
            new cmdarg("-s","-s server.com:222 - server and port",(string server) =>mailer.ServerNPort = server).SetRequired(),
            new cmdarg("-u","-u Message title",(string title) => mailer.Title = title),
            new cmdarg("-a","-a path\\To\\Attachment - ads attachment to mail",(string Attachment) => mailer.attachmentsPaths.Add(Attachment)),
            new cmdarg("-o","-o path\\to\\message_file - adds message body from file",(string messageFile) => mailer.PathToMessageFile = messageFile),
            new cmdarg("-l","-l path\\to\\logfile - sets path to log file and enables logging",(string logfile) => {LogFilePath = logfile; Log = true; } ),
            new cmdarg("-xu","-xu username - username to smtp server login", (string username) => mailer.username = username).SetRequired(),
            new cmdarg("-xp","-xp password - password to smtp server login", (string password) => mailer.password = password).SetRequired(),
            new cmdarg("-html","-html - use if mail body is html", (string x) => mailer.BodyIsHtml = true),
            new cmdarg("-h","-h - displays help message",(string x) => DisplayHelpMessage())

        };

        static List<cmdarg> ActivatedCmdArgs = new List<cmdarg>();

        static Mailer mailer = new Mailer();

        static bool Log;
        static string LogFilePath;

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    ParseCommandLineArgs2(args);
                    CheckForRequiredArgs();
                    InvokeParamSetters();

                    mailer.PrepareSmtpClient()
                    .PrepareFrom()
                    .PrepareTo()
                    .PrepareCC()
                    .PrepareBCC()
                    .PrepareAttachments()
                    .PrepareMessage()
                    .SendMessage();

                    if (Log)
                    {
                        WriteLog($"Sucessfully sent mail - FROM {mailer.From} TO {mailer.To} TITLE {mailer.Title}",LogType.SUCCESS);
                    }
                }
                catch(Exception e)
                {
                    if (Log)
                    {
                        WriteLog($"FAIL - Failed with:{e.GetType().Name} {e.Message}", LogType.ERROR);
                    }
                    WriteErrorMessage($"FAIL - Failed with:{e.GetType().Name} {e.Message}");
                }
            }
            else
            {
                DisplayHelpMessage();
            }
            
        }

       
        static void CheckForRequiredArgs()
        {
            List<cmdarg> RequiredArgs = AvaiableCmdAargs.Where(x => x.IsRequired == true).ToList();
            List<cmdarg> Activated = ActivatedCmdArgs;
            Activated.ForEach(active =>
            {
                IEnumerable<cmdarg> marked = RequiredArgs.Where(x => x.ArgName == active.ArgName);
                if (marked.Count() == 1)
                {
                    RequiredArgs.Remove(marked.Single());
                }           
            });
            
            if(RequiredArgs.Count != 0)
            {
                RequiredArgs.ForEach(r => WriteErrorMessage($"[ERROR] Missing required argument - {r.ArgHelpMessage}"));
                Crash("[CRASH] Mail couln't be sent. Missing required arguments above");
            }
        }


        static void ParseCommandLineArgs2(string[] args)
        {
            cmdarg ToActivate = null;
            bool AlreadyAssignedValue = false;             

            foreach(string arg in args)
            {
                try
                {
                    cmdarg PickedArg = AvaiableCmdAargs.Where(x => x.ArgName == arg).Single();
                    ToActivate = new cmdarg(PickedArg.ArgName, PickedArg.ArgHelpMessage, null, PickedArg.ArgAction);
                    ActivatedCmdArgs.Add(ToActivate);
                    AlreadyAssignedValue = false;
                }
                catch (InvalidOperationException)
                {
                    if (AlreadyAssignedValue)
                    {
                        throw;// TODO: present error to user
                    }
                    else
                    {
                        ToActivate.ArgValue = arg;
                        AlreadyAssignedValue = true;
                    }                   
                }

            }
        }


       
        static void InvokeParamSetters()
        {
            foreach(cmdarg p in ActivatedCmdArgs)
            {
                p.ArgAction.Invoke(p.ArgValue);
            }
        }
        static void DisplayHelpMessage()
        {
            foreach(cmdarg c in AvaiableCmdAargs)
            {
                Console.WriteLine($"{c.ArgName}, {c.ArgHelpMessage}");
            }
        }

        static void WriteErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static void WriteLog(string Message,LogType type)
        {
            DateTime x = DateTime.Now;
            string logMessage = $"[{x.Day}-{x.Month}-{x.Year} {x.Hour}:{x.Minute}:{x.Year}][{type.ToString()}] {Message}\n";
            File.AppendAllText(LogFilePath,logMessage);
        }

        static void Crash(string CrashMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(CrashMessage); 
            Console.ResetColor();
            Environment.Exit(0);
        }



    }
}