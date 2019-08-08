namespace Server.Items
{
  public class NotarizedApplication : Item
  {
    [Constructible]
    public NotarizedApplication() : base(0x14EF)
    {
      LootType = LootType.Blessed;
    }

    public NotarizedApplication(Serial serial) : base(serial)
    {
    }

    public override int LabelNumber => 1073135; // Notarized Application

    public override bool Nontransferable => true;

    public override void AddNameProperties(ObjectPropertyList list)
    {
      base.AddNameProperties(list);
      AddQuestItemProperty(list);
    }

    public override void Serialize(GenericWriter writer)
    {
      base.Serialize(writer);

      writer.Write(0); // Version
    }

    public override void Deserialize(GenericReader reader)
    {
      base.Deserialize(reader);

      int version = reader.ReadInt();
    }
  }
}