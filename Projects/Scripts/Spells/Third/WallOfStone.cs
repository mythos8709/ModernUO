using System;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Third
{
  public class WallOfStoneSpell : MagerySpell, ISpellTargetingPoint3D
  {
    private static SpellInfo m_Info = new SpellInfo(
      "Wall of Stone", "In Sanct Ylem",
      227,
      9011,
      false,
      Reagent.Bloodmoss,
      Reagent.Garlic
    );

    public WallOfStoneSpell(Mobile caster, Item scroll = null) : base(caster, scroll, m_Info)
    {
    }

    public override SpellCircle Circle => SpellCircle.Third;

    public override void OnCast()
    {
      Caster.Target = new SpellTargetPoint3D(this, TargetFlags.None, Core.ML ? 10 : 12);
    }

    public void Target(IPoint3D p)
    {
      if (!Caster.CanSee(p))
      {
        Caster.SendLocalizedMessage(500237); // Target can not be seen.
      }
      else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
      {
        SpellHelper.Turn(Caster, p);

        SpellHelper.GetSurfaceTop(ref p);

        int dx = Caster.Location.X - p.X;
        int dy = Caster.Location.Y - p.Y;
        int rx = (dx - dy) * 44;
        int ry = (dx + dy) * 44;

        bool eastToWest = (rx < 0 || ry < 0) && (rx >= 0 || ry >= 0);

        Effects.PlaySound(p, Caster.Map, 0x1F6);

        for (int i = -1; i <= 1; ++i)
        {
          Point3D loc = new Point3D(eastToWest ? p.X + i : p.X, eastToWest ? p.Y : p.Y + i, p.Z);
          bool canFit = SpellHelper.AdjustField(ref loc, Caster.Map, 22, true);

          //Effects.SendLocationParticles( EffectItem.Create( loc, Caster.Map, EffectItem.DefaultDuration ), 0x376A, 9, 10, 5025 );

          if (!canFit)
            continue;

          Item item = new InternalItem(loc, Caster.Map, Caster);

          Effects.SendLocationParticles(item, 0x376A, 9, 10, 5025);

          //new InternalItem( loc, Caster.Map, Caster );
        }
      }

      FinishSequence();
    }

    [DispellableField]
    private class InternalItem : Item
    {
      private Mobile m_Caster;
      private DateTime m_End;
      private Timer m_Timer;

      public InternalItem(Point3D loc, Map map, Mobile caster) : base(0x82)
      {
        Visible = false;
        Movable = false;

        MoveToWorld(loc, map);

        m_Caster = caster;

        if (caster.InLOS(this))
          Visible = true;
        else
          Delete();

        if (Deleted)
          return;

        m_Timer = new InternalTimer(this, TimeSpan.FromSeconds(10.0));
        m_Timer.Start();

        m_End = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);
      }

      public InternalItem(Serial serial) : base(serial)
      {
      }

      public override bool BlocksFit => true;

      public override void Serialize(GenericWriter writer)
      {
        base.Serialize(writer);

        writer.Write(1); // version

        writer.WriteDeltaTime(m_End);
      }

      public override void Deserialize(GenericReader reader)
      {
        base.Deserialize(reader);

        int version = reader.ReadInt();

        switch (version)
        {
          case 1:
          {
            m_End = reader.ReadDeltaTime();

            m_Timer = new InternalTimer(this, m_End - DateTime.UtcNow);
            m_Timer.Start();

            break;
          }
          case 0:
          {
            TimeSpan duration = TimeSpan.FromSeconds(10.0);

            m_Timer = new InternalTimer(this, duration);
            m_Timer.Start();

            m_End = DateTime.UtcNow + duration;

            break;
          }
        }
      }

      public override bool OnMoveOver(Mobile m)
      {
        if (m is PlayerMobile)
        {
          int noto = Notoriety.Compute(m_Caster, m);
          if (noto == Notoriety.Enemy || noto == Notoriety.Ally)
            return false;
        }

        return base.OnMoveOver(m);
      }

      public override void OnAfterDelete()
      {
        base.OnAfterDelete();

        m_Timer?.Stop();
      }

      private class InternalTimer : Timer
      {
        private InternalItem m_Item;

        public InternalTimer(InternalItem item, TimeSpan duration) : base(duration)
        {
          Priority = TimerPriority.OneSecond;
          m_Item = item;
        }

        protected override void OnTick()
        {
          m_Item.Delete();
        }
      }
    }
  }
}
