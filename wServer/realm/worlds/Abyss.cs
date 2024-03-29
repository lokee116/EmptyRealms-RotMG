﻿namespace wServer.realm.worlds
{
    public class Abyss : World
    {
        public Abyss()
        {
            Name = "Abyss of Demons";
            Background = 0;
            AllowTeleport = true;
            base.FromWorldMap(typeof(RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.wmap.abyss.wmap"));            
        }

        public override World GetInstance(ClientProcessor psr)
        {
            return RealmManager.AddWorld(new Abyss());
        }
    }
}
