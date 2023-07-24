using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Input;

namespace WindowManager.src.data
{
  internal class KeyCombo
  {
    public HashSet<Key>? Modifiers { get; set; }
    public Key? Key { get; set; }

    public KeyCombo()
    {
      this.Modifiers = null;
      this.Key = null;
    }

    public KeyCombo(HashSet<Key> modifiers, Key key)
    {
      this.Modifiers = modifiers;
      this.Key = key;
    }

    public void AddModifier(Key modifier)
    {
      Modifiers ??= new();
      Modifiers.Add(modifier);
    }

    public void RemoveModifier(Key modifier)
    {
      if (Modifiers == null) return;
      Modifiers.Remove(modifier);
    }

    public override string? ToString()
    {
      string mods = "";

      if (Modifiers != null)
      {
        List<Key> modKeys = new(Modifiers);
        modKeys.ForEach(mod => mods += mod.ToString() + " + ");
      }
      var key = Key.ToString();
      return mods + key;
    }
  }
}
