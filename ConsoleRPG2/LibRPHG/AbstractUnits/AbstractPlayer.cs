﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

using MatrixFields;

namespace LibRPHG
{

    public abstract class Abstractplayer : Abstraceunit
    {
        protected int _expirience;
        protected float _exp_mod;
        public void LevelUp()
        {
            LOGS.Add(String.Format("{0} archieved {1} level", NameFull, Level));
            _hp = _hpmax;
            _mp = _mpmax;
            _exp_mod = Math.Max(_exp_mod - .25f, 0.0f);

            // random stat gain
            // ...
        }
        public void RecieveExp(int exp)
        {

            _expirience += Math.Max(1, (int)(exp * _exp_mod));
            LOGS.Add(String.Format("{0} gain {1} exp", Name, (int)(exp * _exp_mod)));
            int _level_prev = _level;
            _level = Math.Min(5, _expirience / 100 + 1);
            if (_level != _level_prev)
                LevelUp();
        }
        public abstract string Prof { get; }

        public override string NameFull { get { return String.Format("{0}, the {1}", Name, Prof); } }

        public void SetDefault()
        {
            _exp_mod = 1.0f;
            _expirience = 0;
        }

        public override void DamageFor(int x)
        {
            float defPercent = .1f * (_def + _def_mod);
            int recievedDamage = (int)Math.Max(1.0f, defPercent * x);
            LOGS.Add(String.Format("{0} was damaged for {1} (blocked {2}), {3}/{4} hp. left", Name, x, x - recievedDamage, getCurrentHP, getMaxHP));
        }

        public override void HealFor(int x)
        {
            LOGS.Add(String.Format("{0} was healed for {1} health up to {2}/{3}", Name, x, getCurrentHP, getMaxHP));
        }
    }


    public abstract class Abstraceunit : Iunit
    {
        public int TeamNumber;

        protected Point _pos;
        protected bool _isDead;
        protected int _level;
        protected string _name;

        protected int _hpmax;
        protected int _hp;
        protected int _mpmax;
        protected int _mp;
        protected int _hp_shield;

        protected int _hp_regen_per_turn;
        protected int _mp_regen_per_turn;

        protected int _mvp;
        protected int _mvpmax;

        protected int _atp;
        protected int _atpmax;
        protected int _att_dmg;
        protected int _att_dist;

        protected int _def;
        protected int _acc;

        protected int _atp_mod;
        protected int _mvp_mod;
        protected int _def_mod;
        protected int _acc_mod;
        protected int _att_dmg_mod;
        protected int _hp_regen_mod;
        protected int _mp_regen_mod;
        public string description;

        public virtual void SetDefaultStats(string name, Point location, int level, int hpmax, int mpmax,
            int spd, int attpoints, int attdamage, int attdist, int def, int acc)
        {
            _isDead = false; _hp_shield = _hp_regen_mod = _hp_regen_per_turn = _mp_regen_per_turn = _mp_regen_mod = 0;
            _atp_mod = _mvp_mod = _def_mod = _acc_mod = _att_dmg_mod = _hp_regen_mod = _mp_regen_mod = 0;
            description = "Abstract unit as itself.";

            _level = level;
            _name = name;
            _pos = location;
            _hpmax = _hp = hpmax;
            _mpmax = _mp = mpmax;
            _mvpmax = _mvp = spd;
            _atpmax = _atp = attpoints;
            _att_dmg = attdamage;
            _att_dist = attdist;
            _def = def;
            _acc = acc;
        }
        protected string StrPlus(int X, int Y)
        {
            return (Y > 0) ? String.Format("({0}+{1})", X, Y) : X + "";
        }

        public virtual string TraceMoveStats()
        {
            return String.Format("\n{0} (lvl.{1})\n{8}\nHP: {2} / {3}\t +{4}\nMP: {5} / {6}\t +{7}\nATT: {9}x{10} at range {11}\nMV: {12}\tDEF: {13}\tACC: {14}\n"
                , NameFull, Level, StrPlus(_hp, _hp_shield), _hpmax, StrPlus(_hp_regen_per_turn, _hp_regen_mod),
                 _mp, _mpmax, StrPlus(_mp_regen_per_turn, _mp_regen_mod), description, StrPlus(_atpmax, _atp_mod),
                 StrPlus(_att_dmg, _att_dmg_mod), _att_dist, StrPlus(_mvpmax, _mvp_mod), StrPlus(_def, _def_mod), StrPlus(_acc, _acc_mod));
        }
        public virtual int Level { get { return _level; } }
        public virtual string Name { get { return _name; } }
        public abstract string NameFull { get; }
        //
        public int getCurrentHP { get { return _hp + _hp_shield; } }
        public int getMaxHP { get { return _hpmax + _hp_shield; } }
        public int getCurrentMP { get { return _mp; } }
        public int getMaxMP { get { return _mpmax; } }
        public virtual Point GetPosition { get { return _pos; } }
        public virtual int GetSpd { get { return _mvpmax; } }
        public virtual int getTeamNumber { get { return TeamNumber; } }

        public virtual List<Prio> CalculateSituation(Battlefield bf)
        {
            bool inverse = (TeamNumber % 2) == 0;
            List<Iunit> units = bf.getUnits;

            List<Prio> res = new List<Prio>();
            for (int i = 0; i < units.Count; i++)
                if (units[i] != this)
                {
                    if (inverse)
                    {
                        // victim
                        if (units[i].getTeamNumber != TeamNumber)
                            res.Add(new Prio(Calculator.GetOblast(units[i].GetPosition, units[i].GetSpd * 2 - GetSpd, true, true), -5.0f));
                        else
                            res.Add(new Prio(Calculator.GetOblast(units[i].GetPosition, 2, true, true), 1.0f)); // 1.0f
                    }
                    else
                    {
                        // hunter
                        if (units[i].getTeamNumber != getTeamNumber)
                            res.Add(new Prio(Calculator.GetOblast(units[i].GetPosition, units[i].GetSpd * 2 - GetSpd, true, true), 5.0f));
                    }
                }
            return res;
        }
        public abstract void DamageFor(int x);
        public void Die()
        {
            LOGS.Add(String.Format("{0} died.", NameFull));
        }
        public abstract void HealFor(int x);
        public virtual bool isDead { get { return _isDead; } }
        public void MoveTo(Point where)
        {
            LOGS.Add(String.Format("{0} moves {1},{2} -> {3},{4}", Name, _pos.X, _pos.Y, where.X, where.Y));
            _pos = where;
        }

        public virtual string TraceBars()
        {
            return (String.Format("\n  {0}:\n  HP:{1}\n  MP:{2}", NameFull, LOGS.TraceBar(_hp + _hp_shield, _hpmax + _hp_shield), LOGS.TraceBar(_mp, _mpmax)));
        }
    }
}