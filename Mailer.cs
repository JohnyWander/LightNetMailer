using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using System.Net.Mail;
using System.Net.Mime;
using System.Net;

namespace LightNetMailer
{
    internal class Mailer
    {
        public string From;
        private string FromName;
        private string FromEmail;

        public string To;
        public string cc;
        public string bcc;

        public string ServerNPort
        {
            set
            {
                string[] splitted = value.Split(':');
                this.ServerAdress = splitted[0];
                this.Port = int.Parse(splitted[1]);
            }
        }
             
        private string ServerAdress;
        private int Port;

        public string Title;
        public string PathToMessageFile;
        public string charset;
        public string PathToLogFile;
        public string username;
        public string password;

        public bool UseSSL = false;

        public bool BodyIsHtml = false;
        public List<string> attachmentsPaths = new List<string>();

        SmtpClient smtpClient;
        MailAddress fromMail;

        MailAddressCollection toMails = new MailAddressCollection();
        MailAddressCollection ccMails = new MailAddressCollection();
        MailAddressCollection bccMails = new MailAddressCollection();

        List<Attachment> attachments = new List<Attachment>();
               
        MailMessage MailMessage;

        public Mailer PrepareSmtpClient()
        {           
            smtpClient = new SmtpClient(ServerAdress,Port);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.EnableSsl = UseSSL;
            NetworkCredential creds = new NetworkCredential(this.username.Trim(), this.password.Trim());         
            smtpClient.Credentials = creds;

            return this;
        }

        public Mailer PrepareFrom()
        {
            if (this.From.Contains("<") && this.From.Contains(">"))
            {
                string email = this.From.Substring(this.From.IndexOf('<'));
                email = email.Trim('<').Trim('>');
                string name = this.From.Substring(0, this.From.IndexOf("<")).Trim();
                this.fromMail = new MailAddress(email, name);
            }
            else
            {
                this.fromMail = new MailAddress(this.From.Trim(), this.From.Trim());
            }

            return this;
        }

        public Mailer PrepareTo()
        {
            if (this.To.Contains(";"))
            {
                string[] addresses = this.To.Split(";");
                foreach(string a in addresses)
                {
                    toMails.Add(new MailAddress(a));
                }
            }
            else
            {
                toMails.Add(new MailAddress(this.To));
            }

            return this;
        }

        public Mailer PrepareBCC()
        {
            if (this.bcc is not null)
            {
                if (this.bcc.Contains(";"))
                {
                    string[] addresses = this.bcc.Split(";");
                    foreach (string a in addresses)
                    {
                        bccMails.Add(new MailAddress(a));
                    }
                }
                else
                {
                    bccMails.Add(new MailAddress(this.bcc));
                }
            }
            return this;
        }

        public Mailer PrepareCC()
        {
            if (this.cc is not null)
            {
                if (this.cc.Contains(";"))
                {
                    string[] addresses = this.cc.Split(";");
                    foreach (string a in addresses)
                    {
                        ccMails.Add(new MailAddress(a));
                    }
                }
                else
                {
                    ccMails.Add(new MailAddress(this.cc));
                }
            }
            return this;
        }

        public Mailer PrepareAttachments()
        {
            foreach(string AttachmentsPath in this.attachmentsPaths)
            {
                this.attachments.Add(new Attachment(AttachmentsPath));              
            }
            return this;
        }

        public Mailer PrepareMessage()
        {
            MailMessage = new MailMessage();

            toMails.ToList().ForEach(mail => this.MailMessage.To.Add(mail));
            ccMails.ToList().ForEach(mail => this.MailMessage.CC.Add(mail));
            bccMails.ToList().ForEach(mail => this.MailMessage.Bcc.Add(mail));
            attachments.ForEach(attachment => this.MailMessage.Attachments.Add(attachment));

            this.MailMessage.From = this.fromMail;
            this.MailMessage.Subject = this.Title;

            if (this.PathToMessageFile != null)
            {
                this.MailMessage.Body = File.ReadAllText(this.PathToMessageFile.Split("=")[1]);
                this.MailMessage.BodyEncoding = Encoding.UTF8;
                this.MailMessage.IsBodyHtml = this.BodyIsHtml;
            }

            return this;
        }

        public Mailer SendMessage()
        {
            smtpClient.Send(MailMessage);
            return this;
           
        }


        public Mailer()
        {


        }

    }
}
