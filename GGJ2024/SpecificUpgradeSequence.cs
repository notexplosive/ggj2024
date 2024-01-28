using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame.Data;

namespace GGJ2024;

public class SpecificUpgradeSequence : UpgradeSequence
{
    private readonly List<Upgrade> _items;
    private int _index;

    public SpecificUpgradeSequence(params Upgrade[] items)
    {
        _items = items.ToList();
        _index = 0;
    }

    public override Upgrade? CheckNextPurchasable()
    {
        if (_items.IsValidIndex(_index))
        {
            return _items[_index];
        }
        return null;
    }
    
    public override Upgrade? PopNextPurchasable()
    {
        if (_items.IsValidIndex(_index))
        {
            var result = _items[_index];
            _index++;
            return result;
        }

        return null;
    }
}
