using System.Collections.Generic;
using Server.Accounting;
using Server.Items;
using Server.Network;

namespace Server.Gumps
{
  public class HouseRaffleManagementGump : Gump
  {
    public enum SortMethod
    {
      Default,
      Name,
      Account,
      Address
    }

    public const int LabelColor = 0xFFFFFF;
    public const int HighlightColor = 0x11EE11;
    private List<RaffleEntry> m_List;
    private int m_Page;
    private SortMethod m_Sort;

    private HouseRaffleStone m_Stone;

    public HouseRaffleManagementGump(HouseRaffleStone stone, SortMethod sort = SortMethod.Default,
      int page = 0) : base(40, 40)
    {
      m_Stone = stone;
      m_Page = page;

      m_List = new List<RaffleEntry>(m_Stone.Entries);
      m_Sort = sort;

      switch (m_Sort)
      {
        case SortMethod.Name:
        {
          m_List.Sort(NameComparer.Instance);

          break;
        }
        case SortMethod.Account:
        {
          m_List.Sort(AccountComparer.Instance);

          break;
        }
        case SortMethod.Address:
        {
          m_List.Sort(AddressComparer.Instance);

          break;
        }
      }

      AddPage(0);

      AddBackground(0, 0, 618, 354, 9270);
      AddAlphaRegion(10, 10, 598, 334);

      AddHtml(10, 10, 598, 20, Color(Center("Raffle Management"), LabelColor));

      AddHtml(45, 35, 100, 20, Color("Location:", LabelColor));
      AddHtml(145, 35, 250, 20, Color(m_Stone.FormatLocation(), LabelColor));

      AddHtml(45, 55, 100, 20, Color("Ticket Price:", LabelColor));
      AddHtml(145, 55, 250, 20, Color(m_Stone.FormatPrice(), LabelColor));

      AddHtml(45, 75, 100, 20, Color("Total Entries:", LabelColor));
      AddHtml(145, 75, 250, 20, Color(m_Stone.Entries.Count.ToString(), LabelColor));

      AddButton(440, 33, 0xFA5, 0xFA7, 3);
      AddHtml(474, 35, 120, 20, Color("Sort by name", LabelColor));

      AddButton(440, 53, 0xFA5, 0xFA7, 4);
      AddHtml(474, 55, 120, 20, Color("Sort by account", LabelColor));

      AddButton(440, 73, 0xFA5, 0xFA7, 5);
      AddHtml(474, 75, 120, 20, Color("Sort by address", LabelColor));

      AddImageTiled(13, 99, 592, 242, 9264);
      AddImageTiled(14, 100, 590, 240, 9274);
      AddAlphaRegion(14, 100, 590, 240);

      AddHtml(14, 100, 590, 20, Color(Center("Entries"), LabelColor));

      if (page > 0)
        AddButton(567, 104, 0x15E3, 0x15E7, 1);
      else
        AddImage(567, 104, 0x25EA);

      if ((page + 1) * 10 < m_List.Count)
        AddButton(584, 104, 0x15E1, 0x15E5, 2);
      else
        AddImage(584, 104, 0x25E6);

      AddHtml(14, 120, 30, 20, Color(Center("DEL"), LabelColor));
      AddHtml(47, 120, 250, 20, Color("Name", LabelColor));
      AddHtml(295, 120, 100, 20, Color(Center("Address"), LabelColor));
      AddHtml(395, 120, 150, 20, Color(Center("Date"), LabelColor));
      AddHtml(545, 120, 60, 20, Color(Center("Num"), LabelColor));

      int idx = 0;
      Mobile winner = m_Stone.Winner;

      for (int i = page * 10; i >= 0 && i < m_List.Count && i < (page + 1) * 10; ++i, ++idx)
      {
        RaffleEntry entry = m_List[i];

        if (entry == null)
          continue;

        AddButton(13, 138 + idx * 20, 4002, 4004, 6 + i);

        int x = 45;
        int color = winner != null && entry.From == winner ? HighlightColor : LabelColor;

        string name = null;

        if (entry.From != null)
        {
          if (entry.From.Account is Account acc)
            name = $"{entry.From.Name} ({acc})";
          else
            name = entry.From.Name;
        }

        if (name != null)
          AddHtml(x + 2, 140 + idx * 20, 250, 20, Color(name, color));

        x += 250;

        if (entry.Address != null)
          AddHtml(x, 140 + idx * 20, 100, 20, Color(Center(entry.Address.ToString()), color));

        x += 100;

        AddHtml(x, 140 + idx * 20, 150, 20, Color(Center(entry.Date.ToString()), color));
        x += 150;

        AddHtml(x, 140 + idx * 20, 60, 20, Color(Center("1"), color));
        x += 60;
      }
    }

