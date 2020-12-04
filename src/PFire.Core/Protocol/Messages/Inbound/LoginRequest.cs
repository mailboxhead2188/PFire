﻿using PFire.Core.Protocol.Messages;
using PFire.Protocol.Messages.Outbound;
using PFire.Session;

namespace PFire.Protocol.Messages.Inbound
{
    public sealed class LoginRequest : XFireMessage
    {
        public LoginRequest() : base(XFireMessageType.LoginRequest) { }

        [XMessageField("name")]
        public string Username { get; set; }

        [XMessageField("password")]
        public string Password {
            get;
            set;
        }

        [XMessageField("flags")]
        public int Flags { get; private set; }

        public override void Process(XFireClient context)
        {
            var user = context.Server.Database.QueryUser(Username);
            if (user != null)
            {
                if (user.Password != Password)
                {
                    context.SendAndProcessMessage(new LoginFailure());
                    return;
                }
            }
            else
            {
                user = context.Server.Database.InsertUser(Username, Password, context.Salt);
            }

            // Remove any older sessions from this user (duplicate logins)
            var otherSession = context.Server.GetSession(user);
            if (otherSession != null)
            {
                context.Server.RemoveSession(otherSession);
                otherSession.TcpClient.Close();
            }

            context.User = user;

            var success = new LoginSuccess();
            context.SendAndProcessMessage(success);
        }
    }
}
