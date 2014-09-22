﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.cliPackets;
using wServer.logic;
using wServer.realm.commands;
using wServer.svrPackets;

namespace wServer.realm.entities
{
    partial class Player
    {
        public void PlayerText(RealmTime time, PlayerTextPacket pkt)
        {
            if (pkt.Text[0] == '/')
            {
                string[] x = pkt.Text.Trim().Split(' ');
                ProcessCmd(x[0].Trim('/'), x.Skip(1).ToArray());
            }
            else
            {
                string txt = Encoding.ASCII.GetString(
                    Encoding.Convert(
                        Encoding.UTF8,
                        Encoding.GetEncoding(
                            Encoding.ASCII.EncodingName,
                            new EncoderReplacementFallback(string.Empty),
                            new DecoderExceptionFallback()
                            ),
                        Encoding.UTF8.GetBytes(pkt.Text)
                    )
                );
                if (txt != "")
                {
                    string chatColor = "";
                    if (Client.Account.Rank > 3)
                        chatColor = "@";
                    else if (Client.Account.Rank == 3)
                        chatColor = "#";
                    
                    Owner.BroadcastPacket(new TextPacket()
                    {
                        Name = chatColor + Name,
                        ObjectId = Id,
                        Stars = Stars,
                        BubbleTime = 5,
                        Recipient = "",
                        Text = txt,
                        CleanText = txt
                    }, null);
                    foreach (var e in Owner.Enemies)
                    {
                        foreach (var b in e.Value.CondBehaviors)
                        {
                            if (b.Condition == BehaviorCondition.OnChat)
                            {
                                b.Behave(BehaviorCondition.OnChat, e.Value, time, null, pkt.Text, this);
                            }
                        }
                    }
                    foreach (var e in Owner.PlayersCollision.HitTest(X, Y, 8))
                    {
                        if (!(e is NPC))
                            continue;
                        foreach (var b in e.CondBehaviors)
                        {
                            if (b.Condition == BehaviorCondition.OnChat)
                            {
                                b.Behave(BehaviorCondition.OnChat, e, time, null, pkt.Text, this, "NPC");
                            }
                        }
                    }
                    foreach (var e in Owner.EnemiesCollision.HitTest(X, Y, 8))
                    {
                        if (!(e is NPC))
                            continue;
                        foreach (var b in e.CondBehaviors)
                        {
                            if (b.Condition == BehaviorCondition.OnChat)
                            {
                                b.Behave(BehaviorCondition.OnChat, e, time, null, pkt.Text, this, "NPC");
                            }
                        }
                    }
                }
            }
        }

        public void SendInfo(string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "",
                Text = text
            });
        }
        public void SendError(string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "*Error*",
                Text = text
            });
        }
        public void SendGuild(string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "",
                Recipient = "*Guild*",
                Text = text
            });
        }
        public void SendClientText(string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "*Client*",
                Text = text
            });
        }
        public void SendHelp(string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "*Help*",
                Text = text
            });
        }
        public void SendEnemy(string name, string text)
        {
            Owner.BroadcastPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "#" + name,
                Text = text
            }, null);
        }
        public void SendText(string sender, string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = sender,
                Text = text
            });
        }
        
        static Dictionary<string, ICommand> cmds;

        bool CmdReqRank(int r)
        {
            if (psr.Account.Rank < r)
            {
                psr.SendPacket(new TextPacket()
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "*Error*",
                    Text = "Insufficient permissions!"
                });
                return false;
            }
            else
                return true;
        }

        bool CmdBypass(string[] names)
        {
            if (names.Contains(psr.Account.Name))
                return true;
            return false;
        }
        void ExecCmd(ICommand command, string[] args)
        {
            if(command.RequiredRank <= psr.Account.Rank)
            {
                command.Execute(this, args);
            }
        }
        void ProcessCmd(string cmd, string[] args)
        {
            if (cmds == null)
            {
                cmds = new Dictionary<string, ICommand>();
                var t = typeof(ICommand);
                foreach (var i in t.Assembly.GetTypes())
                    if (t.IsAssignableFrom(i) && i != t)
                    {
                        var instance = (ICommand)Activator.CreateInstance(i);
                        cmds.Add(instance.Command, instance);
                    }
            }

            ICommand command;
            if (!cmds.TryGetValue(cmd, out command))
            {
                psr.SendPacket(new TextPacket()
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "*Error*",
                        Text = "Unknown Command!"
                    });
                return;
            }
            try
            {
                ExecCmd(command, args);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
                psr.SendPacket(new TextPacket()
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "*Error*",
                    Text = "Error when executing the command!"
                });
            }
        }

        internal void TellRecieved(int objId, int stars, string from, string to, string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 10,
                Stars = stars,
                Name = to,
                Recipient = from,
                Text = text
            });
        }

        internal void AnnouncementRecieved(string text)
        {
            psr.Player.SendText("#Announcment", text);
        }

        internal void GuildRecieved(int objId, int stars, string from, string text)
        {
            psr.SendPacket(new TextPacket()
            {
                BubbleTime = 10,
                Stars = stars,
                Name = "*Guild*",
                Recipient = from,
                Text = text
            });
        }
    }
}