    public string Right(string text) => $"<DIV ALIGN=RIGHT>{text}</DIV>";

    public string Center(string text) => $"<CENTER>{text}</CENTER>";

    public string Color(string text, int color) => $"<BASEFONT COLOR=#{color:X6}>{text}</BASEFONT>";

    public override void OnResponse(NetState sender, RelayInfo info)
    {
      Mobile from = sender.Mobile;
      int buttonId = info.ButtonID;

      switch (buttonId)
      {
        case 1: // Previous
        {
          if (m_Page > 0)
            m_Page--;

          from.SendGump(new HouseRaffleManagementGump(m_Stone, m_Sort, m_Page));

          break;
        }
        case 2: // Next
        {
          if ((m_Page + 1) * 10 < m_Stone.Entries.Count)
            m_Page++;

          from.SendGump(new HouseRaffleManagementGump(m_Stone, m_Sort, m_Page));

          break;
        }
        case 3: // Sort by name
        {
          from.SendGump(new HouseRaffleManagementGump(m_Stone, SortMethod.Name));

          break;
        }
        case 4: // Sort by account
        {
          from.SendGump(new HouseRaffleManagementGump(m_Stone, SortMethod.Account));

          break;
        }
        case 5: // Sort by address
        {
          from.SendGump(new HouseRaffleManagementGump(m_Stone, SortMethod.Address));

          break;
        }
        default: // Delete
        {
          buttonId -= 6;

          if (buttonId >= 0 && buttonId < m_List.Count)
          {
            m_Stone.Entries.Remove(m_List[buttonId]);

            if (m_Page > 0 && m_Page * 10 >= m_List.Count - 1)
              m_Page--;

            from.SendGump(new HouseRaffleManagementGump(m_Stone, m_Sort, m_Page));
          }

          break;
        }
      }
    }

    private class NameComparer : IComparer<RaffleEntry>
    {
      public static readonly IComparer<RaffleEntry> Instance = new NameComparer();

      public int Compare(RaffleEntry x, RaffleEntry y)
      {
        bool xIsNull = x?.From == null;
        bool yIsNull = y?.From == null;

        if (xIsNull && yIsNull)
          return 0;
        if (xIsNull)
          return -1;
        if (yIsNull)
          return 1;

        int result = Insensitive.Compare(x.From.Name, y.From.Name);

        return result == 0 ? x.Date.CompareTo(y.Date) : result;
      }
    }

    private class AccountComparer : IComparer<RaffleEntry>
    {
      public static readonly IComparer<RaffleEntry> Instance = new AccountComparer();

      public int Compare(RaffleEntry x, RaffleEntry y)
      {
        bool xIsNull = x?.From == null;
        bool yIsNull = y?.From == null;

        if (xIsNull && yIsNull)
          return 0;
        if (xIsNull)
          return -1;
        if (yIsNull)
          return 1;

        Account a = x.From.Account as Account;
        Account b = y.From.Account as Account;

        if (a == null && b == null)
          return 0;
        if (a == null)
          return -1;
        if (b == null)
          return 1;

        int result = Insensitive.Compare(a.Username, b.Username);

        return result == 0 ? x.Date.CompareTo(y.Date) : result;
      }
    }

    private class AddressComparer : IComparer<RaffleEntry>
    {
      public static readonly IComparer<RaffleEntry> Instance = new AddressComparer();

      public int Compare(RaffleEntry x, RaffleEntry y)
      {
        bool xIsNull = x?.Address == null;
        bool yIsNull = y?.Address == null;

        if (xIsNull && yIsNull)
          return 0;
        if (xIsNull)
          return -1;
        if (yIsNull)
          return 1;

        byte[] a = x.Address.GetAddressBytes();
        byte[] b = y.Address.GetAddressBytes();

        for (int i = 0; i < a.Length && i < b.Length; i++)
        {
          int compare = a[i].CompareTo(b[i]);

          if (compare != 0)
            return compare;
        }

        return x.Date.CompareTo(y.Date);
      }
    }
  }
}
