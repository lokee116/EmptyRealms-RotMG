﻿using System;
using System.Collections.Generic;
using wServer.realm;
using wServer.realm.entities;
using wServer.svrPackets;

namespace wServer.logic.attack
{
    class SimpleAttack : Behavior
    {
        float radius;
        int projectileIndex;
        private SimpleAttack(float radius, int projectileIndex)
        {
            this.radius = radius;
            this.projectileIndex = projectileIndex;
        }
        static readonly Dictionary<Tuple<float, int>, SimpleAttack> instances = new Dictionary<Tuple<float, int>, SimpleAttack>();
        public static SimpleAttack Instance(float radius, int projectileIndex = 0)
        {
            var key = new Tuple<float, int>(radius, projectileIndex);
            SimpleAttack ret;
            if (!instances.TryGetValue(key, out ret))
                ret = instances[key] = new SimpleAttack(radius, projectileIndex);
            return ret;
        }

        Random rand = new Random();
        protected override bool TickCore(RealmTime time)
        {
            if (Host.Self.HasConditionEffect(ConditionEffects.Stunned)) return false;
            float dist = radius;
            Entity player = GetNearestEntity(ref dist, null);
            if (player != null)
            {
                var chr = Host as Character;
                var angle = Math.Atan2(player.Y - chr.Y, player.X - chr.X);
                var desc = chr.ObjectDesc.Projectiles[projectileIndex];

                var prj = chr.CreateProjectile(
                    desc, chr.ObjectType, chr.Random.Next(desc.MinDamage, desc.MaxDamage),
                    time.tickTimes, new Position() { X = chr.X, Y = chr.Y }, (float)angle);
                chr.Owner.EnterWorld(prj);
                if (projectileIndex == 0) //(false)
                    chr.Owner.BroadcastPacket(new ShootPacket()
                    {
                        BulletId = prj.ProjectileId,
                        OwnerId = Host.Self.Id,
                        ContainerType = Host.Self.ObjectType,
                        Position = prj.BeginPos,
                        Angle = prj.Angle,
                        Damage = (short)prj.Damage
                    }, null);
                else
                    chr.Owner.BroadcastPacket(new MultiShootPacket()
                    {
                        BulletId = prj.ProjectileId,
                        OwnerId = Host.Self.Id,
                        Position = prj.BeginPos,
                        Angle = prj.Angle,
                        Damage = (short)prj.Damage,
                        BulletType = (byte)(desc.BulletType),
                        AngleIncrement = 0,
                        NumShots = 1,
                    }, null);
                return true;
            }
            return false;
        }
    }
    class NPCSimpleAttack : Behavior
    {
        float radius;
        int projectileIndex;
        private NPCSimpleAttack(float radius, int projectileIndex)
        {
            this.radius = radius;
            this.projectileIndex = projectileIndex;
        }
        static readonly Dictionary<Tuple<float, int>, NPCSimpleAttack> instances = new Dictionary<Tuple<float, int>, NPCSimpleAttack>();
        public static NPCSimpleAttack Instance(float radius, int projectileIndex = 0)
        {
            var key = new Tuple<float, int>(radius, projectileIndex);
            NPCSimpleAttack ret;
            if (!instances.TryGetValue(key, out ret))
                ret = instances[key] = new NPCSimpleAttack(radius, projectileIndex);
            return ret;
        }

        Random rand = new Random();
        protected override bool TickCore(RealmTime time)
        {
            if (Host.Self.HasConditionEffect(ConditionEffects.Stunned)) return false;
            float dist = radius;
            Entity player = GetNearestEntity(ref dist, 0);
            if (player != null)
            {
                var chr = Host as Character;
                var angle = Math.Atan2(player.Y - chr.Y, player.X - chr.X);
                var desc = chr.ObjectDesc.Projectiles[projectileIndex];

                var elem = XmlDatas.TypeToElement[Host.Self.ObjectType];
                var att = elem.Element("Attack") != null ? Convert.ToInt32(elem.Element("Attack").Value) : 0;

                var prj = chr.CreateProjectile(
                    desc, chr.ObjectType, chr.Random.Next(desc.MinDamage, desc.MaxDamage) + att,
                    time.tickTimes, new Position() { X = chr.X, Y = chr.Y }, (float)angle);
                chr.Owner.EnterWorld(prj);
                /*if (projectileIndex == 0) //(false)
                    chr.Owner.BroadcastPacket(new ShootPacket()
                    {
                        BulletId = prj.ProjectileId,
                        OwnerId = Host.Self.Id,
                        ContainerType = Host.Self.ObjectType,
                        Position = prj.BeginPos,
                        Angle = prj.Angle,
                        Damage = (short)prj.Damage
                    }, null);
                else*/
                chr.Owner.BroadcastPacket(new MultiShootPacket()
                {
                    BulletId = prj.ProjectileId,
                    OwnerId = Host.Self.Id,
                    Position = prj.BeginPos,
                    Angle = prj.Angle,
                    Damage = (short)prj.Damage,
                    BulletType = (byte)(desc.BulletType),
                    AngleIncrement = 0,
                    NumShots = 1,
                }, null);
                return true;
            }
            return false;
        }
    }
}
