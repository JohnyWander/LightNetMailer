using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;

namespace LightNetMailer
{
    internal class Program
    {
        static List<cmdarg> AvaiableCmdAargs = new List<cmdarg>()
        {
            new cmdarg("f","-f specify from name like \"name <email@mail.com>\"",(string from) => mailer.From = from),
            new cmdarg("t","-t email@mail.com / email@mail.com;email1@mail.com - to mail address use \";\" for multiple recipents",(string to) => mailer.To = to),
            new cmdarg("cc","-cc email@mail.com / email@mail.com;email1@email.com - carbon copy adress \";\" for multiple recipents",(string cc) => mailer.cc = cc),
            new cmdarg("bcc","-bcc email@mail.com - mail@mail.com;email1@email.com - carbon copy adress \";\" for multiple recipents ",(string bcc) => mailer.bcc = bcc),
            new cmdarg("s","-s server.com:222 - server and port",(string server) =>mailer.ServerNPort = server),
            new cmdarg("u","-u Message title",(string title) => mailer.Title = title),
            new cmdarg("a","-a path\\To\\Attachment - ads attachment to mail",(string Attachment) => mailer.attachmentsPaths.Add(Attachment)),
            new cmdarg("o","-o path\\to\\message_file - adds message body from file",(string messageFile) => mailer.PathToMessageFile = messageFile ),
            new cmdarg("l","-l path\\to\\logfile - sets path to log file and enables logging",(string logfile) => mailer.PathToLogFile = logfile),
            new cmdarg("xu","-xu username - username to smtp server login", (string username) => mailer.username = username),
            new cmdarg("xp","-xp password - password to smtp server login", (string password) => mailer.password = password),
            new cmdarg("h","-h - displays help message",(string x) => DisplayHelpMessage())

        };

        static List<cmdarg> ActivatedCmdArgs = new List<cmdarg>();

        static Mailer mailer = new Mailer();



        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                ParseCommandLineArgs(args);
                InvokeParamSetters();

                mailer.PrepareSmtpClient();
                mailer.PrepareFrom();
                mailer.PrepareTo();
                mailer.PrepareCC();
                mailer.PrepareBCC();
                mailer.PrepareAttachments();
                mailer.PrepareMessage();
                mailer.SendMessage();

            }
            Console.ReadLine();
        }

        static void ParseCommandLineArgs(string[] args)
        {
            StringBuilder fullargs = new StringBuilder();
            fullargs.Append(" ");
            foreach (string arg in args)
            {
              
                fullargs.Append(arg + " ");
            }

            //Console.WriteLine(fullargs);
            string fullargsstring = fullargs.ToString();
            string[] splittedArgPairs = fullargsstring.Split(" -", StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in splittedArgPairs)
            {
                string pName;
                string value;
                string[] split = s.Split(" ",2);
                pName = split[0];
                value = split[1];

                
                IEnumerable<cmdarg> pickedParam= null;
              
                pickedParam = AvaiableCmdAargs.Where(arg => arg.ArgName == pName);
                

                if (pickedParam.Count() == 1)
                {
                    cmdarg p = pickedParam.ToArray()[0];
                    ActivatedCmdArgs.Add(new cmdarg(p.ArgName,p.ArgHelpMessage,value,p.ArgAction));

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




    }
}